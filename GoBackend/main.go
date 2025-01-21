package main

import (
    "bytes"
    "context"
    "encoding/json"
    "fmt"
    "io/ioutil"
    "net/http"
    "os"
    "time"

    "github.com/gin-gonic/gin"
    "github.com/go-redis/redis/v8"
    "github.com/joho/godotenv"
    "github.com/sirupsen/logrus"
)

var (
    redisClient  *redis.Client
    ctx          = context.Background()
    logrusLogger = logrus.New()
)

// ChatRequest represents the incoming chat message
type ChatRequest struct {
    Message string `json:"message" binding:"required"`
}

// ChatResponse represents the AI's response
type ChatResponse struct {
    Response string `json:"response"`
}

func main() {
    // Load environment variables from .env file
    err := godotenv.Load()
    if err != nil {
        logrusLogger.Warn("No .env file found. Using environment variables.")
    }

    // Initialize the logger
    logrusLogger.SetFormatter(&logrus.TextFormatter{
        FullTimestamp: true,
    })
    logrusLogger.SetOutput(os.Stdout)
    logrusLogger.SetLevel(logrus.InfoLevel)

    // Initialize the database
    InitDatabase()

    // Initialize Redis
    initRedis()

    // Initialize Gin router
    router := gin.Default()

    // Define routes
    router.GET("/", func(c *gin.Context) {
        logrusLogger.Debug("Root endpoint hit")
        c.JSON(http.StatusOK, gin.H{"message": "Llama Middleware Server is running."})
    })

    router.POST("/chat", handleChat)

    router.GET("/version", func(c *gin.Context) {
        version := getEnv("VERSION", "1.0.0")
        c.JSON(http.StatusOK, gin.H{"version": version})
    })

    // Optional: Migrate existing chat history from JSON file
    jsonFilePath := getEnv("CHAT_HISTORY_FILE", "chat_history.json")
    if _, err := os.Stat(jsonFilePath); err == nil {
        err := migrateChatHistory(jsonFilePath)
        if err != nil {
            logrusLogger.Errorf("Failed to migrate chat history: %v", err)
        } else {
            logrusLogger.Info("Chat history migrated successfully")
        }
    } else {
        logrusLogger.Info("No chat history JSON file found. Skipping migration.")
    }

    // Start the server
    port := getEnv("PORT", "11343")
    if err := router.Run(fmt.Sprintf(":%s", port)); err != nil {
        logrusLogger.Fatalf("Failed to run server: %v", err)
    }
}

// Initialize Redis client
func initRedis() {
    redisAddr := getEnv("REDIS_ADDR", "localhost:6379")
    redisPassword := getEnv("REDIS_PASSWORD", "") // e.g. 'test' if set
    redisDB := 0

    redisClient = redis.NewClient(&redis.Options{
        Addr:     redisAddr,
        Password: redisPassword,
        DB:       redisDB,
    })

    // Test Redis connection
    _, err := redisClient.Ping(ctx).Result()
    if err != nil {
        logrusLogger.Fatalf("Failed to connect to Redis: %v", err)
    }

    logrusLogger.Info("Connected to Redis successfully")
}

// Handle /chat endpoint
func handleChat(c *gin.Context) {
    var req ChatRequest
    if err := c.ShouldBindJSON(&req); err != nil {
        logrusLogger.Warnf("Invalid request payload: %v", err)
        c.JSON(http.StatusBadRequest, gin.H{"error": "Invalid request payload."})
        return
    }

    userMessage := req.Message
    if userMessage == "" {
        c.JSON(http.StatusBadRequest, gin.H{"error": "Message cannot be empty."})
        return
    }

    logrusLogger.Infof("Received message: %s", userMessage)

    // Retrieve chat history from Redis cache
    cachedHistory, err := redisClient.Get(ctx, "chat_history").Result()
    var messages []map[string]string
    if err == nil {
        // Cache hit
        err = json.Unmarshal([]byte(cachedHistory), &messages)
        if err != nil {
            logrusLogger.Warnf("Failed to unmarshal cached chat history: %v", err)
            messages = nil
        } else {
            logrusLogger.Debug("Retrieved chat history from cache")
        }
    }

    if messages == nil {
        // Cache miss - retrieve from database
        messages, err = getChatHistory(10)
        if err != nil {
            logrusLogger.Errorf("Failed to retrieve chat history from DB: %v", err)
            c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to retrieve chat history."})
            return
        }

        // Cache the chat history
        cacheData, _ := json.Marshal(messages)
        err = redisClient.Set(ctx, "chat_history", cacheData, 5*time.Minute).Err()
        if err != nil {
            logrusLogger.Warnf("Failed to set chat history in cache: %v", err)
        } else {
            logrusLogger.Debug("Chat history cached successfully")
        }
    }

    // Append the new user message
    messages = append(messages, map[string]string{"role": "user", "content": userMessage})

    // Prepare the payload for Ollama server
    payload := map[string]interface{}{
        "model":       "llama3.2",
        "messages":    messages,
        "temperature": 0.7,
        "top_p":       0.9,
        "max_tokens":  8000,
        "stream":      false,
    }

    payloadBytes, err := json.Marshal(payload)
    if err != nil {
        logrusLogger.Errorf("Failed to marshal payload: %v", err)
        c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to process request."})
        return
    }

    logrusLogger.Debugf("Payload sent to Ollama: %s", string(payloadBytes))

    // Send request to Ollama server with indefinite or large timeout
    client := &http.Client{
        Timeout: 0, // or time.Hour * 24 if you want "effectively infinite"
    }

    ollamaReq, err := http.NewRequest("POST", "http://10.0.0.232:11434/v1/chat/completions", bytes.NewBuffer(payloadBytes))
    if err != nil {
        logrusLogger.Errorf("Failed to create request to Ollama: %v", err)
        c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to create request to AI service."})
        return
    }
    ollamaReq.Header.Set("Content-Type", "application/json")

    resp, err := client.Do(ollamaReq)
    if err != nil {
        logrusLogger.Errorf("Failed to send request to Ollama: %v", err)
        c.JSON(http.StatusBadGateway, gin.H{"error": "Failed to communicate with AI service."})
        return
    }
    defer resp.Body.Close()

    if resp.StatusCode != http.StatusOK {
        bodyBytes, _ := ioutil.ReadAll(resp.Body)
        logrusLogger.Errorf("Ollama server returned non-OK status: %d, body: %s", resp.StatusCode, string(bodyBytes))
        c.JSON(http.StatusBadGateway, gin.H{"error": "AI service returned an error."})
        return
    }

    // Parse the response
    var ollamaResp map[string]interface{}
    decoder := json.NewDecoder(resp.Body)
    if err := decoder.Decode(&ollamaResp); err != nil {
        logrusLogger.Errorf("Failed to decode Ollama response: %v", err)
        c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to decode AI response."})
        return
    }

    choices, ok := ollamaResp["choices"].([]interface{})
    if !ok || len(choices) == 0 {
        logrusLogger.Error("Ollama response does not contain choices")
        c.JSON(http.StatusInternalServerError, gin.H{"error": "No response from AI."})
        return
    }

    firstChoice, ok := choices[0].(map[string]interface{})
    if !ok {
        logrusLogger.Error("First choice in Ollama response is not a valid object")
        c.JSON(http.StatusInternalServerError, gin.H{"error": "Invalid AI response format."})
        return
    }

    message, ok := firstChoice["message"].(map[string]interface{})
    if !ok {
        logrusLogger.Error("Message field in Ollama response is not a valid object")
        c.JSON(http.StatusInternalServerError, gin.H{"error": "Invalid AI response format."})
        return
    }

    aiMessage, ok := message["content"].(string)
    if !ok {
        logrusLogger.Error("Content field in message is not a string")
        c.JSON(http.StatusInternalServerError, gin.H{"error": "Invalid AI response content."})
        return
    }

    // Insert into DB: user message & AI response
    err = appendToChatHistory(userMessage, aiMessage)
    if err != nil {
        logrusLogger.Errorf("Failed to append to chat history: %v", err)
        c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to save chat history."})
        return
    }

    logrusLogger.Infof("AI response: %s", aiMessage)

    // Update the cache with the new message
    messages = append(messages, map[string]string{"role": "assistant", "content": aiMessage})
    if len(messages) > 20 {
        messages = messages[len(messages)-20:]
    }
    cacheData, err := json.Marshal(messages)
    if err != nil {
        logrusLogger.Warnf("Failed to marshal updated chat history for cache: %v", err)
    } else {
        err = redisClient.Set(ctx, "chat_history", cacheData, 5*time.Minute).Err()
        if err != nil {
            logrusLogger.Warnf("Failed to update chat history in cache: %v", err)
        }
    }

    // Respond to the client
    c.JSON(http.StatusOK, ChatResponse{Response: aiMessage})
}

// Append chat history to the database
func appendToChatHistory(userInput, aiResponse string) error {
    // We store them in the same Chat row
    chat := Chat{
        UserMessage: userInput,
        BotResponse: aiResponse,
        CreatedAt:   time.Now(),
    }
    result := DB.Create(&chat)
    if result.Error != nil {
        return result.Error
    }
    return nil
}

// Retrieve chat history from the database, merges them to a single array of role/content
func getChatHistory(limit int) ([]map[string]string, error) {
    var chats []Chat
    // We store user + AI in one row, so we do 2 * limit to get effectively limit user messages
    // Adjust as needed
    result := DB.Order("id desc").Limit(limit * 2).Find(&chats)
    if result.Error != nil {
        return nil, result.Error
    }

    // Reverse so oldest is first
    for i, j := 0, len(chats)-1; i < j; i, j = i+1, j-1 {
        chats[i], chats[j] = chats[j], chats[i]
    }

    messages := []map[string]string{
        {"role": "system", "content": "You are a helpful assistant."},
    }

    for _, chat := range chats {
        messages = append(messages, map[string]string{"role": "user", "content": chat.UserMessage})
        messages = append(messages, map[string]string{"role": "assistant", "content": chat.BotResponse})
    }

    return messages, nil
}

// Migrate existing chat history from a JSON file to the database
type ChatHistoryEntry struct {
    User string `json:"user"`
    Bot  string `json:"bot"`
}

func migrateChatHistory(jsonFilePath string) error {
    logrusLogger.Infof("Checking for existing chat history JSON file at %s", jsonFilePath)
    file, err := os.Open(jsonFilePath)
    if err != nil {
        return fmt.Errorf("failed to open chat history file: %v", err)
    }
    defer file.Close()

    var history []ChatHistoryEntry
    decoder := json.NewDecoder(file)
    if err := decoder.Decode(&history); err != nil {
        return fmt.Errorf("failed to decode chat history JSON: %v", err)
    }

    migratedEntries := 0
    for _, entry := range history {
        userMessage := entry.User
        botResponse := entry.Bot

        if userMessage == "" || botResponse == "" {
            logrusLogger.Warnf("Skipping incomplete chat entry: %+v", entry)
            continue
        }

        // Check if already exists
        var existing Chat
        // We look up by user_message + bot_response
        result := DB.Where("user_message = ? AND bot_response = ?", userMessage, botResponse).First(&existing)
        if result.Error == nil {
            logrusLogger.Infof("Chat entry already exists, skipping: %+v", entry)
            continue
        }

        // Insert
        chat := Chat{
            UserMessage: userMessage,
            BotResponse: botResponse,
            CreatedAt:   time.Now(),
        }
        if err := DB.Create(&chat).Error; err != nil {
            logrusLogger.Errorf("Failed to create chat entry: %v", err)
            continue
        }
        migratedEntries++
    }

    logrusLogger.Infof("Migration completed. %d entries added to the database.", migratedEntries)
    return nil
}

// database.go
package main

import (
    "fmt"
    "log"
    "os"

    "gorm.io/driver/postgres"
    "gorm.io/gorm"
    "gorm.io/gorm/logger"
)

var DB *gorm.DB

// InitDatabase initializes the database connection and performs migrations
func InitDatabase() {
    dsn := getEnv("DATABASE_URL", "host=localhost user=postgres password=YOURPASS dbname=llama_middleware port=5432 sslmode=disable TimeZone=UTC")
    var err error
    DB, err = gorm.Open(postgres.Open(dsn), &gorm.Config{
        Logger: logger.Default.LogMode(logger.Info), // Adjust log level as needed
    })
    if err != nil {
        log.Fatalf("Failed to connect to database: %v", err)
    }

    // Auto-migrate the ChatMessage model
    err = DB.AutoMigrate(&ChatMessage{})
    if err != nil {
        log.Fatalf("Failed to auto-migrate ChatMessage model: %v", err)
    }

    fmt.Println("Database connected and migrated successfully")
}

// getEnv retrieves environment variables or returns a default value
func getEnv(key, defaultVal string) string {
    if value, exists := os.LookupEnv(key); exists {
        return value
    }
    return defaultVal
}


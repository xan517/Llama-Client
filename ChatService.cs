using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using NLog; // Ensure NLog is installed via NuGet
using Polly;
using Polly.Retry;

namespace WindowsFormsApp1
{
    public class ChatService
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private readonly string apiUrl;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly AsyncRetryPolicy<HttpResponseMessage> retryPolicy;

        public ChatService(string host)
        {
            // e.g., host = "localhost" or "10.0.0.232", depending on your Go server's IP
            // The Go API listens on port 11343 by default
            apiUrl = $"http://{host}:11343/chat";

            // Clear any default headers just in case
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.Timeout = System.Threading.Timeout.InfiniteTimeSpan;

            // Define a retry policy with exponential backoff (3 retries)
            retryPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .Or<HttpRequestException>()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (outcome, timespan, retryCount, context) =>
                    {
                        string reason = outcome.Exception != null
                            ? outcome.Exception.Message
                            : outcome.Result.StatusCode.ToString();

                        logger.Warn($"Retry {retryCount} after {timespan.Seconds}s due to {reason}");
                    }
                );
        }

        /// <summary>
        /// Sends a user message to the Go API at /chat and returns the AI's response.
        /// </summary>
        /// <param name="userMessage">The user's message to send.</param>
        /// <returns>A string containing the AI's response or an error message.</returns>
        public async Task<string> SendMessageAsync(string userMessage)
        {
            try
            {
                // Prepare the JSON payload
                var payload = new { message = userMessage };
                var jsonPayload = JsonConvert.SerializeObject(payload);

                logger.Info($"Sending message to Middleware: {userMessage}");

                // Use Polly retry policy
                var response = await retryPolicy.ExecuteAsync(async () =>
                {
                    // Create fresh StringContent on each attempt
                    using (var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json"))
                    {
                        logger.Info("Attempting POST to /chat endpoint...");
                        return await httpClient.PostAsync(apiUrl, content);
                    }
                });

                logger.Info($"Received HTTP Status: {response.StatusCode}");

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    logger.Error("The API endpoint was not found.");
                    return "[Error]: The API endpoint was not found. Please check the API URL.";
                }

                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                logger.Info($"Response Content: {responseString}");

                // Deserialize response object
                var responseObject = JsonConvert.DeserializeObject<ChatResponse>(
                    responseString,
                    new JsonSerializerSettings
                    {
                        ContractResolver = new DefaultContractResolver
                        {
                            NamingStrategy = new SnakeCaseNamingStrategy()
                        },
                        MissingMemberHandling = MissingMemberHandling.Ignore,
                        NullValueHandling = NullValueHandling.Ignore
                    }
                );

                if (responseObject != null && !string.IsNullOrEmpty(responseObject.Response))
                {
                    logger.Info($"AI Response: {responseObject.Response}");
                    return responseObject.Response;
                }
                else
                {
                    logger.Error("No valid response content received from the server.");
                    return "[Error]: No valid response content received from the server.";
                }
            }
            catch (HttpRequestException httpEx)
            {
                logger.Error($"HTTP Error: {httpEx.Message}");
                return $"[HTTP Error]: {httpEx.Message}";
            }
            catch (JsonException jsonEx)
            {
                logger.Error($"JSON Parsing Error: {jsonEx.Message}");
                return $"[JSON Parsing Error]: {jsonEx.Message}";
            }
            catch (Exception ex)
            {
                logger.Error($"Unexpected Error: {ex.Message}\nStack Trace: {ex.StackTrace}");
                return $"[Unexpected Error]: {ex.Message}";
            }
        }
    }
}

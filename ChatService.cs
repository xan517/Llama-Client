// ChatService.cs
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
        private readonly string apiKey; // API Key for authentication
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly AsyncRetryPolicy<HttpResponseMessage> retryPolicy;

        public ChatService(string host, string apiKey)
        {
            apiUrl = $"http://{host}:11343/chat"; // Correct endpoint
            this.apiKey = apiKey;

            if (!string.IsNullOrEmpty(apiKey))
            {
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
            }

            // Define retry policy: retry 3 times with exponential backoff
            retryPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .Or<HttpRequestException>()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (outcome, timespan, retryCount, context) =>
                    {
                        logger.Warn($"Retry {retryCount} after {timespan.Seconds}s due to {outcome.Exception?.Message ?? outcome.Result.StatusCode.ToString()}");
                    });
        }

        /// <summary>
        /// Sends a user message to the AI server and returns the AI's response.
        /// </summary>
        public async Task<string> SendMessageAsync(string userMessage)
        {
            try
            {
                var payload = new
                {
                    message = userMessage
                };

                var jsonPayload = JsonConvert.SerializeObject(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                logger.Info($"Sending message to Middleware: {userMessage}");

                // Execute the POST request with retry policy
                var response = await retryPolicy.ExecuteAsync(() => httpClient.PostAsync(apiUrl, content));

                logger.Info($"Received HTTP Status: {response.StatusCode}");

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    logger.Error("The API endpoint was not found.");
                    return "[Error]: The API endpoint was not found. Please check the API URL.";
                }

                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();

                logger.Info($"Response Content: {responseString}");

                var responseObject = JsonConvert.DeserializeObject<ChatResponse>(responseString, new JsonSerializerSettings
                {
                    ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new SnakeCaseNamingStrategy()
                    },
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore
                });

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
                logger.Error($"Unexpected Error: {ex.Message}");
                return $"[Unexpected Error]: {ex.Message}";
            }
        }
    }

  
}

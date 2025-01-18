// ChatHistoryManager.cs
using System;
using System.IO;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    public class ChatHistoryManager
    {
        private readonly string logDirectory;
        private readonly string logFilePath;

        public ChatHistoryManager(string conversationId)
        {
            logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            logFilePath = Path.Combine(logDirectory, $"chat_{conversationId}.log");
        }

        /// <summary>
        /// Writes a message to the log file asynchronously using Task.Run.
        /// </summary>
        public async Task WriteMessageAsync(string message)
        {
            try
            {
                await Task.Run(() => File.AppendAllText(logFilePath, message + Environment.NewLine));
            }
            catch (Exception ex)
            {
                // Handle errors (e.g., log to a separate error log or notify the user)
                // For simplicity, write to console
                Console.WriteLine($"[Log Error]: {ex.Message}");
            }
        }

        /// <summary>
        /// Reads all messages from the log file asynchronously using Task.Run.
        /// </summary>
        public async Task<string[]> ReadAllMessagesAsync()
        {
            try
            {
                if (File.Exists(logFilePath))
                {
                    return await Task.Run(() => File.ReadAllLines(logFilePath));
                }
                return Array.Empty<string>();
            }
            catch (Exception ex)
            {
                // Handle errors appropriately
                Console.WriteLine($"[Read History Error]: {ex.Message}");
                return Array.Empty<string>();
            }
        }
    }
}

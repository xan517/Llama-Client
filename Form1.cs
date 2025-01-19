// MainForm.cs
using CefSharp;
using CefSharp.WinForms;
using HtmlAgilityPack;
using Newtonsoft.Json;
using NLog;
using System;
using System.Diagnostics;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class MainForm : Form
    {
        private string conversationId;
        private StringBuilder chatHistory; // Store all chat messages

        private ChatHistoryManager chatHistoryManager;
        private ChatService chatService;
        private MarkdownRenderer markdownRenderer;
        private ThemeManager themeManager;
        

        public MainForm()
        {
            InitializeComponent();
            InitializeMarkdownBrowser();
            BindEventHandlers();

            conversationId = Guid.NewGuid().ToString();
            chatHistory = new StringBuilder();

            // Initialize Managers
            chatHistoryManager = new ChatHistoryManager(conversationId);
            markdownRenderer = new MarkdownRenderer();
            themeManager = new ThemeManager();
           
           
        }

        /// <summary>
        /// Binds UI event handlers.
        /// </summary>
        private void BindEventHandlers()
        {
            // inputTextBox KeyDown event is already linked in Designer
            connectButton.Click += connectButton_Click;
            zoomTrackBar.Scroll += zoomTrackBar_Scroll;
            this.FormClosing += MainForm_FormClosing;
            this.Load += MainForm_Load;
        }

        /// <summary>
        /// Handles form load to apply themes and load chat history.
        /// </summary>
        private async void MainForm_Load(object sender, EventArgs e)
        {
            themeManager.ApplyDarkTheme(this);

            // Load chat history from log file
            var messages = await chatHistoryManager.ReadAllMessagesAsync();
            foreach (var message in messages)
            {
                await AppendMarkdownAsync(message);
            }
        }

        /// <summary>
        /// Handles the Connect button click to set the API URL and initialize ChatService.
        /// </summary>
        private async void connectButton_Click(object sender, EventArgs e)
        {
            string host = hostNameBox.Text.Trim();
            string apiKey = apiKeyBox.Text.Trim();

            Console.WriteLine($"Host entered: {host}");
            Console.WriteLine($"API key entered: {apiKey}");

            if (string.IsNullOrEmpty(host))
            {
                MessageBox.Show("Please enter the Middleware server host.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(apiKey))
            {
                MessageBox.Show("Please enter the API key.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                chatService = new ChatService(host, apiKey);
                Console.WriteLine("ChatService initialized successfully.");

                if (!chatHistory.ToString().Contains("[Info]: Connected to Middleware server"))
                {
                    await AppendMarkdownAsync("[Info]: Connected to Middleware server.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to initialize ChatService: {ex.Message}");
                MessageBox.Show($"Failed to connect: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Handles the Enter key press in the input textbox to send user input.
        /// </summary>
        private async void InputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                e.SuppressKeyPress = true; // Prevent default new line behavior
                string userInput = inputTextBox.Text.Trim();
                inputTextBox.Clear();

                if (!string.IsNullOrEmpty(userInput))
                {
                    await AppendMarkdownAsync($"**You:** {userInput}");
                    await SendUserInput(userInput);
                }
            }
        }

        /// <summary>
        /// Sends the user's input to the AI server and handles the response.
        /// </summary>
        private async Task SendUserInput(string input)
        {
            if (chatService == null)
            {
                await AppendMarkdownAsync("[Error]: Please connect to the Middleware server first.");
                return;
            }

            try
            {
                ShowLoadingSpinner(true); // Show spinner

                string aiResponse = await chatService.SendMessageAsync(input);
                await AppendMarkdownAsync($"**AI:** {aiResponse}");
            }
            catch (Exception ex)
            {
                await AppendMarkdownAsync($"[Unexpected Error]: {ex.Message}");
            }
            finally
            {
                ShowLoadingSpinner(false); // Hide spinner
            }
        }

        /// <summary>
        /// Initializes the Chromium browser with a base HTML template that includes Highlight.js and necessary scripts.
        /// </summary>
        private void InitializeMarkdownBrowser()
        {
            string initialHtml = @"
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset='UTF-8'>
            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
            <style>
                body {
                    font-family: Arial, sans-serif;
                    margin: 0;
                    padding: 10px;
                    background-color: #121212;
                    color: #e0e0e0; /* Light text color */
                    box-sizing: border-box;
                }
                #content {
                    max-width: 100%;
                    overflow-wrap: break-word;
                }
                /* Ensure all child elements inherit text color */
                #content, #content * {
                    color: inherit;
                }
                /* Highlight.js Theme */
                .hljs {
                    display: block;
                    overflow-x: auto;
                    padding: 0.5em;
                    background: #272822;
                    color: #f8f8f2;
                }
                /* Additional styling for code blocks */
                pre {
                    background-color: #272822;
                    padding: 10px;
                    border-radius: 5px;
                    overflow-x: auto;
                }
                /* Responsive Images */
                img {
                    max-width: 100%;
                    height: auto;
                }
                /* Optional: Style links */
                a {
                    color: #1e88e5;
                }
                /* Loading Spinner Styles */
                #loadingSpinner {
                    position: fixed;
                    top: 50%;
                    left: 50%;
                    transform: translate(-50%, -50%);
                    width: 50px;
                    height: 50px;
                    border: 5px solid #333;
                    border-top: 5px solid #1e88e5;
                    border-radius: 50%;
                    animation: spin 1s linear infinite;
                    display: none; /* Hidden by default */
                    z-index: 1000;
                }
                @keyframes spin {
                    0% { transform: rotate(0deg); }
                    100% { transform: rotate(360deg); }
                }
            </style>
            <!-- Highlight.js Library with All Languages -->
            <script src='https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.8.0/highlight.min.js'></script>
            <!-- Choose a Highlight.js Theme -->
            <link rel='stylesheet' href='https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.8.0/styles/monokai-sublime.min.css'>
            <script>
                // Initialize Highlight.js
                document.addEventListener('DOMContentLoaded', (event) => {
                    try {
                        hljs.highlightAll();
                    } catch (error) {
                        console.error('Highlight.js initialization failed:', error);
                    }
                });

                // Function to re-apply syntax highlighting after new content is loaded
                function applyHighlighting() {
                    try {
                        hljs.highlightAll();
                    } catch (error) {
                        console.error('Highlight.js failed to apply syntax highlighting:', error);
                    }
                }

                // Function to smoothly scroll to the bottom of the page
                function scrollToBottom() {
                    window.scrollTo({
                        top: document.body.scrollHeight,
                        behavior: 'smooth'
                    });
                }

                // Function to append HTML to the content div
                function appendMessage(html) {
                    var content = document.getElementById('content');
                    var messageDiv = document.createElement('div');
                    messageDiv.innerHTML = html;
                    content.appendChild(messageDiv);
                    applyHighlighting();
                    scrollToBottom();
                }
            </script>
        </head>
        <body>
            <div id='content'>Welcome to Llama Base!</div>
            <div id='loadingSpinner'></div> <!-- Loading Spinner -->
        </body>
        </html>";

            webBrowserOutput.LoadHtml(initialHtml, "http://localhost");
        }



        /// <summary>
        /// Appends markdown content to the chat history and updates the browser with syntax highlighting.
        /// </summary>
        private async Task AppendMarkdownAsync(string markdown)
        {
            // Prevent duplicates by checking the last message in the history
            var lastMessage = chatHistory.ToString().Trim().Split(new[] { Environment.NewLine }, StringSplitOptions.None).LastOrDefault();

            if (lastMessage == markdown.Trim())
            {
                return; // Skip appending duplicate messages
            }

            // Append to chatHistory
            if (chatHistory.Length > 0)
            {
                chatHistory.AppendLine();
            }

            chatHistory.AppendLine(markdown);

            // Convert Markdown to HTML
            string html = markdownRenderer.ConvertToHtml(markdown);

            // Load HTML into HtmlAgilityPack for processing
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);

            // Select all <pre><code> nodes
            var codeNodes = doc.DocumentNode.SelectNodes("//pre/code");

            if (codeNodes != null)
            {
                foreach (var codeNode in codeNodes)
                {
                    string codeContent = codeNode.InnerText;
                    string detectedLanguage = LanguageDetector.DetectLanguage(codeContent);

                    if (detectedLanguage != "plaintext")
                    {
                        // Add the language- class to the <code> tag
                        codeNode.SetAttributeValue("class", $"language-{detectedLanguage}");
                    }
                }
            }

            // Get the modified HTML
            string processedHtml = doc.DocumentNode.InnerHtml;

            // Wrap in a div to ensure structure
            string wrappedHtml = $"<div>{processedHtml}</div>";

            // Escape the HTML content for JavaScript string
            string escapedHtml = EscapeJavaScriptString(wrappedHtml);

            // Append the new message via JavaScript
            string script = $"appendMessage(`{escapedHtml}`);";

            // Execute the script on the browser
            webBrowserOutput.ExecuteScriptAsync(script);

            // Write the message to the log file
            await chatHistoryManager.WriteMessageAsync(markdown);
        }

        /// <summary>
        /// Escapes special characters in a string to safely inject into JavaScript.
        /// </summary>
        private string EscapeJavaScriptString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            return input
                .Replace("\\", "\\\\")
                .Replace("`", "\\`")
                .Replace("$", "\\$")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n")
                .Replace("\"", "\\\"")
                .Replace("'", "\\'");
        }

        /// <summary>
        /// Shows or hides the loading spinner.
        /// </summary>
        private void ShowLoadingSpinner(bool show)
        {
            string script = $@"
                var spinner = document.getElementById('loadingSpinner');
                if (spinner) {{
                    spinner.style.display = '{(show ? "block" : "none")}';
                }}";
            webBrowserOutput.ExecuteScriptAsync(script);
        }

        /// <summary>
        /// Adjusts the zoom level of the browser based on the trackbar value.
        /// </summary>
        private void zoomTrackBar_Scroll(object sender, EventArgs e)
        {
            if (webBrowserOutput != null)
            {
                webBrowserOutput.SetZoomLevel(zoomTrackBar.Value);
            }

            this.Text = $"Llama Base - Zoom Level: {zoomTrackBar.Value}";
        }

        /// <summary>
        /// Handles form closing to properly shut down CefSharp.
        /// </summary>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // No need to call Cef.Shutdown here since it's handled in Program.cs
        }
    }

    /// <summary>
    /// Represents the structure of the AI server's chat response.
    /// </summary>
    public class ChatResponse
    {
        [JsonProperty("response")]
        public string Response { get; set; }
    }
}

using System;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms;
using Markdig;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace WindowsFormsApp1
{
    public partial class MainForm : Form
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private string apiUrl = "http://10.0.0.232:11434/api/chat"; // Default Ollama server URL
        private string conversationId;
        private StringBuilder chatHistory; // Store all chat messages

        public MainForm()
        {
            InitializeComponent();
            InitializeMarkdownBrowser();
            inputTextBox.KeyDown += InputTextBox_KeyDown;
            connectButton.Click += connectButton_Click;
            zoomTrackBar.Scroll += zoomTrackBar_Scroll;
            this.FormClosing += MainForm_FormClosing;
            this.Load += MainForm_Load;

            conversationId = Guid.NewGuid().ToString();
            chatHistory = new StringBuilder();
        }

        /// <summary>
        /// Initializes the Chromium browser with a base HTML template.
        /// </summary>
        private void InitializeMarkdownBrowser()
        {
            // Load initial content with Highlight.js integrated
            string initialHtml = @"
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
        /// Handles actions after the frame has finished loading, such as applying syntax highlighting and scrolling.
        /// </summary>
        private void WebBrowserOutput_FrameLoadEnd(object sender, CefSharp.FrameLoadEndEventArgs e)
        {
            if (e.Frame.IsMain)
            {
                // Execute the combined highlighting and scrolling function
                webBrowserOutput.ExecuteScriptAsync("applyHighlighting();");
            }
        }

        /// <summary>
        /// Handles the Connect button click to set the API URL.
        /// </summary>
        private void connectButton_Click(object sender, EventArgs e)
        {
            string host = hostNameBox.Text.Trim();

            if (string.IsNullOrEmpty(host))
            {
                MessageBox.Show("Please enter the Ollama server host.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            apiUrl = $"http://{host}:11434/api/chat";

            // Add the connection message only if it hasn't already been added
            if (!chatHistory.ToString().Contains("[Info]: Connected to Ollama server"))
            {
                AppendMarkdown("[Info]: Connected to Ollama server.");
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
                    AppendMarkdown($"**You:** {userInput}");
                    await SendUserInput(userInput);
                }
            }
        }

        /// <summary>
        /// Sends the user's input to the AI server and handles the response.
        /// </summary>
        private async Task SendUserInput(string input)
        {
            try
            {
                ShowLoadingSpinner(true); // Show spinner

                var payload = new
                {
                    model = "llama3.2",
                    messages = new[]
                    {
                        new { role = "user", content = input }
                    },
                    stream = false
                };

                var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

                // Remove or comment out the sending message
                // AppendMarkdown("[Info]: Sending request to AI server...");

                var response = await httpClient.PostAsync(apiUrl, content);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
               // AppendMarkdown($"[Debug]: Raw response: {responseString}");

                var responseObject = JsonConvert.DeserializeObject<OllamaChatResponse>(responseString, new JsonSerializerSettings
                {
                    ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new SnakeCaseNamingStrategy()
                    },
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore
                });

                if (responseObject != null && responseObject.Message != null && !string.IsNullOrEmpty(responseObject.Message.Content))
                {
                    AppendMarkdown($"**AI:** {responseObject.Message.Content}");
                }
                else
                {
                    AppendMarkdown("[Error]: No valid response content received from the server.");
                }
            }
            catch (HttpRequestException httpEx)
            {
                AppendMarkdown($"[HTTP Error]: {httpEx.Message}");
            }
            catch (JsonException jsonEx)
            {
                AppendMarkdown($"[JSON Parsing Error]: {jsonEx.Message}");
            }
            catch (Exception ex)
            {
                AppendMarkdown($"[Unexpected Error]: {ex.Message}");
            }
            finally
            {
                ShowLoadingSpinner(false); // Hide spinner
            }
        }

        /// <summary>
        /// Appends markdown content to the chat history and updates the browser.
        /// </summary>
        private void AppendMarkdown(string markdown)
        {
            // Prevent duplicates by checking the last message in the history
            var lastMessage = chatHistory.ToString().Trim().Split(new[] { Environment.NewLine }, StringSplitOptions.None).LastOrDefault();

            if (lastMessage == markdown.Trim())
            {
                return; // Skip appending duplicate messages
            }

            if (chatHistory.Length > 0)
            {
                chatHistory.AppendLine();
            }

            chatHistory.AppendLine(markdown);

            // Use Markdown pipeline to convert to HTML
            var pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .Build();

            string fullHtml = Markdown.ToHtml(chatHistory.ToString(), pipeline);

            // Ensure Highlight.js-compatible structure for code blocks
            string wrappedHtml = fullHtml.Replace("<code>", "<code class=\"language-csharp\">");

            // Escape the HTML content for JavaScript injection
            string escapedHtml = EscapeJavaScriptString(wrappedHtml);

            // Append the message and trigger syntax highlighting
            string script = $@"
        appendMessage(`{escapedHtml}`);
        applyHighlighting();"; // Re-apply syntax highlighting
            webBrowserOutput.ExecuteScriptAsync(script);
        }



        /// <summary>
        /// Escapes special characters in a string to safely inject into JavaScript.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>The escaped string.</returns>
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

        /// <summary>
        /// Applies a dark theme to all controls recursively.
        /// </summary>
        private void ApplyDarkTheme(Control control)
        {
            if (control is GroupBox)
            {
                control.BackColor = Color.FromArgb(30, 30, 30);
                control.ForeColor = Color.White;
            }
            else if (control is TextBox || control is RichTextBox)
            {
                control.BackColor = Color.FromArgb(45, 45, 48);
                control.ForeColor = Color.White;
            }
            else if (control is Button button)
            {
                button.BackColor = Color.FromArgb(50, 50, 50);
                button.ForeColor = Color.White;
            }
            else
            {
                control.BackColor = Color.FromArgb(30, 30, 30);
                control.ForeColor = Color.White;
            }

            foreach (Control childControl in control.Controls)
            {
                ApplyDarkTheme(childControl);
            }
        }

        /// <summary>
        /// Handles form load to apply themes and initial settings.
        /// </summary>
        private void MainForm_Load(object sender, EventArgs e)
        {
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ForeColor = Color.White;

            foreach (Control control in this.Controls)
            {
                ApplyDarkTheme(control);
            }
        }
    }

    /// <summary>
    /// Represents the structure of the AI server's chat response.
    /// </summary>
    public class OllamaChatResponse
    {
        [JsonProperty("model")]
        public string Model { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("message")]
        public OllamaMessage Message { get; set; }

        [JsonProperty("done_reason")]
        public string DoneReason { get; set; }

        [JsonProperty("done")]
        public bool Done { get; set; }
    }

    /// <summary>
    /// Represents the message structure within the AI server's response.
    /// </summary>
    public class OllamaMessage
    {
        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }
    }
}

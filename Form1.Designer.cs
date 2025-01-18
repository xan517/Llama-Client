// MainForm.Designer.cs
using System.Drawing;
using System.Windows.Forms;
using CefSharp.WinForms; // Ensure CefSharp.WinForms is referenced
using CefSharp;

namespace WindowsFormsApp1
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        private ChromiumWebBrowser webBrowserOutput; // Ensure CefSharp.WinForms.ChromiumWebBrowser is used

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
                if (webBrowserOutput != null)
                {
                    webBrowserOutput.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.hostNameBox = new System.Windows.Forms.TextBox();
            this.hostLabel = new System.Windows.Forms.Label();
            this.connectButton = new System.Windows.Forms.Button();
            this.connectionGroupBox = new System.Windows.Forms.GroupBox();
            this.apiKeyBox = new System.Windows.Forms.TextBox();
            this.apiKeyLabel = new System.Windows.Forms.Label(); // Added Label for API Key
            this.zoomLabel = new System.Windows.Forms.Label();
            this.zoomTrackBar = new System.Windows.Forms.TrackBar();
            this.chatGroupBox = new System.Windows.Forms.GroupBox();
            this.inputGroupBox = new System.Windows.Forms.GroupBox();
            this.inputTextBox = new System.Windows.Forms.RichTextBox();
            this.connectionGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.zoomTrackBar)).BeginInit();
            this.inputGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // hostNameBox
            // 
            this.hostNameBox.Location = new System.Drawing.Point(12, 39);
            this.hostNameBox.MaxLength = 60;
            this.hostNameBox.Name = "hostNameBox";
            this.hostNameBox.Size = new System.Drawing.Size(150, 20);
            this.hostNameBox.TabIndex = 0;
            this.hostNameBox.Text = "10.0.0.232";
            // 
            // hostLabel
            // 
            this.hostLabel.AutoSize = true;
            this.hostLabel.Location = new System.Drawing.Point(9, 23);
            this.hostLabel.Name = "hostLabel";
            this.hostLabel.Size = new System.Drawing.Size(80, 13);
            this.hostLabel.TabIndex = 1;
            this.hostLabel.Text = "Hostname or IP";
            // 
            // apiKeyBox
            // 
            this.apiKeyBox.Location = new System.Drawing.Point(168, 39);
            this.apiKeyBox.MaxLength = 100;
            this.apiKeyBox.Name = "apiKeyBox";
            this.apiKeyBox.Size = new System.Drawing.Size(200, 20);
            this.apiKeyBox.TabIndex = 2;
            this.apiKeyBox.UseSystemPasswordChar = true; // Mask API Key input
            // 
            // apiKeyLabel
            // 
            this.apiKeyLabel.AutoSize = true;
            this.apiKeyLabel.Location = new System.Drawing.Point(165, 23);
            this.apiKeyLabel.Name = "apiKeyLabel";
            this.apiKeyLabel.Size = new System.Drawing.Size(47, 13);
            this.apiKeyLabel.TabIndex = 3;
            this.apiKeyLabel.Text = "API Key";
            // 
            // connectButton
            // 
            this.connectButton.Location = new System.Drawing.Point(374, 37);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(75, 23);
            this.connectButton.TabIndex = 4;
            this.connectButton.Text = "Connect";
            this.connectButton.UseVisualStyleBackColor = true;
            this.connectButton.Click += new System.EventHandler(this.connectButton_Click);
            // 
            // connectionGroupBox
            // 
            this.connectionGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.connectionGroupBox.Controls.Add(this.apiKeyLabel); // Added API Key Label
            this.connectionGroupBox.Controls.Add(this.apiKeyBox); // Added API Key Box
            this.connectionGroupBox.Controls.Add(this.zoomLabel);
            this.connectionGroupBox.Controls.Add(this.zoomTrackBar);
            this.connectionGroupBox.Controls.Add(this.connectButton);
            this.connectionGroupBox.Controls.Add(this.hostNameBox);
            this.connectionGroupBox.Controls.Add(this.hostLabel);
            this.connectionGroupBox.Location = new System.Drawing.Point(12, 12);
            this.connectionGroupBox.Name = "connectionGroupBox";
            this.connectionGroupBox.Size = new System.Drawing.Size(870, 80);
            this.connectionGroupBox.TabIndex = 5;
            this.connectionGroupBox.TabStop = false;
            this.connectionGroupBox.Text = "Connection";
            // 
            // zoomLabel
            // 
            this.zoomLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.zoomLabel.AutoSize = true;
            this.zoomLabel.Location = new System.Drawing.Point(550, 23);
            this.zoomLabel.Name = "zoomLabel";
            this.zoomLabel.Size = new System.Drawing.Size(101, 13);
            this.zoomLabel.TabIndex = 6;
            this.zoomLabel.Text = "Chat Window Zoom";
            // 
            // zoomTrackBar
            // 
            this.zoomTrackBar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.zoomTrackBar.Location = new System.Drawing.Point(553, 39);
            this.zoomTrackBar.Minimum = -10;
            this.zoomTrackBar.Maximum = 10;
            this.zoomTrackBar.Name = "zoomTrackBar";
            this.zoomTrackBar.Size = new System.Drawing.Size(200, 45);
            this.zoomTrackBar.TabIndex = 5;
            this.zoomTrackBar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.zoomTrackBar.Value = 0;
            this.zoomTrackBar.Scroll += new System.EventHandler(this.zoomTrackBar_Scroll);
            // 
            // chatGroupBox
            // 
            this.chatGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chatGroupBox.Location = new System.Drawing.Point(12, 98);
            this.chatGroupBox.Name = "chatGroupBox";
            this.chatGroupBox.Size = new System.Drawing.Size(870, 370);
            this.chatGroupBox.TabIndex = 6;
            this.chatGroupBox.TabStop = false;
            this.chatGroupBox.Text = "Chat";
            // 
            // webBrowserOutput
            // 
            this.webBrowserOutput = new ChromiumWebBrowser("about:blank")
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(18, 18, 18), // Match dark theme
                Name = "webBrowserOutput",
                Size = new System.Drawing.Size(864, 344),
                TabIndex = 1
            };
            this.chatGroupBox.Controls.Add(this.webBrowserOutput); // Add webBrowserOutput to chatGroupBox

            // 
            // inputGroupBox
            // 
            this.inputGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.inputGroupBox.Controls.Add(this.inputTextBox);
            this.inputGroupBox.Location = new System.Drawing.Point(12, 474);
            this.inputGroupBox.Name = "inputGroupBox";
            this.inputGroupBox.Size = new System.Drawing.Size(870, 93);
            this.inputGroupBox.TabIndex = 7;
            this.inputGroupBox.TabStop = false;
            this.inputGroupBox.Text = "Input";
            // 
            // inputTextBox
            // 
            this.inputTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inputTextBox.Location = new System.Drawing.Point(3, 16);
            this.inputTextBox.Name = "inputTextBox";
            this.inputTextBox.Size = new System.Drawing.Size(864, 74);
            this.inputTextBox.TabIndex = 0;
            this.inputTextBox.Text = "";
            this.inputTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.InputTextBox_KeyDown);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(894, 612);
            this.Controls.Add(this.inputGroupBox);
            this.Controls.Add(this.chatGroupBox);
            this.Controls.Add(this.connectionGroupBox);
            this.Name = "MainForm";
            this.Text = "Llama Base";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.connectionGroupBox.ResumeLayout(false);
            this.connectionGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.zoomTrackBar)).EndInit();
            this.inputGroupBox.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox hostNameBox;
        private System.Windows.Forms.Label hostLabel;
        private System.Windows.Forms.Button connectButton;
        private System.Windows.Forms.GroupBox connectionGroupBox;
        private System.Windows.Forms.GroupBox chatGroupBox;
        
        private System.Windows.Forms.GroupBox inputGroupBox;
        private System.Windows.Forms.RichTextBox inputTextBox;
        private System.Windows.Forms.TrackBar zoomTrackBar; // Zoom control
        private System.Windows.Forms.Label zoomLabel;
        private System.Windows.Forms.TextBox apiKeyBox;
        private System.Windows.Forms.Label apiKeyLabel; // Label for API Key
    }
}

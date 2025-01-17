using System.Drawing;

namespace WindowsFormsApp1
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
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
            this.chatGroupBox = new System.Windows.Forms.GroupBox();
            this.webBrowserOutput = new CefSharp.WinForms.ChromiumWebBrowser("about:blank")
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
                BackColor = Color.FromArgb(30, 30, 30), // Match dark mode
            };
            this.inputGroupBox = new System.Windows.Forms.GroupBox();
            this.inputTextBox = new System.Windows.Forms.RichTextBox();
            this.zoomTrackBar = new System.Windows.Forms.TrackBar();
            this.zoomLabel = new System.Windows.Forms.Label();
            this.connectionGroupBox.SuspendLayout();
            this.chatGroupBox.SuspendLayout();
            this.inputGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.zoomTrackBar)).BeginInit();
            this.SuspendLayout();
            // 
            // hostNameBox
            // 
            this.hostNameBox.Location = new System.Drawing.Point(12, 39);
            this.hostNameBox.MaxLength = 60;
            this.hostNameBox.Name = "hostNameBox";
            this.hostNameBox.Size = new System.Drawing.Size(100, 20);
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
            // connectButton
            // 
            this.connectButton.Location = new System.Drawing.Point(118, 36);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(75, 23);
            this.connectButton.TabIndex = 2;
            this.connectButton.Text = "Connect";
            this.connectButton.UseVisualStyleBackColor = true;
            this.connectButton.Click += new System.EventHandler(this.connectButton_Click);
            // 
            // connectionGroupBox
            // 
            this.connectionGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.connectionGroupBox.Controls.Add(this.zoomLabel);
            this.connectionGroupBox.Controls.Add(this.zoomTrackBar);
            this.connectionGroupBox.Controls.Add(this.connectButton);
            this.connectionGroupBox.Controls.Add(this.hostNameBox);
            this.connectionGroupBox.Controls.Add(this.hostLabel);
            this.connectionGroupBox.Location = new System.Drawing.Point(12, 12);
            this.connectionGroupBox.Name = "connectionGroupBox";
            this.connectionGroupBox.Size = new System.Drawing.Size(865, 79);
            this.connectionGroupBox.TabIndex = 3;
            this.connectionGroupBox.TabStop = false;
            this.connectionGroupBox.Text = "Connection";
            // 
            // chatGroupBox
            // 
            this.chatGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chatGroupBox.Controls.Add(this.webBrowserOutput);
            this.chatGroupBox.Location = new System.Drawing.Point(-1, 98);
            this.chatGroupBox.Name = "chatGroupBox";
            this.chatGroupBox.Size = new System.Drawing.Size(893, 370);
            this.chatGroupBox.TabIndex = 4;
            this.chatGroupBox.TabStop = false;
            this.chatGroupBox.Text = "Chat";
            // 
            // webBrowserOutput
            // 
            this.webBrowserOutput.ActivateBrowserOnCreation = false;
            this.webBrowserOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowserOutput.Location = new System.Drawing.Point(3, 16);
            this.webBrowserOutput.Name = "webBrowserOutput";
            this.webBrowserOutput.Size = new System.Drawing.Size(887, 351);
            this.webBrowserOutput.TabIndex = 0;
            // 
            // inputGroupBox
            // 
            this.inputGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.inputGroupBox.Controls.Add(this.inputTextBox);
            this.inputGroupBox.Location = new System.Drawing.Point(-1, 474);
            this.inputGroupBox.Name = "inputGroupBox";
            this.inputGroupBox.Size = new System.Drawing.Size(893, 93);
            this.inputGroupBox.TabIndex = 5;
            this.inputGroupBox.TabStop = false;
            // 
            // inputTextBox
            // 
            this.inputTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inputTextBox.Location = new System.Drawing.Point(3, 16);
            this.inputTextBox.Name = "inputTextBox";
            this.inputTextBox.Size = new System.Drawing.Size(887, 74);
            this.inputTextBox.TabIndex = 0;
            this.inputTextBox.Text = "";
            // 
            // zoomTrackBar
            // 
            this.zoomTrackBar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.zoomTrackBar.Location = new System.Drawing.Point(612, 23);
            this.zoomTrackBar.Minimum = -10;
            this.zoomTrackBar.Name = "zoomTrackBar";
            this.zoomTrackBar.Size = new System.Drawing.Size(247, 45);
            this.zoomTrackBar.TabIndex = 6;
            this.zoomTrackBar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.zoomTrackBar.Scroll += new System.EventHandler(this.zoomTrackBar_Scroll);
            // 
            // zoomLabel
            // 
            this.zoomLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.zoomLabel.AutoSize = true;
            this.zoomLabel.Location = new System.Drawing.Point(686, 7);
            this.zoomLabel.Name = "zoomLabel";
            this.zoomLabel.Size = new System.Drawing.Size(101, 13);
            this.zoomLabel.TabIndex = 7;
            this.zoomLabel.Text = "Chat Window Zoom";

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
            this.connectionGroupBox.ResumeLayout(false);
            this.connectionGroupBox.PerformLayout();
            this.chatGroupBox.ResumeLayout(false);
            this.inputGroupBox.ResumeLayout(false);
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.zoomTrackBar)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox hostNameBox;
        private System.Windows.Forms.Label hostLabel;
        private System.Windows.Forms.Button connectButton;
        private System.Windows.Forms.GroupBox connectionGroupBox;
        private System.Windows.Forms.GroupBox chatGroupBox;
        private CefSharp.WinForms.ChromiumWebBrowser webBrowserOutput;
        private System.Windows.Forms.GroupBox inputGroupBox;
        private System.Windows.Forms.RichTextBox inputTextBox;
        private System.Windows.Forms.TrackBar zoomTrackBar; // Zoom control
        private System.Windows.Forms.Label zoomLabel;
    }
}

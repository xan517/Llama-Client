using System;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms;

namespace WindowsFormsApp1
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Initialize CefSharp before any ChromiumWebBrowser instances are created
            var settings = new CefSettings
            {
                // Optional: Configure CefSharp settings here
                WindowlessRenderingEnabled = true,
                BackgroundColor = Cef.ColorSetARGB(255, 18, 18, 18) // Dark gray background
            };

            // Initialize CefSharp with the specified settings
            Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());

            // Shutdown CefSharp when the application exits
            Cef.Shutdown();
        }
    }
}

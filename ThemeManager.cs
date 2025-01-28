// ThemeManager.cs
using System.Drawing;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public class ThemeManager
    {
        private readonly Color backColor = Color.FromArgb(30, 30, 30);
        private readonly Color foreColor = Color.White;
        private readonly Color textBoxBackColor = Color.FromArgb(45, 45, 48);
        private readonly Color buttonBackColor = Color.FromArgb(50, 50, 50);

        /// <summary>
        /// Applies a dark theme to all controls recursively.
        /// </summary>
        public void ApplyDarkTheme(Control control)
        {
            if (control is GroupBox)
            {
                control.BackColor = backColor;
                control.ForeColor = foreColor;
            }
            else if (control is TextBox || control is RichTextBox)
            {
                control.BackColor = textBoxBackColor;
                control.ForeColor = foreColor;
            }
            else if (control is Button button)
            {
                button.BackColor = buttonBackColor;
                button.ForeColor = foreColor;
            }
            else
            {
                control.BackColor = backColor;
                control.ForeColor = foreColor;
            }

            foreach (Control childControl in control.Controls)
            {
                ApplyDarkTheme(childControl);
            }
        }
    }
}

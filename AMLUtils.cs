using Newtonsoft.Json;
using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace AstroModLoader
{
    public static class AMLUtils
    {
        public static bool IsLinux
        {
            get
            {
                int p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }

        public static string UserAgent
        {
            get
            {
                return "AstroModLoader/" + Application.ProductVersion;
            }
        }

        public static string SerializeObject<T>(T value)
        {
            StringBuilder sb = new StringBuilder(256);
            StringWriter sw = new StringWriter(sb, CultureInfo.InvariantCulture);

            var jsonSerializer = JsonSerializer.CreateDefault();
            using (JsonTextWriter jsonWriter = new JsonTextWriter(sw))
            {
                //jsonWriter.Formatting = Formatting.None;
                jsonWriter.Formatting = Formatting.Indented; jsonWriter.IndentChar = ' '; jsonWriter.Indentation = 4;
                jsonSerializer.Serialize(jsonWriter, value, typeof(T));
            }

            return sw.ToString();
        }

        public static Color ColorFromHTML(string htmlColor)
        {
            string kosherColor = htmlColor;
            if (!kosherColor[0].Equals('#')) kosherColor = "#" + kosherColor;
            return ColorTranslator.FromHtml(kosherColor);
        }

        public static string ColorToHTML(Color color)
        {
            return "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
        }

        public static int ShowBasicButton(this Form frm, string labelText, string button1text, string button2text, string button3text)
        {
            BasicButtonPopup basicButtonPrompt = new BasicButtonPopup();
            basicButtonPrompt.StartPosition = FormStartPosition.Manual;
            basicButtonPrompt.Location = new Point((frm.Location.X + frm.Width / 2) - (basicButtonPrompt.Width / 2), (frm.Location.Y + frm.Height / 2) - (basicButtonPrompt.Height / 2));
            basicButtonPrompt.Text = frm.Text;

            basicButtonPrompt.DisplayText = labelText;
            basicButtonPrompt.button1.Text = button1text;
            if (string.IsNullOrEmpty(button1text)) basicButtonPrompt.button1.Hide();
            basicButtonPrompt.button2.Text = button2text;
            if (string.IsNullOrEmpty(button2text)) basicButtonPrompt.button2.Hide();
            basicButtonPrompt.button3.Text = button3text;
            if (string.IsNullOrEmpty(button3text)) basicButtonPrompt.button3.Hide();

            basicButtonPrompt.ShowDialog();
            return basicButtonPrompt.ResultButton;
        }

        public static void RefreshAllButtonsInControl(this Control ctrl)
        {
            foreach(Control ctrl2 in ctrl.Controls)
            {
                if (ctrl2 is CoolButton butto) butto.RefreshToDefaults();
                if (ctrl2 is ComboBox comboo)
                {
                    comboo.FlatStyle = FlatStyle.Flat;
                    comboo.BackColor = AMLPalette.DropDownBackgroundColor;
                    comboo.ForeColor = AMLPalette.ForeColor;
                    comboo.Font = new Font("Microsoft Sans Serif", 9.75F, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
                }
                if (ctrl2 is ListBox listBox)
                {
                    listBox.BackColor = AMLPalette.BackColor;
                    listBox.ForeColor = AMLPalette.ForeColor;
                }
                RefreshAllButtonsInControl(ctrl2);
            }
        }

        public static string GeneratePriorityFromPositionInList(int pos)
        {
            return pos.ToString().PadLeft(3, '0');
        }

        public static string SanitizeFilename(string name)
        {
            string invalidCharactersRegex = string.Format(@"([{0}]*\.+$)|([{0}]+)", Regex.Escape(new string(Path.GetInvalidFileNameChars())));
            return Regex.Replace(name, invalidCharactersRegex, "_");
        }

        public static bool IsValidUri(string uri)
        {
            if (!Uri.IsWellFormedUriString(uri, UriKind.Absolute)) return false;
            if (!Uri.TryCreate(uri, UriKind.Absolute, out Uri tmp)) return false;
            return tmp.Scheme == Uri.UriSchemeHttp || tmp.Scheme == Uri.UriSchemeHttps;
        }

        public static bool AcceptablySimilar(this Version v1, Version v2)
        {
            return v1.Major == v2.Major && v1.Minor == v2.Minor; // no sense in warning if the current version is 1.16.70.0 and the mod is for 1.16.60.0, who cares
        }
    }
}

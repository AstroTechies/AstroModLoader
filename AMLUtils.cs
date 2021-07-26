using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace AstroModLoader
{
    public static class AMLUtils
    {
        public const int WM_PAINT = 0x000F;
        public const int WM_HSCROLL = 0x0114;
        public const int WM_VSCROLL = 0x0115;

        private static bool _checkForLinux = true;
        private static bool _isLinux = false;

        public static bool IsLinux
        {
            get
            {
                if (_checkForLinux)
                {
                    _checkForLinux = false;
                    int p = (int)Environment.OSVersion.Platform;
                    if ((p == 4) || (p == 6) || (p == 128))
                    {
                        _isLinux = true;
                    }
                    else
                    {
                        _isLinux = Registry.CurrentUser.OpenSubKey(@"Software\Wine") != null;
                    }
                }
                return _isLinux;
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
            if (!htmlColor[0].Equals('#')) htmlColor = "#" + htmlColor;
            return ColorTranslator.FromHtml(htmlColor);
        }

        public static string ColorToHTML(Color color)
        {
            return "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
        }

        public static void AdjustFormPosition(this Form frm1)
        {
            if (frm1.Owner != null) frm1.Location = new Point((frm1.Owner.Location.X + frm1.Owner.Width / 2) - (frm1.Width / 2), (frm1.Owner.Location.Y + frm1.Owner.Height / 2) - (frm1.Height / 2));
        }

        public static BasicButtonPopup GetBasicButton(this Form frm, string labelText, string button1text, string button2text, string button3text)
        {
            BasicButtonPopup basicButtonPrompt = new BasicButtonPopup();
            basicButtonPrompt.Owner = frm;
            basicButtonPrompt.Text = frm.Text;

            // We later adjust this in the button itself to correct error, but good to have this here as a backup
            basicButtonPrompt.StartPosition = FormStartPosition.Manual;
            basicButtonPrompt.Location = new Point((frm.Location.X + frm.Width / 2) - (basicButtonPrompt.Width / 2), (frm.Location.Y + frm.Height / 2) - (basicButtonPrompt.Height / 2));

            basicButtonPrompt.DisplayText = labelText;
            basicButtonPrompt.button1.Text = button1text;
            if (string.IsNullOrEmpty(button1text)) basicButtonPrompt.button1.Hide();
            basicButtonPrompt.button2.Text = button2text;
            if (string.IsNullOrEmpty(button2text)) basicButtonPrompt.button2.Hide();
            basicButtonPrompt.button3.Text = button3text;
            if (string.IsNullOrEmpty(button3text)) basicButtonPrompt.button3.Hide();

            return basicButtonPrompt;
        }

        public static int ShowBasicButton(this Form frm, string labelText, string button1text, string button2text, string button3text)
        {
            BasicButtonPopup basicButtonPrompt = GetBasicButton(frm, labelText, button1text, button2text, button3text);
            basicButtonPrompt.ShowDialog();
            return basicButtonPrompt.ResultButton;
        }

        public static void SetHeightOfAllButtonsInControl(this Control ctrl, int height)
        {
            foreach (Control ctrl2 in ctrl.Controls)
            {
                if (ctrl2 is CoolButton butto) butto.Height = height;
                SetHeightOfAllButtonsInControl(ctrl2, height);
            }
        }

        public static void RefreshAllButtonsInControl(this Control ctrl)
        {
            foreach (Control ctrl2 in ctrl.Controls)
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
        
        public static string FixBasePath(string basePath)
        {
            string[] allDirs = basePath.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });
            if (Path.GetFileName(basePath) != "Astro") basePath = Path.Combine(basePath, "Astro");

            for (int i = 0; i < allDirs.Length; i++)
            {
                if (allDirs[i] == "Astro")
                {
                    basePath = string.Join(Path.DirectorySeparatorChar.ToString(), allDirs.Subsequence(0, i + 1));
                    break;
                }
            }

            basePath = Path.GetFullPath(basePath);
            if (!Directory.Exists(basePath)) return null;
            if (!Directory.Exists(Path.Combine(basePath, "Saved"))) return null;
            return basePath;
        }

        public static string FixGamePath(string gamePath)
        {
            if (Path.GetFileName(gamePath) == "Astro") gamePath = Path.Combine(gamePath, "..");

            gamePath = Path.GetFullPath(gamePath);
            if (!Directory.Exists(gamePath)) return null;
            if (!Directory.Exists(Path.Combine(gamePath, "Astro", "Content"))) return null;
            return gamePath;
        }

        public static bool IsValidPath(string path)
        {
            bool isValidPath = true;
            if (path != null)
            {
                try
                {
                    if (!Directory.Exists(path) && !File.Exists(path)) isValidPath = false;
                }
                catch
                {
                    isValidPath = false;
                }
            }
            return isValidPath;
        }

        private static readonly string[] SizeSuffixes = { "bytes", "KiB", "MiB", "GiB", "TiB", "PiB", "EiB", "ZiB", "YiB" };
        private static readonly int FileSizeDecimalPlaces = 1;
        public static string FormatFileSize(long size)
        {
            int suffixOffset = 0;
            decimal determinedVal = size;

            while (Math.Round(determinedVal, FileSizeDecimalPlaces) >= 1024)
            {
                determinedVal /= 1024;
                suffixOffset++;
            }

            if (suffixOffset >= SizeSuffixes.Length) return "Very big";
            return string.Format("{0:n" + (suffixOffset == 0 ? 0 : FileSizeDecimalPlaces) + "} {1}", determinedVal, SizeSuffixes[suffixOffset]);
        }

        public static bool IsValidUri(string uri)
        {
            if (!Uri.IsWellFormedUriString(uri, UriKind.Absolute)) return false;
            if (!Uri.TryCreate(uri, UriKind.Absolute, out Uri tmp)) return false;
            return tmp.Scheme == Uri.UriSchemeHttp || tmp.Scheme == Uri.UriSchemeHttps;
        }

        /*
            AstroModLoader versions are formatted as follows: MAJOR.MINOR.BUILD.REVISION
            * MAJOR - incremented for very big changes or backwards-incompatible changes
            * MINOR - incremented for notable changes
            * BUILD - incremented for bug fixes or very small improvements
            * REVISION - incremented for test builds of the existing version
            
            2.0.0.0 > 1.5.0.0 > 1.4.1.0 > 1.4.0.1 > 1.4.0.0
        */
        public static bool IsAMLVersionLower(this Version v1)
        {
            Version fullAmlVersion = Assembly.GetExecutingAssembly().GetName().Version;
            return v1.CompareTo(fullAmlVersion) > 0;
        }

        public static bool AcceptablySimilar(this Version v1, Version v2)
        {
            if (v1 == null || v2 == null) return true;
            return v1.Major == v2.Major && v1.Minor == v2.Minor; // no sense in warning if the current version is 1.16.70.0 and the mod is for 1.16.60.0, who cares
        }

        public static T[] Subsequence<T>(this IEnumerable<T> arr, int startIndex, int length)
        {
            return arr.Skip(startIndex).Take(length).ToArray();
        }

        private static Control internalForm;
        public static void InitializeInvoke(Control control)
        {
            internalForm = control;
        }

        public static void InvokeUI(Action act)
        {
            if (internalForm.InvokeRequired)
            {
                internalForm.Invoke(act);
            }
            else
            {
                act();
            }
        }
    }
}

using Newtonsoft.Json;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace AstroModLoader
{
    public static class AMLUtils
    {
        public static string SerializeObject<T>(T value)
        {
            StringBuilder sb = new StringBuilder(256);
            StringWriter sw = new StringWriter(sb, CultureInfo.InvariantCulture);

            var jsonSerializer = JsonSerializer.CreateDefault();
            using (JsonTextWriter jsonWriter = new JsonTextWriter(sw))
            {
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
            return pos.ToString().PadLeft(3).Replace(' ', '0');
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AstroModLoader
{
    public enum ModLoaderTheme
    {
        [EnumMember(Value = "dark")]
        Dark,
        [EnumMember(Value = "light")]
        Light
    }

    public static class AMLPalette
    {
        public static Color BackColor = Color.FromArgb(40, 42, 45);
        public static Color ButtonBackColor = Color.FromArgb(51, 51, 51);
        public static Color ForeColor = Color.FromArgb(225, 225, 225);
        public static Color FooterColor = Color.FromArgb(36, 38, 40);
        public static Color DropDownBackgroundColor = Color.FromArgb(55, 55, 55);
        public static Color HighlightColor = Color.FromArgb(85, 85, 85);
        public static Color AccentColor = Color.FromArgb(255, 231, 149);
        public static ModLoaderTheme CurrentTheme = ModLoaderTheme.Dark;

        public static readonly Dictionary<string, Color> PresetMap = new Dictionary<string, Color>
        {
            { "Sleek Yellow", Color.FromArgb(255, 231, 149) },
            { "Fun Green", Color.FromArgb(171, 238, 151) },
            { "Soft Pink", Color.FromArgb(245, 140, 175) },
            { "Astro Blue", Color.FromArgb(18, 154, 240) },
            { "Alert Red", Color.FromArgb(227, 55, 24) },
            { "Safety Orange", Color.FromArgb(232, 119, 34) },
            { "Titanium Gray", Color.FromArgb(148, 148, 148) },
            { "Polar White", Color.FromArgb(255, 255, 255) },
            { "Jet Black", Color.FromArgb(0, 0, 0) },
        };

        public static void RefreshTheme(Form frm)
        {
            switch (CurrentTheme)
            {
                case ModLoaderTheme.Dark:
                    BackColor = Color.FromArgb(40, 42, 45);
                    ButtonBackColor = Color.FromArgb(51, 51, 51);
                    ForeColor = Color.FromArgb(225, 225, 225);
                    FooterColor = Color.FromArgb(36, 38, 40);
                    DropDownBackgroundColor = Color.FromArgb(55, 55, 55);
                    HighlightColor = Color.FromArgb(85, 85, 85);
                    break;
                case ModLoaderTheme.Light:
                    BackColor = Color.FromArgb(255, 255, 255);
                    ButtonBackColor = Color.FromArgb(235, 235, 235);
                    ForeColor = Color.FromArgb(15, 15, 15);
                    FooterColor = Color.Gainsboro;
                    DropDownBackgroundColor = Color.FromArgb(200, 200, 200);
                    HighlightColor = Color.FromArgb(190, 190, 190);
                    break;
            }

            frm.BackColor = AMLPalette.BackColor;
            frm.ForeColor = AMLPalette.ForeColor;
            if (frm is Form1 frm1)
            {
                frm1.dataGridView1.BackgroundColor = AMLPalette.BackColor;
                frm1.dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = AMLPalette.BackColor;
                frm1.dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = AMLPalette.ForeColor;
                frm1.dataGridView1.ColumnHeadersDefaultCellStyle.SelectionBackColor = AMLPalette.HighlightColor;
                frm1.dataGridView1.ColumnHeadersDefaultCellStyle.SelectionForeColor = AMLPalette.ForeColor;
                frm1.dataGridView1.DefaultCellStyle = frm1.dataGridView1.ColumnHeadersDefaultCellStyle;
                if (frm1.TableManager != null) frm1.TableManager.Refresh();
                frm1.footer.BackColor = AMLPalette.FooterColor;
                frm1.dataGridView1.GridColor = AMLPalette.AccentColor;
                frm1.panel1.BackColor = AMLPalette.AccentColor;
            }
            frm.RefreshAllButtonsInControl();
        }
    }
}

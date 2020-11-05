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
        public static Color FooterLineColor = Color.FromArgb(60, 60, 60);
        public static Color DropDownBackgroundColor = Color.FromArgb(55, 55, 55);
        public static Color HighlightColor = Color.FromArgb(85, 85, 85);
        public static Color AccentColor = Color.FromArgb(255, 231, 149);
        public static Color LinkColor = Color.FromArgb(18, 154, 240);
        public static Color WarningColor = Color.FromArgb(232, 119, 34);
        public static ModLoaderTheme CurrentTheme = ModLoaderTheme.Dark;

        public static readonly Dictionary<string, Color> PresetMap = new Dictionary<string, Color>
        {
            { "Sleek Yellow", Color.FromArgb(255, 231, 149) },
            { "Fun Green", Color.FromArgb(171, 238, 151) },
            { "Soft Pink", Color.FromArgb(245, 140, 175) },
            { "Astro Blue", Color.FromArgb(18, 154, 240) },
            { "Alert Red", Color.FromArgb(227, 55, 24) },
            { "Safety Orange", Color.FromArgb(232, 119, 34) },
            { "Asphalt Gray", Color.FromArgb(96, 96, 96) },
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
                    FooterLineColor = Color.FromArgb(60, 60, 60);
                    DropDownBackgroundColor = Color.FromArgb(55, 55, 55);
                    HighlightColor = Color.FromArgb(85, 85, 85);
                    break;
                case ModLoaderTheme.Light:
                    BackColor = Color.FromArgb(255, 255, 255);
                    ButtonBackColor = Color.FromArgb(240, 240, 240);
                    ForeColor = Color.FromArgb(25, 25, 25);
                    FooterColor = Color.FromArgb(245, 245, 245);
                    FooterLineColor = Color.FromArgb(230, 230, 230);
                    DropDownBackgroundColor = Color.FromArgb(220, 220, 220);
                    HighlightColor = Color.FromArgb(210, 210, 210);
                    break;
            }

            frm.Icon = Properties.Resources.icon;
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

                frm1.modInfo.LinkColor = AMLPalette.LinkColor;
                frm1.modInfo.ActiveLinkColor = AMLPalette.LinkColor;
                frm1.footer.BackColor = AMLPalette.FooterColor;
                frm1.dataGridView1.GridColor = AMLPalette.AccentColor;
                frm1.panel1.BackColor = AMLPalette.AccentColor;
            }
            frm.RefreshAllButtonsInControl();
        }
    }
}

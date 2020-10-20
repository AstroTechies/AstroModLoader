using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AstroModLoader
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
        }

        private void setPathButton_Click(object sender, EventArgs e)
        {
            if (this.Owner is Form1 parentForm)
            {
                parentForm.ModManager.GamePath = gamePathBox.Text;
                parentForm.ModManager.DetermineBinaryFilePath();
                parentForm.ModManager.SyncConfigToDisk();
            }
        }

        private void UpdateColorBoxText()
        {
            bool foundMatch = false;
            foreach (KeyValuePair<string, Color> entry in AMLPalette.PresetMap)
            {
                if (entry.Value.Equals(AMLPalette.AccentColor))
                {
                    accentComboBox.Text = entry.Key;
                    foundMatch = true;
                    break;
                }
            }

            if (!foundMatch)
            {
                accentComboBox.Text = AMLUtils.ColorToHTML(AMLPalette.AccentColor);
            }
        }

        private bool _readyToUpdateTheme = false;
        private void SettingsForm_Load(object sender, EventArgs e)
        {
            if (this.Owner is Form1 parentForm)
            {
                gamePathBox.Text = parentForm.ModManager.GamePath;
                AMLPalette.RefreshTheme(parentForm);
                UpdateColorBoxText();
            }
            themeBox.DataSource = Enum.GetValues(typeof(ModLoaderTheme));
            themeBox.SelectedIndex = (int)AMLPalette.CurrentTheme;
            this.accentComboBox.Items.AddRange(AMLPalette.PresetMap.Keys.ToArray());
            AMLPalette.RefreshTheme(this);
            _readyToUpdateTheme = true;
        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void accentComboBox_UpdateColor(object sender, EventArgs e)
        {
            Color backupColor = Color.FromArgb(AMLPalette.AccentColor.A, AMLPalette.AccentColor.R, AMLPalette.AccentColor.G, AMLPalette.AccentColor.B);
            try
            {
                if (AMLPalette.PresetMap.ContainsKey(accentComboBox.Text))
                {
                    AMLPalette.AccentColor = AMLPalette.PresetMap[accentComboBox.Text];
                }
                else
                {
                    AMLPalette.AccentColor = AMLUtils.ColorFromHTML(accentComboBox.Text);
                }

                if (this.Owner is Form1 parentForm)
                {
                    AMLPalette.RefreshTheme(parentForm);
                    parentForm.ModManager.SyncConfigToDisk();
                }
                AMLPalette.RefreshTheme(this);
                UpdateColorBoxText();
            }
            catch
            {
                MessageBox.Show("Invalid color!", "Uh oh!");
                AMLPalette.AccentColor = backupColor;
                UpdateColorBoxText();
            }
        }

        private void themeBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_readyToUpdateTheme) return;
            Enum.TryParse(themeBox.SelectedValue.ToString(), out ModLoaderTheme nextTheme);
            AMLPalette.CurrentTheme = nextTheme;
            if (this.Owner is Form1 parentForm)
            {
                AMLPalette.RefreshTheme(parentForm);
                parentForm.ModManager.SyncConfigToDisk();
            }
            AMLPalette.RefreshTheme(this);
        }
    }
}

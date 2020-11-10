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

        public void RefreshPlatformComboBox()
        {
            if (Program.CommandLineOptions.ServerMode)
            {
                platformComboBox.Enabled = false;
                platformComboBox.DataSource = new string[] { "Server" };
                platformComboBox.SelectedIndex = 0;
            }
            else
            {
                platformComboBox.Enabled = true;
                platformComboBox.DataSource = BaseForm.ModManager.AllPlatforms;
                platformComboBox.SelectedIndex = platformComboBox.FindStringExact(BaseForm.ModManager.Platform.ToString());
            }
        }

        private Form1 BaseForm;
        private bool _readyToUpdateTheme = false;
        private void SettingsForm_Load(object sender, EventArgs e)
        {
            if (this.Owner is Form1)
            {
                BaseForm = (Form1)this.Owner;
                gamePathBox.Text = BaseForm.ModManager.GamePath;
                localPathBox.Text = BaseForm.ModManager.BasePath;
                AMLPalette.RefreshTheme(BaseForm);
                RefreshPlatformComboBox();
                UpdateColorBoxText();
            }
            themeComboBox.DataSource = Enum.GetValues(typeof(ModLoaderTheme));
            themeComboBox.SelectedIndex = (int)AMLPalette.CurrentTheme;
            accentComboBox.Items.AddRange(AMLPalette.PresetMap.Keys.ToArray());
            AMLPalette.RefreshTheme(this);

            this.AdjustFormPosition();

            gamePathBox.SelectionStart = 0;
            accentComboBox.SelectionLength = 0;
            _readyToUpdateTheme = true;
        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void accentComboBox_UpdateColor(object sender, EventArgs e)
        {
            Color backupColor = Color.FromArgb(AMLPalette.AccentColor.ToArgb());
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
                this.ShowBasicButton("Invalid color!", "OK", null, null);
                AMLPalette.AccentColor = backupColor;
                UpdateColorBoxText();
            }
        }

        private void themeBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_readyToUpdateTheme) return;
            Enum.TryParse(themeComboBox.SelectedValue.ToString(), out ModLoaderTheme nextTheme);
            AMLPalette.CurrentTheme = nextTheme;
            if (this.Owner is Form1 parentForm)
            {
                AMLPalette.RefreshTheme(parentForm);
                parentForm.ModManager.SyncConfigToDisk();
            }
            AMLPalette.RefreshTheme(this);
        }

        private void platformComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_readyToUpdateTheme) return;
            if (BaseForm == null) return;

            Enum.TryParse(platformComboBox.SelectedValue.ToString(), out PlatformType nextPlatform);
            BaseForm.SwitchPlatform(nextPlatform);
            gamePathBox.Text = BaseForm.ModManager.GamePath;
            localPathBox.Text = BaseForm.ModManager.BasePath;
        }

        private void setPathButton_Click(object sender, EventArgs e)
        {
            if (!AMLUtils.IsValidPath(gamePathBox.Text))
            {
                gamePathBox.Text = BaseForm.ModManager.GamePath;
                this.ShowBasicButton("This is not a valid path!", "OK", null, null);
                return;
            }

            if (this.Owner is Form1 parentForm)
            {
                parentForm.ModManager.GamePath = gamePathBox.Text;
                parentForm.ModManager.ApplyGamePathDerivatives();
                parentForm.ModManager.SyncConfigToDisk();
                parentForm.FullRefresh();
            }
        }

        private void setPathButton2_Click(object sender, EventArgs e)
        {
            if (!AMLUtils.IsValidPath(localPathBox.Text))
            {
                localPathBox.Text = BaseForm.ModManager.BasePath;
                this.ShowBasicButton("This is not a valid path!", "OK", null, null);
                return;
            }

            if (this.Owner is Form1 parentForm)
            {
                parentForm.ModManager.CustomBasePath = localPathBox.Text;
                parentForm.ModManager.BasePath = localPathBox.Text;
                parentForm.ModManager.DetermineBasePathDerivatives();

                parentForm.ModManager.ValidPlatformTypesToPaths[PlatformType.Custom] = parentForm.ModManager.GamePath;
                parentForm.ModManager.Platform = PlatformType.Custom;
                parentForm.ModManager.RefreshAllPlatformsList();
                RefreshPlatformComboBox();

                parentForm.ModManager.SyncConfigToDisk();
                parentForm.FullRefresh();
            }
        }
    }
}

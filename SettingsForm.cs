using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace AstroModLoader
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
            versionLabel.Text = "";
        }

        private void UpdateColorBoxText()
        {
            foreach (KeyValuePair<string, Color> entry in AMLPalette.PresetMap)
            {
                if (entry.Value.Equals(AMLPalette.AccentColor))
                {
                    accentComboBox.Text = entry.Key;
                    return;
                }
            }

            accentComboBox.Text = AMLUtils.ColorToHTML(AMLPalette.AccentColor);
        }

        private void UpdateLabels()
        {
            gamePathBox.Text = BaseForm.ModManager.GamePath;
            localPathBox.Text = BaseForm.ModManager.BasePath;
            versionLabel.Text = BaseForm.ModManager.InstalledAstroBuild?.ToString() ?? "Unknown";
            versionLabel.ForeColor = BaseForm.ModManager.MismatchedSteamworksDLL ? AMLPalette.LinkColor : AMLPalette.ForeColor;

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

            refuseMismatchedConnectionsCheckbox.Checked = ModHandler.OurIntegrator.RefuseMismatchedConnections;
        }

        private Form1 BaseForm;
        private bool _readyToUpdateTheme = false;
        private void SettingsForm_Load(object sender, EventArgs e)
        {
            if (this.Owner is Form1)
            {
                BaseForm = (Form1)this.Owner;
                AMLPalette.RefreshTheme(BaseForm);
                this.UpdateLabels();
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

                AMLPalette.RefreshTheme(BaseForm);
                BaseForm.ModManager.SyncConfigToDisk();
                AMLPalette.RefreshTheme(this);
            }
            catch
            {
                this.ShowBasicButton("Invalid color!", "OK", null, null);
                AMLPalette.AccentColor = backupColor;
            }
            UpdateColorBoxText();
        }

        private void themeBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_readyToUpdateTheme) return;
            Enum.TryParse(themeComboBox.SelectedValue.ToString(), out ModLoaderTheme nextTheme);
            AMLPalette.CurrentTheme = nextTheme;
            AMLPalette.RefreshTheme(BaseForm);
            BaseForm.ModManager.SyncConfigToDisk();
            AMLPalette.RefreshTheme(this);
        }

        private void platformComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_readyToUpdateTheme) return;
            if (BaseForm == null) return;

            Enum.TryParse(platformComboBox.SelectedValue.ToString(), out PlatformType nextPlatform);
            BaseForm.SwitchPlatform(nextPlatform);

            this.UpdateLabels();
        }

        private void UpdatePathing(object sender, EventArgs e)
        {
            string correctedGamePath = AMLUtils.FixGamePath(gamePathBox.Text);
            string correctedLocalPath = AMLUtils.FixBasePath(localPathBox.Text);

            if (string.IsNullOrEmpty(correctedGamePath) || !AMLUtils.IsValidPath(correctedGamePath))
            {
                gamePathBox.Text = BaseForm.ModManager.GamePath;
                this.ShowBasicButton("The specified game path is invalid!", "OK", null, null);
                return;
            }

            if (string.IsNullOrEmpty(correctedLocalPath) || !AMLUtils.IsValidPath(correctedLocalPath))
            {
                localPathBox.Text = BaseForm.ModManager.BasePath;
                this.ShowBasicButton("The specified local path is invalid!", "OK", null, null);
                return;
            }

            BaseForm.ModManager.ValidPlatformTypesToPaths[PlatformType.Custom] = correctedGamePath;
            BaseForm.ModManager.CustomBasePath = correctedLocalPath;
            BaseForm.SwitchPlatform(PlatformType.Custom);

            this.UpdateLabels();
        }

        private void refuseMismatchedConnectionsCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            ModHandler.OurIntegrator.RefuseMismatchedConnections = refuseMismatchedConnectionsCheckbox.Checked;
            BaseForm.ModManager.SyncDependentConfigToDisk();
            AMLUtils.InvokeUI(BaseForm.TableManager.Refresh);

            this.UpdateLabels();
        }
    }
}

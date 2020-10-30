using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AstroModLoader
{
    public enum ModLoaderView
    {
        None,
        Mods
    }

    internal enum ColumnType
    {
        Text,
        CheckBox,
        ComboBox
    }

    public class TableHandler
    {
        private DataGridView GridView;
        private ModHandler ModManager;
        private ModLoaderView CurrentView;

        public TableHandler(DataGridView gridView, ModHandler modManager)
        {
            GridView = gridView;
            ModManager = modManager;
            SwitchView(ModLoaderView.Mods);
        }

        private void AddColumns(List<Tuple<string, ColumnType>> ourColumns)
        {
            for (int i = 0; i < ourColumns.Count; i++)
            {
                Tuple<string, ColumnType> columnData = ourColumns[i];

                DataGridViewColumn dgc;
                if (columnData.Item2 == ColumnType.CheckBox)
                {
                    dgc = new DataGridViewCheckBoxColumn
                    {
                        HeaderText = columnData.Item1
                    };
                    dgc.ReadOnly = false;
                }
                else if (columnData.Item2 == ColumnType.ComboBox)
                {
                    dgc = new DataGridViewComboBoxColumn
                    {
                        HeaderText = columnData.Item1
                    };
                    dgc.ReadOnly = false;
                }
                else
                {
                    dgc = new DataGridViewTextBoxColumn
                    {
                        HeaderText = columnData.Item1
                    };
                    dgc.ReadOnly = true;
                }

                dgc.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                if (i >= (ourColumns.Count - 1))
                {
                    dgc.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                }

                dgc.SortMode = DataGridViewColumnSortMode.NotSortable;
                GridView.Columns.Add(dgc);
            }
        }

        public Mod GetCurrentlySelectedMod()
        {
            if (GridView.SelectedRows.Count == 0) return null;
            return ModManager.Mods[GridView.SelectedRows[0].Index];
        }

        public void Refresh()
        {
            GridView.Visible = true;
            GridView.Columns.Clear();
            GridView.Rows.Clear();
            GridView.AllowUserToAddRows = false;
            GridView.ReadOnly = false;
            GridView.AutoGenerateColumns = false;

            switch (CurrentView)
            {
                case ModLoaderView.Mods:
                    if (ModManager.Mods.Count == 0)
                    {
                        ModManager.BaseForm.AdjustModInfoText("You have no mods installed! Drop a .pak file onto this window to install a mod.");
                        break;
                    }

                    AddColumns(new List<Tuple<string, ColumnType>>
                    {
                        Tuple.Create("", ColumnType.CheckBox),
                        Tuple.Create("Name", ColumnType.Text),
                        Tuple.Create("Version", ColumnType.ComboBox),
                        Tuple.Create("Author", ColumnType.Text),
                    });

                    List<DataGridViewRow> newRows = new List<DataGridViewRow>();
                    foreach (Mod mod in ModManager.Mods)
                    {
                        DataGridViewRow row = new DataGridViewRow();
                        row.Tag = mod;
                        row.CreateCells(GridView);
                        row.Cells[0].Value = mod.Enabled;
                        row.Cells[1].Value = mod.Name;

                        if (row.Cells[0] is DataGridViewCheckBoxCell checkCell)
                        {
                            if (ModManager.IsReadOnly)
                            {
                                checkCell.ReadOnly = true;
                                checkCell.ThreeState = true;
                                checkCell.Value = 2;
                            }
                            else
                            {
                                checkCell.ThreeState = false;
                            }
                        }

                        if (row.Cells[2] is DataGridViewComboBoxCell cbCell)
                        {
                            cbCell.DataSource = mod.AvailableVersions.Select(v => v.ToString()).ToList();
                            cbCell.Value = mod.InstalledVersion.ToString();
                            cbCell.FlatStyle = FlatStyle.Flat;
                            cbCell.Style.BackColor = AMLPalette.DropDownBackgroundColor;
                            cbCell.Style.Font = new Font("Microsoft Sans Serif", 9.75F, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
                            cbCell.ReadOnly = ModManager.IsReadOnly;
                        }

                        row.Cells[3].Value = mod.Author;
                        newRows.Add(row);
                    }
                    GridView.Rows.AddRange(newRows.ToArray());
                    GridView.ClearSelection();
                    break;
            }
        }

        public void SwitchView(ModLoaderView newView)
        {
            CurrentView = newView;
            Refresh();
        }
    }
}

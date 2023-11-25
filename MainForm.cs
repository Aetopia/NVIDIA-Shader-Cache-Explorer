using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Threading;
using System;

public class MainForm : Form
{
    private static string FormatBytes(long bytes)
    {
        string[] suffix = ["B", "KB", "MB", "GB", "TB"];
        int i;
        double result = bytes;
        for (i = 0; i < suffix.Length && bytes >= 1024; i++, bytes /= 1024)
        {
            result = bytes / 1024.0;
        }
        return string.Format("{0:0.##} {1}", result, suffix[i]);
    }

    public MainForm()
    {
        Text = "NVIDIA Shader Cache Explorer";
        Font = SystemFonts.MessageBoxFont;
        Dictionary<string, string> processes = [];
        Dictionary<string, long> sizes = [];
        TableLayoutPanel tableLayoutPanel = new() { Dock = DockStyle.Top, AutoSize = true };
        Panel panel = new() { AutoSize = true, Dock = DockStyle.Fill };
        MenuStrip menuStrip = new();
        ListView listView = new()
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            CheckBoxes = true,
            MultiSelect = false,
            HeaderStyle = ColumnHeaderStyle.None,
            BorderStyle = BorderStyle.None
        };
        ToolStripButton refresh = new() { Text = "⟳ Refresh" };
        ToolStripButton delete = new() { Text = "🗑️ Delete", Enabled = false };

        menuStrip.Items.AddRange(new ToolStripItem[] { refresh, delete });
        tableLayoutPanel.Controls.Add(menuStrip);
        panel.Controls.Add(listView);
        Controls.AddRange([panel, tableLayoutPanel]);

        listView.Columns.AddRange([new() { Text = "Process" }, new() { Text = "Size" }]);
        listView.ItemSelectionChanged += (sender, e) => { e.Item.Focused = false; e.Item.Selected = false; };
        listView.ItemChecked += (sender, e) => { delete.Enabled = listView.CheckedItems.Count != 0; };

        refresh.Click += (sender, e) =>
        {
            delete.Enabled = refresh.Enabled = listView.Enabled = false;
            listView.Items.Clear();
            new Thread(() =>
            {
                sizes = NVIDIAShaderCache.GetSizes(processes = NVIDIAShaderCache.GetProcesses());
                foreach (KeyValuePair<string, string> keyValuePair in processes)
                    listView.Items.Add(new ListViewItem(new string[] { keyValuePair.Value, FormatBytes(sizes[keyValuePair.Key]) }));
                refresh.Enabled = listView.Enabled = true;
            }).Start();
        };

        delete.Click += (sender, e) =>
        {
            List<string> paths = NVIDIAShaderCache.GetPaths();
            foreach (ListViewItem listViewItem in listView.CheckedItems)
                foreach (KeyValuePair<string, string> keyValuePair in processes)
                    if (listViewItem.Text == keyValuePair.Value)
                        foreach (string path in paths)
                            if (path.Contains(keyValuePair.Key))
                                try { File.Delete(path); }
                                catch { }
            refresh.PerformClick();
        };

        Resize += (sender, e) =>
        {
            MinimumSize = new(800, 600);
            int width = listView.ClientSize.Width / listView.Columns.Count;
            foreach (ColumnHeader column in listView.Columns)
                column.Width = width;
        };

        OnResize(null);
        refresh.PerformClick();
        CenterToScreen();
    }
}
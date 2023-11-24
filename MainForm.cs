using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System;
using System.IO;

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
        string path = "";
        Dictionary<string, string> processes = [];
        Dictionary<string, long> sizes = [];
        TableLayoutPanel tableLayoutPanel = new() { Dock = DockStyle.Top, AutoSize = true };
        Panel panel = new() { AutoSize = true, Dock = DockStyle.Fill };
        MenuStrip menuStrip = new();
        ListView listView = new() { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, CheckBoxes = true };
        ToolStripButton refresh = new() { Text = "⟳ Refresh" };
        ToolStripButton delete = new() { Text = "🗑️ Delete" };



        menuStrip.Items.AddRange(new ToolStripItem[] { refresh, delete });
        tableLayoutPanel.Controls.Add(menuStrip);
        panel.Controls.Add(listView);
        Controls.AddRange(new Control[] { panel, tableLayoutPanel });

        listView.Columns.AddRange(new ColumnHeader[] { new() { Text = "Process" }, new() { Text = "Size" } });
        listView.ItemSelectionChanged += (sender, e) => { e.Item.Focused = false; e.Item.Selected = false; };

        refresh.Click += (sender, e) =>
        {
            listView.Items.Clear();
            sizes = NVIDIAShaderCache.GetShaderCacheSizes(processes = NVIDIAShaderCache.GetProcesses(path = NVIDIAShaderCache.GetPath()), path);
            foreach (KeyValuePair<string, string> keyValuePair in processes)
                listView.Items.Add(new ListViewItem(new string[] { keyValuePair.Value, FormatBytes(sizes[keyValuePair.Key]) }));
        };

        delete.Click += (sender, e) =>
        {
            foreach (ListViewItem listViewItem in listView.Items)
                if (listViewItem.Checked)
                    foreach (KeyValuePair<string, string> keyValuePair in processes)
                        if (listViewItem.Text == keyValuePair.Value)
                            foreach (string file in Directory.GetFiles(path, $"{keyValuePair.Key}*"))
                                try { File.Delete(file); }
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
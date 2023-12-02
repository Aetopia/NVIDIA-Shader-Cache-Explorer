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
        Dictionary<string, List<string>> processes = null;
        Dictionary<string, long> sizes = null;
        List<string> paths = null;
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
        ToolStripButton select = new() { Text = "✅ Select" };
        ToolStripButton delete = new() { Text = "🗑️ Delete", Enabled = false };

        menuStrip.Items.AddRange(new ToolStripItem[] { refresh, select, delete });
        tableLayoutPanel.Controls.Add(menuStrip);
        panel.Controls.Add(listView);
        Controls.AddRange([panel, tableLayoutPanel]);

        listView.Columns.AddRange([new(), new()]);
        listView.ItemSelectionChanged += (sender, e) => { e.Item.Focused = false; e.Item.Selected = false; };
        listView.ItemChecked += (sender, e) =>
        {
            delete.Enabled = listView.CheckedItems.Count != 0;
            select.Text = listView.CheckedItems.Count != listView.Items.Count ? "✅ Select" : "🟩 Select";
        };

        select.Click += (sender, e) =>
        {
            bool check = listView.CheckedItems.Count != listView.Items.Count;
            foreach (ListViewItem listViewItem in listView.Items)
                listViewItem.Checked = check;
        };

        refresh.Click += (sender, e) =>
        {
            refresh.Enabled = select.Enabled = delete.Enabled = listView.Enabled = false;
            listView.Items.Clear();
            new Thread(() =>
            {

                sizes = NVIDIAShaderCache.GetSizes(processes = NVIDIAShaderCache.GetProcesses(paths = NVIDIAShaderCache.GetPaths()));
                foreach (KeyValuePair<string, List<string>> keyValuePair in processes)
                {
                    listView.Items.Add(new ListViewItem(new string[] { keyValuePair.Key, FormatBytes(sizes[keyValuePair.Key]) }));
                }
                refresh.Enabled = listView.Enabled = true;
                select.Enabled = listView.Items.Count != 0;
                if (listView.Items.Count != 0) select.Text = "✅ Select";
            }).Start();
        };

        delete.Click += (sender, e) =>
        {
            foreach (ListViewItem listViewItem in listView.CheckedItems)
                if (processes.ContainsKey(listViewItem.Text))
                {
                    if (listViewItem.Text == "glcache")
                    {
                        foreach (string path in paths)
                            if (path.ToLower().Contains("glcache"))
                            {
                                try { Directory.Delete($"{path.Split(new string[] { "GLCache" }, StringSplitOptions.RemoveEmptyEntries)[0]}GLCache", true); }
                                catch { }
                                break;
                            }
                    }
                    else
                        foreach (string uid in processes[listViewItem.Text])
                            foreach (string path in paths)
                                if (Path.GetFileNameWithoutExtension(path).StartsWith(uid))
                                    try { File.Delete(path); }
                                    catch { }
                }
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
using System;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;

enum Sort { SizeDescending, SizeAscending, NameAscending, NameDescending }

class Form : System.Windows.Forms.Form
{
    enum _ { B, KB, MB, GB }

    static string String(float _) { var value = (int)Math.Log(_, 1024); return $"{_ / Math.Pow(1024, value):0.00} {(_)value}"; }

    Sort Sort = default;

    IEnumerable<App> Enumerable(App[] source) => Sort switch
    {
        Sort.SizeDescending => source.OrderByDescending(_ => _.Size),
        Sort.SizeAscending => source.OrderBy(_ => _.Size),
        Sort.NameAscending => source.OrderBy(_ => _.Name),
        Sort.NameDescending => source.OrderByDescending(_ => _.Name),
        _ => default,
    };

    void Insert(ListView listView, IEnumerable<App> source)
    {
        MainMenuStrip.Enabled = listView.Enabled = false; listView.Items.Clear();
        foreach (var item in source) listView.Items.Add(new ListViewItem([item.Name, String(item.Size)]) { Tag = item });
        MainMenuStrip.Enabled = listView.Enabled = true;
    }

    internal Form()
    {
        Application.ThreadException += (sender, e) =>
        {
            var exception = e.Exception;
            while (exception.InnerException != null) exception = exception.InnerException;
            Unmanaged.ShellMessageBox(hWnd: Handle, lpcText: exception.Message);
            Close();
        };

        Font font = new("Segoe MDL2 Assets", 15);
        Text = "NVIDIA Shader Cache Explorer";
        Font = SystemFonts.MessageBoxFont;
        ClientSize = LogicalToDeviceUnits(new Size(800, 600));
        CenterToScreen();

        TableLayoutPanel tableLayoutPanel = new()
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink
        };
        Controls.Add(tableLayoutPanel);

        MainMenuStrip = new()
        {
            AutoSize = true,
            Margin = default
        };
        tableLayoutPanel.Controls.Add(MainMenuStrip);

        ToolStripButton toolStripButton1 = new("")
        {
            AutoSize = true,
            Margin = default,
            Font = font
        };
        MainMenuStrip.Items.Add(toolStripButton1);

        ToolStripButton toolStripButton2 = new("")
        {
            AutoSize = true,
            Margin = default,
            Font = font,
        };
        MainMenuStrip.Items.Add(toolStripButton2);

        ToolStripButton toolStripButton3 = new("")
        {
            AutoSize = true,
            Margin = default,
            Font = font
        };
        MainMenuStrip.Items.Add(toolStripButton3);

        ToolStripButton toolStripButton4 = new("")
        {
            AutoSize = true,
            Margin = default,
            Font = font
        };
        MainMenuStrip.Items.Add(toolStripButton4);

        ToolStripButton toolStripButton5 = new("")
        {
            AutoSize = true,
            Margin = default,
            Font = font,
        };
        MainMenuStrip.Items.Add(toolStripButton5);

        ListView listView = new()
        {
            Dock = DockStyle.Fill,
            Margin = default,
            View = View.Details,
            BorderStyle = BorderStyle.None,
            HeaderStyle = ColumnHeaderStyle.None,
            CheckBoxes = true,
            FullRowSelect = true
        };
        listView.Columns.AddRange([new(), new()]);
        listView.ItemSelectionChanged += (sender, e) => e.Item.Focused = e.Item.Selected = false;
        listView.ItemChecked += (sender, e) => e.Item.Focused = false;
        tableLayoutPanel.Controls.Add(listView);

        Resize += (sender, e) =>
        {
            var width = (listView.Width / listView.Columns.Count) - LogicalToDeviceUnits(2);
            foreach (ColumnHeader columnHeader in listView.Columns)
                columnHeader.Width = width;
        };

        App[] apps = default;

        toolStripButton1.Click += async (sender, e) => Insert(listView, Enumerable(apps = await Task.Run(Manager.Get)));

        toolStripButton2.Click += (sender, e) =>
{
    switch (Sort)
    {
        case Sort.SizeDescending:
            Sort = Sort.SizeAscending;
            break;

        case Sort.SizeAscending:
            Sort = Sort.NameAscending;
            break;

        case Sort.NameAscending:
            Sort = Sort.NameDescending;
            break;

        case Sort.NameDescending:
            Sort = Sort.SizeDescending;
            break;
    }

    Insert(listView, Enumerable(apps));
};

        toolStripButton3.Click += (sender, e) => { foreach (ListViewItem listViewItem in listView.Items) listViewItem.Checked = true; };

        toolStripButton4.Click += (sender, e) => { foreach (ListViewItem listViewItem in listView.Items) listViewItem.Checked = false; };

        toolStripButton5.Click += async (sender, e) =>
        {
            await Task.Run(() => Parallel.ForEach(listView.CheckedItems.Cast<ListViewItem>().Select(_ => (App)_.Tag), _ => { _.Delete(); }));
            if (listView.CheckedItems.Count != 0) toolStripButton1.PerformClick();
        };

        OnResize(null);
        toolStripButton1.PerformClick();
    }
}
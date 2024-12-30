using System;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;

class Form : System.Windows.Forms.Form
{
    enum _ { B, KB, MB, GB }

    static string String(float _) { var value = (int)Math.Log(_, 1024); return $"{_ / Math.Pow(1024, value):0.00} {(_)value}"; }

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

        ToolStripButton toolStripButton2 = new("")
        {
            AutoSize = true,
            Margin = default,
            Font = font
        };
        MainMenuStrip.Items.Add(toolStripButton2);

        ToolStripButton toolStripButton3 = new("")
        {
            AutoSize = true,
            Margin = default,
            Font = font
        };
        MainMenuStrip.Items.Add(toolStripButton3);

        ToolStripButton toolStripButton4 = new("")
        {
            AutoSize = true,
            Margin = default,
            Font = font,
        };
        MainMenuStrip.Items.Add(toolStripButton4);

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

        toolStripButton1.Click += async (sender, e) =>
        {
            MainMenuStrip.Enabled = listView.Enabled = false; listView.Items.Clear();
            foreach (var item in await Task.Run(Manager.Get)) listView.Items.Add(new ListViewItem([item.Name, String(item.Size)]) { Tag = item });
            MainMenuStrip.Enabled = listView.Enabled = true;
        };

        toolStripButton2.Click += (sender, e) => { foreach (ListViewItem listViewItem in listView.Items) listViewItem.Checked = true; };

        toolStripButton3.Click += (sender, e) => { foreach (ListViewItem listViewItem in listView.Items) listViewItem.Checked = false; };

        toolStripButton4.Click += async (sender, e) =>
        {
            await Task.Run(() => Parallel.ForEach(listView.CheckedItems.Cast<ListViewItem>().Select(_ => (App)_.Tag), _ => { _.Delete(); }));
            if (listView.CheckedItems.Count != 0) toolStripButton1.PerformClick();
        };

        OnResize(null);
        toolStripButton1.PerformClick();
    }
}
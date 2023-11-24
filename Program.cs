using System.Windows.Forms;
using System.Security.Principal;
using System.Diagnostics;
using System.Reflection;

class Program
{
    static void Main()
    {
        if (new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
        {
            Application.EnableVisualStyles();
            Application.Run(new MainForm());
        }
        else Process.Start(new ProcessStartInfo() { FileName = Assembly.GetEntryAssembly().Location, Verb = "RunAs" });
    }
}

using System.Windows.Forms;

static class Program
{
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new Form());
    }
}
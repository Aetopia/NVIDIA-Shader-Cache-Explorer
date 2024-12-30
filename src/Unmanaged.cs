using System;
using System.Runtime.InteropServices;

[assembly: DefaultDllImportSearchPaths(DllImportSearchPath.System32)]

[StructLayout(LayoutKind.Sequential, Pack = 1)]
struct WIN32_FILE_ATTRIBUTE_DATA
{
    internal int dwFileAttributes;
    internal System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
    internal System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
    internal System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
    internal uint nFileSizeHigh;
    internal uint nFileSizeLow;
}

static class Unmanaged
{
    [DllImport("Kernel32", CharSet = CharSet.Auto, SetLastError = true)]
    internal static extern bool DeleteFile(string lpFileName);

    [DllImport("Shell32", CharSet = CharSet.Auto, SetLastError = true)]
    internal static extern int ShellMessageBox(IntPtr hAppInst = default, IntPtr hWnd = default, string lpcText = default, string lpcTitle = "Error", int fuStyle = 0x00000010);

    [DllImport("Kernel32", CharSet = CharSet.Auto)]
    internal static extern bool GetFileAttributesEx(string lpFileName, int fInfoLevelId, out WIN32_FILE_ATTRIBUTE_DATA lpFileInformation);
}
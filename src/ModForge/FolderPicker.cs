using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace ModForge
{
    /// <summary>The modern Windows folder dialog (the one with the address bar), with the old tree dialog as a fallback.</summary>
    public static class FolderPicker
    {
        public static string Pick(Window owner, string title, string start)
        {
            try
            {
                var dlg = (IFileOpenDialog)new FileOpenDialog();
                try
                {
                    uint options;
                    dlg.GetOptions(out options);
                    dlg.SetOptions(options | 0x20 /* pick folders */ | 0x40 /* file system only */);
                    if (!string.IsNullOrEmpty(title)) dlg.SetTitle(title);
                    if (!string.IsNullOrEmpty(start) && Directory.Exists(start))
                    {
                        IShellItem item;
                        Guid iid = typeof(IShellItem).GUID;
                        if (SHCreateItemFromParsingName(start, IntPtr.Zero, ref iid, out item) == 0 && item != null) dlg.SetFolder(item);
                    }
                    IntPtr hwnd = owner != null ? new WindowInteropHelper(owner).Handle : IntPtr.Zero;
                    if (dlg.Show(hwnd) != 0) return null;
                    IShellItem result;
                    dlg.GetResult(out result);
                    string path;
                    result.GetDisplayName(0x80058000 /* file system path */, out path);
                    return path;
                }
                finally { Marshal.ReleaseComObject(dlg); }
            }
            catch (Exception)
            {
                using (var old = new System.Windows.Forms.FolderBrowserDialog { Description = title ?? "", ShowNewFolderButton = true })
                {
                    if (!string.IsNullOrEmpty(start) && Directory.Exists(start)) old.SelectedPath = start;
                    return old.ShowDialog() == System.Windows.Forms.DialogResult.OK ? old.SelectedPath : null;
                }
            }
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
        static extern int SHCreateItemFromParsingName([MarshalAs(UnmanagedType.LPWStr)] string path, IntPtr bindContext, ref Guid riid, [MarshalAs(UnmanagedType.Interface)] out IShellItem item);

        [ComImport, Guid("DC1C5A9C-E88A-4dde-A5A1-60F82A20AEF7")]
        class FileOpenDialog { }

        [ComImport, Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface IShellItem
        {
            void BindToHandler(IntPtr pbc, ref Guid bhid, ref Guid riid, out IntPtr ppv);
            void GetParent(out IShellItem ppsi);
            void GetDisplayName(uint sigdnName, [MarshalAs(UnmanagedType.LPWStr)] out string ppszName);
            void GetAttributes(uint sfgaoMask, out uint psfgaoAttribs);
            void Compare(IShellItem psi, uint hint, out int piOrder);
        }

        // Method order is the COM vtable order: IModalWindow, IFileDialog, then IFileOpenDialog. Do not reorder.
        [ComImport, Guid("d57c7288-d4ad-4768-be02-9d969532d960"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface IFileOpenDialog
        {
            [PreserveSig] int Show(IntPtr parent);
            void SetFileTypes(uint cFileTypes, IntPtr rgFilterSpec);
            void SetFileTypeIndex(uint iFileType);
            void GetFileTypeIndex(out uint piFileType);
            void Advise(IntPtr pfde, out uint pdwCookie);
            void Unadvise(uint dwCookie);
            void SetOptions(uint fos);
            void GetOptions(out uint pfos);
            void SetDefaultFolder(IShellItem psi);
            void SetFolder(IShellItem psi);
            void GetFolder(out IShellItem ppsi);
            void GetCurrentSelection(out IShellItem ppsi);
            void SetFileName([MarshalAs(UnmanagedType.LPWStr)] string pszName);
            void GetFileName([MarshalAs(UnmanagedType.LPWStr)] out string pszName);
            void SetTitle([MarshalAs(UnmanagedType.LPWStr)] string pszTitle);
            void SetOkButtonLabel([MarshalAs(UnmanagedType.LPWStr)] string pszText);
            void SetFileNameLabel([MarshalAs(UnmanagedType.LPWStr)] string pszLabel);
            void GetResult(out IShellItem ppsi);
            void AddPlace(IShellItem psi, int fdap);
            void SetDefaultExtension([MarshalAs(UnmanagedType.LPWStr)] string pszDefaultExtension);
            void Close(int hr);
            void SetClientGuid(ref Guid guid);
            void ClearClientData();
            void SetFilter(IntPtr pFilter);
            void GetResults(out IntPtr ppenum);
            void GetSelectedItems(out IntPtr ppsai);
        }
    }
}

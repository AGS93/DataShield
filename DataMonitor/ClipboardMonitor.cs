using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace DataMonitor
{
    public class ClipboardMonitor : Form
    {
        private List<ProtectFile> protectFiles = null;

        public ClipboardMonitor()
        {
            InitializeComponent();
        }

        /* Enables the clipboard monitor.
         * 
         * Param: None.
         * Return: None.
         */
        public void Enable()
        {
            NativeMethods.AddClipboardFormatListener(this.Handle);
            Application.Run();
        }

        /* Diables the clipboard monitor.
         * 
         * Param: None.
         * Return: None.
         */
        public void Disable()
        {
            Application.Exit();
        }

        /* Overriding WndProc method for when the clipboard flag gets called.
         * Checks the contents of the clipboard, and if bitmap (print screen),
         * removes and notifies the user. If the content is text, checks if the
         * active window is a protected file and if yes, clears clipboard
         * and notifies user.
         * 
         * Param: Message - WndProc message.
         * Return: None.
         */
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case NativeMethods.WM_CLIPBOARDUPDATE:
                    IDataObject iData = Clipboard.GetDataObject();
                    if (iData.GetDataPresent(DataFormats.Text))
                    {
                        if (CheckActiveWindow())
                        {
                            Clipboard.Clear();
                            MessageBox.Show("Clipboard Disabled!", "Data Shield", MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, (MessageBoxOptions)0x40000);
                        }
                    }
                    else if (iData.GetDataPresent(DataFormats.Bitmap))
                    {
                        Clipboard.Clear();
                        MessageBox.Show("Print Screen Disabled!", "Data Shield", MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, (MessageBoxOptions)0x40000);
                    }
                    break;

                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        /* Checks if the active window is a protected file. This is done
         * by calling the 'GetActiveWindowTitle' method and grabing the first
         * section of it, being the file name. Then if the file name from the
         * active window matches a file within the protected files list,
         * return true.
         * 
         * Param: None.
         * Return: Boolean - true is active window. false if not.
         */
        private Boolean CheckActiveWindow()
        {
            Boolean active = false;
            String window = null;
            protectFiles = Program.monitor.GetProtectedFiles();

            window = GetActiveWindowTitle().Split('-').First();

            foreach (var file in protectFiles)
            {
                String fileName = file.GetFile().Split('\\').Last().Split('.').First();

                if (window.Contains(fileName))
                {
                    active = true;
                }
            }
            return active;
        }

        /* Gets the title of the active window. This is done by
         * obtaining the handle of the foreground window and then
         * passing that into the 'GetWindowText' method. Both being methods
         * from 'user32.dll'.
         * 
         * Param: None.
         * Return: String - title of active window
         */
        private String GetActiveWindowTitle()
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            IntPtr handle = NativeMethods.GetForegroundWindow();

            if (NativeMethods.GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return null;
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ClipboardMonitor));
            this.SuspendLayout();
            // 
            // ClipboardMonitor
            // 
            this.ClientSize = new System.Drawing.Size(646, 305);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ClipboardMonitor";
            this.ShowInTaskbar = false;
            this.WindowState = FormWindowState.Minimized;
            this.ResumeLayout(false);

        }
    }

    internal static class NativeMethods
    {
        public const int WM_CLIPBOARDUPDATE = 0x031D;
        public static IntPtr HWND_MESSAGE = new IntPtr(-3);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
    }
}
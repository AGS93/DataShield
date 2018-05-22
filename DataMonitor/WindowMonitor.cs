using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataMonitor
{
    public class WindowMonitor
    {
        List<ProtectFile> protectedFiles = null;
        Boolean informed = false;
        Thread wm;

        /* A struct object that contains all variable factors for a Window Placement.
         */
        public struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public int showCmd;
            public Point ptMinPosition;
            public Point ptMaxPosition;
            public Rectangle rcNormalPosition;
        }

        /* Starts the 'Monitor' method in it's own Thread.
         * 
         * Param: List<String> - list of windows to monitor for.
         * Return: None.
         */
        public void Start(List<String> windows)
        {
            wm = new Thread(() => Monitor(windows));
            wm.Start();
        }

        /* Stops by invoking the 'Abort' method to the Monitor Thread.
         * 
         * Param: None.
         * Return: None.
         */
        public void Stop()
        {
            wm.Abort();
        }

        /* Monitors all the windows when a protected file(s) is open. If a window
         * that is not allowed to be on screen (normal window or maximised), it will
         * instantly minimise the protect file(s) and notify the user. More details are 
         * provided within the method.
         * 
         * Param: List<String> a list of the windows not allowed to be opened.
         * Return: None.
         */
        public void Monitor(List<String> windows)
        {
            while (true)
            {
                // Gets all processes and cycles through them.
                Process[] processes = Process.GetProcesses();
                foreach (var process in processes)
                {
                    if (process.MainWindowHandle != IntPtr.Zero)
                    {
                        // Cycles through each window in the window list.
                        foreach (String window in windows)
                        {
                            // If the process name contains the window name (e.g. chrome).
                            if (process.ProcessName.Contains(window))
                            {
                                // Identifies the placement of the window, and if either
                                // normal window or maximised, minimises the protected file(s).
                                WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
                                GetWindowPlacement(process.MainWindowHandle, ref placement);
                                switch (placement.showCmd)
                                {
                                    // Window is maximised.
                                    case 1:
                                        MinimiseProtectedFiles();
                                        if (informed == false)
                                        {
                                            informed = true;
                                            Task.Run(() => MessageBox.Show("Protected file(s) minimized while '" + process.ProcessName + "' is an active window."));
                                        }
                                        break;
                                    // Window is normal (on screen).
                                    case 3:
                                        MinimiseProtectedFiles();
                                        if (informed == false)
                                        {
                                            informed = true;
                                            Task.Run(() => MessageBox.Show("Protected file(s) minimized while '" + process.ProcessName + "' is an active window."));
                                        }
                                        break;
                                }
                            }
                        }
                    }
                }
                // Allows 0.1 of a second before repeat to ease use of resources.
                Thread.Sleep(100);
            }
        }

        /* Minimises any protected file(s) that is currently open. This is done by
         * getting the current list of active protected files, cycling through each
         * file and finding the process associated with it, then calling the
         * 'ShowWindowAsync' method passing through the process handle and '2' flag
         * indicating minimise.
         * 
         * Param: None.
         * Return: None.
         */
        public void MinimiseProtectedFiles()
        {
            String fileName = null;
            protectedFiles = Program.monitor.GetProtectedFiles();

            foreach (var file in protectedFiles)
            {
                fileName = file.GetFile().Split('\\').Last();
                fileName = fileName.Split('.').First();

                foreach (var process in Process.GetProcesses())
                {
                    if (process.MainWindowTitle.Contains(fileName))
                    {
                        ShowWindowAsync(process.MainWindowHandle, 2);
                    }
                }
            }
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
    }
}
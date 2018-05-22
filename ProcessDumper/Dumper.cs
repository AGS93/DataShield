using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace ProcessDumper
{
    class Dumper
    {
        /* Main method for application. First dumps all the currently running processes
         * on the system. Then goes on to monitor for additional processes and adds them
         * to the list. Additional methods for sorting the list and hash singular executables
         * are present.
         * 
         * Param: None.
         * Return: None.
         */
        [STAThread]
        static void Main()
        {
            //DumpCurrent();
            //DumpAdditional();
            //SortProcessList();
            GetProcessHash();
            //Application.Run();
        }

        /* Dumps all the current processes running on the system to a text file
         * called 'dump.txt'. It provides the process name followed by the SHA256
         * hash of the process executable.
         * 
         * Param: None.
         * Return: None.
         */
        public static void DumpCurrent()
        {
            List<String> listOfProcesses = new List<String>();
            var processes = Process.GetProcesses();
            foreach (var process in processes)
            {
                try
                {
                    listOfProcesses.Add(process.ProcessName + ".exe " + HashFile(process.MainModule.FileName));
                }
                catch (Exception)
                {
                    listOfProcesses.Add(process.ProcessName + ".exe Unavailable");
                }
            }
            File.WriteAllLines("C:\\dump.txt", listOfProcesses, Encoding.UTF8);
        }

        /* Dumps all additional processes by starting a Management Event Watcher, querying
         * Win32_ProcessStartTrace. When a new process is started, it triggers the Event Arrived
         * method and adds that process to the list of processes.
         * 
         * Param: None.
         * Return: None.
         */
        public static void DumpAdditional()
        {
            ManagementEventWatcher sWatch = new ManagementEventWatcher(new WqlEventQuery("Win32_ProcessStartTrace"));
            sWatch.EventArrived += new EventArrivedEventHandler(NewProcess);
            sWatch.Start();

            void NewProcess(object sender, EventArrivedEventArgs e)
            {
                using (StreamWriter writer = File.AppendText("C:\\dump.txt"))
                {
                    Process process = Process.GetProcessesByName(e.NewEvent.Properties["ProcessName"].Value.ToString().Split('.').First()).First();
                    try
                    {
                        writer.WriteLine(process.ProcessName + ".exe " + HashFile(process.MainModule.FileName));
                    }
                    catch (Exception)
                    {
                        writer.WriteLine(process.ProcessName + ".exe Unavailable");
                    }
                }
            }
        }

        /* Sorts out process dump list. More details given within the method.
         * 
         * Param: None.
         * Return: None.
         */
        public static void SortProcessList()
        {
            // Read the process dump file.
            List<String> processes = File.ReadAllLines("C:\\dump.txt").ToList();

            // Remove duplicates from the list.
            List<String> noDuplicates = processes.Distinct().ToList();

            // Sort list alphabetically.
            noDuplicates.Sort();

            // Save sorted list to file.
            File.WriteAllLines("C:\\Whitelist.txt", noDuplicates, Encoding.UTF8);
        }

        /* Gets the process name and hash of a singular file.
         * 
         * Param: None.
         * Return: None.
         */
        public static void GetProcessHash()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = "c:\\";
            ofd.Filter = "All Files (*)|*";
            ofd.FilterIndex = 2;
            ofd.RestoreDirectory = true;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                Clipboard.SetText(ofd.FileName.Split('\\').Last() + " " + HashFile(ofd.FileName));
                MessageBox.Show("New process and hash set to clipboard memory.");
            }
        }

        /* Produces a hash of the file, the using the SHA256 algorithm which
         * computes a hash result in an byte array. Then the byte array is converted
         * to a string, providing a hash String.
         * 
         * Param: String - path to file.
         * Return: String - hash of file.
         */
        public static String HashFile(String filePath)
        {
            using (FileStream stream = File.OpenRead(filePath))
            {
                SHA256Managed sha = new SHA256Managed();
                byte[] hash = sha.ComputeHash(stream);

                return BitConverter.ToString(hash).Replace("-", String.Empty);
            }
        }
    }
}

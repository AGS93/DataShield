using System;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Windows.Forms;
using System.Management;
using System.Diagnostics;
using System.Net;
using System.IO.Pipes;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace DataMonitor
{
    class Monitor
    {
        List<String> whitelist = new List<String>();
        List<String> authorisedDevices = new List<String>();
        List<ProtectFile> protectedFiles = new List<ProtectFile>();
        List<FileSystemWatcher> fileWatchers = new List<FileSystemWatcher>();
        Utilities utilities = new Utilities();
        WindowMonitor wm = new WindowMonitor();
        EjectDevice ed = new EjectDevice();
        ManagementEventWatcher wWatch, bWatch, newDevice;
        ClipboardMonitor cm;
        Thread clipboard;

        /* Obtains a whitelist of process that are allowed to run on the system. This is
         * done by downloading the a list of whitelisted processes from the whitelist database 
         * table through the server. For extra security, the processes are paired with a SHA256
         * hash of their executable to prevent fake processes being ran.
         * 
         * Param: None.
         * Return: None.
         */
        public void ObtainWhitelist()
        {
            String wl;
            using (WebClient client = new WebClient())
            {
                ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
                wl = client.DownloadString(ConfigurationManager.AppSettings["obtainwhitelist"]);
            }
            whitelist = wl.Split('|').ToList();
        }

        /* Obtains a list of authorised devices that are allowed to used on the system. This is
         * done by downloading the device list from the authorised database table through the server.
         * 
         * Param: None.
         * Return: None.
         */
        public void ObtainAuthorisedDevices()
        {
            String ad;
            using (WebClient client = new WebClient())
            {
                ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
                ad = client.DownloadString(ConfigurationManager.AppSettings["obtainauthoriseddevices"]);
            }
            authorisedDevices = ad.Split('@').ToList();
        }

        /* Calls in to the server to inform its running the Data Monitor application, and
         * allowing the user and system to be included in the encryption of files. This is done
         * by getting the computer name and HWID of the system first and updating the systems table.
         * Then it will get the username of the current logged in user and update the users table.
         * 
         * Param: None.
         * Return: None.
         */
        public void CallIn()
        {
            String name = utilities.ComputerName();
            String hwid = utilities.HWID();
            using (WebClient client = new WebClient())
            {
                String system = name + "|" + hwid;
                ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
                client.UploadString(ConfigurationManager.AppSettings["updatesystems"], system);
            }

            String user = utilities.Username();
            using (WebClient client = new WebClient())
            {
                ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
                client.UploadString(ConfigurationManager.AppSettings["updateusers"], user);
            }
        }

        /* Provides authentication information to other processes that request it. This works by
         * setting up a Named Pipe Server that awaits connections and when requested, calls this 
         * method which grabs the username and HWID of the current system and writes this 
         * information as a response.
         * 
         * Param: None.
         * Return: None.
         */
        public void AuthInfo()
        {
            PipeSecurity ps = new PipeSecurity();
            ps.AddAccessRule(new PipeAccessRule("Users", PipeAccessRights.ReadWrite | PipeAccessRights.CreateNewInstance, AccessControlType.Allow));
            ps.AddAccessRule(new PipeAccessRule("SYSTEM", PipeAccessRights.FullControl, AccessControlType.Allow));
            NamedPipeServerStream pipeServer = new NamedPipeServerStream("AuthInfo", PipeDirection.InOut, 10,
                                            PipeTransmissionMode.Message, PipeOptions.WriteThrough, 1024, 1024, ps);

            StreamWriter sw = new StreamWriter(pipeServer);
            while (true)
            {
                try
                {
                    pipeServer.WaitForConnection();
                    sw.WriteLine(utilities.AuthInfo());
                    sw.Flush();
                    pipeServer.WaitForPipeDrain();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    pipeServer.WaitForPipeDrain();
                    if (pipeServer.IsConnected)
                    {
                        pipeServer.Disconnect();
                    }
                }
            }
        }

        /* A Named Pipe Server listening for requests on files to start and stop monitoring.
         * When an incoming request comes with the 'opened' flag, it grabs the file path,
         * creates an instance of a protected file using the 'ProtectFile' class and adds to the
         * list of active protected files; then starting the file monitoring functions. If an 
         * incoming request has the 'closed' flag, it will remove the closed file from the active 
         * protected files list. Being either 'opened' or 'closed', it always checks if the active
         * protected files list contains any files and will turn on/off all monitoring functions 
         * when needed.
         * 
         * Param: None.
         * Return: None.
         */
        public void FileMonitor()
        {
            String call, option, file;
            
            PipeSecurity ps = new PipeSecurity();
            ps.AddAccessRule(new PipeAccessRule("Users", PipeAccessRights.ReadWrite | PipeAccessRights.CreateNewInstance, AccessControlType.Allow));
            ps.AddAccessRule(new PipeAccessRule("SYSTEM", PipeAccessRights.FullControl, AccessControlType.Allow));
            NamedPipeServerStream pipeServer = new NamedPipeServerStream("FileMonitor", PipeDirection.InOut, 10,
                                            PipeTransmissionMode.Message, PipeOptions.WriteThrough, 1024, 1024, ps);

            StreamReader sr = new StreamReader(pipeServer);
            while (true)
            {
                try
                {
                    pipeServer.WaitForConnection();
                    call = sr.ReadLine();
                    option = call.Split('|').First();
                    file = call.Split('|').Last();
                    if (option.Equals("opened"))
                    {
                        if (!protectedFiles.Any())
                        {
                            EnableClipboardMonitor();
                            DisableBlacklistProcesses();
                            EnableWindowMonitor();
                            EnableFileCopyMonitor();
                        }
                        protectedFiles.Add(new ProtectFile(file));
                    }
                    else if (option.Equals("closed"))
                    {
                        String fileName = file.Split('\\').Last();
                        MessageBox.Show("Please ensure to either re-encrypt file '" + fileName + "', or remove from the protected area. Last access to this file has been logged.");
                        if (protectedFiles.Any())
                        {
                            foreach (var pfile in protectedFiles.ToList())
                            {
                                if (pfile.GetFile().Equals(file))
                                {
                                    protectedFiles.Remove(pfile);
                                }
                            }
                            if (!protectedFiles.Any())
                            {
                                DisableClipboardMonitor();
                                EnableBlacklistProcesses();
                                DisableWindowMonitor();
                                DisableFileCopyMonitor();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    pipeServer.WaitForPipeDrain();
                    if (pipeServer.IsConnected)
                    {
                        pipeServer.Disconnect();
                    }
                }
            }
        }

        /* Monitors all active processes on the system. This is done by creating a Management 
         * Event Watcher that queries 'Win32_ProcessStartTrace', setting an Event Arrived Handler. 
         * When a new process starts, it triggers the 'CheckProcess' method where the newly 
         * created process is checked against the process whitelist and if not present, will 
         * get terminated instantly and the user will be notified. For extra security, the process
         * is paired with a SHA256 hash of the process's executable to prevent fake processes 
         * being ran.
         * 
         * Param: None.
         * Return: None.
         */
        public void ProcessMonitor()
        {
            wWatch = new ManagementEventWatcher(new WqlEventQuery("Win32_ProcessStartTrace"));
            wWatch.EventArrived += new EventArrivedEventHandler(CheckProcess);
            wWatch.Start();

            void CheckProcess(object sender, EventArrivedEventArgs e)
            {
                String process = null;
                var pr = Process.GetProcessesByName(e.NewEvent.Properties["ProcessName"].Value.ToString().Split('.').First());
                foreach (var pro in pr)
                {
                    try
                    {
                        process = pro.ProcessName + ".exe " + utilities.HashFile(pro.MainModule.FileName);
                    }
                    catch (Exception)
                    {
                        process = pro.ProcessName + ".exe Unavailable";
                    }
                }

                if (!whitelist.Contains(process))
                {
                    foreach (var p in Process.GetProcessesByName(e.NewEvent.Properties["ProcessName"].Value.ToString().Replace(".exe", "")))
                    {
                        p.Kill();
                        MessageBox.Show("'" + p.ProcessName + "' is not a valid application. Please contact a system administrator.");
                    }
                }
            }
        }

        /* Monitors all storage devices connected to the computer system and prevents any unauthorised devices
         * from being allowed active on system. This is done by first doing an initial check on all currently
         * connected devices, querying 'Win32_DiskDrive' to obtain all disk drives and comparing their
         * information with the authorised devices list. If a device is not present in the list, it gets ejected
         * via the 'EjectDisk' method. After the initial check, a Management Event Watcher is created monitoring 
         * when a volume change event occurs, being event type 2 meaning 'inserted'; which will then trigger
         * the 'CheckDevice' method again to check the newly inserted device.
         * 
         * Param: None.
         * Return: None.
         */
        public void DeviceMonitor()
        {
            CheckDevice();

            ManagementEventWatcher watcher = new ManagementEventWatcher();
            WqlEventQuery query = new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 2");

            watcher.EventArrived += (s, e) =>
            {
                CheckDevice();
            };
            watcher.Query = query;
            watcher.Start();

            void CheckDevice()
            {
                Thread.Sleep(500);
                try
                {
                    ManagementObjectSearcher drives = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
                    String serialNumber = null;
                    String signature = null;
                    foreach (ManagementObject drive in drives.Get())
                    {
                        // If both the serial number and/or signature are null, assigns '0'.
                        if (drive.Properties["SerialNumber"].Value != null)
                        {
                            serialNumber = drive.Properties["SerialNumber"].Value.ToString();
                        }
                        else
                        {
                            serialNumber = "0";
                        }
                        if (drive.Properties["Signature"].Value != null)
                        {
                            signature = drive.Properties["Signature"].Value.ToString();
                        }
                        else
                        {
                            signature = "0";
                        }

                        // If a drives name, serial number and signature isn't contained within the authorised devices array, deems it as 'unauthorised'.
                        if (!authorisedDevices.Contains(drive.Properties["Caption"].Value.ToString() + "|" + serialNumber + "|" + signature))
                        {
                            Task.Run(() => MessageBox.Show("Device '" + drive.Properties["Caption"].Value + "' not authorised!"));
                            Thread.Sleep(500);

                            // Uses a method within the 'EjectDevice' class to eject the drive, using a utility method to get the volume number needed for 'Dismount' function within 'Win32_Volume'.
                            ed.EjectDrive(utilities.GetVolumeNumber(drive));
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        /* Enables the clipboard monitor. This is done by creating a new instance of
         * the Clipboard monitor object and assigning it to a new Thread, passing through
         * the 'Enable' method and setting the Apartment State to 'STA'.
         * 
         * Param: None.
         * Return: None.
         */
        public void EnableClipboardMonitor()
        {
            cm = new ClipboardMonitor();
            clipboard = new Thread(cm.Enable);
            clipboard.SetApartmentState(ApartmentState.STA);
            clipboard.Start();
        }

        /* Disables the Clipboard monitor by invoking the 'Abort' method on the Thread.
         * 
         * Param: None.
         * Return: None.
         */
        public void DisableClipboardMonitor()
        {
            clipboard.Abort();
        }

        /* Disables all processes on the system that are on the process blacklist,
         * and further monitors all newly created process terminating any that are
         * present on the process blacklist. This is done by first obtaining the 
         * blacklist from the server, then doing an initial termination of all
         * blacklist processes on the system. A Management Event Watcher is then 
         * created to watch for newly created processes and applies the same method of
         * checking/terminating blacklist processes.
         * 
         * Param: None.
         * Return: None.
         */
        public void DisableBlacklistProcesses()
        {
            List<String> blacklist = new List<String>();
            String bl = null;
            using (WebClient client = new WebClient())
            {
                ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
                bl = client.DownloadString(ConfigurationManager.AppSettings["obtainblacklist"]);
            }
            blacklist = bl.Split().ToList();

            foreach (Process p in Process.GetProcesses())
            {
                if (blacklist.Contains(p.ProcessName + ".exe"))
                {
                    p.Kill();
                }
            }

            bWatch = new ManagementEventWatcher(new WqlEventQuery("Win32_ProcessStartTrace"));
            bWatch.EventArrived += new EventArrivedEventHandler(CheckProcess);
            bWatch.Start();

            void CheckProcess(object sender, EventArrivedEventArgs e)
            {
                String process = e.NewEvent.Properties["ProcessName"].Value.ToString();

                if (blacklist.Contains(e.NewEvent.Properties["ProcessName"].Value.ToString()))
                {
                    foreach (Process p in Process.GetProcessesByName(process.Remove(process.Length - 4)))
                    {
                        p.Kill();
                        MessageBox.Show("'" + p.ProcessName + "' is blocked while a protected file is open.");
                    }
                }
            }
        }

        /* Stops the blacklist process Watcher.
         * 
         * Param: None.
         * Return: None.
         */
        public void EnableBlacklistProcesses()
        {
            bWatch.Stop();
        }

        /* Enables the windows monitor. First, a List containing strings is created,
         * allowing for ease of adding/subtracking applications we wish to be deemed an
         * 'unsafe' and the protected file(s) when be minimised when any of these windows
         * are on screen, either maximised or windowed view. Once the List is made, it calls 
         * the 'Start' method, passing through the windows list.
         * 
         * Param: None.
         * Return: None.
         */
        public void EnableWindowMonitor()
        {
            List<String> windows = new List<String>();
            windows.Add("chrome");
            windows.Add("MicrosoftEdge");
            windows.Add("iexplore");
            windows.Add("mspaint");
            windows.Add("PaintStudio.View");
            wm.Start(windows);
        }

        /* Disables the windows monitor by calling the 'Stop' method.
         * 
         * Param: None.
         * Return: None.
         */
        public void DisableWindowMonitor()
        {
            wm.Stop();
        }

        /* Enables the monitor that watches for any newly created files on all storage devices, using the
         * AForge.NET.Imaging library, part of an open source C# framework for Artificial Intelligence.
         * This functiuon works by first getting all the currently connected drives and creating a 
         * File System Watcher for each drive, then adding them to the watchers List<FileSystemWatcher>.
         * To further monitor later connected drives, a Management Event Watcher is listening for newly
         * connected devices by monitoring the Volume Change Event. The new device will have a
         * File System Watcher allocated to it and added to the watchers List. The actual A.I functions
         * and logic of this procedure is handled in the 'Watcher' method in the 'Utilities' class. All
         * details are provided there.
         * 
         * Param: None.
         * Return: None.
         */
        public void EnableFileCopyMonitor()
        {
            var drives = DriveInfo.GetDrives();

            foreach (var drive in drives)
            {
                if (drive.DriveType.ToString() != "CDRom")
                {
                    fileWatchers.Add(utilities.Watcher(drive.Name));
                }
            }

            newDevice = new ManagementEventWatcher();
            WqlEventQuery query = new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 2");

            newDevice.EventArrived += (s, e) =>
            {
                var updatedDrives = DriveInfo.GetDrives();

                foreach (var drive in updatedDrives)
                {
                    if (!drives.Contains(drive) && drive.DriveType.ToString() != "CDRom")
                    {
                        fileWatchers.Add(utilities.Watcher(drive.Name));
                    }
                }
            };
            newDevice.Query = query;
            newDevice.Start();
        }

        /* Disables the monitor that watches for newly created files on all devices. This is done
         * when all protected files are closed and the active protected file list becomes empty.
         * This results in each File System Watcher object within the List getting 'Disposed'. 
         * Finally, it stops the Management Event Watcher for watching newly inputed devices.
         * 
         * Param: None.
         * Return: None.
         */
        public void DisableFileCopyMonitor()
        {
            foreach (var watcher in fileWatchers)
            {
                watcher.Dispose();
            }
            newDevice.Stop();
        }

        /* Gets the current list of active protected files.
         * 
         * Param: None.
         * Return: List of active ProtectFile objects.
         */
        public List<ProtectFile> GetProtectedFiles()
        {
            return protectedFiles;
        }
    }
}

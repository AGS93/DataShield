using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace DeviceUpdater
{
    class Updater
    {
        /* The main method class. First it checks the system's HWID to see if it is authroised to use 
         * the application. If it passes, then checks if the username of the logged in user is banned.
         * If not banned, will allow the user to enter a password to gain access to the application
         * using the password-based authentication system with limited attempts.
         * 
         * This application allows an administrator to update the authorised device table on the database.
         * This is done by first getting all the currently connected devices on the system, querying 
         * 'Win32_DiskDrive' and storing them in a List. It then starts an Management Event Watcher
         * on 'Win32_VolumeChangeEvent', where Event Type is '2' being 'inserted' and ask the user to
         * insert the device they wish to authorised. It will then get a new list of all connected devices 
         * and identify the newly added device. It will then confirm with the user if this is the correct 
         * drive to authorised, and if yes will send the new device details to the server for it then to be
         * added to the authorised database table.
         * 
         * Param: Nome.
         * Return: None.
         */
        [STAThread]
        static void Main()
        {
            if (CheckHWID())
            {
                if (!CheckBanned())
                {
                    if (PasswordProtection())
                    {
                        List<String> currentDevices = GetCurrentDevices();

                        ManagementEventWatcher watcher = new ManagementEventWatcher();
                        WqlEventQuery query = new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 2");

                        watcher.EventArrived += (s, e) =>
                        {
                            CheckDevice();
                        };
                        watcher.Query = query;
                        watcher.Start();

                        MessageBox.Show("Please insert removable media device...");
                        Application.Run();

                        void CheckDevice()
                        {
                            List<String> updatedDevices = GetCurrentDevices();

                            foreach (String device in updatedDevices)
                            {
                                if (!currentDevices.Contains(device))
                                {
                                    DialogResult dialogResult = MessageBox.Show("New device '" + device.Split('|').First() + "' ?", "New Device", MessageBoxButtons.YesNo);
                                    if (dialogResult == DialogResult.Yes)
                                    {
                                        using (WebClient client = new WebClient())
                                        {
                                            ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
                                            String message = client.UploadString(ConfigurationManager.AppSettings["updateauthoriseddevices"], device);
                                            MessageBox.Show(message);
                                            foreach (var process in Process.GetProcessesByName("DataMonitor"))
                                            {
                                                process.Kill();
                                            }
                                            Environment.Exit(1);
                                        }
                                    }
                                    else if (dialogResult == DialogResult.No)
                                    {
                                        MessageBox.Show("Please reinsert the removable media device...");
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("This system is not authorised to use this application.");
                Environment.Exit(1);
            }
        }

        /* Checks if the current computer is authorised to use this application. It does this
         * by calling the 'GetHWID' method, then sends the information to the server. The server
         * then product key, being 'whitelistupdater' and checks that the provided HWID
         * matches the stored HWID.
         *
         * Param: None.
         * Return: Boolean. true - if authorised. false - if not.
         */
        public static Boolean CheckHWID()
        {
            String response;

            String hwid = GetHWID();

            using (WebClient client = new WebClient())
            {
                ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
                response = client.UploadString(ConfigurationManager.AppSettings["checkhwid"], "deviceupdater" + "|" + hwid);
            }

            if (response.Equals("valid"))
            {
                return true;
            }
            return false;
        }

        /* Checks if the current user is banned from using the application. It does this
         * by calling the 'Username' method and sends the information to the server. The
         * server then checks if the user value is within the 'banned' database table.
         *
         * Param: None.
         * Return: Boolean. true - if banned. false - if not.
         */
        public static Boolean CheckBanned()
        {
            String response = null;

            String user = Username();

            using (WebClient client = new WebClient())
            {
                ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
                response = client.UploadString(ConfigurationManager.AppSettings["checkbanned"], user);
            }

            if (response == "banned")
            {
                MessageBox.Show("User '" + user + "' is banned from using this application. Please try again later.");
                return true;
            }
            return false;
        }

        /* A password based authentication system, allowing 3 attempts before being banned for 5 minutes.
         * First, it hashes the password attempt and sends to the server for comparison against stored hash
         * on the database. If attempt is incorrect, the number of attempts +1, and if 3 is surpassed, bans
         * the user.
         * 
         * Param: None.
         * Return: Boolean. true - allowing user access.
         */
        public static Boolean PasswordProtection()
        {
            String response = null;
            Boolean granted = false;
            int attempts = 0;

            while (granted == false)
            {
                Password password = new Password();
                password.ShowDialog();
                String attempt = Hash(password.password);

                using (WebClient client = new WebClient())
                {
                    ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
                    response = client.UploadString(ConfigurationManager.AppSettings["checkupdatedevices"], attempt);
                }
                if (attempts < 2)
                {
                    if (response == "unauthorised")
                    {
                        attempts += 1;
                        MessageBox.Show("Password Inccorect! Please try again.");
                    }
                    else
                    {
                        granted = true;
                    }
                }
                else
                {
                    String user = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                    user = user.Substring(user.LastIndexOf("\\") + 1);
                    using (WebClient client = new WebClient())
                    {
                        ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
                        client.UploadString(ConfigurationManager.AppSettings["banuser"], user);
                    }
                    MessageBox.Show("You are now locked out. Please try again in 5 minutes.");
                    Environment.Exit(0);
                }
            }
            return granted;
        }

        /* Hashes the password attempt inputted by the user. This is done by converting 
         * the password string into a byte array, then using the SHA512 Managed class to 
         * compute the hash using its algorithm. It is then converted from a byte array 
         * to a String and the salt is added to the end.
         * 
         * Param: String - password attempt.
         * Return: String - hashed password attempt.
         */
        public static String Hash(String password)
        {
            String salt = "%4rF9a";

            var bytes = new UTF8Encoding().GetBytes(password);
            byte[] hash;
            using (var algorithm = new SHA512Managed())
            {
                hash = algorithm.ComputeHash(bytes);
            }
            return Convert.ToBase64String(hash) + salt;
        }

        /* Gets the actual current logged in user, instead of returning the inherited user of the 
         * parent process. This is done by querying 'Win32_Process' and getting the 'explorer.exe' 
         * processes, then invoking the 'GetOwner' method to obtain the logged in user.
         * 
         * Param: None.
         * Return: String - current logged in username.
         */
        public static String Username()
        {
            var query = new ObjectQuery("SELECT * FROM Win32_Process WHERE Name = 'explorer.exe'");
            var explorerProcesses = new ManagementObjectSearcher(query).Get();

            foreach (ManagementObject mo in explorerProcesses)
            {
                String[] ownerInfo = new String[2];
                mo.InvokeMethod("GetOwner", (object[])ownerInfo);

                return ownerInfo[0];
            }
            return String.Empty;
        }

        /* Gets all the disk drive devices currently active on the system. This is
         * done by querying 'Win32_DiskDrive' to obtain all the drive objects.
         * It will then ensure if there are any null values, gives it a value
         * of '0'. Finally, it adds each drive to the list of current drives.
         * 
         * Param: None.
         * Return: List - a list of each current connected disk drive.
         */
        public static List<String> GetCurrentDevices()
        {
            List<String> currentDevices = new List<String>();
            try
            {
                ManagementObjectSearcher drives = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
                foreach (ManagementObject drive in drives.Get())
                {
                    if (drive.Properties["SerialNumber"].Value != null)
                    {
                        if (drive.Properties["Signature"].Value != null)
                        {
                            currentDevices.Add(drive.Properties["Caption"].Value.ToString() + "|" + drive.Properties["SerialNumber"].Value.ToString() + "|" + drive.Properties["Signature"].Value.ToString());
                        }
                        else
                        {
                            currentDevices.Add(drive.Properties["Caption"].Value.ToString() + "|" + drive.Properties["SerialNumber"].Value.ToString() + "|" + "0");
                        }
                    }
                    else
                    {
                        currentDevices.Add(drive.Properties["Caption"].Value.ToString() + "|" + "0" + "|" + "0");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return currentDevices;
        }

        /* Gets the Hardware Identification of the current system. This is done by
         * first getting the CPU ID via querying 'Win32_Processor', and grabbing the 
         * Processer ID. Then it gets the serial numnber of the logical disk (C:).
         * Finally, it combines the CPU ID and serial number, forming the HWID.
         * 
         * Param: None.
         * Return: String - the HWID of the current computer system.
         */
        public static String GetHWID()
        {
            String hwid, cpu, hdd;

            ManagementObjectSearcher moc = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_processor");
            ManagementObjectCollection list = moc.Get();
            cpu = null;
            foreach (ManagementObject mo in list)
            {
                cpu = mo["ProcessorId"].ToString();
                break;
            }

            ManagementObject disk = new ManagementObject(@"win32_logicaldisk.deviceid=""c:""");
            disk.Get();
            hdd = disk["VolumeSerialNumber"].ToString();

            hwid = Hash(cpu + hdd);

            return hwid;
        }
    }
}

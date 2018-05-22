using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace WhitelistUpdater
{
    static class Updater
    {
        /* The main method class. First it checks the system's HWID to see if it is authroised to use 
         * the application. If it passes, then checks if the username of the logged in user is banned.
         * If not banned, will allow the user to enter a password to gain access to the application
         * using the password-based authentication system with limited attempts.
         * 
         * This application allows an administrator to update the process whitelist table on the database. 
         * This is done by reading a text file containing all the whitelisted processes and updating the 
         * 'whitelist' table.
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
                        String[] whitelist = null;

                        OpenFileDialog ofd = new OpenFileDialog();
                        ofd.InitialDirectory = "c:\\";
                        ofd.Filter = "Text Files (*.txt)|*.txt";
                        ofd.FilterIndex = 2;
                        ofd.RestoreDirectory = true;

                        if (ofd.ShowDialog() == DialogResult.OK)
                        {
                            try
                            {
                                if ((whitelist = File.ReadAllLines(ofd.FileName)) != null)
                                {
                                    String wl = String.Join(",", whitelist);
                                    using (WebClient client = new WebClient())
                                    {
                                        ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
                                        String message = client.UploadString(ConfigurationManager.AppSettings["updatewhitelist"], wl);
                                        MessageBox.Show(message);
                                        foreach (var process in Process.GetProcessesByName("DataMonitor"))
                                        {
                                            process.Kill();
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
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

        /* Checks if the current computer is authorised to use the application. It does this
         * by calling the 'GetHWID' method, then sends the information to the server. The server
         * looks up the product name, being 'whitelistupdater' and checks that the provided HWID
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
                response = client.UploadString(ConfigurationManager.AppSettings["checkhwid"], "whitelistupdater" + "|" + hwid);
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
                    response = client.UploadString(ConfigurationManager.AppSettings["checkupdatewhitelist"], attempt);
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
                } else
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

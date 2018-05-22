using System;
using System.Configuration;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.ServiceProcess;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.Imaging;
using IFilterTextReader;
using Microsoft.Win32;
using System.Drawing.Imaging;

namespace DataMonitor
{
    class Utilities
    {
        /* Checks if the Data Monitor service is installed. If not, indicates fresh installtion and 
         * performs a set of tasks to set up the environment for the Data Shield application to function.
         * It installs the service, authorises the current removable media devices, installs the
         * RSA Key Container for the encrypted configuration files, installs the Data Shield
         * Proxy X509 Certificate in Trusted Root Certification Authorities, and finally, sets
         * the Proxy Setting for the current user. It will then start the service once completed.
         * 
         * Param: None.
         * Return: Boolean - true if service didnt exist. false if it did.
         */
        public Boolean CheckService()
        {
            ServiceController ct = ServiceController.GetServices().FirstOrDefault(s => s.ServiceName == "Data Monitor");
            if (ct == null)
            {
                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.FileName = "C:\\Program Files (x86)\\Data Shield\\ProcessProtector.exe";
                startInfo.Arguments = "-install";
                startInfo.Verb = "runas";
                process.StartInfo = startInfo;
                process.Start();
                Thread.Sleep(10000);
                InstallRSAKeys();
                AuthoriseCurrentDevices();
                InstallCA();
                MessageBox.Show("Installtion Successful! Data Monitor now active.");
                ct = ServiceController.GetServices().FirstOrDefault(s => s.ServiceName == "Data Monitor");
                ct.Start();
                return true;
            }
            return false;
        }

        /* Installs the custom RSA Key Container to the system, and grants the system 
         * access. It then deletes the Key Container XML file from the computer. This 
         * is so all of Data Shield's configuration files can be decrypted in run-time
         * on the system.
         * 
         * Param: None.
         * Return: None.
         */
        public void InstallRSAKeys()
        {
            Process.Start("CMD.exe", @"/C C:\Windows\Microsoft.NET\Framework64\v4.0.30319\aspnet_regiis -pi ""datashield"" ""C:\Program Files (x86)\Data Shield\datashieldkeys.xml""");
            Thread.Sleep(3000);

            Process.Start("CMD.exe", @"/C C:\Windows\Microsoft.NET\Framework64\v4.0.30319\aspnet_regiis -pa ""datashield"" ""NT AUTHORITY\NETWORK SERVICE""");
            Thread.Sleep(1000);

            File.Delete("C:\\Program Files (x86)\\Data Shield\\datashieldkeys.xml");
        }

        /* Authorises all the current connected devices on the computer system. This
         * is done by using a Management Object Searcher to query 'Win_32_DiskDrive'
         * and get all the current drives. It then checks if either the serial number
         * and signatures are null or not, giving a value of '0' if null. Finally,
         * each drive is sent to the server to be added to the authorised devices
         * table in the database.
         * 
         * Param: None.
         * Return: None.
         */
        public void AuthoriseCurrentDevices()
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

            foreach (String device in currentDevices)
            {
                using (WebClient client = new WebClient())
                {
                    ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
                    client.UploadString(ConfigurationManager.AppSettings["updateauthoriseddevices"], device);
                }
            }
        }

        /* Installs the Data Shield Proxys X509 Certificate in Trusted Root 
         * Certification Authorities, using the 'X509Store' object.
         * 
         * Param: None.
         * Return: None.
         */
        public void InstallCA()
        {
            String cert = "C:\\Program Files (x86)\\Data Shield\\proxyCA.der";
            X509Certificate2 certificate = new X509Certificate2(cert);
            using (X509Store store = new X509Store(StoreName.Root, StoreLocation.LocalMachine))
            {
                store.Open(OpenFlags.ReadWrite);
                store.Add(certificate);
                store.Close();
            }
        }

        /* Sets the proxy settings for the local user by launching the 'ProxySettings'
         * application, using the 'CreateProcessAsUser' class, ensuring it sets the proxy 
         * settings for the actual logged in user and not the parent token, being 'SYSTEM' 
         * from the Data Monitor process.
         * 
         * Param: None.
         * Return: None.
         */
        public void SetProxy()
        {
            Thread.Sleep(10000);
            CreateProcessAsUser.ProcessAsUser.Launch("C:\\Program Files (x86)\\Data Shield\\ProxySettings.exe");
        }

        /* Checks if the 'AppData' directories for Data Shield are currently present.
         * If not, it will created them.
         * 
         * Param: None.
         * Return: None.
         */
        public void CheckDirectories()
        {
            String dsFolder = "C:\\Users\\" + Username() + "\\AppData\\Roaming\\Data Shield";
            if (!Directory.Exists(dsFolder))
            {
                Directory.CreateDirectory(dsFolder);
                Directory.CreateDirectory(dsFolder + "\\Samples");
                Directory.CreateDirectory(dsFolder + "\\Temp");
            }
        }

        /* Checks the Registry to see if both the file encryption and decryption context
         * menu option values are present. If they are not, it will insert the values using
         * the Registry class.
         * 
         * Param: None.
         * Return: None.
         */
        public void CheckContextMenu()
        {
            RegistryKey key;
            String kName;

            kName = "HKEY_CLASSES_ROOT\\Directory\\Background\\shell\\Encrypt File\\command";
            if (Registry.GetValue(kName, null, null) == null)
            {
                key = Registry.ClassesRoot.CreateSubKey("Directory\\Background\\shell\\Encrypt File\\command");
                key.SetValue("", "C:\\Program Files (x86)\\Data Shield\\FileEncryption.exe");
                key.Close();
            }

            kName = "HKEY_CLASSES_ROOT\\Directory\\Background\\shell\\Decrypt File\\command";
            if (Registry.GetValue(kName, null, null) == null)
            {
                key = Registry.ClassesRoot.CreateSubKey("Directory\\Background\\shell\\Decrypt File\\command");
                key.SetValue("", "C:\\Program Files (x86)\\Data Shield\\FileDecryption.exe");
                key.Close();
            }
        }

        /* Gets the Hardware Identification of the current system. This is done by
         * first getting the CPU ID via querying 'Win32_Processor' and grabbing
         * the Processer ID. Then it gets the serial numnber of the logical disk (C:).
         * Finally, it combines the CPU ID and serial number, forming the HWID.
         * 
         * Param: None.
         * Return: String - the HWID of the current computer system.
         */
        public String HWID()
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

            return hwid = cpu + hdd;
        }

        /* Gets the name of the computer by using the 'Environment' class.
         * 
         * Param: None.
         * Return: String - name of the computer.
         */
        public String ComputerName()
        {
            return Environment.MachineName;
        }

        /* Gets the actual current logged in user, instead of return the inherited user of the 
         * parent process. This is done by querying 'Win32_Process' and getting the 'explorer.exe'
         * process and invoking the 'GetOwner' method to obtain the logged in user name.
         * 
         * Param: None.
         * Return: String - current logged in username.
         */
        public String Username()
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

        /* Produces a string that acts as the authentication information for file protection.
         * This is done by getting the current logged in username and HWID of the system.
         * 
         * Param: None.
         * Return: String - the authentication information.
         */
        public String AuthInfo()
        {
            return Username() + " " + HWID();
        }

        /* Gets the volume number of a given drive. This is done by getting the drive's ID,
         * and querying the assocaitor 'Win32_DiskDriveToDiskPartition' with the given ID.
         * It will then perform a partition search, matching with the device ID allowing us
         * to obtain the given drives Logical Volume Number.
         * 
         * Param: ManagementObject - the drive to get the volume number.
         * Return: String - the volume number of the drive.
         */
        public String GetVolumeNumber(ManagementObject drive)
        {
            String volumeNumber = null;

            String antecedent = drive["DeviceID"].ToString();
            antecedent = antecedent.Replace(@"\", "\\");
            String query2 = "ASSOCIATORS OF {Win32_DiskDrive.DeviceID='" + antecedent + "'} WHERE AssocClass = Win32_DiskDriveToDiskPartition";
            using (ManagementObjectSearcher partitionSearch = new ManagementObjectSearcher(query2))
            {
                foreach (ManagementObject part in partitionSearch.Get())
                {
                    String query3 = "ASSOCIATORS OF {Win32_DiskPartition.DeviceID='" + part["DeviceID"] + "'} WHERE AssocClass = Win32_LogicalDiskToPartition";
                    using (ManagementObjectSearcher logicalpartitionsearch = new ManagementObjectSearcher(query3))
                        foreach (ManagementObject logicalpartition in logicalpartitionsearch.Get())
                            volumeNumber = logicalpartition["DeviceID"].ToString();
                }
            }
            return volumeNumber;
        }

        /* Encrypts any text that is passed to it. It uses AES encryption with a salt,
         * key and IV. More detail is given within the method.
         * 
         * Param: String - clear text.
         * Return: String - encrypted text.
         */
        public String EncryptText(String clearText)
        {
            // Setting decryption password.
            String password = ConfigurationManager.AppSettings["encryptpassword"];

            // Setting salt in a byte array.
            byte[] salt = new byte[] { 0x029, 0x33, 0x71, 0x18, 0x44, 0x76, 0x33, 0x48 };

            // Setting number of iterations to 1756.
            const int iterations = 1756;

            // Encodes the clear text by getting the bytes and storing in a btye array.
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);

            // Initialise AES object, setting key, IV and mode.
            AesManaged aes = new AesManaged();
            aes.BlockSize = aes.LegalBlockSizes[0].MaxSize;
            aes.KeySize = aes.LegalKeySizes[0].MaxSize;
            Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(password, salt, iterations);
            aes.Key = key.GetBytes(aes.KeySize / 8);
            aes.IV = key.GetBytes(aes.BlockSize / 8);
            aes.Mode = CipherMode.CBC;
            ICryptoTransform transform = aes.CreateDecryptor(aes.Key, aes.IV);

            // Using a Memory Stream to decrypt text via the use of a Crypto Stream and AES encryptor method.
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(clearBytes, 0, clearBytes.Length);
                    cs.Close();
                }

                // Converting from bytes to string.
                clearText = Convert.ToBase64String(ms.ToArray());
            }
            return clearText;
        }

        /* A method that creates a File System Watcher for a given drive with the objective
         * of stopping the copying of any active protected file(s) on the system. This is done by
         * first watching for any new files being created that have a specific file extension, defined 
         * in the extensions List. When created, it will raise an event and trigger the 'OnCreated' 
         * method. From here, the file is copied to a temporary folder, the contents is read and a
         * PNG image is made containing the text using the 'IFilterTextReader'. The image is then
         * compared against all the 'Sample' images using the AForge.NET Imaging library, an
         * Artificial Intelligence library with image processing routines and filters. If the similarity
         * threshold 0.85% or above, it is considered as a copy attempt and the file is closed, deleted
         * and the event is logged with an administrator being informed. This works for most file formats.
         * A breakdown of each section, where necessary, is given below.
         * 
         * A DISCLAIMER, 'IFilterTextReader' is under the The Code Project Open License (CPOL) 1.02. 
         * My usage is in compliance with the license. All credits go to Kees van Spelde.
         * URL Link: https://github.com/Sicos1977/IFilterTextReader
         * 
         * A DISCLAIMER, 'AForge.NET' Framework is published under LGPL-3.0-only or LGPL-3.0-or-later license. 
         * I have taken the required steps to validate my use of this library. All credits go to AForge.NET
         * Framework. I do not take credit for any of it's code or methodology.
         * URL Link: http://www.aforgenet.com/framework/license.html
         */
        public FileSystemWatcher Watcher(String drive)
        {
            List<String> extensions = new List<String> { "doc", "docx", "docm", "txt", "xlsx", "ppt", "pptx", "pdf" };
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = drive;
            watcher.IncludeSubdirectories = true;
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                                     | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            watcher.Filter = "*.*";
            watcher.Created += new FileSystemEventHandler(OnCreated);
            watcher.EnableRaisingEvents = true;

            void OnCreated(object source, FileSystemEventArgs e)
            {
                Thread.Sleep(1000);

                // Checks if the newly created file has an extension that matches one in the extension list.
                if (extensions.Contains(e.FullPath.Split('.').Last()) && !e.FullPath.Split('\\').Last().Contains("~$"))
                {
                    try
                    {
                        if (!File.Exists("C:\\Users\\" + Username() + "\\AppData\\Roaming\\Data Shield\\(ds)" + e.FullPath.Split('\\').Last()))
                        {
                            // Makes a copy of the newly created file with '(ds)' at the front. This is to ensure we can read it due to the original file
                            // potentially being used by another process. The file contents is then read using 'IFilter'.
                            File.Copy(e.FullPath, "C:\\Users\\" + Username() + "\\AppData\\Roaming\\Data Shield\\(ds)" + e.FullPath.Split('\\').Last());
                            TextReader reader = new FilterReader("C:\\Users\\" + Username() + "\\AppData\\Roaming\\Data Shield\\(ds)" + e.FullPath.Split('\\').Last());
                            using (reader)
                            {
                                // Removes any new lines and indentations, allowing for all outputs to be the same text format.
                                String text = Regex.Replace(reader.ReadToEnd(), @"\t|\n|\r", "");

                                // Creates a PNG image of the text from the file.
                                MakeImage(text);

                                // Using the AForge.Imaging class, compares the newly created file contents image with all the images
                                // stored in the 'Sample' folder, being all active protected files. If it gets a similarity threshold
                                // of more than 0.85%, it kills the parent processes, logs the copy attempt and deletes the file.
                                if (CompareImage())
                                {
                                    Task.Run(() => MessageBox.Show("An attempt to copy a protected file has been identified. This has been logged and an administrator has been notified."));
                                    KillProcesses();
                                    LogCopyAttempt();
                                    Thread.Sleep(1000);
                                    File.Delete(e.FullPath);
                                }
                            }
                            // Deletes the temporary copy made of the file and clears the 'Temp' folder.
                            File.Delete("C:\\Users\\" + Username() + "\\AppData\\Roaming\\Data Shield\\(ds)" + e.FullPath.Split('\\').Last());
                            ClearTemp();
                        }
                    }
                    catch (Exception)
                    {
                        // If there is an error.
                        //Task.Run(() => MessageBox.Show(ex.Source));
                    }
                }
            }

            /* Creates an image of the text from the recently created file, making one or more
             * based on how many sample (active protected file) images there are. This will create multiple
             * images based on the size dimensions of each sample image as only imagesas of the same size can
             * be compared together. More details are given below.
             * 
             * Param: String - text to be drawn to image.
             * Return: None.
             */
            void MakeImage(String text)
            {
                // Get all the PNG samples files from the 'Sample' folder.
                DirectoryInfo di = new DirectoryInfo("C:\\Users\\" + Username() + "\\AppData\\Roaming\\Data Shield\\Samples\\");
                FileInfo[] samples = di.GetFiles("*.png");

                if (samples.Length > 0)
                {
                    // For each sample image, create a new image with the new text using the sample's
                    // size dimensions. Save the output in the 'Temp' folder.
                    foreach (var sample in samples)
                    {
                        Bitmap img = new Bitmap(sample.FullName);
                        Bitmap bitmap = new Bitmap(1, 1);
                        Font font = new Font("Arial", 8, FontStyle.Regular, GraphicsUnit.Pixel);
                        Graphics graphics = Graphics.FromImage(bitmap);
                        bitmap = new Bitmap(bitmap, new Size(img.Width, img.Height));
                        graphics = Graphics.FromImage(bitmap);
                        graphics.Clear(Color.White);
                        graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
                        graphics.DrawString(text, font, new SolidBrush(Color.FromArgb(0, 0, 0)), 0, 0);
                        graphics.Flush();
                        graphics.Dispose();
                        bitmap.Save("C:\\Users\\" + Username() + "\\AppData\\Roaming\\Data Shield\\Temp\\" + sample.Name);
                        img.Dispose();
                        bitmap.Dispose();
                    }
                }
                else
                {
                    //MessageBox.Show("No samples.");
                }
            }

            /* Compares the temp images (being the newly created files) against the sample
             * images (active protected files) using AForge.Net Imaging. Details on how it works
             * are given below.
             * 
             * Param: None.
             * Return: Boolean - if the temp image had a similarity rate of 85% or higher.
             */
            Boolean CompareImage()
            {
                // Get all the sample PNG images from the Sample folder.
                DirectoryInfo di = new DirectoryInfo("C:\\Users\\" + Username() + "\\AppData\\Roaming\\Data Shield\\Samples\\");
                FileInfo[] samples = di.GetFiles("*.png");

                // Cycle through each sample image to be tested against temp image(s).
                foreach (var sample in samples)
                {
                    // Get all the temp PNG images from the Temp folder.
                    di = new DirectoryInfo("C:\\Users\\" + Username() + "\\AppData\\Roaming\\Data Shield\\Temp\\");
                    FileInfo[] temps = di.GetFiles("*.png");

                    // Cycle through each temp image to be compared against the sample image.
                    foreach (var temp in temps)
                    {
                        // Loads both the sample and temp images as bitmap images.
                        Bitmap imageOne = new Bitmap(sample.FullName);
                        Bitmap imageTwo = new Bitmap(temp.FullName);

                        // Allows for an almost double width size for either less than or more compared to the sample dimensions.
                        if (imageOne.Width > imageTwo.Width / 1.75 && imageOne.Width < imageTwo.Width * 1.75)
                        {
                            Bitmap newBitmap1 = ChangePixelFormat(new Bitmap(imageOne), PixelFormat.Format24bppRgb);
                            Bitmap newBitmap2 = ChangePixelFormat(new Bitmap(imageTwo), PixelFormat.Format24bppRgb);
                            
                            // Setup the AForge library, with the threshold being at 85% similarity.
                            ExhaustiveTemplateMatching tm = new ExhaustiveTemplateMatching(0.85f);

                            try
                            {
                                // Perform the comparison of the two images.
                                var results = tm.ProcessImage(newBitmap1, newBitmap2);

                                // If no results, indicates the similarity was below 85% and deemed as a non-copy attempt
                                // and returns false.
                                if (results.Length > 0)
                                {
                                    // If one or more results come back, indicates the file(s) had a 85% or more similarity,
                                    // meaning it was a copy of a protected file(s) and returns true.
                                    //MessageBox.Show(results[0].Similarity.ToString());
                                    imageOne.Dispose();
                                    imageTwo.Dispose();
                                    return true;
                                }
                            }
                            catch (Exception ex)
                            {
                                // In the case an error occurs.
                                MessageBox.Show(ex.Message);
                            }
                        }
                        imageOne.Dispose();
                        imageTwo.Dispose();
                    }
                }
                return false;
            }

            /* Changes the pixel format of the input image.
             * 
             * Param: Bitmap - input image. PixelFormat - new image format.
             * Return: new bitmap image.
             */
            Bitmap ChangePixelFormat(Bitmap inputImage, PixelFormat newFormat)
            {
                return (inputImage.Clone(new Rectangle(0, 0, inputImage.Width, inputImage.Height), newFormat));
            }

            /* Kills all the known processes used by protected files.
             * 
             * Param: None.
             * Return: None.
             */
            void KillProcesses()
            {
                var processess = Process.GetProcesses();
                foreach (var process in processess)
                {
                    if (process.ProcessName.Equals("WINWORD") || process.ProcessName.Equals("POWERPNT")
                        || process.ProcessName.Equals("EXCEL") || process.ProcessName.Equals("notepad")
                        || process.ProcessName.Equals("MSPUB"))
                    {
                        process.Kill();
                    }
                }
            }

            /* Logs the copy of protected file attempt to the server, which notifies an admin and logs
             * it in the database being the file name, user, date and time.
             * 
             * Param: None.
             * Return: None.
             */
            void LogCopyAttempt()
            {
                DirectoryInfo di = new DirectoryInfo("C:\\Users\\" + Username() + "\\AppData\\Roaming\\Data Shield\\Samples\\");
                String file = di.GetFiles().First().Name.Replace(".png", "");

                using (WebClient client = new WebClient())
                {
                    ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
                    client.UploadString(ConfigurationManager.AppSettings["logcopyattempt"], file + "|" + Username() + "|" + ComputerName());
                }
            }

            /* Clears the Temp folder. This is done after the newly created file has been analysed.
             * 
             * Param: None.
             * Return: None.
             */
            void ClearTemp()
            {
                DirectoryInfo di = new DirectoryInfo("C:\\Users\\" + Username() + "\\AppData\\Roaming\\Data Shield\\Temp\\");

                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
            }

            return watcher;
        }

        /* Produces a hash of the file, the using the SHA256 algorithm which
         * computes a hash result in an byte array. Then the byte array is converted
         * to a string, providing a hash String.
         * 
         * Param: String - path to file.
         * Return: String - hash of file.
         */
        public String HashFile(String filePath)
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

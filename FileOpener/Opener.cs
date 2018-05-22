using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace FileOpener
{
    class Opener
    {
        /* The main method for opening a file once it has been decrypted. First, it decrypts the 'open file.txt' text document in memory
         * and reads the last line, being the most recent file to be opened. It then starts a new process opening that file, and
         * adds an Event Handler for when it exits. When the process is closed, it deletes the image snapshot of the file for the AI
         * image comparison, then uses a Named Pipe Client to inform the Data Monitor process that the file has been closed and to 
         * remove it from the list of open protected files.
         * 
         * Param: None.
         * Return: None.
         */
        static void Main()
        {
            String filePath = DecryptText(File.ReadLines("C:\\Users\\" + Username() + "\\AppData\\Roaming\\Data Shield\\Open Files.txt").Last());
            Process process = new Process();
            process.Exited += new EventHandler(Exited);
            process.StartInfo.FileName = filePath;
            process.EnableRaisingEvents = true;
            process.Start();

            void Exited(object sender, EventArgs e)
            {
                File.Delete("C:\\Users\\" + Username() + "\\AppData\\Roaming\\Data Shield\\Samples\\" + filePath.Split('\\').Last() + ".png");

                NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", "FileMonitor");
                if (pipeClient.IsConnected != true)
                {
                    pipeClient.Connect();
                }
                StreamWriter sw = new StreamWriter(pipeClient);
                sw.WriteLine("closed|" + filePath);
                sw.Flush();
                pipeClient.Close();

                Environment.Exit(1);
            }
            Application.Run();
        }

        /* Decrypts the open file document in memory. It uses AES encryption with a salt,
         * key and IV. More detail is given within the method.
         * 
         * Param: String - encrypted text.
         * Return: String - decrypted text.
         */
        public static String DecryptText(String encryptedText)
        {
            // Setting decryption password.
            String password = ConfigurationManager.AppSettings["encryptionpassword"];

            // Setting salt in a byte array.
            byte[] salt = new byte[] { 0x029, 0x33, 0x71, 0x18, 0x44, 0x76, 0x33, 0x48 };

            // Setting number of iterations to 1756.
            int iterations = 1756;

            // Removes spaces and converting into bytes.
            encryptedText = encryptedText.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(encryptedText);

            // Initialise AES object, setting key, IV and mode.
            AesManaged aes = new AesManaged();
            aes.BlockSize = aes.LegalBlockSizes[0].MaxSize;
            aes.KeySize = aes.LegalKeySizes[0].MaxSize;
            Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(password, salt, iterations);
            aes.Key = key.GetBytes(aes.KeySize / 8);
            aes.IV = key.GetBytes(aes.BlockSize / 8);
            aes.Mode = CipherMode.CBC;
            ICryptoTransform transform = aes.CreateDecryptor(aes.Key, aes.IV);

            // Using a Memory Stream to decrypt text via the use of a Crypto Stream and AES decryptor method.
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(cipherBytes, 0, cipherBytes.Length);
                    cs.Close();
                }

                // Converting from bytes to string.
                encryptedText = Encoding.Unicode.GetString(ms.ToArray());
            }
            return encryptedText;
        }

        /* Gets the actual current logged in user, instead of return the inherited user of the 
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
    }
}

using System;
using System.Configuration;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Security.Cryptography;

namespace FileDecryption
{
    class Utilities
    {
        /* Gets current logged in username and system name. This is done using a Named Pipe
         * Client that communicates with the Data Monitor Process, which provides the requested
         * information.
         * 
         * Param: None.
         * Return: String - current logged in user and system.
         */
        public String GetCurrentUserAndSystem()
        {
            String info;

            NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", "AuthInfo");
            if (pipeClient.IsConnected != true)
            {
                pipeClient.Connect();
            }

            StreamReader sr = new StreamReader(pipeClient);
            info = sr.ReadLine();
            pipeClient.Dispose();
            pipeClient.Close();
            return info;
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

        /* Checks if the current user and system are authorised to use the protected file
         * before decryption. It does this by calling the a method to obtain user and system
         * information, then hashes the protected file and sends the infomation to the server.
         * The server than checks against the database using the file hash as the key to check
         * if the user and system are in the authroised field for that particular file.
         * 
         * Param: String - file path to encrypted file.
         * Return: Boolean - true if authorised. false if not.
         */
        public Boolean CheckAuthorisation(String filePath)
        {
            String result = null;

            String info = GetCurrentUserAndSystem();

            String ehash = HashFile(filePath);
            
            using (WebClient client = new WebClient())
            {
                ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
                result = client.UploadString(ConfigurationManager.AppSettings["checkauthorisation"], info + " " + ehash);
            }
            if (result.Equals("authorised"))
            {
                return true;
            }
            return false;
        }

        /* Decrypts the file using AES encryption with a salt, key and IV. 
         * More detail is given within the method.
         * 
         * Param: String - source location of file to encrypt.
         *        String - destination output for encrypted file.
         *        String - password for decryption.
         * Return: None.
         */
        public void DecryptFile(string source, string destination, string password)
        {
            // Setting salt in a byte array.
            byte[] salt = new byte[] { 0x029, 0x33, 0x71, 0x18, 0x44, 0x76, 0x33, 0x48 };

            // Setting number of iterations to 1756.
            const int iterations = 1756;

            // Initialise AES object, setting key, IV and mode.
            AesManaged aes = new AesManaged();
            aes.BlockSize = aes.LegalBlockSizes[0].MaxSize;
            aes.KeySize = aes.LegalKeySizes[0].MaxSize;
            Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(password, salt, iterations);
            aes.Key = key.GetBytes(aes.KeySize / 8);
            aes.IV = key.GetBytes(aes.BlockSize / 8);
            aes.Mode = CipherMode.CBC;
            ICryptoTransform transform = aes.CreateDecryptor(aes.Key, aes.IV);

            // Using file stream to output crypto stream transform decryption to a file.
            using (FileStream dest = new FileStream(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                using (CryptoStream cryptoStream = new CryptoStream(dest, transform, CryptoStreamMode.Write))
                {
                    try
                    {
                        using (FileStream src = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            src.CopyTo(cryptoStream);
                        }
                    }
                    catch (CryptographicException exception)
                    {
                        if (exception.Message == "Padding is invalid and cannot be removed.")
                            throw new ApplicationException(null, exception);
                        else
                            throw;

                    }
                }
            }
        }

        /* Logs last access to the protected file. This is done by getting the current
         * logged in user and system, getting a hash of the decrypted file then sending 
         * that information to the server. It then gets timestamped and updates that file's
         * last accessed record.
         * 
         * Param: String - path to file.
         * Return: None.
         */
        public void LogAccess(String filePath)
        {
            String info = GetCurrentUserAndSystem().Split(' ').First() + "|" + Environment.MachineName;

            String dhash = HashFile(filePath);

            using (WebClient client = new WebClient())
            {
                ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
                client.UploadString(ConfigurationManager.AppSettings["logfileaccess"], info + "|" + dhash);
            }
        }

        /* Notifies the Data Monitor process that a new file has been decrypted and to
         * open the file and monitor it. This is done through a Named Pipe Client,
         * passing through the 'opened' flag, along with the file path.
         * 
         * Param: String - path to file.
         * Return: None.
         */
        public void FileDecryptedCall(String file)
        {
            NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", "FileMonitor");
            if (pipeClient.IsConnected != true)
            {
                pipeClient.Connect();
            }

            StreamWriter sw = new StreamWriter(pipeClient);
            sw.WriteLine("opened|" + file);
            sw.Flush();
            pipeClient.Close();
        }
    }
}

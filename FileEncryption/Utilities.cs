using System;
using System.Configuration;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace FileEncryption
{ 
    class Utilities
    {
        /* Gets all the users that have called into the Data Shield server and are registered.
         * It obtains a list of them and arranges them alphabetically.
         * 
         * Param: None.
         * Return: List - of all users.
         */
        public List<String> GetUsers()
        {
            List<String> users = new List<String>();
            String listOfUsers;

            using (WebClient client = new WebClient())
            {
                ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
                listOfUsers = client.DownloadString(ConfigurationManager.AppSettings["obtainusers"]);
            }
            foreach (String user in listOfUsers.Split())
            {
                users.Add(user);
            }

            users.Sort();
            return users;
        }

        /* Gets all the systems that have called into the Data Shield server and are registered.
         * It obtains a list of them and arranges them alphabetically.
         * 
         * Param: None.
         * Return: List - of all systems.
         */
        public List<String> GetSystems()
        {
            List<String> systems = new List<String>();
            String listOfSystems;

            using (WebClient client = new WebClient())
            {
                ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
                listOfSystems = client.DownloadString(ConfigurationManager.AppSettings["obtainsystems"]);
            }
            foreach (String user in listOfSystems.Split())
            {
                systems.Add(user);
            }

            systems.Sort();
            return systems;
        }

        /* Gets current logged in username and system name. This is done using a Named Pipe
         * Client that communicates with the Data Monitor process, which provides the requested
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

            return info.Split(' ').First() + "@" + Environment.MachineName;
        }

        /* Formats the list of authorised users.
         * 
         * Param: ListBox - list of authorised users.
         * Return: String - authorised users.
         */
        public String FormatUsers(ListBox users)
        {
            String authedUsers = null;

            foreach (var user in users.Items)
            {
                authedUsers += user.ToString() + " ";
            }
            return authedUsers;
        }

        /* Formats the list of authorised systems. Each system comes
         * paired with its HWID, which is hidden from the user and only
         * the system name is shown to the user.
         * 
         * Params: List - list of all systems. ListBox - list of authorised systems.
         * Return: String - authorised systems.
         */
        public String FormatSystems(List<String> systems, ListBox filteredSystems)
        {
            String authedSystems = null;

            foreach (var filteredSystem in filteredSystems.Items)
            {
                foreach (String system in systems)
                {
                    if (system.Split('|')[0].Equals(filteredSystem.ToString()))
                    {
                        // Only adding the HWID of the system, not the system name.
                        authedSystems += system.ToString().Split('|').Last() + " ";
                    }
                }
            }
            return authedSystems;
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

        /* Checks if the file is already encrypted or not. This is done by performing a
         * hash on the file, sending to to the server for it then to be checked up against
         * the protected file database table. If a match is found, this indicates the file
         * is already encrypted.
         * 
         * Param: String - path to file.
         * Return: Boolean - true if file is already encrypted. false if not.
         */
        public Boolean CheckFileState(String filePath)
        {
            String hash = HashFile(filePath);
            String response = null;

            using (WebClient client = new WebClient())
            {
                ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
                response = client.UploadString(ConfigurationManager.AppSettings["checkfilestate"], hash);
            }
            if (response.Equals("encrypted"))
            {
                return false;
            }
            return true;
        }

        /* Updates the protected file table within the database, adding the newly encrypted file.
         * This is done by sending the file name, decrypted and encrypted hash of the file and the
         * authorised users and systems to the server for it then to be processed and added to the
         * database.
         * 
         * Params: String - name of file. String - hash of file before encrypted.
         *         String - hash of file after encryption. String - authorised users.
         *         String - authorised systems.
         * Return: None.
         */
        public void UpdateProtectedFiles(String fileName, String decryptedHash, String encryptedHash, String authedUsers, String authedSystems)
        {
            String protectedFile = fileName.Split('\\').Last() + "@" + decryptedHash + "@" + encryptedHash + "@" + authedUsers + "@" + authedSystems 
                + "@" + GetCurrentUserAndSystem();

            using (WebClient client = new WebClient())
            {
                ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
                client.UploadString(ConfigurationManager.AppSettings["updateprotectedfiles"], protectedFile);
            }
        }

        /* Encrypts the file using AES encryption with a salt, key and IV. 
         * More detail is given within the method.
         * 
         * Param: String - source location of file to encrypt.
         *        String - destination output for encrypted file.
         *        String - password for decryption.
         * Return: None.
         */
        public void EncryptFile(String source, String destination, String password)
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
            ICryptoTransform transform = aes.CreateEncryptor(aes.Key, aes.IV);

            // Using file stream to output crypto stream transform encryption to a file.
            using (FileStream dest = new FileStream(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                using (CryptoStream cryptoStream = new CryptoStream(dest, transform, CryptoStreamMode.Write))
                {
                    using (FileStream src = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        src.CopyTo(cryptoStream);
                    }
                }
            }
        }
    }
}

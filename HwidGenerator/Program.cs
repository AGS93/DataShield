using System;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace HwidGenerator
{
    class Program
    {
        /* Gets the Hardware Identification of the current system. This is done by
         * first getting the CPU ID via querying 'Win32_Processor' and grabbing
         * the Processer ID. Then it gets the serial numnber of the logical disk (C:).
         * Finally, it combines the CPU ID and serial number together and hashes it, 
         * forming the HWID and sets the result to clipboard memory.
         * 
         * Param: None.
         * Return: None.
         */
        [STAThread]
        static void Main()
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

            Clipboard.SetText(hwid);
            MessageBox.Show("HWID has been generated and set to clipboard memory.");

            Environment.Exit(1);
        }

        /* Hashes the password attempt inputted by the user. This is done by
         * converting the password string into a byte array, then using the SHA512 Managed 
         * class to compute the hash using its algorithm. It is then converted from byte to
         * a String and the salt is added to the end.
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
    }
}

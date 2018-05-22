using System;
using System.Management;

namespace DeviceDumper
{
    class Dumper
    {
        /* The main method for this application. The purpose is to get a list of all the
         * current disk drive devices connected to the system. This is done by querying
         * 'Win32_DiskDrive' via a Management Object Searcher. It will then print
         * out to console the name, serial number and signature of each drive.
         * 
         * Param: None.
         * Return: None.
         */
        static void Main()
        {
            ManagementObjectSearcher drives = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
            foreach (ManagementObject drive in drives.Get())
            {
                Console.WriteLine(drive.Properties["Caption"].Value);
                Console.WriteLine(drive.Properties["SerialNumber"].Value);
                Console.WriteLine(drive.Properties["Signature"].Value);
            }
            Console.ReadLine();
        }
    }
}

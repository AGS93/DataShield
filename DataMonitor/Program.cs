using System;
using System.Threading;
using System.Windows.Forms;

namespace DataMonitor
{
    static class Program
    {
        public static Monitor monitor = new Monitor();
        public static Utilities utilities = new Utilities();

        /* The main entry point for the data monitor application. Here is where the
         * core functions are controlled at a high-level. Each function that would use
         * a reasonable amount of resources is allocated its own Thread. It also gives
         * a clear and effective approach of being able to turn each feature on and off.
         * More details are given below.
         * 
         * Param: None.
         * Return: None.
         */
        [STAThread]
        static void Main()
        {
            /* Checks if the Data Monitor service is installed. If not, indicates fresh installtion and 
             * performs a set of tasks to set up the environment for the Data Shield application to function.
             * More details are given within the method.
             */
            if (utilities.CheckService())
                Environment.Exit(1);

            /* Checks if the 'Encrypt File' and 'Decrypt File' keys are present in context menu and 
             * if not, will add to the registry. More details are given within the method.
             */
            utilities.CheckContextMenu();

            /* Checks if the Data Shield's directories are present. If not, will create them.
             * More details are given within the method.
             */
            utilities.CheckDirectories();
            
            /* Obtains latest edition of the process whitelist from the database.
             * More details are given within the method.
             */
            monitor.ObtainWhitelist();

            /* Obtains latest edition of the authorised Removable Media Devices from database.
             * More details are given within the method.
             */
            monitor.ObtainAuthorisedDevices();

            /* Calls in to the server with current User and System infromation to be checked
             * and updated if needed. More details are given within the method.
             */
            monitor.CallIn();

            /* Checks that the proxy settings for the current user are set and active.
             * More details are given within the method.
             */
            Thread sp = new Thread(utilities.SetProxy);
            sp.Start();

            /* Starts Named Pipe Server for incoming requests for user and system information.
             * More details are given within the method.
             */
            Thread ai = new Thread(monitor.AuthInfo);
            ai.Start();

            /* Starts Named Pipe Server for incoming notifications upon a file being decrypted and
             * closed. More details are given within the method.
             */
            Thread fm = new Thread(monitor.FileMonitor);
            fm.Start();

            /* Starts monitoring the processes on the system against a set whitelist, terminating
             * any that are not listed. More details are given within the method.
             */
            Thread tr = new Thread(monitor.ProcessMonitor);
            tr.Start();

            /* Starts monitoring for unauthorised Removable Media Devices on the system, ejecting if
             * unathorised. More details are given within the method.
             */
            Thread dm = new Thread(monitor.DeviceMonitor);
            dm.Start();

            Application.Run();
        }
    }
}
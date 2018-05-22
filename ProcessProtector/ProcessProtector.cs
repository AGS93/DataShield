using System;
using System.Configuration;
using System.Diagnostics;
using System.Management;
using System.Net;
using System.ServiceProcess;
using System.Threading;

namespace ProcessProtector
{
    public partial class ProcessProtector : ServiceBase
    {
        public ProcessProtector()
        {
            InitializeComponent();
        }

        /* An override for the 'OnStart' method that creates a new Thread for the 'Checker'
         * method, which checks that the Data Monitor process is running.
         */
        protected override void OnStart(string[] args)
        {
            Thread c = new Thread(Checker);
            c.Start();
        }

        /* An override for the 'OnStop' method that gets the user and system name to then
         * log the service termination to the Data Monitor server.
         */
        protected override void OnStop()
        {
            // Get current user and system.
            String user = GetUser();
            String system = GetSystem();

            // Log service termination.
            using (WebClient client = new WebClient())
            {
                ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
                client.UploadString(ConfigurationManager.AppSettings["logservicetermination"], user + "|" + system);
            }
        }

        /* Gets the actual current logged in user, instead of returning the inherited user of the 
         * parent process. This is done by querying 'Win32_Process' and getting the 'explorer.exe' 
         * processes, then invoking the 'GetOwner' method to obtain the logged in user.
         * 
         * Param: None.
         * Return: String - current logged in username.
         */
        public String GetUser()
        {
            var query = new ObjectQuery("SELECT * FROM Win32_Process WHERE Name = 'explorer.exe'");
            var explorerProcesses = new ManagementObjectSearcher(query).Get();

            foreach (ManagementObject mo in explorerProcesses)
            {
                String[] ownerInfo = new String[2];
                mo.InvokeMethod("GetOwner", (object[])ownerInfo);

                return ownerInfo[0];
            }
            return null;
        }

        /* Gets the name of the computer by using the 'Environment' class.
         * 
         * Param: None.
         * Return: String - name of the computer.
         */
        public String GetSystem()
        {
            return Environment.MachineName;
        }

        /* Checks if the Data Monitor process 'DataMonitor' is currently active. If not
         * will use the 'ApplicationLoader' class and invoke the 'StartProcessAndBypassUAC'
         * method to start the process as a 'SYSTEM' user and bypass UAC. It does this check
         * every 0.5 seconds to ease use of system resources.
         * 
         * Param: None.
         * Return: None.
         */
        public void Checker()
        {
            while (true)
            {
                if (Process.GetProcessesByName("DataMonitor").Length == 0)
                {
                    string applicationName = "C:\\Program Files (x86)\\Data Shield\\DataMonitor.exe";
                    ApplicationLoader.PROCESS_INFORMATION procInfo;
                    ApplicationLoader.StartProcessAndBypassUAC(applicationName, out procInfo);
                }
                Thread.Sleep(500);
            }
        }
    }
}

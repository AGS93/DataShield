using System;
using System.Configuration.Install;
using System.Reflection;
using System.ServiceProcess;

namespace ProcessProtector
{
    static class Program
    {
        /* The main method for the process protector. You can either install/uninstall the service by
         * providing the arguments '-install' or '-uninstall'. If no arguments ar given, it will run the
         * service like normal.
         * 
         * Param: String array - if to install or uninstall
         * Return: None.
         */
        public static void Main(string[] args)
        {
            if (Environment.UserInteractive)
            {
                if (args.Length > 0)
                {
                    switch (args[0])
                    {
                        case "-install":
                            {
                                ManagedInstallerClass.InstallHelper(new string[] { Assembly.GetExecutingAssembly().Location });
                                break;
                            }
                        case "-uninstall":
                            {
                                ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetExecutingAssembly().Location });
                                break;
                            }
                    }
                }
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[] 
                {
                    new ProcessProtector()
                };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}

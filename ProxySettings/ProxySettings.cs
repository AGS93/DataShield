using System;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace ProxySettings
{
    class ProxySettings
    {
        /* The main methof for setting the proxy settings for the current user. This is done
         * by adding/setting values such as the proxy server IP address, an override address,
         * then enabling it. Finally, the proxy options are refreshed so it will take effect.
         * 
         * Param: None.
         * Return: None.
         */
        static void Main()
        {
            String userRoot = "HKEY_CURRENT_USER";
            String subkey = "Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings";
            String keyName = userRoot + "\\" + subkey;

            Registry.SetValue(keyName, "ProxyServer", "192.168.52.136:3128");
            Registry.SetValue(keyName, "ProxyOverride", "192.168.52.128");
            Registry.SetValue(keyName, "ProxyEnable", 1);

            InternetSetOption(IntPtr.Zero, INTERNET_OPTION_SETTINGS_CHANGED, IntPtr.Zero, 0);
            InternetSetOption(IntPtr.Zero, INTERNET_OPTION_REFRESH, IntPtr.Zero, 0);
        }

        [DllImport("wininet.dll")]
        public static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int dwBufferLength);
        public const int INTERNET_OPTION_SETTINGS_CHANGED = 39;
        public const int INTERNET_OPTION_REFRESH = 37;
    }
}

using System;
using System.Windows.Forms;

namespace FileEncryption
{
    class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Encryptor());
        }
    }
}

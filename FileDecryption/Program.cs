using System;
using System.Configuration;
using System.IO;
using System.Windows.Forms;

namespace FileDecryption
{
    class Program
    {
        private static OpenFileDialog ofd = new OpenFileDialog();
        private static Utilities utilities = new Utilities();

        /* The main method for this application. First it opens a File Dialog Window
         * for the user to choose the file they wish to decrypt. It will then check to
         * see if both the user and system is authorised to decrypt the selected file. 
         * If so, the file will be decrypted and last access will be logged on the database.
         * A Named Pipe Client will inform the Data Monitor process that the file has been
         * decrypted. More detail is given within the methods.
         * 
         * Param: None.
         * Return: None.
         */
        [STAThread]
        static void Main()
        {
            ofd.InitialDirectory = "C:\\";
            ofd.Filter = "All Files (*)|*";
            ofd.FileName = null;
            ofd.FilterIndex = 2;
            ofd.RestoreDirectory = true;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                if (utilities.CheckAuthorisation(ofd.FileName))
                {
                    DialogResult dialogResult = MessageBox.Show("If yes, the following applications:" + "\n\n - Skype\n - Skype for Business\n - Microsoft Messages \n - Outlook\n - Notepad\n - Wordpad\n\n" +
                        "Will be terminated instantly and further blocked while any protected file(s) are open.", "Warning!", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {
                        try
                        {
                            utilities.DecryptFile(ofd.FileName, ofd.FileName.Replace(" (encrypted).", "."), ConfigurationManager.AppSettings["encryptionpassword"]);
                            utilities.LogAccess(ofd.FileName.Replace(" (encrypted).", "."));
                            File.Delete(ofd.FileName);
                            utilities.FileDecryptedCall(ofd.FileName.Replace(" (encrypted).", "."));
                            MessageBox.Show("File Decrypted!");
                        }
                        catch
                        {
                            MessageBox.Show("Decryption Unsuccessful!");
                            return;
                        }
                    }
                    else if (dialogResult == DialogResult.No)
                    {
                        MessageBox.Show("Decryption Canceled.");
                    }
                }
                else
                {
                    MessageBox.Show("Either the user, or system is unauthorised to access this file.");
                }
            }
        }
    }
}

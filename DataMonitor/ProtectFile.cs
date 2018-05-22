using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using IFilterTextReader;

namespace DataMonitor
{
    class ProtectFile
    {
        String file = null;
        Utilities utilities = new Utilities();

        /* The Constructor method. This sets the path to the file, performs
         * an image snapshop of the file by calling the 'SnapshotFile' method
         * and then opens the file by calling the 'OpenFile' method.
         * 
         * Param: String - path to file.
         * Return: None.
         */
        public ProtectFile(String filePath)
        {
            file = filePath;
            SnapshotFile(filePath);
            OpenFile(filePath);
        }

        /* Performs a image snapshop of the file before it is opened to use as a sample for
         * the A.I image comparison when attempting to detect any copies of the file. This is done
         * by first reading the contents of the actual file, using the NuGet package 'IFilterTextReader',
         * a C# TextReader that gets text from different file formats through the IFilter interface.
         * Using the reader, obtains all the text within the document and removing all new lines and indentations
         * to ensure each document is read the same. Then using a Bitmap, Font and Graphics objects to
         * draw the contents onto a Bitmap image. Finally, the Bitmap image is saved as a PNG to the 
         * 'Samples' folder for future analysis if needed.
         * 
         * A DISCLAIMER, 'IFilterTextReader' is under the The Code Project Open License (CPOL) 1.02.
         * My usage is in compliance with the license. All credits go to Kees van Spelde.
         * URL Link: https://github.com/Sicos1977/IFilterTextReader
         */
        public void SnapshotFile(String filePath)
        {
            try
            {
                TextReader reader = new FilterReader(filePath);
                using (reader)
                {
                    var text = Regex.Replace(reader.ReadToEnd(), @"\t|\n|\r", "");
                    Bitmap bitmap = new Bitmap(1, 1);
                    Font font = new Font("Arial", 8, FontStyle.Regular, GraphicsUnit.Pixel);
                    Graphics graphics = Graphics.FromImage(bitmap);
                    int width = (int)graphics.MeasureString(text, font).Width;
                    int height = (int)graphics.MeasureString(text, font).Height;
                    bitmap = new Bitmap(bitmap, new Size(width, height));
                    graphics = Graphics.FromImage(bitmap);
                    graphics.Clear(Color.White);
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
                    graphics.DrawString(text, font, new SolidBrush(Color.FromArgb(0, 0, 0)), 0, 0);
                    graphics.Flush();
                    graphics.Dispose();
                    bitmap.Save("C:\\Users\\" + utilities.Username() + "\\AppData\\Roaming\\Data Shield\\Samples\\" + filePath.Split('\\').Last() + ".png");
                    bitmap.Dispose();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /* Opens the decrypted file. This is done by first adding the file path to the 'Open Files.txt'
         * document in an encrypted form. Then the 'FileOpener' application is lunched using the
         * 'CreateProcessAsUser' class so the file is ran as the logged in user and not 'SYSTEM',
         * being the inherited user from the Data Monitor process. This is due to issues with 'SYSTEM'
         * user processes not working correctly with Microsoft Office applications. The 'FileOpener' 
         * application will decrypt the 'Open Files.txt' document in memory, read the last line and 
         * open the file as the logged in user.
         * 
         * Param: String - path to file.
         * Return: None.
         */
        public void OpenFile(String filePath)
        {
            File.AppendAllText("C:\\Users\\" + utilities.Username() + "\\AppData\\Roaming\\Data Shield\\Open Files.txt", utilities.EncryptText(filePath) + Environment.NewLine);
            CreateProcessAsUser.ProcessAsUser.Launch("C:\\Program Files (x86)\\Data Shield\\FileOpener.exe");
        }

        /* Gets the file.
         * 
         * Param: None.
         * Return: String - path to file.
         */
        public String GetFile()
        {
            return file;
        }
    }
}

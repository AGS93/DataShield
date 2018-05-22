using System;
using System.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace FileEncryption
{
    public partial class Encryptor : Form
    {
        Utilities utilities;
        List<String> users, systems;

        /* Constructor method. Initialises the form and Utilities object. Next gets all
         * the users and systems from the Data Monitor server and updates the 'Users' and
         * 'Systems' List Boxes.
         */
        public Encryptor()
        {
            InitializeComponent();
            utilities = new Utilities();
            users = utilities.GetUsers();
            UpdateUsers();
            systems = utilities.GetSystems();
            UpdateSystems();
        }

        public void UpdateUsers()
        {
            foreach (String user in users)
            {
                if (user != "")
                {
                    lbUsers.Items.Add(user);
                }
            }
        }

        public void UpdateSystems()
        {
            foreach (String system in systems)
            {
                if (system != "")
                {
                    lbSystems.Items.Add(system.Split('|')[0]);
                }
            }
        }

        /* File encryption button click method. First presents the user with a
         * Open File Dialog window to select the file they wish to encrypt. Then
         * checks to ensure the file is not already encrypted, gets a hash before
         * encrypting, encrypts, then gets a hash of the file when encrypted. This
         * information is then send to the server to be updated to the protected
         * files database table. More details are given within the method.
         */
        private void btnEncryptor_Click(object sender, EventArgs e)
        {
            if (lbAuthUsers.Items.Count > 0 && lbAuthSystems.Items.Count > 0)
            {
                // Formats selected authorised users and systems.
                String authedUsers = utilities.FormatUsers(lbAuthUsers);
                String authedSystems = utilities.FormatSystems(systems, lbAuthSystems);

                // Presents an Open File Dialog window to the user to select
                // the file needed to be encrypted.
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.InitialDirectory = "C:\\";
                ofd.Filter = "All Files (*)|*";
                ofd.FileName = null;
                ofd.FilterIndex = 2;
                ofd.RestoreDirectory = true;

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    // Checks to ensure the file is not already encrypted.
                    if (utilities.CheckFileState(ofd.FileName))
                    {
                        // Gets hash of file before it is encrypted.
                        String decryptedHash = utilities.HashFile(ofd.FileName);

                        // Encrypts the file.
                        utilities.EncryptFile(ofd.FileName, ofd.FileName.Replace(".", " (encrypted)."), ConfigurationManager.AppSettings["encryptionpassword"]);

                        // Gets a hash of file once it has been encrypted.
                        String encryptedHash = utilities.HashFile(ofd.FileName.Replace(".", " (encrypted)."));

                        // Deletes the old decrypted file.
                        File.Delete(ofd.FileName);

                        // Updates protected files table with newly protected file information.
                        utilities.UpdateProtectedFiles(ofd.FileName, decryptedHash, encryptedHash, authedUsers, authedSystems);
                        MessageBox.Show("File Encrypted!");
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("File is already encrypted!");
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select both one or more authorised user(s) and system(s).");
            }
        }

        private void btnAddUser_Click(object sender, EventArgs e)
        {
            if (lbUsers.SelectedItem != null)
            {
                lbAuthUsers.Items.Add(lbUsers.SelectedItem);
                lbUsers.Items.Remove(lbUsers.SelectedItem);
            }
        }

        private void btnAddAllUsers_Click(object sender, EventArgs e)
        {
            lbAuthUsers.Items.AddRange(lbUsers.Items);
            lbUsers.Items.Clear();
        }

        private void btnRemoveUser_Click(object sender, EventArgs e)
        {
            if (lbAuthUsers.SelectedItem != null)
            {
                lbUsers.Items.Add(lbAuthUsers.SelectedItem);
                lbAuthUsers.Items.Remove(lbAuthUsers.SelectedItem);
            }
        }

        private void btnRemoveAllUsers_Click(object sender, EventArgs e)
        {
            lbUsers.Items.AddRange(lbAuthUsers.Items);
            lbAuthUsers.Items.Clear();
        }

        private void btnAddSystem_Click(object sender, EventArgs e)
        {
            if (lbSystems.SelectedItem != null)
            {
                lbAuthSystems.Items.Add(lbSystems.SelectedItem);
                lbSystems.Items.Remove(lbSystems.SelectedItem);
            }
        }

        private void btnAddAllSystems_Click(object sender, EventArgs e)
        {
            lbAuthSystems.Items.AddRange(lbSystems.Items);
            lbSystems.Items.Clear();
        }

        private void btnRemoveSystem_Click(object sender, EventArgs e)
        {
            if (lbAuthSystems.SelectedItem != null)
            {
                lbSystems.Items.Add(lbAuthSystems.SelectedItem);
                lbAuthSystems.Items.Remove(lbAuthSystems.SelectedItem);
            }
        }

        private void btnRemoveAllSystems_Click(object sender, EventArgs e)
        {
            lbSystems.Items.AddRange(lbAuthSystems.Items);
            lbAuthSystems.Items.Clear();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}

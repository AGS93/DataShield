using System;
using System.Windows.Forms;

namespace WhitelistUpdater
{
    public partial class Password : Form
    {
        public String password { get; set; }

        public Password()
        {
            InitializeComponent();
            this.ControlBox = false;
            tbPassword.PasswordChar = '*';
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (tbPassword != null)
            {
                password = tbPassword.Text;
                this.Close();
            }
            else
            {
                MessageBox.Show("Please enter a password.");
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Environment.Exit(1);
        }
    }
}

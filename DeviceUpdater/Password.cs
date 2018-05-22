using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeviceUpdater
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

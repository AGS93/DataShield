namespace FileEncryption
{
    partial class Encryptor
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Encryptor));
            this.btnEncryptor = new System.Windows.Forms.Button();
            this.lbUsers = new System.Windows.Forms.ListBox();
            this.lbAuthUsers = new System.Windows.Forms.ListBox();
            this.btnAddUser = new System.Windows.Forms.Button();
            this.btnAddAllUsers = new System.Windows.Forms.Button();
            this.btnRemoveUser = new System.Windows.Forms.Button();
            this.btnRemoveAllUsers = new System.Windows.Forms.Button();
            this.lbSystems = new System.Windows.Forms.ListBox();
            this.btnAddSystem = new System.Windows.Forms.Button();
            this.btnAddAllSystems = new System.Windows.Forms.Button();
            this.btnRemoveSystem = new System.Windows.Forms.Button();
            this.btnRemoveAllSystems = new System.Windows.Forms.Button();
            this.lbAuthSystems = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.btnExit = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // btnEncryptor
            // 
            this.btnEncryptor.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnEncryptor.Location = new System.Drawing.Point(442, 527);
            this.btnEncryptor.Name = "btnEncryptor";
            this.btnEncryptor.Size = new System.Drawing.Size(192, 27);
            this.btnEncryptor.TabIndex = 0;
            this.btnEncryptor.Text = "Encrypt File";
            this.btnEncryptor.UseVisualStyleBackColor = true;
            this.btnEncryptor.Click += new System.EventHandler(this.btnEncryptor_Click);
            // 
            // lbUsers
            // 
            this.lbUsers.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbUsers.FormattingEnabled = true;
            this.lbUsers.ItemHeight = 15;
            this.lbUsers.Location = new System.Drawing.Point(25, 87);
            this.lbUsers.Name = "lbUsers";
            this.lbUsers.Size = new System.Drawing.Size(192, 184);
            this.lbUsers.TabIndex = 1;
            // 
            // lbAuthUsers
            // 
            this.lbAuthUsers.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbAuthUsers.FormattingEnabled = true;
            this.lbAuthUsers.ItemHeight = 15;
            this.lbAuthUsers.Location = new System.Drawing.Point(442, 87);
            this.lbAuthUsers.Name = "lbAuthUsers";
            this.lbAuthUsers.Size = new System.Drawing.Size(192, 184);
            this.lbAuthUsers.TabIndex = 2;
            // 
            // btnAddUser
            // 
            this.btnAddUser.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAddUser.Location = new System.Drawing.Point(263, 102);
            this.btnAddUser.Name = "btnAddUser";
            this.btnAddUser.Size = new System.Drawing.Size(138, 23);
            this.btnAddUser.TabIndex = 3;
            this.btnAddUser.Text = "Add User";
            this.btnAddUser.UseVisualStyleBackColor = true;
            this.btnAddUser.Click += new System.EventHandler(this.btnAddUser_Click);
            // 
            // btnAddAllUsers
            // 
            this.btnAddAllUsers.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAddAllUsers.Location = new System.Drawing.Point(263, 134);
            this.btnAddAllUsers.Name = "btnAddAllUsers";
            this.btnAddAllUsers.Size = new System.Drawing.Size(138, 23);
            this.btnAddAllUsers.TabIndex = 4;
            this.btnAddAllUsers.Text = "Add All";
            this.btnAddAllUsers.UseVisualStyleBackColor = true;
            this.btnAddAllUsers.Click += new System.EventHandler(this.btnAddAllUsers_Click);
            // 
            // btnRemoveUser
            // 
            this.btnRemoveUser.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRemoveUser.Location = new System.Drawing.Point(263, 207);
            this.btnRemoveUser.Name = "btnRemoveUser";
            this.btnRemoveUser.Size = new System.Drawing.Size(138, 23);
            this.btnRemoveUser.TabIndex = 5;
            this.btnRemoveUser.Text = "Remove User";
            this.btnRemoveUser.UseVisualStyleBackColor = true;
            this.btnRemoveUser.Click += new System.EventHandler(this.btnRemoveUser_Click);
            // 
            // btnRemoveAllUsers
            // 
            this.btnRemoveAllUsers.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRemoveAllUsers.Location = new System.Drawing.Point(263, 239);
            this.btnRemoveAllUsers.Name = "btnRemoveAllUsers";
            this.btnRemoveAllUsers.Size = new System.Drawing.Size(138, 23);
            this.btnRemoveAllUsers.TabIndex = 6;
            this.btnRemoveAllUsers.Text = "Remove All";
            this.btnRemoveAllUsers.UseVisualStyleBackColor = true;
            this.btnRemoveAllUsers.Click += new System.EventHandler(this.btnRemoveAllUsers_Click);
            // 
            // lbSystems
            // 
            this.lbSystems.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbSystems.FormattingEnabled = true;
            this.lbSystems.ItemHeight = 15;
            this.lbSystems.Location = new System.Drawing.Point(25, 322);
            this.lbSystems.Name = "lbSystems";
            this.lbSystems.Size = new System.Drawing.Size(192, 184);
            this.lbSystems.TabIndex = 7;
            // 
            // btnAddSystem
            // 
            this.btnAddSystem.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAddSystem.Location = new System.Drawing.Point(263, 341);
            this.btnAddSystem.Name = "btnAddSystem";
            this.btnAddSystem.Size = new System.Drawing.Size(138, 23);
            this.btnAddSystem.TabIndex = 8;
            this.btnAddSystem.Text = "Add System";
            this.btnAddSystem.UseVisualStyleBackColor = true;
            this.btnAddSystem.Click += new System.EventHandler(this.btnAddSystem_Click);
            // 
            // btnAddAllSystems
            // 
            this.btnAddAllSystems.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAddAllSystems.Location = new System.Drawing.Point(263, 373);
            this.btnAddAllSystems.Name = "btnAddAllSystems";
            this.btnAddAllSystems.Size = new System.Drawing.Size(138, 23);
            this.btnAddAllSystems.TabIndex = 9;
            this.btnAddAllSystems.Text = "Add All";
            this.btnAddAllSystems.UseVisualStyleBackColor = true;
            this.btnAddAllSystems.Click += new System.EventHandler(this.btnAddAllSystems_Click);
            // 
            // btnRemoveSystem
            // 
            this.btnRemoveSystem.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRemoveSystem.Location = new System.Drawing.Point(263, 439);
            this.btnRemoveSystem.Name = "btnRemoveSystem";
            this.btnRemoveSystem.Size = new System.Drawing.Size(138, 23);
            this.btnRemoveSystem.TabIndex = 10;
            this.btnRemoveSystem.Text = "Remove System";
            this.btnRemoveSystem.UseVisualStyleBackColor = true;
            this.btnRemoveSystem.Click += new System.EventHandler(this.btnRemoveSystem_Click);
            // 
            // btnRemoveAllSystems
            // 
            this.btnRemoveAllSystems.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRemoveAllSystems.Location = new System.Drawing.Point(263, 471);
            this.btnRemoveAllSystems.Name = "btnRemoveAllSystems";
            this.btnRemoveAllSystems.Size = new System.Drawing.Size(138, 23);
            this.btnRemoveAllSystems.TabIndex = 11;
            this.btnRemoveAllSystems.Text = "Remove All";
            this.btnRemoveAllSystems.UseVisualStyleBackColor = true;
            this.btnRemoveAllSystems.Click += new System.EventHandler(this.btnRemoveAllSystems_Click);
            // 
            // lbAuthSystems
            // 
            this.lbAuthSystems.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbAuthSystems.FormattingEnabled = true;
            this.lbAuthSystems.ItemHeight = 15;
            this.lbAuthSystems.Location = new System.Drawing.Point(442, 322);
            this.lbAuthSystems.Name = "lbAuthSystems";
            this.lbAuthSystems.Size = new System.Drawing.Size(192, 184);
            this.lbAuthSystems.TabIndex = 12;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(34, 51);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(175, 24);
            this.label1.TabIndex = 13;
            this.label1.Text = "Unauthorised Users";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(22, 286);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(197, 24);
            this.label2.TabIndex = 14;
            this.label2.Text = "Unauthorised Systems";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(462, 51);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(154, 24);
            this.label3.TabIndex = 15;
            this.label3.Text = "Authorised Users";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(449, 286);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(176, 24);
            this.label4.TabIndex = 16;
            this.label4.Text = "Authorised Systems";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(322, 168);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(14, 24);
            this.label5.TabIndex = 17;
            this.label5.Text = "|";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(322, 405);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(14, 24);
            this.label6.TabIndex = 18;
            this.label6.Text = "|";
            // 
            // btnExit
            // 
            this.btnExit.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnExit.Location = new System.Drawing.Point(25, 527);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(192, 27);
            this.btnExit.TabIndex = 19;
            this.btnExit.Text = "Exit";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackgroundImage = global::FileEncryption.Properties.Resources.Text;
            this.pictureBox1.Location = new System.Drawing.Point(279, 34);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(107, 41);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // Encryptor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(660, 573);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lbAuthSystems);
            this.Controls.Add(this.btnRemoveAllSystems);
            this.Controls.Add(this.btnRemoveSystem);
            this.Controls.Add(this.btnAddAllSystems);
            this.Controls.Add(this.btnAddSystem);
            this.Controls.Add(this.lbSystems);
            this.Controls.Add(this.btnRemoveAllUsers);
            this.Controls.Add(this.btnRemoveUser);
            this.Controls.Add(this.btnAddAllUsers);
            this.Controls.Add(this.btnAddUser);
            this.Controls.Add(this.lbAuthUsers);
            this.Controls.Add(this.lbUsers);
            this.Controls.Add(this.btnEncryptor);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Encryptor";
            this.ShowInTaskbar = false;
            this.Text = "Encryptor";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnEncryptor;
        private System.Windows.Forms.ListBox lbUsers;
        private System.Windows.Forms.ListBox lbAuthUsers;
        private System.Windows.Forms.Button btnAddUser;
        private System.Windows.Forms.Button btnAddAllUsers;
        private System.Windows.Forms.Button btnRemoveUser;
        private System.Windows.Forms.Button btnRemoveAllUsers;
        private System.Windows.Forms.ListBox lbSystems;
        private System.Windows.Forms.Button btnAddSystem;
        private System.Windows.Forms.Button btnAddAllSystems;
        private System.Windows.Forms.Button btnRemoveSystem;
        private System.Windows.Forms.Button btnRemoveAllSystems;
        private System.Windows.Forms.ListBox lbAuthSystems;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}
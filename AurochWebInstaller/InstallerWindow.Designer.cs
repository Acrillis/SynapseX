namespace AurochWebInstaller
{
    partial class InstallerWindow
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
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.AurochWebsiteLink = new System.Windows.Forms.LinkLabel();
            this.InstallButton = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.WindowLogBox = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::AurochWebInstaller.Properties.Resources.Logo_Without_Text;
            this.pictureBox1.Location = new System.Drawing.Point(85, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(386, 270);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // AurochWebsiteLink
            // 
            this.AurochWebsiteLink.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.AurochWebsiteLink.AutoSize = true;
            this.AurochWebsiteLink.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.AurochWebsiteLink.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.AurochWebsiteLink.LinkColor = System.Drawing.Color.Gray;
            this.AurochWebsiteLink.Location = new System.Drawing.Point(197, 466);
            this.AurochWebsiteLink.Name = "AurochWebsiteLink";
            this.AurochWebsiteLink.Size = new System.Drawing.Size(157, 13);
            this.AurochWebsiteLink.TabIndex = 1;
            this.AurochWebsiteLink.TabStop = true;
            this.AurochWebsiteLink.Text = "https://auroch.synapse.to";
            this.AurochWebsiteLink.VisitedLinkColor = System.Drawing.Color.Gray;
            this.AurochWebsiteLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.AurochWebsiteLink_LinkClicked);
            // 
            // InstallButton
            // 
            this.InstallButton.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.InstallButton.Location = new System.Drawing.Point(83, 288);
            this.InstallButton.Name = "InstallButton";
            this.InstallButton.Size = new System.Drawing.Size(126, 32);
            this.InstallButton.TabIndex = 0;
            this.InstallButton.Text = "Install";
            this.InstallButton.UseVisualStyleBackColor = true;
            this.InstallButton.Click += new System.EventHandler(this.InstallButton_Click);
            // 
            // button1
            // 
            this.button1.Enabled = false;
            this.button1.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(215, 288);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(126, 32);
            this.button1.TabIndex = 3;
            this.button1.Text = "Repair";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.Enabled = false;
            this.button2.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button2.Location = new System.Drawing.Point(347, 288);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(126, 32);
            this.button2.TabIndex = 4;
            this.button2.Text = "Uninstall";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // WindowLogBox
            // 
            this.WindowLogBox.BackColor = System.Drawing.Color.White;
            this.WindowLogBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.WindowLogBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.WindowLogBox.Location = new System.Drawing.Point(85, 326);
            this.WindowLogBox.Multiline = true;
            this.WindowLogBox.Name = "WindowLogBox";
            this.WindowLogBox.ReadOnly = true;
            this.WindowLogBox.Size = new System.Drawing.Size(386, 137);
            this.WindowLogBox.TabIndex = 5;
            // 
            // InstallerWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(563, 482);
            this.Controls.Add(this.WindowLogBox);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.InstallButton);
            this.Controls.Add(this.AurochWebsiteLink);
            this.Controls.Add(this.pictureBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "InstallerWindow";
            this.ShowIcon = false;
            this.Text = "Auroch — Web Installer";
            this.Load += new System.EventHandler(this.InstallerWindow_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.LinkLabel AurochWebsiteLink;
        private System.Windows.Forms.Button InstallButton;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox WindowLogBox;
    }
}


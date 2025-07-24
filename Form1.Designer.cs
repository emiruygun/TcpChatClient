namespace TcpChatClient
{
    partial class Form1
    {
        /// <summary>
        ///Gerekli tasarımcı değişkeni.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///Kullanılan tüm kaynakları temizleyin.
        /// </summary>
        ///<param name="disposing">yönetilen kaynaklar dispose edilmeliyse doğru; aksi halde yanlış.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer üretilen kod

        /// <summary>
        /// Tasarımcı desteği için gerekli metot - bu metodun 
        ///içeriğini kod düzenleyici ile değiştirmeyin.
        /// </summary>
        private void InitializeComponent()
        {
            this.textMessage = new System.Windows.Forms.TextBox();
            this.btnSend = new System.Windows.Forms.Button();
            this.listMessage = new System.Windows.Forms.ListBox();
            this.listUsers = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // textMessage
            // 
            this.textMessage.Location = new System.Drawing.Point(218, 395);
            this.textMessage.Name = "textMessage";
            this.textMessage.Size = new System.Drawing.Size(461, 22);
            this.textMessage.TabIndex = 0;
            this.textMessage.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textMessage_KeyDown);
            // 
            // btnSend
            // 
            this.btnSend.Location = new System.Drawing.Point(685, 394);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(91, 23);
            this.btnSend.TabIndex = 1;
            this.btnSend.Text = "GÖNDER";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // listMessage
            // 
            this.listMessage.FormattingEnabled = true;
            this.listMessage.ItemHeight = 16;
            this.listMessage.Location = new System.Drawing.Point(218, 12);
            this.listMessage.Name = "listMessage";
            this.listMessage.Size = new System.Drawing.Size(558, 372);
            this.listMessage.TabIndex = 2;
            // 
            // listUsers
            // 
            this.listUsers.FormattingEnabled = true;
            this.listUsers.ItemHeight = 16;
            this.listUsers.Location = new System.Drawing.Point(13, 13);
            this.listUsers.Name = "listUsers";
            this.listUsers.Size = new System.Drawing.Size(186, 404);
            this.listUsers.TabIndex = 3;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.listUsers);
            this.Controls.Add(this.listMessage);
            this.Controls.Add(this.btnSend);
            this.Controls.Add(this.textMessage);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textMessage;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.ListBox listMessage;
        private System.Windows.Forms.ListBox listUsers;
    }
}


using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.VisualBasic;

namespace TcpChatClient
{
    public partial class Form1 : Form
    {
        TcpClient client;
        NetworkStream stream;
        Dictionary<string, List<string>> chatHistory = new Dictionary<string, List<string>>();
        Dictionary<string, int> unreadCounts = new Dictionary<string, int>();
        string myUsername;
        private System.Windows.Forms.Timer flashTimer;
        private bool isFlashing = false;
        private string originalTitle;
        private NotifyIcon notifyIcon;

        

        public Form1(string username)
        {
            InitializeComponent();
            myUsername = username;

            originalTitle = this.Text;

            flashTimer = new System.Windows.Forms.Timer();
            flashTimer.Interval = 500;
            flashTimer.Tick += FlashTimer_Tick;

            notifyIcon = new NotifyIcon();
            notifyIcon.Visible = true;
            notifyIcon.Icon = SystemIcons.Information;
            notifyIcon.BalloonTipTitle = "Yeni Mesaj!";
           
            // Form arka planı
            this.BackColor = Color.FromArgb(245, 245, 245);

            // listUsers düzenle
            listUsers.BackColor = Color.FromArgb(240, 240, 240);
            listUsers.BorderStyle = BorderStyle.None;

            
            

            // textMessage
            textMessage.BorderStyle = BorderStyle.None;
            textMessage.BackColor = Color.White;
            textMessage.Font = new Font("Segoe UI", 10);

            // btnSend
            btnSend.BackColor = Color.FromArgb(0, 120, 215);
            btnSend.ForeColor = Color.White;
            btnSend.FlatStyle = FlatStyle.Flat;
            btnSend.FlatAppearance.BorderSize = 0;

            btnSend.MouseEnter += (s, e) => btnSend.BackColor = Color.FromArgb(0, 150, 245);
            btnSend.MouseLeave += (s, e) => btnSend.BackColor = Color.FromArgb(0, 120, 215);

            textMessage.Margin = new Padding(5);
            btnSend.Padding = new Padding(5);

            listUsers.SelectedIndexChanged += (s, e) =>
            {
                listUsers.BackColor = Color.WhiteSmoke;
            };

            try
            {
                client = new TcpClient("192.168.1.197", 5000);
                stream = client.GetStream();

                byte[] nameData = Encoding.UTF8.GetBytes(myUsername);
                stream.Write(nameData, 0, nameData.Length);
                new Thread(ReceiveMessages).Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bağlantı kurulamadı: " + ex.Message);
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (listUsers.SelectedItem == null)
            {
                MessageBox.Show("Lütfen bir kullanıcı seçin.");
                return;
            }

            string targetUser = TrimParens(listUsers.SelectedItem.ToString());
            string message = textMessage.Text.Trim();

            if (!string.IsNullOrEmpty(message))
            {
                string formattedMessage = targetUser + "|" + message;
                byte[] data = Encoding.UTF8.GetBytes(formattedMessage);
                stream.Write(data, 0, data.Length);

                var bubble = new MessageBubble("Merhaba!", isOwnMessage: true);
                panelMessages.Controls.Add(bubble);


                string time = DateTime.Now.ToShortTimeString();
                

                if (!chatHistory.ContainsKey(targetUser))
                    chatHistory[targetUser] = new List<string>();

                chatHistory[targetUser].Add($"Ben [{time}]: {message}");
                textMessage.Clear();

                
            }
        }

        private void RefreshChatWindow(string username)
        {
            panelMessages.Controls.Clear();

            if (chatHistory.ContainsKey(username))
            {
                foreach (var msg in chatHistory[username])
                {
                    // Mesajı sen mi göndermişsin kontrolü
                    bool isOwn = msg.StartsWith("Ben");

                    // Baloncuk oluştur
                    var bubble = new MessageBubble(msg, isOwnMessage: isOwn);
                    panelMessages.Controls.Add(bubble);
                }

                // Son mesajı görünür yap
                if (panelMessages.Controls.Count > 0)
                    panelMessages.ScrollControlIntoView(panelMessages.Controls[panelMessages.Controls.Count - 1]);
            }
        }


        private void listUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listUsers.SelectedItem == null)
                return;

            string selectedUser = TrimParens(listUsers.SelectedItem.ToString());

            // ⬇ Sunucuya mesaj geçmişi isteği gönder
            string historyRequest = $"GETHISTORY|{selectedUser}";
            byte[] data = Encoding.UTF8.GetBytes(historyRequest);
            stream.Write(data, 0, data.Length);

            

            RefreshChatWindow(selectedUser);
        }


        private void UpdateUserListDisplay()
        {
            string currentSelected = listUsers.SelectedItem != null ? TrimParens(listUsers.SelectedItem.ToString()) : null;

            listUsers.Items.Clear();
            foreach (var user in chatHistory.Keys.Where(u => u != myUsername))
            {
                string display = user;
                if (unreadCounts.ContainsKey(user) && unreadCounts[user] > 0)
                    display += $" ({unreadCounts[user]})";

                listUsers.Items.Add(display);
            }

            if (currentSelected != null)
            {
                foreach (var item in listUsers.Items)
                {
                    if (TrimParens(item.ToString()) == currentSelected)
                    {
                        listUsers.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        private void ReceiveMessages()
        {
            byte[] buffer = new byte[1024];
            int byteCount;

            try
            {
                while ((byteCount = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, byteCount);

                    if (message.StartsWith("HISTORY|"))
                    {
                        string[] parts = message.Split('|');
                        if (parts.Length >= 3)
                        {
                            string[] users = parts[1].Split(',');
                            string userA = users[0].Trim();
                            string userB = users[1].Trim();

                            string otherUser = (userA == myUsername) ? userB : userA;

                            if (!chatHistory.ContainsKey(otherUser))
                                chatHistory[otherUser] = new List<string>();

                            // Eski mesajları temizlemeden güncelliyoruz (istersen temizleyebilirsin)
                            for (int i = 2; i < parts.Length; i++)
                            {
                                chatHistory[otherUser].Add(parts[i].Trim());
                            }

                            Invoke(new Action(() =>
                            {
                                if (listUsers.SelectedItem != null &&
                                    TrimParens(listUsers.SelectedItem.ToString()) == otherUser)
                                {
                                    RefreshChatWindow(otherUser);
                                }
                            }));
                        }
                        continue;
                    }


                    if (message.StartsWith("USERLIST|"))
                    {
                        string[] users = message.Substring(9).Split(',');

                        Invoke(new Action(() =>
                        {
                            foreach (string user in users)
                            {
                                string clean = TrimParens(user);
                                if (!string.IsNullOrWhiteSpace(clean) && clean != myUsername)
                                {
                                    if (!chatHistory.ContainsKey(clean))
                                        chatHistory[clean] = new List<string>();

                                    if (!unreadCounts.ContainsKey(clean))
                                        unreadCounts[clean] = 0;
                                }
                            }
                            UpdateUserListDisplay();
                        }));
                    }
                    else
                    {
                        int idx = message.IndexOf(":");
                        if (idx > 0)
                        {
                            string fromUser = TrimParens(message.Substring(0, idx).Trim());
                            string actualMessage = message.Substring(idx + 1).Trim();

                            Invoke(new Action(() =>
                            {
                                if (!chatHistory.ContainsKey(fromUser))
                                    chatHistory[fromUser] = new List<string>();

                                chatHistory[fromUser].Add($"{fromUser}: {actualMessage}");

                                var bubble = new MessageBubble(actualMessage, isOwnMessage: false);
                                panelMessages.Controls.Add(bubble);
                                panelMessages.ScrollControlIntoView(bubble);


                                System.Media.SystemSounds.Asterisk.Play();
                                notifyIcon.BalloonTipText = $"{fromUser} yeni bir mesaj gönderdi: \"{actualMessage}\"";
                                notifyIcon.ShowBalloonTip(3000);

                                if (!this.Focused)
                                {
                                    isFlashing = true;
                                    flashTimer.Start();
                                }

                                if (!unreadCounts.ContainsKey(fromUser))
                                    unreadCounts[fromUser] = 0;

                                if (listUsers.SelectedItem == null)
                                {
                                    foreach (var item in listUsers.Items)
                                    {
                                        if (TrimParens(item.ToString()) == fromUser)
                                        {
                                            listUsers.SelectedItem = item;
                                            break;
                                        }
                                    }
                                }

                                if (listUsers.SelectedItem != null && TrimParens(listUsers.SelectedItem.ToString()) == fromUser)
                                {
                                    RefreshChatWindow(fromUser);
                                }
                                else
                                {
                                    unreadCounts[fromUser]++;
                                }

                                UpdateUserListDisplay();
                            }));
                        }
                    }
                }
            }
            catch { }
        }

  

        private string TrimParens(string input)
        {
            int idx = input.IndexOf(" (");
            return idx >= 0 ? input.Substring(0, idx).Trim() : input.Trim();
        }

        private void textMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnSend.PerformClick();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void FlashTimer_Tick(object sender, EventArgs e)
        {
            if (this.Text == originalTitle)
                this.Text = "📩 Yeni Mesaj!";
            else
                this.Text = originalTitle;
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            if (isFlashing)
            {
                flashTimer.Stop();
                this.Text = originalTitle;
                isFlashing = false;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Test mesajları
            var messages = new List<ChatMessageDto>
    {
        new ChatMessageDto { FromUser = "Ali", ToUser = "Veli", Message = "Merhaba Veli!", Timestamp = DateTime.Now },
        new ChatMessageDto { FromUser = "Veli", ToUser = "Ali", Message = "Selam Ali!", Timestamp = DateTime.Now.AddMinutes(1) },
        new ChatMessageDto { FromUser = "Ali", ToUser = "Veli", Message = "Nasılsın?", Timestamp = DateTime.Now.AddMinutes(2) },
    };

            foreach (var msg in messages)
            {
                bool isMine = msg.FromUser == myUsername;
                var bubble = new MessageBubble(msg.Message, isMine);
                bubble.Margin = new Padding(5);

                panelMessages.Controls.Add(bubble);

                var bubble1 = new MessageBubble("Selam!", isOwnMessage: true);
                panelMessages.Controls.Add(bubble);

                panelMessages.Controls.Add(new MessageBubble("Merhaba, nasılsın?", false));
                panelMessages.Controls.Add(new MessageBubble("İyiyim, sen?", true));


            }
        }

    }
}

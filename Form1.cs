using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TcpChatClient
{
    public partial class Form1 : Form
    {
        TcpClient client;
        NetworkStream stream;
        Dictionary<string, List<string>> chatHistory = new Dictionary<string, List<string>>();
        Dictionary<string, int> unreadCounts = new Dictionary<string, int>();
        string myUsername = "BenimAdım";  // Buraya kendin için istediğin kullanıcı adını yaz

        public Form1()  
        {
            InitializeComponent();

            client = new TcpClient("127.0.0.1", 5000);
            stream = client.GetStream();

            // Kullanıcı adını gönder
            byte[] nameData = Encoding.UTF8.GetBytes(myUsername);
            stream.Write(nameData, 0, nameData.Length);

            new Thread(ReceiveMessages).Start();
        }


        private void btnSend_Click(object sender, EventArgs e)
        {
            if (listUsers.SelectedItem == null)
            {
                MessageBox.Show("Lütfen bir kullanıcı seçin.");
                return;
            }

            string targetUser = listUsers.SelectedItem.ToString();
            string message = textMessage.Text;

            if (!string.IsNullOrWhiteSpace(message))
            {
                string formattedMessage = targetUser + "|" + message;
                byte[] data = Encoding.UTF8.GetBytes(formattedMessage);
                stream.Write(data, 0, data.Length);

                string time = DateTime.Now.ToShortTimeString();
                listMessage.Items.Add($"Ben (→ {targetUser}) [{time}]: {message}");
                textMessage.Clear();

                if (!chatHistory.ContainsKey(targetUser))
                    chatHistory[targetUser] = new List<string>();

               chatHistory[targetUser].Add($"Ben [{time}]: {message}");
                RefreshChatWindow(targetUser); 
            }

        }

        private void RefreshChatWindow(string username)
        {
            listMessage.Items.Clear();
            if (chatHistory.ContainsKey(username))
            {
                foreach (var msg in chatHistory[username])
                {
                    listMessage.Items.Add(msg);
                }
            }
        }
        private void listUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listUsers.SelectedItem != null)
            {
                string selectedUser = listUsers.SelectedItem.ToString();

                if (unreadCounts.ContainsKey(selectedUser))
                {
                    unreadCounts[selectedUser] = 0;
                    UpdateUserListDisplay();
                }

                RefreshChatWindow(selectedUser);
            }


        }
        private void UpdateUserListDisplay()
        {
            object previouslySelected = listUsers.SelectedItem;  // Seçili kullanıcıyı hatırla

            listUsers.Items.Clear();
            foreach (var user in chatHistory.Keys)
            {
                string displayName = user;
                if (unreadCounts.ContainsKey(user) && unreadCounts[user] > 0)
                {
                    displayName += $" ({unreadCounts[user]})";
                }
                listUsers.Items.Add(displayName);
            }

            // Eğer önce seçilmişse tekrar seç
            if (previouslySelected != null)
            {
                foreach (var item in listUsers.Items)
                {
                    if (item.ToString().StartsWith(previouslySelected.ToString()))
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

                    if (message.StartsWith("USERLIST|"))
                    {
                        string userListString = message.Substring(9);
                        string[] users = userListString.Split(',');

                        Invoke(new Action(() =>
                        {
                            listUsers.Items.Clear();
                            foreach (string user in users)
                            {
                                if (!string.IsNullOrWhiteSpace(user))
                                {
                                    listUsers.Items.Add(user);

                                    if (!chatHistory.ContainsKey(user))
                                        chatHistory[user] = new List<string>();

                                    if (!unreadCounts.ContainsKey(user))
                                        unreadCounts[user] = 0;
                                    // Eğer listede kendi adın yoksa, ekle
                                    if (!users.Contains(myUsername))
                                        users = users.Append(myUsername).ToArray();

                                }
                            }
                        }));
                    }
                    else
                    {
                        // Mesaj: "Gönderen: Mesajİçeriği"
                        int idx = message.IndexOf(":");
                        if (idx > 0)
                        {
                            string fromUser = message.Substring(0, idx).Trim();
                            string actualMessage = message.Substring(idx + 1).Trim();

                            Invoke(new Action(() =>
                            {
                                if (!chatHistory.ContainsKey(fromUser))
                                    chatHistory[fromUser] = new List<string>();

                                chatHistory[fromUser].Add(fromUser + ": " + actualMessage);

                                if (!unreadCounts.ContainsKey(fromUser))
                                    unreadCounts[fromUser] = 0;

                                // Eğer bu kullanıcı seçili değilse unread artır
                                if (listUsers.SelectedItem == null || listUsers.SelectedItem.ToString() != fromUser)
                                {
                                    unreadCounts[fromUser]++;
                                }

                                UpdateUserListDisplay();

                                // Eğer şu anda bu kullanıcı seçiliyse mesajları göster
                                if (listUsers.SelectedItem != null && listUsers.SelectedItem.ToString() == fromUser)
                                {
                                    RefreshChatWindow(fromUser);
                                }
                            }));
                        }
                    }
                }
            }
            catch 
            {
                
            }

        }

        private void textMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnSend.PerformClick(); // Sanki butona tıklamış gibi
                e.Handled = true; // Enter’ın bip sesini engelle
                e.SuppressKeyPress = true; // Enter karakteri textbox’a yazılmasın
            }
        }
    }

}

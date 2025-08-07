using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TcpChatClient
{
    public partial class MessageBubble : UserControl
    {
        public MessageBubble()
        {
            InitializeComponent();
            this.BackColor = Color.Transparent;
        }

        public MessageBubble(string message, bool isOwnMessage)
        {
            InitializeComponent();
            this.BackColor = Color.Transparent;
            this.Padding = new Padding(0);
            this.Margin = new Padding(5);
            this.AutoSize = true;

            var container = new Panel();
            container.BackColor = isOwnMessage ? Color.LightBlue : Color.LightGray;
            container.Padding = new Padding(10);
            container.Margin = new Padding(5);
            container.AutoSize = true;
            container.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            container.MaximumSize = new Size(300, 0);
            container.Dock = DockStyle.Right;

            var lbl = new Label();
            lbl.Text = message;
            lbl.Font = new Font("Segoe UI", 10);
            lbl.ForeColor = Color.Black;
            lbl.AutoSize = true;
            lbl.MaximumSize = new Size(280, 0);
            lbl.Dock = DockStyle.Fill;

            container.Controls.Add(lbl);
            this.Controls.Add(container);

            // Hizalama için container paneli sağa veya sola dayamak:
            this.Dock = DockStyle.Top;
            this.Anchor = isOwnMessage ? AnchorStyles.Right : AnchorStyles.Left;
        }
        public void SetMessage(string sender, string message, DateTime time, bool isOwnMessage)
        {
            Label lbl = new Label();
            lbl.AutoSize = true;
            lbl.MaximumSize = new Size(250, 0);
            lbl.Text = $"{message}\n\n{time:HH:mm}";
            lbl.Font = new Font("Segoe UI", 10);
            lbl.BackColor = isOwnMessage ? Color.LightBlue : Color.LightGray;
            lbl.Padding = new Padding(10);
            lbl.ForeColor = Color.Black;

            Panel bubblePanel = new Panel();
            bubblePanel.Padding = new Padding(5);
            bubblePanel.Controls.Add(lbl);
            bubblePanel.AutoSize = true;
            bubblePanel.BackColor = Color.Transparent;
            bubblePanel.Dock = DockStyle.Top;

            if (isOwnMessage)
                lbl.TextAlign = ContentAlignment.MiddleRight;
            else
                lbl.TextAlign = ContentAlignment.MiddleLeft;

            this.Controls.Add(bubblePanel);
            this.Height = bubblePanel.Height + 10;
        }

        private void MessageBubble_Load(object sender, EventArgs e)
        {

        }
    }
}

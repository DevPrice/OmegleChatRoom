using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OmegleChatRoom;

namespace OmegleChat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private LocalUser Admin;
        private ChatRoom ChatRoom;

        public MainWindow()
        {
            InitializeComponent();

            Admin = new LocalUser();
            ChatRoom = new ChatRoom(Admin);
            ChatRoom.CommandEngineEnabled = true;
            ChatRoom.NewMessage += ChatRoom_NewMessage;
            ChatRoom.StatusChanged += ChatRoom_StatusChanged;

            UserBox.ItemsSource = ChatRoom.Users;

            ReceiveBox.Document.Blocks.Clear();

            SendBox.KeyDown += SendBox_KeyDown;
            AutoReconnectCheck.Checked += AutoReconnectCheck_Checked;
            AddUserBtn.Click += AddUserBtn_Click;
            KickContext.Click += KickCtx_Click;
            MuteContext.Click += MuteCtx_Click;
        }

        private void AddUserBtn_Click(object sender, RoutedEventArgs e)
        {
            ChatRoom.Connect(Admin, new string[] { });

            RefreshUserBox();
        }

        private void KickCtx_Click(object sender, RoutedEventArgs e)
        {
            foreach (IUser user in UserBox.SelectedItems)
            {
                ChatRoom.Kick(Admin, new string[] { user.Name });
            }

            RefreshUserBox();
        }

        private void MuteCtx_Click(object sender, RoutedEventArgs e)
        {
            foreach (IUser user in UserBox.SelectedItems)
            {
                ChatRoom.Mute(Admin, new string[] { user.Name });
            }

            RefreshUserBox();
        }

        private void AutoReconnectCheck_Checked(object sender, RoutedEventArgs e)
        {
            ChatRoom.AutoReconnect = (bool)((CheckBox)sender).IsChecked;
        }

        private void SendBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return && SendBox.Text.Length > 0)
            {
                ChatRoom.SendMessage(SendBox.Text);
                SendBox.Text = String.Empty;
            }
        }

        private void DisplayMessage(string message)
        {
            Dispatcher.Invoke(() =>
                {
                    Run run = new Run(message);
                    run.Foreground = Brushes.DarkSlateGray;
                    ReceiveBox.Document.Blocks.Add(new Paragraph(run));
                    ReceiveBox.ScrollToEnd();
                });
        }

        private void DisplayMessage(IUser sender, string message)
        {
            Dispatcher.Invoke(() =>
                {
                    Run nameRun = new Run(sender.Name + "\t");
                    Run messageRun = new Run(message);

                    nameRun.Foreground = (sender.PriviledgeLevel != PriviledgeLevel.Mod) ? Brushes.Red : (sender is LocalUser ? Brushes.Blue : Brushes.Green);
                    messageRun.Foreground = Brushes.Black;

                    Paragraph paragraph = new Paragraph();
                    paragraph.Inlines.Add(nameRun);
                    paragraph.Inlines.Add(messageRun);

                    ReceiveBox.Document.Blocks.Add(paragraph);
                    ReceiveBox.ScrollToEnd();
                });
        }

        private void RefreshUserBox()
        {
            Dispatcher.Invoke(() =>
            {
                var selectedUser = UserBox.SelectedItem;

                UserBox.ItemsSource = null;
                UserBox.ItemsSource = ChatRoom.Users;

                UserBox.SelectedItem = selectedUser;
            });
        }

        private void ChatRoom_NewMessage(object sender, MessageEventArgs e)
        {
            if (sender is IUser)
            {
                DisplayMessage((IUser)sender, e.Message);
            }
            else
            {
                DisplayMessage(e.Message);
            }
        }

        private void ChatRoom_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            RefreshUserBox();
        }
    }
}

using System;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Controls;
namespace TwitchBot
{
    public partial class MainWindow : Window
    {
        #region Globals
        public static string channelToJoin;
        private string oauth;
        private Thread thread;
        private DispatcherTimer infoDisplayTimer;
        private string[] info;
        private bool infoAllowed = true;
        private int infoInterval = 30;
        #endregion

        #region Constructor
        public MainWindow(string chToJoin, string oauth)
        {
            InitializeComponent();
            channelToJoin = chToJoin;
            this.oauth = oauth;
            Initialization();
        }
        #endregion

        #region Events
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }
        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            IrcClient.SendChatMessage(sendChat.Text);
            recieveChat.Text += "PRIVMSG #" + "hap_pyBot" + " :" + sendChat.Text + "\n";
            sendChat.Text = "";
        }
        private void send_Enter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                IrcClient.SendChatMessage(sendChat.Text);
                recieveChat.Text += "PRIVMSG #" + "hap_pyBot" + " :" + sendChat.Text + "\n";
                sendChat.Text = "";
            }
        }
        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult m = MessageBox.Show("Save the name list?", "Save?", MessageBoxButton.YesNoCancel);

            if (m == MessageBoxResult.Yes) Bot.SaveNames(channelToJoin);
            
            else if (m == MessageBoxResult.Cancel) return;

            Application.Current.Shutdown();
            
        }
        private void minimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void delete_Default(object sender, MouseEventArgs e)
        {
            if (sendChat.Text == "Send a message")
            {
                sendChat.Text = "";
            }
            sendChat.Focus();
        }
        private void infoDisplayTimer_Tick(object sender, EventArgs e)
        {
            if (!infoAllowed) return; 

             if ((int)DateTime.Today.DayOfWeek == 1)
            {
                IrcClient.SendChatMessage(info[0]);
            }

            else if ((int)DateTime.Today.DayOfWeek == 5
                  || (int)DateTime.Today.DayOfWeek == 6
                  || (int)DateTime.Today.DayOfWeek == 7)
            {
                IrcClient.SendChatMessage(info[1]);
            }
            else
            {
                IrcClient.SendChatMessage("/me Say more stuff!");
            }
        }
        #endregion

        #region Functions
        private void Initialization()
        {
            infoDisplayTimer = new System.Windows.Threading.DispatcherTimer();
            infoDisplayTimer.Tick += new EventHandler(infoDisplayTimer_Tick);
            infoDisplayTimer.Interval = new TimeSpan(0, infoInterval, 0);
            infoDisplayTimer.Start();

            info = Bot.LoadTextFile("info");
            scroll.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;

            IrcClient.IrcStart("irc.twitch.tv", 6667, "hap_pybot", "oauth:" + oauth);
            if (IrcClient.connected)
            {
                IrcClient.JoinRoom(channelToJoin);
                StartThread();
            }
            else Application.Current.Shutdown();

        }
        private void StartThread()
        {
            thread = new Thread(() => CheckChat());
            thread.IsBackground = true;
            thread.Start();
        }
        private void CheckChat()
        {
            bool paused = false;

            while (true)
            {
                string message = IrcClient.ReadMessage();
                if (message != null)
                {
                    Dispatcher.Invoke(() => recieveChat.Text += message + "\n");
                    Dispatcher.Invoke(() => scroll.ScrollToBottom());


                    if (message.Contains("PING")) IrcClient.Pong();
                    if (message.Contains("!Pause")) paused = true;
                    while (paused)
                    {
                        string pausedMessage = IrcClient.ReadMessage();
                        if (pausedMessage.Contains("PING")) IrcClient.Pong();
                        if (pausedMessage.Contains("!Pause")) paused = false;
                        Thread.Sleep(80);
                    }


                    if (message.Contains("!Info")) infoAllowed = true ? false : true;
                    if (message.Contains("!Load")) Bot.LoadNames(message);
                    if (message.Contains("!Save")) Bot.SaveNames(message);
                    if (message.Contains("!Random")) Bot.Random(message);

                    if (message.Contains("!Remove ") 
                        && !message.Contains("VIP")) Bot.Remove(message);
                    if (message.Contains("!Remove") 
                        && message.Contains("VIP ")) Bot.RemoveVIP(message);

                    if (message.Contains("!Hello")) Bot.GreetUser(message);
                    if (message.Contains("!RickRoll")) Bot.RickRoll(message);
                    if (message.Contains("!Strippers")) Bot.Strippers(message);
                    if (message.Contains("!Trials")) Bot.Trials(message);
                    if (message.Contains("!VIP ")) Bot.VIP(message);
                    if (message.Contains("!Donate")) Bot.Donate();

                    if (message.Contains("!Commands") 
                        || message.Contains("!commands")) Bot.Commands();
                    if (message.Contains("!List")
                        && !message.Contains("VIP") 
                        && !message.Contains("Vip")) Bot.List();

                    if (message.Contains("!Slots")) Bot.Spin(message);
                    if (message.Contains("!Score")) Bot.ListScore(message);

                    if ((message.Contains("!List") 
                        && message.Contains("VIP"))
                        || message.Contains("!VipList")
                        || message.Contains("!VIPList")
                        || message.Contains("!ListVip")) Bot.ListVIP();

                    if (message.Contains("!Millersucks")
                        || message.Contains("!MillerSucks")
                        || message.Contains("!millersucks")) Bot.MillerSucks();

                    if (message.Contains("!CandyBar")
                        || message.Contains("!Candybar")
                        || message.Contains("!candybar")) Bot.CandyBar();

                    message = null;
                }
                Thread.Sleep(40);
            }
        }
        #endregion
    }
}






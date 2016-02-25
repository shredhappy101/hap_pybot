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
        public static string ChannelToJoin;
        private readonly string _oauth;
        private Thread _thread;
        private DispatcherTimer _infoDisplayTimer;
        private string[] _info;
        private bool _infoAllowed = true;
        private const int InfoInterval = 30;

        #endregion

        #region Constructor
        public MainWindow(string chToJoin, string oauth)
        {
            InitializeComponent();
            ChannelToJoin = chToJoin;
            this._oauth = oauth;
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
            if (e.Key != Key.Enter) return;
            IrcClient.SendChatMessage(sendChat.Text);
            recieveChat.Text += "PRIVMSG #" + "hap_pyBot" + " :" + sendChat.Text + "\n";
            sendChat.Text = "";
        }
        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            var m = MessageBox.Show("Save the name list?", "Save?", MessageBoxButton.YesNoCancel);

            switch (m)
            {
                case MessageBoxResult.Yes:
                    Bot.SaveNames(ChannelToJoin);
                    break;
                case MessageBoxResult.Cancel:
                    return;
                case MessageBoxResult.None:
                    break;
                case MessageBoxResult.OK:
                    break;
                case MessageBoxResult.No:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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
            if (!_infoAllowed) return;

            switch ((int)DateTime.Today.DayOfWeek)
            {
                case 1:
                    IrcClient.SendChatMessage(_info[0]);
                    break;
                case 5:
                case 6:
                case 7:
                    IrcClient.SendChatMessage(_info[1]);
                    break;
                default:
                    IrcClient.SendChatMessage("/me Say more stuff!");
                    break;
            }
        }

        #endregion

        #region Functions

        private void Initialization()
        {
            _infoDisplayTimer = new System.Windows.Threading.DispatcherTimer();
            _infoDisplayTimer.Tick += new EventHandler(infoDisplayTimer_Tick);
            _infoDisplayTimer.Interval = new TimeSpan(0, InfoInterval, 0);
            _infoDisplayTimer.Start();

            _info = Bot.LoadTextFile("_info");
            scroll.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;

            IrcClient.IrcStart("irc.twitch.tv", 6667, "hap_pybot", "_oauth:" + _oauth);
            if (IrcClient.Connected)
            {
                IrcClient.JoinRoom(ChannelToJoin);
                StartThread();
            }
            else Application.Current.Shutdown();
        }

        private void StartThread()
        {
            _thread = new Thread(CheckChat) { IsBackground = true };
            _thread.Start();
        }

        private void CheckChat()
        {
            var paused = false;

            while (true)
            {
                var message = IrcClient.ReadMessage();
                if (message != null)
                {
                    var localMessage = message;
                    Dispatcher.Invoke(() => recieveChat.Text += localMessage + "\n");
                    Dispatcher.Invoke(() => scroll.ScrollToBottom());


                    if (message.Contains("PING")) IrcClient.Pong();
                    if (message.Contains("!Pause")) paused = true;
                    while (paused)
                    {
                        var pausedMessage = IrcClient.ReadMessage();
                        if (pausedMessage.Contains("PING")) IrcClient.Pong();
                        if (pausedMessage.Contains("!Pause")) paused = false;
                        Thread.Sleep(80);
                    }


                    if (message.Contains("!Info")) _infoAllowed = !_infoAllowed;
                    if (message.Contains("!Load")) Bot.LoadNames(message);
                    if (message.Contains("!Save")) Bot.SaveNames(message);
                    if (message.Contains("!Random")) Bot.Random(message);

                    if (message.Contains("!Remove ") && !message.Contains("Vip")) Bot.Remove(message);
                    if (message.Contains("!Remove") && message.Contains("Vip ")) Bot.RemoveVip(message);

                    if (message.Contains("!Hello")) Bot.GreetUser(message);
                    if (message.Contains("!RickRoll")) Bot.RickRoll(message);
                    if (message.Contains("!Strippers")) Bot.Strippers(message);
                    if (message.Contains("!Trials")) Bot.Trials(message);
                    if (message.Contains("!Vip ")) Bot.Vip(message);
                    if (message.Contains("!Donate")) Bot.Donate();

                    if (message.Contains("!Commands") || message.Contains("!commands")) Bot.Commands();
                    if (message.Contains("!List") && !message.Contains("Vip") && !message.Contains("Vip")) Bot.List();

                    if (message.Contains("!Slots")) Bot.Spin(message);
                    if (message.Contains("!Score")) Bot.ListScore(message);

                    if ((message.Contains("!List") && message.Contains("Vip")) || message.Contains("!VipList") || message.Contains("!VIPList") || message.Contains("!ListVip")) Bot.ListVip();

                    if (message.Contains("!Millersucks") || message.Contains("!MillerSucks") || message.Contains("!millersucks")) Bot.MillerSucks();

                    if (message.Contains("!CandyBar") || message.Contains("!Candybar") || message.Contains("!candybar")) Bot.CandyBar();

                    message = null;
                }
                Thread.Sleep(40);
            }
        }

        #endregion
    }
}






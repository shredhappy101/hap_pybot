using System;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Controls;
namespace TwitchBot
{
    public partial class MainWindow
    {
        #region Globals
        public static string ChannelToJoin;
        private IrcClient irc;
        private Bot bot;
        private readonly string oauth;
        private Thread thread;
        private DispatcherTimer infoDisplayTimer;
        private string[] info;
        private bool infoAllowed = true;
        private const int InfoInterval = 30;

        #endregion

        #region Constructor
        public MainWindow(string chToJoin, string oauth)
        {
            InitializeComponent();
            ChannelToJoin = chToJoin;
            this.oauth = oauth;
            Initialization();
        }
        #endregion

        #region Events
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }
        private void sendButton_Click(object s, RoutedEventArgs e)
        {
            Send();
        }

        private void send_Enter(object s, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            Send();
        }

        private void closeButton_Click(object s, RoutedEventArgs e)
        {
            Shutdown();
        }

        private void minimizeButton_Click(object s, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void delete_Default(object s, MouseEventArgs e)
        {
            if (sendChat.Text == "Send a message") sendChat.Text = "";          
            sendChat.Focus();
        }

        private void infoDisplayTimer_Tick(object s, EventArgs e)
        {
            if (!infoAllowed) return;

            switch ((int)DateTime.Today.DayOfWeek)
            {
                case 1:
                    irc.SendChatMessage(info[0]);
                    break;
                case 5:
                case 6:
                case 7:
                    irc.SendChatMessage(info[1]);
                    break;
                default:
                    irc.SendChatMessage("/me Say more stuff!");
                    break;
            }
        }

        #endregion

        #region Functions

        private void Initialization()
        {
            infoDisplayTimer = new DispatcherTimer();
            infoDisplayTimer.Tick += infoDisplayTimer_Tick;
            infoDisplayTimer.Interval = new TimeSpan(0, InfoInterval, 0);
            infoDisplayTimer.Start();

            irc = new IrcClient("irc.twitch.tv", 6667, "hap_pybot", "oauth:" + oauth);
            bot = new Bot(irc);
            info = bot.LoadTextFile("_info");
            scroll.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;

            

            if (irc.Connected)
            {
                irc.JoinRoom(ChannelToJoin);
                StartThread();
            }
            else Application.Current.Shutdown();
        }

        private void StartThread()
        {
            thread = new Thread(CheckChat) { IsBackground = true };
            thread.Start();
        }

        private void CheckChat()
        {
            var paused = false;

            while (true)
            {
                var message = irc.ReadMessage();
                if (message != null)
                {
                    var localMessage = message;
                    Dispatcher.Invoke(() => recieveChat.Text += localMessage + "\n");
                    Dispatcher.Invoke(() => scroll.ScrollToBottom());


                    if (message.Contains("PING")) irc.Pong();
                    if (message.Contains("!Pause")) paused = true;
                    while (paused)
                    {
                        var pausedMessage = irc.ReadMessage();
                        if (pausedMessage.Contains("PING")) irc.Pong();
                        if (pausedMessage.Contains("!Pause")) paused = false;
                        Thread.Sleep(100);
                    }


                    if (message.Contains("!Info")) infoAllowed = !infoAllowed;
                    if (message.Contains("!Load")) bot.LoadNames(message);
                    if (message.Contains("!Save")) bot.SaveNames(message);
                    if (message.Contains("!Random")) bot.Random(message);

                    if (message.Contains("!Remove ") 
                        && !message.Contains("Vip")) bot.Remove(message);

                    if (message.Contains("!Remove") 
                        && message.Contains("Vip ")) bot.RemoveVip(message);

                    if (message.Contains("!Hello")) bot.GreetUser(message);
                    if (message.Contains("!RickRoll")) bot.RickRoll(message);
                    if (message.Contains("!Strippers")) bot.Strippers(message);
                    if (message.Contains("!Trials")) bot.Trials(message);
                    if (message.Contains("!Vip ")) bot.Vip(message);
                    if (message.Contains("!Donate")) bot.Donate();
                    if (message.Contains("!Slots")) bot.Spin(message);
                    if (message.Contains("!Score")) bot.ListScore(message);

                    if (message.Contains("!Commands") 
                        || message.Contains("!commands")) bot.Commands();

                    if (message.Contains("!List") 
                        && !message.Contains("Vip") 
                        && !message.Contains("Vip")) bot.List();

                    if ((message.Contains("!List") && message.Contains("Vip")) 
                        || message.Contains("!VipList") 
                        || message.Contains("!VIPList") 
                        || message.Contains("!ListVip")) bot.ListVip();
                }
                Thread.Sleep(40);
            }
            // ReSharper disable once FunctionNeverReturns
        }

        private void Send()
        {
            irc.SendChatMessage(sendChat.Text);
            recieveChat.Text += "PRIVMSG #" + "hap_pyBot" + " :" + sendChat.Text + "\n";
            sendChat.Text = "";
        }

        private void Shutdown()
        {
            var m = MessageBox.Show("Save the name list?", "Save?", MessageBoxButton.YesNoCancel);

            switch (m)
            {
                case MessageBoxResult.Yes:
                    bot.SaveNames(ChannelToJoin);
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

        #endregion
    }
}






using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace TwitchBot
{
    public partial class MainWindow : Window
    {
        #region Globals
        private IrcClient irc;
        private string channelToJoin;
        private string oauth;
        private List<string> joinedUsers;
        private List<string> vipUsers;
        private Stopwatch stopwatch;
        private Thread thread;
        private DispatcherTimer dispatcherTimer;
        private string[] info;
        private bool infoAllowed = true;
        #endregion
        #region Constructor
        public MainWindow(string channelToJoin, string oauth)
        {
            InitializeComponent();
            this.channelToJoin = channelToJoin;
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
            irc.SendChatMessage(sendChat.Text);
            recieveChat.Text += "PRIVMSG #" + "hap_pyBot" + " :" + sendChat.Text + "\n";
            sendChat.Text = "";
        }
        private void send_Enter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                irc.SendChatMessage(sendChat.Text);
                recieveChat.Text += "PRIVMSG #" + "hap_pyBot" + " :" + sendChat.Text + "\n";
                sendChat.Text = "";
            }

        }
        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
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
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (!infoAllowed) return; 

             if ((int)DateTime.Today.DayOfWeek == 1)
            {
                irc.SendChatMessage(info[0]);
            }

            else if ((int)DateTime.Today.DayOfWeek == 5
                  || (int)DateTime.Today.DayOfWeek == 6
                  || (int)DateTime.Today.DayOfWeek == 7)
            {
                irc.SendChatMessage(info[1]);
            }
            else
            {
                irc.SendChatMessage("/me It's either Tuesday, Wednesday, or Thursday, so, I have nothing to say...");
            }
        }
        #endregion
        #region Functions
        private void Initialization()
        {
            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 30, 0);
            dispatcherTimer.Start();

            joinedUsers = new List<string>();
            stopwatch = new Stopwatch();
            vipUsers = new List<string>();


            info = LoadTextFile("info.txt");
            scroll.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            irc = new IrcClient("irc.twitch.tv", 6667, "hap_pybot", "oauth:" + oauth);
            if (irc.connected == true)
            {
                JoinTwitchChatIrc();
                StartThread();
            }
            else Exit();

        }
        public void JoinTwitchChatIrc()
        {
            irc.JoinRoom(channelToJoin);
        }
        private void StartThread()
        {
            thread = new Thread(() => CheckChat());
            thread.IsBackground = true;
            thread.Start();
        }
        private void Exit()
        {
            Application.Current.Shutdown();
        }
        private string ParseUserName(string message)
        {
            string userName = message.Split('!')[0].TrimStart(new char[] { ':' });
            return userName;
        }
        private string[] LoadTextFile(string fileName)
        {
            string info = @AppDomain.CurrentDomain.BaseDirectory + "info.txt";
            if (!File.Exists(info))
            {
                File.WriteAllText(info, "/me Change what is displayed every 20 minutes in the info.txt file.");
            }

            string[] load = File.ReadAllText(@AppDomain.CurrentDomain.BaseDirectory + fileName).Split('%');
            return load;
        }
        private void CheckChat()
        {
            bool paused = false;

            while (true)
            {
                string message = irc.ReadMessage();
                if (message != null)
                {
                    Dispatcher.Invoke(() => recieveChat.Text += message + "\n");
                    Dispatcher.Invoke(() => scroll.ScrollToBottom());

                    if (message.Contains("!Pause")) paused = true ? false : true;
                    if (paused) return;

                    if (message.Contains("!Info")) infoAllowed = true ? false : true;
                    if (message.Contains("!Load")) Load(message);
                    if (message.Contains("!Save")) Save(message);
                    if (message.Contains("!Random")) Random(message);
                    if (message.Contains("!Remove ") && !message.Contains("VIP")) Remove(message);
                    if (message.Contains("!Remove") && message.Contains("VIP ")) RemoveVIP(message);

                    if (message.Contains("!Hello")) GreetUser(message);
                    if (message.Contains("!RickRoll")) RickRoll(message);
                    if (message.Contains("!Strippers")) Strippers(message);
                    if (message.Contains("!Trials")) Trials(message);
                    if (message.Contains("!VIP ")) VIP(message);
                    if (message.Contains("!Commands") || message.Contains("!commands")) Commands();
                    if (message.Contains("!Donate")) Donate();
                    if (message.Contains("!List") && !message.Contains("VIP") && !message.Contains("Vip")) List();

                    //if (message.Contains("!Slots")) Spin(message);
                    //if (message.Contains("!Score")) ListScore(message);

                    if ((message.Contains("!List") && message.Contains("VIP"))
                        || message.Contains("!VipList")
                        || message.Contains("!VIPList")
                        || message.Contains("!ListVip"))
                        ListVIP();

                    if (message.Contains("!Millersucks")
                        || message.Contains("!MillerSucks")
                        || message.Contains("!millersucks"))
                        MillerSucks();

                    if (message.Contains("!CandyBar")
                        || message.Contains("!Candybar")
                        || message.Contains("!candybar"))
                        CandyBar();

                    if (message.Contains("PING")) RespondToPing();

                    message = null;
                }
                Thread.Sleep(40);
            }
        }
        #endregion
        #region Respond Functions
        private void Spin(string message)
        {
            string user = ParseUserName(message);
            
            Tuple<string[], bool, int> results = Slots.Spin(user);
            if (results == null)
            {
                return;
            }
            irc.SendChatMessage("/me " + results.Item1[0] + " " + results.Item1[1] + " " + results.Item1[2]);

            int score = results.Item3;
            if (results.Item2 == true)
            {
                irc.SendChatMessage("/me " + user + " plus " + score + " points!");
            }
        }
        private void ListScore(string message)
        {
            string user = ParseUserName(message);
            irc.SendChatMessage("/me " + user + " has " + Slots.Score(user) + " points!");
        }
        private void GreetUser(string message)
        {
            string user = ParseUserName(message);
            irc.SendChatMessage("/me Hello " + user + "!");
        }
        private void Commands()
        {
            irc.SendChatMessage("/me My commands are !Trials, !Donate, !List, !VipList and a few others...");
        }
        private void MillerSucks()
        {
            irc.SendChatMessage("/me Yeah he does! Kappa");
        }
        private void CandyBar()
        {
            irc.SendChatMessage("/me lolol miller still sucks Kappa");
        }
        private void Donate()
        {
            irc.SendChatMessage("/me If you like my work feel free to donate! Though there is no obligation, it is greatly appreciated. Game on Guardians. " + "https://goo.gl/PNXlyb");
        }
        private void RespondToPing()
        {
            irc.Pong();
            Dispatcher.Invoke(() => recieveChat.Text += ("Pong" + "\n"));
        }
        private void Strippers(string message)
        {
            irc.SendChatMessage("/me ( . Y . )");
        }
        private void RickRoll(string message)
        {
            irc.SendChatMessage("/me Never gunna give you up never gunna let you down...");
        }
        private void Save(string message)
        {
            string userName = ParseUserName(message);
            if (userName != "shredhappy101" && userName != "amsuperfly" && userName != "swiftbloodshed") //userName != channelToJoin;
            {
                irc.SendChatMessage("/me No!");
                return;
            }

            string users = "";
            users = string.Join(",", joinedUsers);
            string vip = "";
            vip = string.Join(",", vipUsers);

            File.WriteAllText(@AppDomain.CurrentDomain.BaseDirectory + "names.txt", users + "%" + vip);
            irc.SendChatMessage("/me List saved!");
        }
        private void Load(string message)
        {
            string userName = ParseUserName(message);
            if (userName != "shredhappy101" && userName != "amsuperfly" && userName != "swiftbloodshed")
            {
                irc.SendChatMessage("/me No!");
                return;
            }

            string namesTmp = @AppDomain.CurrentDomain.BaseDirectory + "names.txt";
            if (!File.Exists(namesTmp))
            {
                File.CreateText(namesTmp);
                irc.SendChatMessage("/me The list is empty");
            }

            string[] names = LoadTextFile("names.txt");

            string[] users = names[0].Split(',');
            string[] VIP = names[1].Split(',');
            joinedUsers = users.ToList();
            vipUsers = VIP.ToList();
            irc.SendChatMessage("/me List loaded!");
        }
        private void Trials(string message)
        {
            string userName = ParseUserName(message);

            if (userName == "amsuperfly" || userName == "swiftbloodshed" || userName == "shredhappy101")
            {
                return;
            }

            if (vipUsers.Contains(userName))
            {
                irc.SendChatMessage("/me" + userName + ", you are already VIP!");
                return;
            }
            if (!joinedUsers.Contains(userName))
            {
                joinedUsers.Add(userName);
                irc.SendChatMessage("/me" + userName + " has signed up for Trials! ");
                return;
            }
            if (joinedUsers.Contains(userName))
            {
                irc.SendChatMessage("/me You've already signed up, " + userName + "! This guy's tryn' to cheat! Kappa");
                return;
            }
        }
        private void VIP(string message)
        {
            string userName = ParseUserName(message);

            if (userName != "amsuperfly" && userName != "swiftbloodshed")
            {
                irc.SendChatMessage("/me You do not have permission to add a VIP user!");
                return;
            }

            string userToVIP = message.Split(' ')[4];
            if (vipUsers.Contains(userToVIP))
            {
                irc.SendChatMessage("/me" + userName + " is already a VIP!");
            }
            joinedUsers.Remove(userToVIP);
            vipUsers.Add(userToVIP);
            irc.SendChatMessage("/me You added " + userToVIP + "!");
        }
        private void Remove(string message)
        {
            string userName = ParseUserName(message);
            if (userName != "amsuperfly" && userName != "swiftbloodshed" && userName != "shredhappy101")
            {
                irc.SendChatMessage("/me You do not have permission to remove a user!");
                return;
            }
            string userToRemove = message.Split(' ')[4];
            if (!joinedUsers.Contains(userToRemove))
            {
                irc.SendChatMessage(userToRemove + " is not on the list!");
                return;
            }
            joinedUsers.Remove(userToRemove);
            irc.SendChatMessage("/me You removed " + userToRemove + "!");
        }
        private void RemoveVIP(string message)
        {
            string userName = ParseUserName(message);

            if (userName != "amsuperfly" && userName != "swiftbloodshed")
            {
                irc.SendChatMessage("/me You do not have permission to remove a VIP user!");
                return;
            }
            string userToRemove = message.Split(' ')[4];
            if (!vipUsers.Contains(userToRemove))
            {
                irc.SendChatMessage(userToRemove + " is not a VIP!");
                return;
            }
            vipUsers.Remove(userToRemove);
            irc.SendChatMessage("/me You removed " + userToRemove + "!");
        }
        private void List()
        {
            if (joinedUsers.Count == 0)
            {
                irc.SendChatMessage("/me The Follower list is empty!");
                return;
            }
            string users = "";
            users = string.Join(", ", joinedUsers);
            irc.SendChatMessage("/me Followers: " + users);
        }
        private void ListVIP()
        {
            if (vipUsers.Count == 0)
            {
                irc.SendChatMessage("/me The VIP list is empty!");
                return;
            }
            string vip = "";
            vip = string.Join(", ", vipUsers);
            irc.SendChatMessage("/me VIP: " + vip);
        }
        private void Random(string message)
        {
            string userName = ParseUserName(message);
            if (userName != "amsuperfly" && userName != "swiftbloodshed" && userName != "shredhappy101")
            {
                irc.SendChatMessage("/me You do not have permission to use the randomizer");
                return;
            }
            Random rand = new Random();
            irc.SendChatMessage("/me Congrats! " + joinedUsers[rand.Next(1, joinedUsers.Count)] + " has been chosen for Trials!");
        }
        #endregion
    }
}






using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Threading;

namespace TwitchBot
{
    public static class Bot
    {
        private static List<string> joinedUsers = new List<string>();
        private static List<string> vipUsers = new List<string>();
        private static string dir = @AppDomain.CurrentDomain.BaseDirectory;


        private static string ParseUserName(string message)
        {
            string userName = message.Split('!')[0].TrimStart(new char[] { ':' });
            return userName;
        }
        public static string[] LoadTextFile(string fileName)
        {
            string text = fileName + ".txt";
            if (!File.Exists(text)) File.WriteAllText(text, "%");

            string[] load = File.ReadAllText(dir + text).Split('%');
            return load;
        }
        private static void ParseNames()
        {
            string[] names = LoadTextFile("names");
            string[] users = names[0].Split(',');
            string[] VIP = names[1].Split(',');
            joinedUsers = users.ToList();
            vipUsers = VIP.ToList();
        }
        private static bool IsAllowed(string userName)
        {
            if (userName != MainWindow.channelToJoin && userName != "amsuperfly")
            {
                IrcClient.SendChatMessage("/me No!");
                return false;
            }
            else return true;
        }

        #region Respond Functions
        public static void Spin(string message)
        {
            string user = ParseUserName(message);

            Tuple<string[], bool, int> results = Slots.Spin(user);
            if (results == null)
            {
                return;
            }
            IrcClient.SendChatMessage("/me " + results.Item1[0] + " " + results.Item1[1] + " " + results.Item1[2]);

            int score = results.Item3;
            if (results.Item2 == true)
            {
                IrcClient.SendChatMessage("/me " + user + " plus " + score + " points!");
            }
        }
        public static void ListScore(string message)
        {
            string user = ParseUserName(message);
            IrcClient.SendChatMessage("/me " + user + " has " + Slots.Score(user) + " points!");
        }
        public static void GreetUser(string message)
        {
            string user = ParseUserName(message);
            IrcClient.SendChatMessage("/me Hello " + user + "!");
        }
        public static void Commands()
        {
            IrcClient.SendChatMessage("/me My commands are !Hello, !Trials, !Donate, !List, !VipList, !Strippers, !RickRoll and a few others...");
        }
        public static void MillerSucks()
        {
            IrcClient.SendChatMessage("/me Yeah he does! Kappa");
        }
        public static void CandyBar()
        {
            IrcClient.SendChatMessage("/me lolol miller still sucks Kappa");
        }
        public static void Donate()
        {
            IrcClient.SendChatMessage("/me If you like my work feel free to donate! Though there is no obligation, it is greatly appreciated. Game on Guardians. " + "https://goo.gl/PNXlyb");
        }
        public static void Strippers(string message)
        {
            IrcClient.SendChatMessage("/me ( . Y . )");
        }
        public static void RickRoll(string message)
        {
            IrcClient.SendChatMessage("/me Never gunna give you up, never gunna let you down...");
        }
        public static void SaveNames(string message)
        {
            string userName = ParseUserName(message);
            if (IsAllowed(userName))
            {
                string users = "";
                users = string.Join(",", joinedUsers);

                string vip = "";
                vip = string.Join(",", vipUsers);

                File.WriteAllText(dir + "names.txt", users + "%" + vip);
                IrcClient.SendChatMessage("/me List saved!");
            }
        }
        public static void LoadNames(string message)
        {
            string userName = ParseUserName(message);
            IsAllowed(userName);
            ParseNames();
            IrcClient.SendChatMessage("/me List loaded!");
        }
        public static void Trials(string message)
        {
            string userName = ParseUserName(message);

            if (userName == "amsuperfly" || userName == "swiftbloodshed" || userName == "shredhappy101")
            {
                return;
            }

            if (vipUsers.Contains(userName))
            {
                IrcClient.SendChatMessage("/me" + userName + ", no need, VIP!");
                return;
            }
            if (!joinedUsers.Contains(userName))
            {
                joinedUsers.Add(userName);
                IrcClient.SendChatMessage("/me" + userName + " has signed up for Trials! ");
                return;
            }
            else
            {  
                IrcClient.SendChatMessage("/me You've already signed up, " + userName + "! This guy's tryn' to cheat! Kappa");
                return;
            }
        }
        public static void VIP(string message)
        {
            string userName = ParseUserName(message);

            if (userName != "amsuperfly" && userName != "swiftbloodshed")
            {
                IrcClient.SendChatMessage("/me You do not have permission to add a VIP user!");
                return;
            }

            string userToVIP = message.Split(' ')[4];
            if (vipUsers.Contains(userToVIP))
            {
                IrcClient.SendChatMessage("/me" + userName + " is already a VIP!");
            }
            joinedUsers.Remove(userToVIP);
            vipUsers.Add(userToVIP);
            IrcClient.SendChatMessage("/me You added " + userToVIP + "!");
        }
        public static void Remove(string message)
        {
            string userName = ParseUserName(message);
            if (IsAllowed(userName))
            {
                string userToRemove = message.Split(' ')[4];
                if (!joinedUsers.Contains(userToRemove))
                {
                    IrcClient.SendChatMessage(userToRemove + " is not on the list!");
                    return;
                }
                joinedUsers.Remove(userToRemove);
                IrcClient.SendChatMessage("/me You removed " + userToRemove + "!");
            }
        }
        public static void RemoveVIP(string message)
        {
            string userName = ParseUserName(message);

            if (IsAllowed(userName))
            {
                string userToRemove = message.Split(' ')[4];
                if (!vipUsers.Contains(userToRemove))
                {
                    IrcClient.SendChatMessage(userToRemove + " is not a VIP!");
                    return;
                }
                vipUsers.Remove(userToRemove);
                IrcClient.SendChatMessage("/me You removed " + userToRemove + "!");
            }
        }
        public static void List()
        {
            if (joinedUsers.Count == 0)
            {
                IrcClient.SendChatMessage("/me The Follower list is empty!");
                return;
            }
            string users = "";
            users = string.Join(", ", joinedUsers);
            IrcClient.SendChatMessage("/me Followers: " + users);
        }
        public static void ListVIP()
        {
            if (vipUsers.Count == 0)
            {
                IrcClient.SendChatMessage("/me The VIP list is empty!");
                return;
            }
            string vip = "";
            vip = string.Join(", ", vipUsers);
            IrcClient.SendChatMessage("/me VIP: " + vip);
        }
        public static void Random(string message)
        {
            string userName = ParseUserName(message);
            if (IsAllowed(userName))
            {
                Random rand = new Random();
                IrcClient.SendChatMessage("/me Congrats! " + joinedUsers[rand.Next(1, joinedUsers.Count)] + " has been chosen for Trials!");
            }
        }
        #endregion


    }
}

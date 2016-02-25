using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TwitchBot
{
    public static class Bot
    {
        private static List<string> _joinedUsers;
        private static List<string> _vipUsers;
        private static readonly string Dir = @AppDomain.CurrentDomain.BaseDirectory;
        private static readonly Random Rand;

        static Bot() 
        {
            _joinedUsers = new List<string>();
            _vipUsers = new List<string>();
            Rand = new Random();
        }

        private static string ParseUserName(string message)
        {
            var userName = message.Split('!')[0].TrimStart(':');
            return userName;
        }
        public static string[] LoadTextFile(string fileName)
        {
            var text = fileName + ".txt";
            if (!File.Exists(text)) File.WriteAllText(text, @"%");

            var load = File.ReadAllText(Dir + text).Split('%');
            return load;
        }
        private static void ParseNames()
        {
            var names = LoadTextFile("names");
            var users = names[0].Split(',');
            var vip = names[1].Split(',');
            _joinedUsers = users.ToList();
            _vipUsers = vip.ToList();
        }
        private static bool IsAllowed(string userName)
        {
            if (userName != MainWindow.ChannelToJoin && userName != "shredhappy101")
            {
                IrcClient.SendChatMessage("/me No!");
                return false;
            }
            else return true;
        }


        #region Respond Functions
        public static void Spin(string message)
        {
            var user = ParseUserName(message);

            var results = Slots.Spin(user);
            if (results == null)
            {
                return;
            }
            IrcClient.SendChatMessage("/me " + results.Item1[0] + " " + results.Item1[1] + " " + results.Item1[2]);

            var score = results.Item3;
            if (results.Item2)
            {
                IrcClient.SendChatMessage("/me " + user + " plus " + score + " points!");
            }
        }
        public static void ListScore(string message)
        {
            var user = ParseUserName(message);
            IrcClient.SendChatMessage("/me " + user + " has " + Slots.Score(user) + " points!");
        }
        public static void GreetUser(string message)
        {
            var user = ParseUserName(message);
            IrcClient.SendChatMessage("/me Hello " + user + "!");
        }
        public static void Commands()
        {
            IrcClient.SendChatMessage("/me My commands are !Hello, !Trials, !Donate, !List, !VipList, !Strippers, !RickRoll !Slots and a few others...");
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
            IrcClient.SendChatMessage("/me If you like my work feel free to donate! Though there is no obligation, it is greatly appreciated." + "LINK");
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
            var userName = ParseUserName(message);
            if (!IsAllowed(userName)) return;

            var users = string.Join(",", _joinedUsers);
            var vip = string.Join(",", _vipUsers);

            File.WriteAllText(Dir + "names.txt", users + @"%" + vip);
            IrcClient.SendChatMessage("/me List saved!");
        }
        public static void LoadNames(string message)
        {
            var userName = ParseUserName(message);
            IsAllowed(userName);
            ParseNames();
            IrcClient.SendChatMessage("/me List loaded!");
        }
        public static void Trials(string message)
        {
            var userName = ParseUserName(message);

            if (userName == MainWindow.ChannelToJoin) return;

            if (_vipUsers.Contains(userName))
            {
                IrcClient.SendChatMessage("/me" + userName + ", no need, Vip!");
                return;
            }
            if (!_joinedUsers.Contains(userName))
            {
                _joinedUsers.Add(userName);
                IrcClient.SendChatMessage("/me" + userName + " has signed up for Trials! ");
            }
            else
            {  
                IrcClient.SendChatMessage("/me You've already signed up, " + userName + "! This guy's tryn' to cheat! Kappa");
            }
        }
        public static void Vip(string message)
        {
            var userName = ParseUserName(message);

            if (!IsAllowed(userName))
            {
                IrcClient.SendChatMessage("/me You do not have permission to add a Vip user!");
                return;
            }

            var userToVip = message.Split(' ')[4];
            if (_vipUsers.Contains(userToVip))
            {
                IrcClient.SendChatMessage("/me" + userName + " is already a Vip!");
            }
            _joinedUsers.Remove(userToVip);
            _vipUsers.Add(userToVip);
            IrcClient.SendChatMessage("/me You added " + userToVip + "!");
        }
        public static void Remove(string message)
        {
            var userName = ParseUserName(message);
            if (!IsAllowed(userName)) return;
            var userToRemove = message.Split(' ')[4];
            if (!_joinedUsers.Contains(userToRemove))
            {
                IrcClient.SendChatMessage(userToRemove + " is not on the list!");
                return;
            }
            _joinedUsers.Remove(userToRemove);
            IrcClient.SendChatMessage("/me You removed " + userToRemove + "!");
        }
        public static void RemoveVip(string message)
        {
            var userName = ParseUserName(message);

            if (!IsAllowed(userName)) return;
            var userToRemove = message.Split(' ')[4];
            if (!_vipUsers.Contains(userToRemove))
            {
                IrcClient.SendChatMessage(userToRemove + " is not a Vip!");
                return;
            }
            _vipUsers.Remove(userToRemove);
            IrcClient.SendChatMessage("/me You removed " + userToRemove + "!");
        }
        public static void List()
        {
            if (_joinedUsers.Count == 0)
            {
                IrcClient.SendChatMessage("/me The Follower list is empty!");
                return;
            }
            var users = string.Join(", ", _joinedUsers);
            IrcClient.SendChatMessage("/me Followers: " + users);
        }
        public static void ListVip()
        {
            if (_vipUsers.Count == 0)
            {
                IrcClient.SendChatMessage("/me The Vip list is empty!");
                return;
            }
            var vip = string.Join(", ", _vipUsers);
            IrcClient.SendChatMessage("/me Vip: " + vip);
        }
        public static void Random(string message)
        {
            var userName = ParseUserName(message);
            if (IsAllowed(userName))
                IrcClient.SendChatMessage("/me Congrats! " + _joinedUsers[Rand.Next(1, _joinedUsers.Count)] +
                                          " has been chosen for Trials!");
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TwitchBot
{
    public class Bot
    {
        private List<string> joinedUsers;
        private List<string> vipUsers;
        private readonly string dir = @AppDomain.CurrentDomain.BaseDirectory;
        private readonly Random rand;
        private IrcClient irc;
        private Slots slots;

        public Bot(IrcClient irc) 
        {
            joinedUsers = new List<string>();
            vipUsers = new List<string>();
            rand = new Random();
            this.irc = irc;
            slots = new Slots();
        }

        private static string ParseUserName(string message) => message.Split('!')[0].TrimStart(':');

        public string MakeStr(params string[] strs) => string.Join(string.Empty, strs);

        public string[] LoadTextFile(string fileName)
        {
            var txt = fileName + ".txt";
            if (!File.Exists(txt)) File.WriteAllText(txt, @"%");
            return File.ReadAllText(dir + txt).Split('%');
        }

        private void ParseNames()
        {
            var names = LoadTextFile("names");
            joinedUsers = names[0].Split(',').ToList();
            vipUsers = names[1].Split(',').ToList();
        }

        private bool IsAllowed(string userName)
        {
            if (userName != MainWindow.ChannelToJoin && userName != "shredhappy101")
            {
                irc.SendChatMessage("/me No!");
                return false;
            }
            return true;
        }

        #region Respond Functions
        public void Spin(string message)
        {
            var user = ParseUserName(message);

            var results = slots.Spin(user);
            if (results == null)
            {
                return;
            }
            irc.SendChatMessage(MakeStr("/me ", results.Item1[0] ," " , results.Item1[1], " ", results.Item1[2]));

            var score = results.Item3;
            if (results.Item2)
            {
                irc.SendChatMessage(MakeStr("/me ", user, " plus ", score.ToString(), " points!"));
            }
        }
        public void ListScore(string message)
        {
            var user = ParseUserName(message);
            irc.SendChatMessage(MakeStr("/me ", user, " has ", slots.Score(user).ToString(), " points!"));
        }

        public void GreetUser(string message) => irc.SendChatMessage(MakeStr("/me Hello ", ParseUserName(message), "!"));

        public void Commands() => irc.SendChatMessage("/me My commands are !Hello, !Trials, !Donate, !List, !VipList, !Strippers, !RickRoll !Slots and a few others...");

        public void Donate() => irc.SendChatMessage(MakeStr("/me If you like my work feel free to donate! Though there is no obligation, it is greatly appreciated.", " LINK"));

        public void Strippers(string message) => irc.SendChatMessage("/me ( . Y . )");

        public void RickRoll(string message) => irc.SendChatMessage("/me Never gunna give you up, never gunna let you down...");

        public void SaveNames(string message)
        {
            if (!IsAllowed(ParseUserName(message))) return;

            var users = string.Join(",", joinedUsers);
            var vip = string.Join(",", vipUsers);

            File.WriteAllText(dir + "names.txt" + users + @"%", vip);
            irc.SendChatMessage("/me List saved!");
        }
        public void LoadNames(string message)
        {
            if (!IsAllowed(ParseUserName(message))) return;
            ParseNames();
            irc.SendChatMessage("/me List loaded!");
        }

        public void Trials(string message)
        {
            var userName = ParseUserName(message);

            if (ParseUserName(message) == MainWindow.ChannelToJoin) return;

            if (vipUsers.Contains(userName))
            {
                irc.SendChatMessage(MakeStr("/me", userName, ", no need, Vip!"));
                return;
            }
            if (!joinedUsers.Contains(userName))
            {
                joinedUsers.Add(userName);
                irc.SendChatMessage(MakeStr("/me", userName, " has signed up for Trials! "));
            }
            else
            {
                irc.SendChatMessage(MakeStr("/me You've already signed up, ", userName, "! This guy's tryn' to cheat! Kappa"));
            }
        }
        public void Vip(string message)
        {
            var userName = ParseUserName(message);

            if (!IsAllowed(userName))
            {
                irc.SendChatMessage("/me You do not have permission to add a Vip user!");
                return;
            }

            var userToVip = message.Split(' ')[4];
            if (vipUsers.Contains(userToVip))
            {
                irc.SendChatMessage(MakeStr("/me", userName, " is already a Vip!"));
            }
            joinedUsers.Remove(userToVip);
            vipUsers.Add(userToVip);
            irc.SendChatMessage(MakeStr("/me You added ", userToVip, "!"));
        }
        public void Remove(string message)
        {
            var userName = ParseUserName(message);
            if (!IsAllowed(userName)) return;
            var userToRemove = message.Split(' ')[4];
            if (!joinedUsers.Contains(userToRemove))
            {
                irc.SendChatMessage(MakeStr(userToRemove, " is not on the list!"));
                return;
            }
            joinedUsers.Remove(userToRemove);
            irc.SendChatMessage(MakeStr("/me You removed ", userToRemove, "!"));
        }
        public void RemoveVip(string message)
        {
            var userName = ParseUserName(message);

            if (!IsAllowed(userName)) return;
            var userToRemove = message.Split(' ')[4];
            if (!vipUsers.Contains(userToRemove))
            {
                irc.SendChatMessage(MakeStr(userToRemove, " is not a Vip!"));
                return;
            }
            vipUsers.Remove(userToRemove);
            irc.SendChatMessage(MakeStr("/me You removed ", userToRemove, "!"));
        }
        public void List()
        {
            if (joinedUsers.Count == 0)
            {
                irc.SendChatMessage("/me The Follower list is empty!");
                return;
            }
            var users = string.Join(", ", joinedUsers);
            irc.SendChatMessage(MakeStr("/me Followers: ", users));
        }
        public void ListVip()
        {
            if (vipUsers.Count == 0)
            {
                irc.SendChatMessage("/me The Vip list is empty!");
                return;
            }
            var vip = string.Join(", ", vipUsers);
            irc.SendChatMessage(MakeStr("/me Vip: ", vip));
        }
        public void Random(string message)
        {
            var userName = ParseUserName(message);
            if (IsAllowed(userName))
                irc.SendChatMessage(MakeStr("/me Congrats! ", joinedUsers[rand.Next(1, joinedUsers.Count)],
                                          " has been chosen for Trials!"));
        }
        #endregion
    }
}

﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TwitchBot
{
    static class Slots
    {
        #region Globals
        private static string[] icons, state;
        private static Dictionary<string, int> scores;
        private static int index = 0;
        private static Stopwatch stopwatch;
        private static bool throttled, win = false;
        #endregion

        #region Constructor
        static Slots()
        {
            icons = new string[] { "<3", "MrDestructoid", "BloodTrail", "OSrob", "SMSkull" };
            scores = new Dictionary<string, int>();
            state = new string[3];
            stopwatch = new Stopwatch();
        }
        #endregion

        #region Functions
        private static string CheckStates()
        {
            if (state[0] == state[1] && state[1] == state[2]) return state[0];

            return "";
        }

        private static string getRandIcon()
        {
            Random rand = new Random();
            int randIcon = rand.Next(0, 90);

            if      (randIcon <= 9) return icons[4];
            else if (randIcon <= 20) return icons[3];
            else if (randIcon <= 35) return icons[2];
            else if (randIcon <= 60) return icons[1];
            else if (randIcon <= 90) return icons[0];
            else return null;
        }

        private static void UpdateDict(string user, int score)
        {
            if (scores.ContainsKey(user)) scores[user] += score; 
            else scores.Add(user, score);
            if (scores[user] < 0) scores[user] = 0;
        }

        private static void SetState(string user, string icon)
        {
            switch (icon)
            {
                case "<3":
                    state[index] = "<3";
                    break;
                case "MrDestructoid":
                    state[index] = "MrDestructoid";
                    break;
                case "BloodTrail":
                    state[index] = "BloodTrail";
                    break;
                case "OSrob":
                    state[index] = "OSrob";
                    break;
                case "SMSkull":
                    state[index] = "SMSkull";
                    break;
                default:
                    break;
            }
        }

        private static int GetScoreToAdd()
        {
            int score = 0;
            string icon = CheckStates();
            if      (icon == "<3")            return score = 1;
            else if (icon == "MrDestructoid") return score = 2;
            else if (icon == "BloodTrail")    return score = 5;
            else if (icon == "OSrob")         return score = 10;
            else if (icon == "SMSkull")       return score = -5;
            else return score;
        }

        public static Tuple<string[], bool, int> Spin(string user)
        {
            while (throttled)
            {
                if (stopwatch.Elapsed.Seconds >= 15)
                {
                    stopwatch.Stop();
                    throttled = false;
                }
                Thread.Sleep(80);
            }

            if (!throttled )
            {
                if (stopwatch.Elapsed.Seconds >= 15)
                {
                    stopwatch.Stop();
                    throttled = false;
                }

                for (index = 0; index < 3; index++) SetState(user, getRandIcon());

                index = 0;
                throttled = true;                           
                UpdateDict(user, GetScoreToAdd());
                stopwatch.Start();
                return Tuple.Create(state, win, scores[user]);
            }

            return null;
        }

        public static int Score(string user)
        {
            if (scores.ContainsKey(user))
            {
                int score = scores[user];
                return score;
            }
            else
            {
                scores.Add(user, 0);
                return 0;
            }
        }
        #endregion
    }
    }
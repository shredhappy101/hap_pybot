using System;
using System.Collections.Generic;

namespace TwitchBot
{
    public static class Slots
    {
        #region Globals
        private static readonly string[] Icons, State;
        private static readonly Dictionary<string, int> Scores;
        private static int _index;
        private const bool Win = false;
        private static readonly Random Rand;
        #endregion

        #region Constructor
        static Slots()
        {
            Icons = new[] { "<3", "MrDestructoid", "BloodTrail", "Kappa", "SMSkull" };
            Scores = new Dictionary<string, int>();
            State = new string[3];
            Rand = new Random();
        }
        #endregion

        #region Functions
        private static string CheckStates()
        {
            if (State[0] == State[1] && State[1] == State[2]) return State[0];

            return "";
        }

        private static string GetRandIcon()
        {
            var randIcon = Rand.Next(0, 90);

            if (randIcon <= 9) return Icons[4];
            if (randIcon <= 20) return Icons[3];
            if (randIcon <= 35) return Icons[2];
            if (randIcon <= 60) return Icons[1];
            if (randIcon <= 90) return Icons[0];
            return null;
        }

        private static void UpdateDict(string user, int score)
        {
            if (Scores.ContainsKey(user)) Scores[user] += score;
            else Scores.Add(user, score);
            if (Scores[user] < 0) Scores[user] = 0;
        }

        private static void SetState(string icon)
        {
            switch (icon)
            {
                case "<3":
                    State[_index] = "<3";
                    break;
                case "MrDestructoid":
                    State[_index] = "MrDestructoid";
                    break;
                case "BloodTrail":
                    State[_index] = "BloodTrail";
                    break;
                case "OSrob":
                    State[_index] = "OSrob";
                    break;
                case "SMSkull":
                    State[_index] = "SMSkull";
                    break;
            }
        }

        private static int GetScoreToAdd()
        {
            var icon = CheckStates();
            switch (icon)
            {
                case "<3":
                    return 1;
                case "MrDestructoid":
                    return 2;
                case "BloodTrail":
                    return 5;
                case "OSrob":
                    return 10;
                case "SMSkull":
                    return -5;
                default:
                    return 0;
            }
        }

        public static Tuple<string[], bool, int> Spin(string user)//need throttle
        {
            for (_index = 0; _index < 3; _index++) SetState(GetRandIcon());
            _index = 0;
            UpdateDict(user, GetScoreToAdd());
            return Tuple.Create(State, Win, Scores[user]);
        }

        public static int Score(string user)
        {
            if (Scores.ContainsKey(user))
            {
                var score = Scores[user];
                return score;
            }
            Scores.Add(user, 0);
            return 0;
        }
        #endregion
    }
}

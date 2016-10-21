using System;
using System.Collections.Generic;

namespace TwitchBot
{
    public class Slots
    {
        #region Globals
        private readonly string[] icons, state;
        private readonly Dictionary<string, int> scores;
        private int index;
        private const bool Win = false;
        private readonly Random rand;
        #endregion

        #region Constructor
        public Slots()
        {
            icons = new[] { "<3", "MrDestructoid", "BloodTrail", "Kappa", "SMSkull" };
            scores = new Dictionary<string, int>();
            state = new string[3];
            rand = new Random();
        }
        #endregion

        #region Functions
        private string CheckStates()
        {
            if (state[0] == state[1] && state[1] == state[2]) return state[0];
            return string.Empty;
        }

        private string GetRandIcon()
        {
            var randIcon = rand.Next(0, 90);

            if (randIcon <= 9) return icons[4];
            if (randIcon <= 20) return icons[3];
            if (randIcon <= 35) return icons[2];
            if (randIcon <= 60) return icons[1];
            return randIcon <= 90 ? icons[0] : null;
        }

        private void UpdateDict(string user, int score)
        {
            if (scores.ContainsKey(user)) scores[user] += score;
            else scores.Add(user, score);
            if (scores[user] < 0) scores[user] = 0;
        }

        private void SetState(string icon)
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
            }
        }

        private int GetScoreToAdd()
        {
            switch (CheckStates())
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

        public Tuple<string[], bool, int> Spin(string user) //needs throttling
        {
            for (index = 0; index < 3; index++) SetState(GetRandIcon());
            index = 0;
            UpdateDict(user, GetScoreToAdd());
            return Tuple.Create(state, Win, scores[user]);
        }

        public int Score(string user)
        {
            if (scores.ContainsKey(user)) return scores[user];
            scores.Add(user, 0);
            return 0;
        }
        #endregion
    }
}

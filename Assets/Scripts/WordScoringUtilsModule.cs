using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ArtScan.TeamsModule;

namespace ArtScan.WordScoringUtilsModule
{
    public static class WordScoring
    {
        public static string ChooseRandomFromList(List<string> list)
        {
            int index = UnityEngine.Random.Range(0,list.Count);
            return list[index];
        }
        public static string GetWinner(List<string> wordChoices, Team[] teams, Dictionary<string,Dictionary<string,int>> wordPoints)
        {
            Dictionary<string,int> scores = GetScores(wordChoices, wordPoints);

            List<string> exclude = new List<string>();
            foreach (Team team in teams)
            {
                if (!String.IsNullOrEmpty(team.namesake) && !exclude.Contains(team.namesake))
                {
                    exclude.Add(team.namesake);
                }
            }
                                
            IEnumerable<KeyValuePair<string,int>> filtered = scores.Where(kvp => !exclude.Contains(kvp.Key));

            List<string> best = GetGreatest(filtered);

            string winner = ChooseRandomFromList(best);

            return winner;
        }
        public static List<string> GetGreatest(IEnumerable<KeyValuePair<string,int>> scores)
        {
            int greatest = 0;
            List<string> best = new List<string>();
            foreach(KeyValuePair<string,int> score in scores)
            {
                string personName = score.Key;

                if (score.Value > greatest)
                {
                    greatest = score.Value;
                    best = new List<string>() {personName};
                }
                else if (score.Value == greatest)
                {
                    best.Add(personName);
                }
            }
            return best;
        }

        public static Dictionary<string,int> GetScores(List<string> choices, Dictionary<string,Dictionary<string,int>> wordPoints)
        {

            Dictionary<string,int> scores = new Dictionary<string,int>();

            foreach(string choice in choices)
            {
                Dictionary<string,int> points = wordPoints[choice];

                foreach(KeyValuePair<string,int> point in points)
                {
                    string personName = point.Key;
                    
                    if (!scores.ContainsKey(personName))
                    {
                        scores.Add(personName,0);
                    }

                    scores[personName] = scores[personName] + point.Value;
                }
            }

            return scores;
        }
    }
}

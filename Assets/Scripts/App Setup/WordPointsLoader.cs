using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using ArtScan;
using ArtScan.WordScoringUtilsModule;
using rlmg.logging;

namespace ArtScan.WordPoints
{
    [System.Serializable]
    public class WordPointsContent
    {
        public Dictionary< string, Dictionary<string,int> > wordPoints;
    }

    public class WordPointsLoader : ContentLoader
    {
        public GameState gameState;
        public WordPointsContent wordPointsContent;

        protected override IEnumerator PopulateContent(string contentData)
        {
            if (gameState == null)
                yield break;

            List<Dictionary<string, object>> csvData = CSVReader.Read(contentData);

            if (csvData == null)
                yield break;

            wordPointsContent = new WordPointsContent();
            wordPointsContent.wordPoints = new Dictionary< string, Dictionary<string,int> >();

            for (var r = 0; r < csvData.Count; r++)
            {
                Dictionary<string,object> row = csvData[r];

                string name = "";
                Dictionary<string,int> points = new Dictionary<string,int>();

                foreach(KeyValuePair<string,object> column in row)
                {
                    if (column.Key == "Word")
                    {
                        name = column.Value.ToString();
                    }
                    else
                    {
                        points.Add( column.Key, GetIntFromString(column.Value.ToString()) );
                    }                
                
                }

                wordPointsContent.wordPoints.Add(name,points);
            }

            // yield return StartCoroutine(LoadImagesViaFilenames(timelineContent));
            // Debug.Log("...loaded "+wordPointsContent.wordPoints.Count+" wordpoints.");
            RLMGLogger.Instance.Log("...loaded "+wordPointsContent.wordPoints.Count+" wordpoints.", MESSAGETYPE.INFO);
            gameState.wordPointsContent = wordPointsContent;

            // SampleOdds();

        }

        public IEnumerable<TValue> RandomValues<TKey, TValue>(IDictionary<TKey, TValue> dict)
        {
            List<TValue> values = Enumerable.ToList(dict.Values);
            int size = dict.Count;
            while(true)
            {
                yield return values[UnityEngine.Random.Range(0,dict.Count)];
            }
        }

        public IEnumerable<TKey> RandomKeys<TKey, TValue>(IDictionary<TKey, TValue> dict)
        {
            List<TKey> keys = Enumerable.ToList(dict.Keys);
            while(true)
            {
                yield return keys[UnityEngine.Random.Range(0,dict.Count)];
            }
        }

        private void SampleOdds()
        {
            Dictionary< string, Dictionary<string,int> > wordPoints = wordPointsContent.wordPoints;
            //scores array
            Dictionary<string,int> scores = new Dictionary<string, int>();
            foreach (string name in wordPoints.First().Value.Keys)
            {
                scores[name] = 0;
            }
            //selection loop
            int tieCount = 0;
            int times = 100000;
            for (int i=0; i<times; i++)
            {
                List<string> chosen = RandomKeys(wordPoints).Take(3).ToList();

                Dictionary<string,int> chosenScores = WordScoring.GetScores(chosen, wordPoints);

                List<string> best = WordScoring.GetGreatest(chosenScores);
                if (best.Count > 1) tieCount += 1;
                string winner = WordScoring.ChooseRandomFromList(best);

                scores[winner] += 1;
            }
            //results
            string results = "";
            foreach (KeyValuePair<string,int> kvp in scores)
            {
                results += "Person: " + kvp.Key + ", count: " + kvp.Value + ", percentage: " + Mathf.Round( (float)kvp.Value / (float)times * 100 ).ToString() + "%\n";
            }
            Debug.Log(results);
            Debug.Log("Ties: " + tieCount + " percentage: " + Mathf.Round( (float)tieCount / (float)times * 100 ).ToString() + "%");

        }

        float GetFloatFromString(string stringToParse)
        {
            if (!string.IsNullOrEmpty(stringToParse))
            {
                float parsedResult;

                if (float.TryParse(stringToParse, out parsedResult))
                {
                    return parsedResult;
                }
                else
                {
                    Debug.LogError(stringToParse + " can't be parsed into a float.");

                    return 0f;
                }
            }
            else
            {
                return 0f;
            }
        }

        int GetIntFromString(string stringToParse)
        {
            if (!string.IsNullOrEmpty(stringToParse))
            {
                int parsedResult;

                if (int.TryParse(stringToParse, out parsedResult))
                {
                    return parsedResult;
                }
                else
                {
                    Debug.LogError(stringToParse + " can't be parsed into an integer.");

                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }
    }
}



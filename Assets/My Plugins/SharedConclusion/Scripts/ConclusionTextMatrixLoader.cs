using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using rlmg.logging;

public class ConclusionTextMatrixLoader : ContentLoader
{
    //public ConclusionTextManager conclusionText;

    public ConclusionTextMatrix contentMatrix;

    protected override IEnumerator PopulateContent(string contentData)
    {
        // if (conclusionText == null)
        //     conclusionText = GetComponent<ConclusionTextManager>();

        // if (conclusionText == null)
        //     yield break;


        List<Dictionary<string, object>> csvData = CSVReader.Read(contentData);


        if (csvData == null)
            yield break;


        //ConclusionTextMatrix contentMatrix = new ConclusionTextMatrix();
        contentMatrix = new ConclusionTextMatrix();
        contentMatrix.options = new ConclusionTextMatrix.ConclusionTextOption[csvData.Count];

        for (var i = 0; i < csvData.Count; i++)
        {
            contentMatrix.options[i] = new ConclusionTextMatrix.ConclusionTextOption();

            string roverSuccessString = csvData[i]["Rover"].ToString();
            contentMatrix.options[i].roverSuccess = GetIntFromString(roverSuccessString);

            string mapSuccessString = csvData[i]["Map"].ToString();
            contentMatrix.options[i].mapSuccess = GetIntFromString(mapSuccessString);

            string artSuccessString = csvData[i]["Art"].ToString();
            contentMatrix.options[i].artSuccess = GetIntFromString(artSuccessString);

            string charterSuccessString = csvData[i]["Charter"].ToString();
            contentMatrix.options[i].charterSuccess = GetIntFromString(charterSuccessString);

            string treasureSuccessString = csvData[i]["Treasure"].ToString();
            contentMatrix.options[i].treasureSuccess = GetIntFromString(treasureSuccessString);

            contentMatrix.options[i].sentence1 = csvData[i]["Sentence 1"].ToString().Replace("<br>", "\n").Replace("\"\"", "\"");;
            contentMatrix.options[i].sentence2 = csvData[i]["Sentence 2"].ToString().Replace("<br>", "\n").Replace("\"\"", "\"");;
        }

        //conclusionText.contentMatrix = contentMatrix;
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
                //Debug.LogError(stringToParse + " can't be parsed into a float.");
                RLMGLogger.Instance.Log(stringToParse + " can't be parsed into a float.", MESSAGETYPE.ERROR);


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
                //Debug.LogError(stringToParse + " can't be parsed into an integer.");
                RLMGLogger.Instance.Log(stringToParse + " can't be parsed into an integer.", MESSAGETYPE.ERROR);

                return 0;
            }
        }
        else
        {
            return 0;
        }
    }
}
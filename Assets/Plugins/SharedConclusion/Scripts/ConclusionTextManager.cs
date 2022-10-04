using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ConclusionTextManager : MonoBehaviour
{
    // public ConclusionTextMatrix contentMatrix;
    public ConclusionTextMatrixLoader contentMatrixLoader;

    public TMP_Text uiText;

    public float textFadeInDur = 1f;
    
    void Start()
    {
        
    }

    void OnEnable()
    {
        if (Client.instance == null || Client.instance.team == null || Client.instance.team.MoonshotTeamData == null)
        {
            Debug.Log("ConclusionTextManager couldn't find team data through Client.instance");
            
            return;
        }

        if (uiText == null)
        {
            return;
        }

        uiText.text = string.Empty;

        StartCoroutine(UpdateTextAfterLoadingMatrix());
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    void Update()
    {
        if (uiText != null && uiText.color.a < 1f && !string.IsNullOrEmpty(uiText.text))
        {
            uiText.color = new Color(uiText.color.r, uiText.color.g, uiText.color.b, uiText.color.a + Time.deltaTime / textFadeInDur);
        }
    }

    IEnumerator UpdateTextAfterLoadingMatrix()
    {
        if (contentMatrixLoader == null)
        {
            yield break;
        }

        //while (!contentMatrixLoader.HasLoadedContent)  //I'd need to reference the loader for this
        while (contentMatrixLoader.contentMatrix == null || contentMatrixLoader.contentMatrix.options == null || contentMatrixLoader.contentMatrix.options.Length < 1)
        {
            yield return null;
        }
        
        UpdateText(contentMatrixLoader.contentMatrix);
    }

    void UpdateText(ConclusionTextMatrix contentMatrix)
    {
        //TODO: wait until the matrix CSV is loaded!
        
        // if (Client.instance == null || Client.instance.team == null || Client.instance.team.MoonshotTeamData == null)
        // {
        //     Debug.Log("ConclusionTextManager couldn't find team data through Client.instance");
            
        //     return;
        // }

        // if (uiText == null)
        // {
        //     return;
        // }

        bool roverSuccess = Client.instance.team.MoonshotTeamData.roverStepsCompleted >= 2;
        bool mapSuccess = Client.instance.team.MoonshotTeamData.mapRoundsCompleted >= 2;
        bool artSuccess = Client.instance.team.MoonshotTeamData.artworks != null && Client.instance.team.MoonshotTeamData.artworks.Length > 0;
        bool charterSuccess = Client.instance.team.MoonshotTeamData.charterQsCompleted >= 4;
        bool treasureSuccess = Client.instance.team.MoonshotTeamData.huntNumFound >= 4;

        Debug.Log("ConclusionTextManager    roverSuccess=" + roverSuccess + "   mapSuccess=" + mapSuccess + "   artSuccess=" + artSuccess + "   charterSuccess=" + charterSuccess + "   treasureSuccess=" + treasureSuccess);
        //Debug.Log("System.Convert.ToBoolean(contentMatrix.options[0].roverSuccess) = " + System.Convert.ToBoolean(contentMatrix.options[0].roverSuccess));
        
        for (int i = 0; i < contentMatrix.options.Length; i++)
        {
            if (roverSuccess == System.Convert.ToBoolean(contentMatrixLoader.contentMatrix.options[i].roverSuccess) &&
                mapSuccess == System.Convert.ToBoolean(contentMatrix.options[i].mapSuccess) &&
                artSuccess == System.Convert.ToBoolean(contentMatrix.options[i].artSuccess) &&
                charterSuccess == System.Convert.ToBoolean(contentMatrix.options[i].charterSuccess) &&
                treasureSuccess == System.Convert.ToBoolean(contentMatrix.options[i].treasureSuccess))
            {
                uiText.text = contentMatrix.options[i].sentence1 + "\n\n" + contentMatrix.options[i].sentence2;

                Debug.Log("dynamic conclusion text: " + contentMatrix.options[i].sentence1 + "\n\n" + contentMatrix.options[i].sentence2);

                uiText.color = new Color(uiText.color.r, uiText.color.g, uiText.color.b, 0f);

                break;
            }

        }
    }
}

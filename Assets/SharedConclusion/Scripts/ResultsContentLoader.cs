using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

public class ResultsContentLoader : ContentLoader
{
    public TeamArtworks teamArtworks;

    public AudioClip placeholderReadAloudAudio;

    [System.Serializable]
    public class TeamArtworks
    {
        public int teamIndex;
        public Artwork[] artworks;
    }

    [System.Serializable]
    public class Artwork
    {
        public string filePath;
        public Image image;
    }
    
    protected override IEnumerator PopulateContent(string contentData)
    {
        teamArtworks = JsonConvert.DeserializeObject<TeamArtworks>(contentData);

        //yield return StartCoroutine(LoadImagesViaFilenames(honoreeContent));
        yield return StartCoroutine(LoadImagesViaFilenames());

        //InstantiateCellsFromContent(honoreeContent);
        // InstantiateCellsFromContent();

        // SortByDeathDate();
    }

    public IEnumerator LoadImagesViaFilenames()
    {
        for (int i = 0; i < teamArtworks.artworks.Length; i++)
        {
            if (!string.IsNullOrEmpty(teamArtworks.artworks[i].filePath))
            {
                //string imgFilePath = "";
                //string imgFilePath = teamArtworks.artworks[i].filePath;
                string imgFilePath = GetCachedFilePath(teamArtworks.artworks[i].filePath, ContentDirectory);
                
                yield return StartCoroutine(LoadSpriteFromFilepath(imgFilePath, result => teamArtworks.artworks[i].image.sprite = result));
            }
        }
    }
    
}

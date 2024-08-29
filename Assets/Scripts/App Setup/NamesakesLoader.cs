using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.EventSystems;
using rlmg.logging;
using ArtScan;

namespace ArtScan.NamesakesModule
{
    [System.Serializable]
    public class Namesake
    {
        public string moonbaseName;

        public string moonbaseNameImageFilename;
        public Texture2D moonbaseNameImage;
        public string fullName;
        public string description;

        public string imageFilename;
        public Texture2D texture;
    }

    public class NamesakesLoader : ContentLoader
    {
        public EventSystem eventSystem;
        public GameState gameState;

        public string moonbaseNameImageDir = "Namesake_Texts";
        public string imageDir = "Namesake_Images";

        private Texture2D GetTexture2DFromPath(string path)
        {
            Texture2D tex = new Texture2D(2,2);
            tex.name = "Config texture for " + path;

            try
            {
                byte[] bytes = File.ReadAllBytes(path);
                tex.LoadImage(bytes);
            }
            catch (Exception e)
            {
                RLMGLogger.Instance.Log(String.Format("Failed to read namesake images: {0}.",e.ToString()), MESSAGETYPE.ERROR);
            }

            return tex;
        }

        protected override IEnumerator PopulateContent(string contentData)
        {
            if (gameState != null)
            {
                gameState.namesakesData = JsonConvert.DeserializeObject<Dictionary<string,Namesake>>(contentData);

                if (gameState.namesakesData == null)
                    yield break;

                string moonbaseNameImageDirPath = Path.Join(Application.streamingAssetsPath,moonbaseNameImageDir);
                string dirPath = Path.Join(Application.streamingAssetsPath,imageDir);
                

                foreach (KeyValuePair<string,Namesake> kvp in gameState.namesakesData)
                {
                    Namesake namesake = kvp.Value;

                    string moonbaseNameImagePath = Path.Join( moonbaseNameImageDirPath, namesake.moonbaseNameImageFilename );
                    namesake.moonbaseNameImage = GetTexture2DFromPath(moonbaseNameImagePath);

                    string imagePath = Path.Join(dirPath,namesake.imageFilename);
                    namesake.texture = GetTexture2DFromPath(imagePath);
                }

                yield break;
            }
        }
    }
}





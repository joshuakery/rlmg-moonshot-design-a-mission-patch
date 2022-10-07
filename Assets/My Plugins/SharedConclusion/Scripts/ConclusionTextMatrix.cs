using UnityEngine;
  
[System.Serializable]
public class ConclusionTextMatrix
{
    public ConclusionTextOption[] options;
  
    [System.Serializable]
    public class ConclusionTextOption
    {
        public int roverSuccess;
        public int mapSuccess;
        public int artSuccess;
        public int charterSuccess;
        public int treasureSuccess;

        public string sentence1;
        public string sentence2;
    }
}
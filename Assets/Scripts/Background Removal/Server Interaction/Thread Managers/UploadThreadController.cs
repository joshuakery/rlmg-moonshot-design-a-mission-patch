using UnityEngine;
using System.Collections;
using rlmg.logging;

namespace ArtScan.ScanSavingModule
{
    [CreateAssetMenu(menuName = "Thread Controller/Upload Thread Controller"), System.Serializable]
    public class UploadThreadController : ScriptableObject
    {
        //Thread Class
        private class UploadThread : MultiThreading.ThreadedJob
        {
            //parameters
            public string filename;

            protected override void ThreadFunction()
            {
                if (RLMGLogger.Instance != null)
                    RLMGLogger.Instance.Log(string.Format("Uploading {0}", filename), MESSAGETYPE.INFO);
                else
                    Debug.Log(string.Format("Uploading {0}", filename));

                ClientSend.SendFileToServer(filename);
            }
        }

        private UploadThread uploadThread;

        public void AbortThread()
        {
            if (uploadThread != null && !uploadThread.IsDone)
            {
                Debug.Log("Ending parallel upload thread...");
                uploadThread.Abort();
                uploadThread = null;
                Debug.Log("...ended.");
            }
        }

        //Methods
        public void Upload(string filename)
        {
            if (uploadThread == null || uploadThread.IsDone)
            {
                uploadThread = new UploadThread();
                uploadThread.filename = filename;

                uploadThread.Start();
            }
        }

        public IEnumerator UploadCoroutine(string filename)
        {
            if (uploadThread == null || uploadThread.IsDone)
            {
                uploadThread = new UploadThread();
                uploadThread.filename = filename;

                uploadThread.Start();

                yield return uploadThread.WaitFor();
            }
        }

    }
}






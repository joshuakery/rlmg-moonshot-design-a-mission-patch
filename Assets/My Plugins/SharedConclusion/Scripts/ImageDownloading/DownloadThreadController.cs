using UnityEngine;
using System.Collections;
using System.Threading;
using rlmg.logging;

[CreateAssetMenu(menuName = "Thread Controller/Download Thread Controller"), System.Serializable]
public class DownloadThreadController : ScriptableObject
{
    //Thread Class
    public class DownloadThread : MultiThreading.ThreadedJob
    {
        //parameters
        public string filename;
        public string dirPath;

        protected override void ThreadFunction(CancellationToken token)
        {
            if (!System.String.IsNullOrEmpty(filename) && !System.String.IsNullOrEmpty(dirPath))
            {
                ClientSend.GetFileFromServer(filename, dirPath);
            }
                
        }
    }

    private DownloadThread downloadThread;

    public void AbortThread()
    {
        if (downloadThread != null && !downloadThread.IsDone)
        {
            Debug.Log("Ending parallel download thread...");
            downloadThread.Abort();
            Debug.Log("...ended.");
        }
    }

    //Methods
    public void Download(string filename, string dirPath)
    {
        if (downloadThread == null || downloadThread.IsDone)
        {
            downloadThread = new DownloadThread();
            downloadThread.filename = filename;
            downloadThread.dirPath = dirPath;

            downloadThread.Start();
        }
    }

    public IEnumerator DownloadCoroutine(string filename, string dirPath)
    {
        if (downloadThread == null || downloadThread.IsDone)
        {
            downloadThread = new DownloadThread();
            downloadThread.filename = filename;
            downloadThread.dirPath = dirPath;

            downloadThread.Start();

            yield return downloadThread.WaitFor();
        }
    }
}


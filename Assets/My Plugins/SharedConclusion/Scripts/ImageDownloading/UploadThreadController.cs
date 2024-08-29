using UnityEngine;
using System.Collections;
using System.Threading;
using rlmg.logging;

[CreateAssetMenu(menuName = "Thread Controller/Upload Thread Controller"), System.Serializable]
public class UploadThreadController : ScriptableObject
{
    //Thread Class
    private class UploadThread : MultiThreading.ThreadedJob
    {
        //parameters
        public string filename;

        protected override void ThreadFunction(CancellationToken token)
        {
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
            Debug.Log("...ended.");
        }
    }

    //Methods
    public void Upload(string filename)
    {
        if (uploadThread == null || uploadThread.IsDone)
        {
            UploadThread uploadThread = new UploadThread();
            uploadThread.filename = filename;

            uploadThread.Start();
        }
    }

    public IEnumerator UploadCoroutine(string filename)
    {
        if (uploadThread == null || uploadThread.IsDone)
        {
            UploadThread uploadThread = new UploadThread();
            uploadThread.filename = filename;

            uploadThread.Start();

            yield return uploadThread.WaitFor();
        }    
    }

}




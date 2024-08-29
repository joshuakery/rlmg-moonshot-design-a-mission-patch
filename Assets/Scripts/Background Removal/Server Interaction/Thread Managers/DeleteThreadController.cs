using UnityEngine;
using System.Collections;
using System.Threading;
using rlmg.logging;

namespace ArtScan.ScanSavingModule
{
    [CreateAssetMenu(menuName = "Thread Controller/Delete Thread Controller"), System.Serializable]
    public class DeleteThreadController : ScriptableObject
    {
        //Thread Class
        private class DeleteThread : MultiThreading.ThreadedJob
        {
            //parameters
            public string filename;

            protected override void ThreadFunction(CancellationToken token)
            {
                ClientSend.DeleteFileFromServer(filename);
            }
        }

        private DeleteThread deleteThread;

        public void CancelThread()
        {
            if (deleteThread != null && !deleteThread.IsDone)
            {
                Debug.Log("Ending parallel delete thread...");
                deleteThread.Cancel();
                deleteThread = null;
                Debug.Log("...ended.");
            }
        }

        //Methods
        public void Delete(string filename)
        {
            if (deleteThread == null || deleteThread.IsDone)
            {
                deleteThread = new DeleteThread();
                deleteThread.filename = filename;

                deleteThread.Start();
            }
        }

        public IEnumerator DeleteCoroutine(string filename)
        {
            if (deleteThread == null || deleteThread.IsDone)
            {
                deleteThread = new DeleteThread();
                deleteThread.filename = filename;

                deleteThread.Start();

                yield return deleteThread.WaitFor();
            }
        }
    }
}




using UnityEngine;
using System.Collections;
using rlmg.logging;

[CreateAssetMenu(menuName = "Thread Controller/Delete Thread Controller"), System.Serializable]
public class DeleteThreadController : ScriptableObject
{
    //Thread Class
    private class DeleteThread : MultiThreading.ThreadedJob
    {
        //parameters
        public string filename;

        protected override void ThreadFunction()
        {
            ClientSend.DeleteFileFromServer(filename);
        }
    }

    private DeleteThread deleteThread;

    public void AbortThread()
    {
        if (deleteThread != null && !deleteThread.IsDone)
        {
            RLMGLogger.Instance.Log("Ending parallel delete thread...", MESSAGETYPE.INFO);
            deleteThread.Abort();
            RLMGLogger.Instance.Log("...ended", MESSAGETYPE.INFO);
        }
    }

    //Methods
    public void Delete(string filename)
    {
        if (deleteThread == null || deleteThread.IsDone)
        {
            DeleteThread deleteThread = new DeleteThread();
            deleteThread.filename = filename;

            deleteThread.Start();
        }
    }

    public IEnumerator DeleteCoroutine(string filename)
    {
        if (deleteThread == null || deleteThread.IsDone)
        {
            DeleteThread deleteThread = new DeleteThread();
            deleteThread.filename = filename;

            deleteThread.Start();

            yield return deleteThread.WaitFor();
        }
    }
}


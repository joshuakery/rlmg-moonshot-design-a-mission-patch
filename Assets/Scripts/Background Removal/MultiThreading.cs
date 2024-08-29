using UnityEngine;
using System.Threading;
using System.Collections;

public class MultiThreading {
    public class ThreadedJob {
        private System.Threading.Thread m_Thread = null;
        private bool m_IsDone = false;
        private CancellationTokenSource cts;

        public ThreadedJob()
        {
            cts = new CancellationTokenSource();
        }
        public bool IsDone {
            get {
                bool tmp;
                tmp = m_IsDone;
                return tmp;
            }
            set {
                m_IsDone = value;
            }
        }
        public virtual void SetAffinity() {
            m_Thread.Priority = System.Threading.ThreadPriority.Highest;
            m_Thread.IsBackground = true;
        }
        public virtual void Start() {
            m_Thread = new Thread(Run);
            m_Thread.Start();
        }
        public virtual void Abort() {
            m_Thread.Abort();
        }

        public virtual void Cancel()
        {
            cts.Cancel();
        }

        protected virtual void ThreadFunction(CancellationToken token) { }

        protected virtual void OnFinished() { }

        public virtual bool Update() {
            if(IsDone)
            {
                /*m_Thread.Join();*/
                OnFinished();

                if (cts != null)
                    cts.Dispose();

                return true;
            }
            return false;
        }
        public IEnumerator WaitFor() {
            while(!Update()) {
                yield return null;
            }
        }
        private void Run() {
            if (cts != null)
                ThreadFunction(cts.Token);

            IsDone = true;
        }
    }
}
using UnityEngine;
using System.Collections;

namespace ArtScan.ScanSavingModule
{
    public class AbortThreads : MonoBehaviour
    {
        public UploadThreadController uploadThreadController;
        public DownloadThreadController downloadThreadController;
        public DeleteThreadController deleteThreadController;

        private void OnEnable()
        {
            if (Client.instance != null)
            {
                Client.instance.onStartRound += StartRound;
                Client.instance.onResumeRound += ResumeRound;
                Client.instance.onEndMission += EndMission;
            }
        }

        private void OnDisable()
        {
            if (Client.instance != null)
            {
                Client.instance.onStartRound -= StartRound;
                Client.instance.onResumeRound -= ResumeRound;
                Client.instance.onEndMission -= EndMission;
            }
        }

        private void OnDestroy()
        {
            CancelAll();
        }

        private void CancelAll()
        {
            uploadThreadController.CancelThread();
            downloadThreadController.CancelThread();
            deleteThreadController.CancelThread();
        }

        private void StartRound(string _teamName, float _roundDuration, float _roundBufferDuration, int _round, string _JsonTeamData)
        {
            CancelAll();
        }

        private void ResumeRound(string _teamName, float _roundDurationRemaining, float _roundBufferDurationRemaining, MissionState _missionState, int _round, string _JsonTeamData)
        {
            CancelAll();
        }

        private void EndMission()
        {
            CancelAll();
        }
    }
}




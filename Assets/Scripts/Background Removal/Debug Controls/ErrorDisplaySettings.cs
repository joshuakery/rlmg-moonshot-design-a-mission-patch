namespace ArtScan.ErrorDisplayModule
{
    [System.Serializable]
    public class ErrorDisplaySettings
    {
        public float checkForDisconnectInterval = 60f;

        public bool doAttemptCameraRestart = true;

        public float cameraDisconnectTimeout = 10f;

        public float refinedScanTimeout = 5f;

        public bool doAttemptCameraRestartIfWrongCamera = false;
    }

}

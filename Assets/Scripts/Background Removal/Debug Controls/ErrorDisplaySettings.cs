namespace ArtScan.ErrorDisplayModule
{
    [System.Serializable]
    public class ErrorDisplaySettings
    {
        public float checkForDisconnectInterval = 5f;

        public bool doAttemptCameraRestartIfMissingDevice = true;

        //public float cameraDisconnectTimeout = 10f;

        public bool doAttemptCameraRestartIfFrozenUpdateCount = true;

        public bool doAttemptCameraRestartIfWrongCamera = false;

        public float refinedScanTimeout = 5f;
    }

}

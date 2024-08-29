namespace ArtScan.ErrorDisplayModule
{
    [System.Serializable]
    public class ErrorDisplaySettings
    {
        public float checkForDisconnectInterval = 5f;

        public bool doAttemptCameraRestartIfMissingDevice = true;

        public bool doAttemptCameraRestartIfFrozenUpdateCount = true;

        public bool doAttemptCameraRestartIfWrongCamera = false;

        public bool doSaveCurrentAndLastWebCamTexturesToDisk = false;

        public bool doVerboseLoggingOfDisconnectHandler = false;

        public float refinedScanTimeout = 5f;
    }

}

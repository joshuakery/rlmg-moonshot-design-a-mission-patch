using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils.Helper;
using OpenCVForUnity.UnityUtils;
using ArtScan;
using ArtScan.PresentationUtilsModule;

namespace ArtScan.ScanSavingModule
{
    [CreateAssetMenu(fileName = "SavedScanManager", menuName = "Saved Scan Manager", order = 1)]
    public class SavedScanManager : ScriptableObject
    {
        [SerializeField]
        private GameState gameState;

        [SerializeField]
        private RemoveBackgroundSettings settings;

        private int scanMax
        {
            get
            {
                if (gameState != null) { return gameState.scanMax; }
                else { return 0; }
            }
        }

        private Texture2D[] _scans;
        public Texture2D[] scans;
        public List<int> nextToReplace;

        public bool allScansEmpty
        {
            get
            {
                return System.Array.TrueForAll(scans, scan => scan == null);
            }
        }

        /// <summary>
        /// Destroy all existing Texture2D in _scans
        /// And recreate arrays and nextToReplace list
        /// </summary>
        public void ClearScans()
        {
            if (_scans != null)
            {
                for (int i = 0; i < _scans.Length; i++)
                {
                    Texture2D _scan = _scans[i];
                    if (_scan != null)
                    {
                        Destroy(_scan);
                        _scans[i] = null;

                        if (scans != null && scans.Length > i)
                            scans[i] = null;
                    }
                }
            }

            _scans = new Texture2D[scanMax];
            scans = new Texture2D[scanMax];
            for (int i = 0; i < _scans.Length; i++)
            {
                if (settings != null)
                {
                    _scans[i] = new Texture2D(settings.targetWidth, settings.targetHeight, TextureFormat.RGBA32, false);
                }
                else
                {
                    _scans[i] = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                }

                scans[i] = null;
            }

            if (nextToReplace == null) { nextToReplace = new List<int>(); }
            else { nextToReplace.Clear(); }
        }

        /// <summary>
        /// Clears the scanSlots array so that scans appear to be reset
        /// Without cleaning Textures out of memory
        /// </summary>
        private void ClearScanSlots()
        {
            if (scans != null)
            {
                for (int i = 0; i < scans.Length; i++)
                {
                    scans[i] = null;
                }
            }
            else
            {
                scans = new Texture2D[scanMax];
            }

            if (nextToReplace == null) { nextToReplace = new List<int>(); }
            else { nextToReplace.Clear(); }
        }

        /// <summary>
        /// Gets next index to replace in _scans array based on the oldest item
        /// </summary>
        /// <returns>Index of next scan to replace</returns>
        public int GetNextScanIndex()
        {
            //If there's room, just add it to the list
            for (int i = 0; i < scans.Length; i++)
            {
                if (scans[i] == null)
                {
                    return i;
                }
            }

            if (nextToReplace == null) { nextToReplace = new List<int>(); }

            //Else replace the oldest scan
            if (nextToReplace.Count > 0)
            {
                int toRemove = nextToReplace[0];
                return toRemove;
            }
            //Should never happen
            else
            {
                int arbitraryIndex = 0;
                return arbitraryIndex;
            }
        }

        /// <summary>
        /// Checks if the Texture2D at _scans[index] matches the given Texture2D
        /// </summary>
        /// <param name="index">Index in _scans to check</param>
        /// <param name="src">Compared to _scans[index]</param>
        private void PrepareScanDestinationTexture(int index, Texture2D src)
        {
            PrepareScanDestinationTexture(index, src.width, src.height, src.format);
        }

        /// <summary>
        /// Checks if the Texture2D at _scans[index] matches the given size and format
        /// If not, Destroys the existings Texture2D and instantiates a new one
        /// </summary>
        /// <param name="index">Index in _scans to check</param>
        /// <param name="width">Compared to _scans[index].width</param>
        /// <param name="height">Compared to _scans[index].height</param>
        /// <param name="format">Compared to _scans[index].format</param>
        private void PrepareScanDestinationTexture(int index, int width, int height, TextureFormat format)
        {
            if (_scans[index] != null &&
                (width != _scans[index].width || height != _scans[index].height ||
                format != _scans[index].format))
            {
                Destroy(_scans[index]);
                _scans[index] = null;
                scans[index] = null;
            }

            if (_scans[index] == null)
            {
                _scans[index] = new Texture2D(width, height, format, false);
                _scans[index].name = "Scan Slot #" + index.ToString();
            }
        }

        /// <summary>
        /// Copies the src Texture2D to a Texture2D in the _scans array,
        /// creating a new Texture2D of the appropriate size if needed
        /// </summary>
        public void AddScanAsCopyOfTexture(Texture2D src)
        {
            if (src != null)
            {
                int index = GetNextScanIndex();
                //first assure that preview and destination texture are the same size and type
                PrepareScanDestinationTexture(index, src);
                //then copy into the destination texture
                Graphics.CopyTexture(src, _scans[index]);
                //update our scans array
                scans[index] = _scans[index];
                //finally, if used, pop out this index from the beginning of nextToReplace. regardless, add it to the end
                if (nextToReplace == null) { nextToReplace = new List<int>(); }
                if (nextToReplace.Count > 0 && nextToReplace[0] == index) { nextToReplace.RemoveAt(0); }
                nextToReplace.Add(index);
            }
        }

        /// <summary>
        /// Copies a Mat read from file into Texture2D in _scans array,
        /// creating a new Texture2D of the appropriate size if needed
        /// </summary>
        /// <param name="filepath">Filepath to read from</param>
        /// <param name="webCamTextureToMatHelper">Instance used to get Mat format</param>
        /// <param name="atIndex">Optional _scans index to add to</param>
        private void AddScanFromFile(string filepath, myWebCamTextureToMatHelper webCamTextureToMatHelper, int atIndex = -1)
        {
            using (
                Mat src = OpenCVForUnity.ImgcodecsModule.Imgcodecs.imread(
                        filepath,
                        OpenCVForUnity.ImgcodecsModule.Imgcodecs.IMREAD_UNCHANGED
                    ),

                    displayMat = new Mat(
                        settings.targetHeight,
                        settings.targetWidth,
                        webCamTextureToMatHelper.GetMat().type(),
                        new Scalar(0, 0, 0, 0)
                    )
            )
            {

                Imgproc.cvtColor(src, src, Imgproc.COLOR_BGRA2RGBA); ;

                if (src.size().width > 0 && src.size().height > 0)
                    PresentationUtils.MakeReadyToPresent(
                        src, displayMat,
                        settings.doCropToBoundingBox, settings.doSizeToFit
                    );

                int index = (atIndex >= 0 && atIndex < scanMax) ? atIndex : GetNextScanIndex();

                PrepareScanDestinationTexture(index, displayMat.cols(), displayMat.rows(), TextureFormat.RGBA32);

                Utils.fastMatToTexture2D(displayMat, _scans[index], true, 0, true);

                scans[index] = _scans[index];

                if (nextToReplace == null) { nextToReplace = new List<int>(); }
                if (nextToReplace.Count > 0 && nextToReplace[0] == index) { nextToReplace.RemoveAt(0); }
                nextToReplace.Add(index);

            }
        }

        /// <summary>
        /// Reads scans from file and adds them to the _scans array
        /// </summary>
        /// <param name="webCamTextureToMatHelper">Instance passed along to get Mat format</param>
        public void ReadScans(string[] filenames, myWebCamTextureToMatHelper webCamTextureToMatHelper)
        {
            ClearScanSlots();

            string dirPath = Path.Join(Application.streamingAssetsPath, settings.saveDir);

            DirectoryInfo mainDI = new DirectoryInfo(dirPath);

            if (filenames != null && mainDI.Exists)
            {
                for (int i = 0; i < filenames.Length; i++)
                {
                    string filename = filenames[i];
                    if (!System.String.IsNullOrEmpty(filename))
                    {
                        string filepath = Path.Join(dirPath, filename);
                        AddScanFromFile(filepath, webCamTextureToMatHelper);
                    }
                }
            }
        }

        /// <summary>
        /// Clears the scanSlot reference to the scan at the given index
        /// And moves the file to the trashDir
        /// </summary>
        /// <param name="filename">Name of the scan to trash</param>
        /// <param name="index">Scan slot array index of the scan to trash</param>
        public void TrashScanAndRemoveFromScans(string filename, int index)
        {
            //remove from scanSlots without removing from memory
            System.Array.Clear(scans, index, 1);

            //remove from replacement order
            if (nextToReplace.Contains(index))
                nextToReplace.Remove(index);

            //do not remove from local artworks list - but since the image is deleted it won't matter
            //Array.Clear(currentTeam.artworks, index, 1);
            //WordSaving.SaveTeamsToFile(saveFile,teams);

            //do not remove from server artworks list - but since the image is deleted it won't matter
            //Array.Clear(Client.instance.team.MoonshotTeamData.artworks, index, 1);
            //ClientSend.SendStationDataToServer();

            TrashScan(filename);
        }

        /// <summary>
        /// Moves the given file from the saveDir to the trashDir
        /// </summary>
        /// <param name="filename">File to move</param>
        public void TrashScan(string filename)
        {
            string saveDirPath = Path.Join(Application.streamingAssetsPath, settings.saveDir);
            string trashDirPath = Path.Combine(Application.streamingAssetsPath, settings.trashDir);
            ScanSavingModule.ScanSaving.MoveFile(saveDirPath, trashDirPath, filename);

            //ClientSend.DeleteFileFromServer(filename);
            //DeleteThreadController.Delete(filename);
        }

        /// <summary>
        /// Moves the given file from the trashDir to the saveDir
        /// And reads the file to Texture2D via Mat and adds it to the _scans array
        /// </summary>
        /// <param name="filename">File to move</param>
        /// <param name="index">Index to add scan to</param>
        /// <param name="webCamTextureToMatHelper">Instance to pass along for Mat format</param>
        public void UnTrashScanAndReadToScans(string filename, int index, myWebCamTextureToMatHelper webCamTextureToMatHelper)
        {
            UnTrashScan(filename);

            string saveDirPath = Path.Join(Application.streamingAssetsPath, settings.saveDir);
            string filepath = Path.Join(saveDirPath, filename);

            AddScanFromFile(filepath, webCamTextureToMatHelper, index);

        }

        /// <summary>
        /// Moves given file from trashDir to saveDir
        /// </summary>
        /// <param name="filename">File to move</param>
        public void UnTrashScan(string filename)
        {
            string saveDirPath = Path.Join(Application.streamingAssetsPath, settings.saveDir);
            string trashDirPath = Path.Combine(Application.streamingAssetsPath, settings.trashDir);
            ScanSavingModule.ScanSaving.MoveFile(trashDirPath, saveDirPath, filename);

            //string fullPath = Path.Join(saveDirPath, filename);
            //ClientSend.SendFileToServer(fullPath);
            //UploadThreadController.Upload(fullPath);
        }
    }
}



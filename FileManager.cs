#region Directives 
using System;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

using System.IO;
using Microsoft.Win32;
#endregion


namespace Mp3Player
{
    /// <summary>
    /// FileManager extends Iterator class to provide itration over array of
    /// files.
    /// </summary>
    class FileManager : Iterator
    {

        #region Constructors
        public FileManager()
        { }
        #endregion


        #region Fields
        List<string> files = new List<string>();
        #endregion


        #region Properties
        public Iterator.RepeatModeEnum RepeatMode
        {
            get { return base.RepeatMode;  }
            set { base.RepeatMode = value; }
        }

        public string currentFile 
        {
            get { return files[base.CurrentInde]; }
        }

        public int Count 
        {
            get { return files.Count; }
        }
        #endregion


        #region Public Methods
        public string NextFile()
        {
            return files[base.Next()];
        }

        public string PreviousFile()
        {
            return files[base.Previous()];
        }

        public string RandomFile()
        {
            return files[base.Shuffle()];
        }


        #region -- Doc --
        /// <summary>
        /// Method to add files to the list.
        /// </summary>
        /// <param name="fileToAdd">String array which is to add to the list. </param>
        /// <param name="showDialog">boolean value for showing open file dialog or not, default is false. </param>
        /// <param name="clearFiles">boolean value for clearing previous files from list or not. default is false</param>
        /// <returns>If succed, will return true, else false.</returns>
        #endregion
        public bool AddFiles(string[] fileToAdd, bool showDialog=false, bool clearFiles = false)
        {
            bool isSuccess = false;

            if(showDialog)
            {
                if (ShowAddFilesDialog(!clearFiles)) isSuccess = true;
            }
            else
            {
                if (AddToPlaylist(fileToAdd, ref files, clearFiles)) isSuccess = true;
            }

            /////////////////////////////
            base.ArraySize = files.Count;
            /////////////////////////////
            
            return isSuccess;
        }
        #endregion


        #region Private Methods
        // public method to start Windows Explorer instance for 
        // current file directory >>>
        public void ShowExplorer()
        {
            #region -- Bug --
            // there is a bug in Iterator class that return NEGATIVE VALUE from CurrentIndexPropert. 
            #endregion
            try
            {
                Process.Start(new FileInfo(files[base.CurrentInde]).DirectoryName);
            }
            catch
            {
                base.InitialIndex = 0;
                ShowExplorer();
            }
        }
        #endregion


        #region Private Methods
        // Method to check suppourted file formats before adding to playlist [M]
        #region -- Doc --
        /// <summary>
        /// method to chekc if the provided file is of supported file format or not.
        /// </summary>
        /// <param name="filePath">file path.</param>
        /// <returns>Return true if file is supported.</returns>
        #endregion
        private bool IsValidFile(string filePath)
        {
            try
            {
                FileInfo info = new FileInfo(filePath);
                switch (info.Extension.ToString().ToLower())
                {
                    case ".mp3":
                    case ".ogg":
                    case ".mod":
                    case "xm":
                    case "s3m":
                    case "wav":
                        return true;
                    default:
                        return false;
                }
            }
            catch { return false; }
        }


        // Method to add file to a playlist [M]
        #region -- Doc --
        /// <summary>
        /// Method to add file to a playlist, with file format validation.
        /// </summary>
        /// <param name="list">List suppose to add to the Playlist. </param>
        /// <param name="playList">Target Playlist. </param>
        /// <param name="clearList">Boolean value for clearing list or not, default is false.</param>
        /// <returns>Returns true if file(s) added to playlist. </returns>
        #endregion
        private bool AddToPlaylist(string[] list, ref List<string> playList, bool clearList = false)
        {
            List<string> tempList = new List<string>();
            bool isSuccess = false;

            foreach (string filePaht in list)
            {
                if (clearList) 
                    playList.Clear();
                
                if (IsValidFile(filePaht)) 
                    tempList.Add(filePaht);
            }

            if (tempList.Count > 0)
            {
                if (clearList) 
                    playList.Clear();
                
                playList.AddRange(tempList);
                //base.ArraySize = playList.Count;
                isSuccess = true;
            }

            return isSuccess;
        }


        // Method to show Open-File-Dialog and add files to playslist [M]
        #region -- Doc --
        /// <summary>
        /// Method to show open file dialog box and replace or append 
        /// selected files to playlist.
        /// </summary>
        /// <param name="appendToList">Boolean value represent whether to append files to list or not. </param>
        #endregion
        private bool ShowAddFilesDialog(bool appendToList = true)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            dialog.Filter = "Audio Files | *.mp3; *.ogg; *.mod; *.xm; *.s3m; *.wav";
            bool isSuccess = false;

            if (appendToList)
            {
                dialog.ShowDialog();

                if (dialog.FileNames.Length > 0)
                {
                    if (AddToPlaylist(dialog.FileNames, ref files))
                    {
                        //base.ArraySize = files.Count; // <-- update loop reange.
                        isSuccess = true;
                    }
                }
            }
            else
            {
                dialog.ShowDialog();

                if (dialog.FileNames.Length > 0)
                {
                    // note that the last true argument will clear the list.
                    if (AddToPlaylist(dialog.FileNames, ref files, true))
                    {
                        //base.ArraySize = files.Count;     // <-- update loop range &
                        //StartPlay(loop_iterate.InitialIndex);  // <-- Start To Play.
                        isSuccess = true;
                    }
                }
            }

            return isSuccess;
        }
        #endregion

    }// end of class.
}

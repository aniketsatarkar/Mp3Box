#region Directives
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using TagLib;
#endregion 


namespace Mp3Player
{
    /// <summary>
    /// AlbumArt containing utility static methods for getting Id3 data
    /// from mp3 files.
    /// </summary>
    public class AlbumArt
    {

        #region Static Methods
        /// <summary>
        /// Method to get ImageSource from file path. 
        /// </summary>
        /// <param name="filePath">String representing path of the file.</param>
        /// <returns>Album art as a ImageSource.</returns>
        public static ImageSource getAlbumart(string filePath)
        {
            // if unsupported file is added then return null >>>
            try
            {
                TagLib.File file = TagLib.File.Create(filePath);
                Tag tag = file.Tag;

                if (tag.Pictures.Length >= 1)
                {
                    // getting image as array of bytes...
                    byte[] bytes = (byte[])tag.Pictures[0].Data.Data;

                    // Cretating a bitmap image from Stream of bytes.
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = new System.IO.MemoryStream(bytes);
                    bitmap.DecodePixelWidth = 200;
                    bitmap.DecodePixelHeight = 200;
                    bitmap.EndInit();

                    // sreturning bitmap image as ImageSource, to use with Image as Source.
                    return bitmap as ImageSource;
                }
                else return null;
            }
            catch
            {
                return null;
            }
        }


        /// <summary>
        /// Method to get song tital.
        /// </summary>
        /// <param name="filePath">String represent path of the file.</param>
        /// <returns>String that represent song tital.</returns>
        public static string getSongTital(string filePath)
        {
            // if unsupported file is added, then return an empty string...!!!
            try
            {
                TagLib.File file = TagLib.File.Create(filePath);
                Tag tag = file.Tag;
                return tag.Title;
            }
            catch
            {
                return string.Empty;
            }
        }
        #endregion

    }// end of class.
}

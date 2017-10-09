using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetFacts.Download
{
    public static class DownloadTypes
    {
        public enum Categories
        {
            Undefined,
            Text,
            Image,
            Video,
            Sound
        }

        public static Categories Guess(string url)
        {
            string ext = Path.GetExtension(url).ToLower();
            switch (ext)
            {
                case ".htm":
                case ".html":
                    return Categories.Text;

                case ".bmp":
                case ".gif":
                case ".jpeg":
                case ".jpg":
                case ".png":               
                    return Categories.Image;

                case ".avi":
                case ".mp4":
                    return Categories.Video;

                case ".mp3":
                    return Categories.Sound;

                default:
                    return Categories.Undefined;
            }
        }
    }
}

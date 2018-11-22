using System;
using System.Collections.Generic;
using YoutubeExtractor;

namespace FastDownload
{
    class Video
    {
        public AdaptiveType AdaptiveType { get; set; }
        public int AudioBitrate { get; set; }
        public string AudioExtension { get; set; }
        public AudioType AudioType { get; set; }
        public bool CanExtractAudio { get; set; }
        public string DownloadUrl { get; set; }
        public int FormatCode { get; set; }
        public bool Is3D { get; set; }
        public bool RequiresDecryption { get; set; }
        public int Resolution { get; set; }
        public string Title { get; set; }
        public string VideoExtension { get; set; }
        public string CodeId { get; set; }
        public string FileName { get; set; }
        public VideoType VideoType { get; set; }
        public List<Tuple<int, int>> Segments { get; set; }
    }
}

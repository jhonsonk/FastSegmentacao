using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeExtractor;

namespace FastDownload
{
    class Program
    {
        //int resolutionMin = 360;
        //VideoType videoType = VideoType.Mp4;
        static string DirectoryVideos = @"Videos";
        static string DirectorySegmentacion = @"Segmentation";
        static string FastNameProc = "Project2.exe";

        static void Main(string[] args)
        {
            var videos = LoadFileVideos();

            List<Video> ListaDeVideos = new List<Video>();
            List<Video> videosCarregados = new List<Video>();

            if (File.Exists(DirectoryVideos + "\\lista.json"))
            {
                using (var reader = new StreamReader(DirectoryVideos + "\\lista.json"))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        videosCarregados = JsonConvert.DeserializeObject<List<Video>>(line);
                    }
                    reader.Close();
                }
            }

            foreach (var item in videos)
            {
                var temp = videosCarregados.Find(a => a.CodeId == item.Item2);
                if (temp == null)
                {
                    var reta = DownloadVideo(item.Item1, item.Item2);
                    if (reta != null)
                    {
                        ListaDeVideos.Add(reta);
                    }
                }
                else
                {
                    ListaDeVideos.Add(temp);
                }
            }

            /*Videos*/
            string ret = JsonConvert.SerializeObject(ListaDeVideos);
            using (var writer = new StreamWriter(DirectoryVideos + "\\lista.json"))
            {
                writer.WriteLine(ret);
                writer.Close();
            }

            List<Video> ListaRetorno = new List<Video>();
            foreach (var item in ListaDeVideos)
            {
                SegmentationVideo(item);
                ListaRetorno.Add(item);
            }

            ret = JsonConvert.SerializeObject(ListaRetorno);

            using (var writer = new StreamWriter("list.json"))
            {
                writer.WriteLine(ret);
                writer.Close();
            }
        }

        public static Video DownloadVideo(string link, string codigo)
        {
            try
            {
                IEnumerable<VideoInfo> videoInfos = null;
                try
                {
                    videoInfos = DownloadUrlResolver.GetDownloadUrls(link, true);
                }
                catch (Exception ex)
                {
                    Console.Write("0");
                    // videoInfos = DownloadUrlResolver.DecryptDownloadUrl(videoInfo);
                }

                VideoInfo videoInfo = videoInfos.First(info => info.VideoType == VideoType.Mp4);
                string videoPath = Path.Combine(DirectoryVideos, codigo + videoInfo.VideoExtension);

                if (!Directory.Exists(DirectoryVideos))
                    Directory.CreateDirectory(DirectoryVideos);

                if (!File.Exists(videoPath))
                {
                    if (videoInfo.RequiresDecryption)
                    {
                        DownloadUrlResolver.DecryptDownloadUrl(videoInfo);
                    }

                    var videoDownloader = new VideoDownloader(videoInfo, Path.Combine(DirectoryVideos, codigo + videoInfo.VideoExtension));
                    videoDownloader.Execute();
                }

                Video video = new Video();
                video.CodeId = codigo;
                video.FileName = videoPath;
                video.AdaptiveType = videoInfo.AdaptiveType;
                video.AudioBitrate = videoInfo.AudioBitrate;
                video.AudioExtension = videoInfo.AudioExtension;
                video.AudioType = videoInfo.AudioType;
                video.CanExtractAudio = videoInfo.CanExtractAudio;
                video.DownloadUrl = videoInfo.DownloadUrl;
                video.FormatCode = videoInfo.FormatCode;
                video.Is3D = videoInfo.Is3D;
                video.RequiresDecryption = videoInfo.RequiresDecryption;
                video.Resolution = videoInfo.Resolution;
                video.Title = videoInfo.Title;
                video.VideoExtension = videoInfo.VideoExtension;
                video.VideoType = videoInfo.VideoType;
                video.Segments = new List<Tuple<int, int>>();
                return video;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public static List<Tuple<string, string>> LoadFileVideos()
        {
            using (var reader = new StreamReader("list.video"))
            {
                List<Tuple<string, string>> listreturn = new List<Tuple<string, string>>();
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    string[] values = line.Split('=');
                    Tuple<string, string> video = new Tuple<string, string>(line, values.Last());
                    listreturn.Add(video);
                }
                return listreturn;
            }
        }

        public static void SegmentationVideo(Video video)
        {
            if (!Directory.Exists(DirectorySegmentacion))
                Directory.CreateDirectory(DirectorySegmentacion);

            string segmentacao = Path.Combine(DirectorySegmentacion, video.CodeId + ".csv");
            string segmentacaoCopy = Path.Combine(DirectorySegmentacion, "copy_" + video.CodeId + ".csv");

            if (File.Exists(segmentacao))
                File.Delete(segmentacao);

            if (File.Exists(segmentacaoCopy))
                File.Delete(segmentacaoCopy);

            System.Diagnostics.Process.Start(FastNameProc, video.FileName + " " + segmentacao + " 3 0.25 1.5 2 9");

            while (true)
            {
                if (File.Exists(segmentacao))
                {
                    try
                    {
                        File.Copy(segmentacao, segmentacaoCopy);
                        using (var reader = new StreamReader(segmentacaoCopy))
                        {
                            while (!reader.EndOfStream)
                            {
                                string line = reader.ReadLine();
                                string[] values = line.Split(',');
                                video.Segments.Add(new Tuple<int, int>(Convert.ToInt32(values[0]), Convert.ToInt32(values[1])));
                            }
                            reader.Close();
                            File.Delete(segmentacaoCopy);
                            break;
                        }
                    }
                    catch { }
                }
            }
        }
    }
}

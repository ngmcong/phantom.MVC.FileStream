namespace phantom.MVC.FileStream
{
    using DataEntities;
    using Microsoft.AspNetCore.StaticFiles;
    using System.IO;

    public class Globals
    {
        public static Globals Instance = new Globals()
        {
            VideoInfoCollection = LoadVideoInfoCollection()
        };
        public const string FolderPath = @"F:\Collection";
        public List<DataEntities.VideoInfo> VideoInfoCollection = new List<VideoInfo>();

        public static string ConvertImageBase64String(string file)
        {
            string base64Image;
            using (Stream stream = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    base64Image = Convert.ToBase64String(memoryStream.ToArray());
                    memoryStream.Close();
                    memoryStream.Dispose();
                }
                stream.Close();
                stream.Dispose();
            }
            return base64Image;
        }
        public static string GetMimeMapping(string filePath)
        {
            string? contentType;
            new FileExtensionContentTypeProvider().TryGetContentType(filePath, out contentType);
            return contentType ?? "application/octet-stream";
        }

        private static List<VideoInfo> LoadVideoInfoCollection()
        {
            var videoInfoCollection = new List<VideoInfo>();
            //string dataDirectory = Path.Combine(Directory.GetCurrentDirectory(), "App_Data");
            //if (Directory.Exists(AppDomain.CurrentDomain.GetData("DataDirectory")!.ToString()) == false) Directory.CreateDirectory(AppDomain.CurrentDomain.GetData("DataDirectory")!.ToString()!);
            var otherVideos = Directory.GetFiles(FolderPath, "*.*", SearchOption.AllDirectories).Where(f => f.EndsWith(".mp4") || f.EndsWith(".wmv") || f.EndsWith(".ts") || f.EndsWith(".mkv") || f.EndsWith(".cpf"));
            foreach (var file in otherVideos)
            {
                FileInfo fileInfo = new FileInfo(file);
                if (fileInfo.Name.StartsWith(".")) continue;
                var dirName = System.IO.Path.GetDirectoryName(file)!.Split('\\').Last();

                #region MyRegion
                var videoInf = new VideoInfo { GroupName = dirName };
                videoInf.FullPath = file;
                videoInf.Extention = fileInfo.Extension.TrimStart('.');
                videoInf.VideoName = videoInf.FileName = fileInfo.Name.Remove(fileInfo.Name.Length - videoInf.Extention.Length - 1);
                videoInf.MineType = GetMimeMapping(file);
                #endregion

                videoInf.GroupName = file.Split('\\').Skip(3).First();

                videoInfoCollection.Add(videoInf);
            }
            videoInfoCollection = videoInfoCollection.OrderBy(x => x.GroupName).ThenBy(x => x.FileName).ToList();
            videoInfoCollection.Select((v, i) => new { v, i }).ToList().ForEach(x => { x.v.Index = x.i; x.v.MineType = x.v.MineType == "application/octet-stream" ? "video/mp4" : x.v.MineType; });
            return videoInfoCollection;
        }
    }
}

using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using phantom.MVC.FileStream.Models;

namespace phantom.MVC.FileStream.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private const int pageSize = 10;
        private static Dictionary<string, string> dirImageCollection = new Dictionary<string, string>();

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            ViewBag.GroupNames = Globals.Instance.VideoInfoCollection?.GroupBy(x => x.GroupName).Where(x => x.Count() > 1).Select(x => x.Key).Distinct();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
#if DEBUG
#else
        [OutputCache(Duration = 1440, VaryByParam = "pageIndex", Location = System.Web.UI.OutputCacheLocation.Client)]
#endif
        public async Task<JsonResult> GetVideos(int pageIndex, string? groupName = null)
        {
            var jsonDatas = Globals.Instance.VideoInfoCollection?.Where(x => string.IsNullOrEmpty(groupName) || x.GroupName == groupName).Skip(pageIndex * pageSize).Take(pageSize).ToList();
            if (jsonDatas?.Any(x => string.IsNullOrEmpty(x.Base64Image)) == true)
            {
                foreach (var videoInf in jsonDatas)
                {
                    if (string.IsNullOrEmpty(videoInf.Base64Image) == false || string.IsNullOrEmpty(videoInf.FullPath)) continue;
                    string? base64Image = null;
                    if (dirImageCollection.ContainsKey(videoInf.FullPath))
                    {
                        base64Image = dirImageCollection[videoInf.FullPath];
                        goto Base64Image;
                    }
                    var dirName = Path.GetDirectoryName(videoInf.FullPath)!;
                    var imageFile = Directory.GetFiles(dirName, "*.*", SearchOption.TopDirectoryOnly).Where(f => f.EndsWith(".jpeg") || f.EndsWith(".jpg") || f.EndsWith(".png"));
                    if (imageFile.Count() == 1)
                    {
                        var filePath = imageFile.ElementAt(0);
                        base64Image = Globals.ConvertImageBase64String(filePath);
                        goto Base64Image;
                    }
                    #region Grab thumbnail from a video
                    await Task.CompletedTask;
                    //var ffmpegFilePath = @"F:\ffmpeg-7.1-essentials_build\bin\ffmpeg.exe";
                    //var service = MediaToolkitService.CreateInstance(ffmpegFilePath);
                    //string dataDirectory = Path.Combine(Directory.GetCurrentDirectory(), "App_Data");
                    //var saveThumbnailTask = new FfTaskSaveThumbnail(videoInf.FullPath,
                    //    Path.Combine(dataDirectory, @"To_Save_Image.jpg"),
                    //    TimeSpan.FromSeconds(10)
                    //);
                    //await service.ExecuteAsync(saveThumbnailTask);

                    //using (var engine = new Engine(@"F:\ffmpeg-7.1-essentials_build\bin\ffmpeg.exe"))
                    //{
                    //    var inputFile = new MediaFile { Filename = videoInf.FullPath };
                    //    engine.GetMetadata(inputFile);

                    //    // Saves the frame located on the 15th second of the video.
                    //    double seconds = 15;
                    //    if (inputFile.Metadata.Duration.TotalSeconds <= 15)
                    //    {
                    //        seconds = inputFile.Metadata.Duration.TotalSeconds / 2;
                    //    }
                    //    var options = new ConversionOptions { Seek = TimeSpan.FromSeconds(seconds) };

                    //    string dataDirectory = Path.Combine(Directory.GetCurrentDirectory(), "App_Data");
                    //    if (Directory.Exists(AppDomain.CurrentDomain.GetData("DataDirectory")!.ToString()) == false) Directory.CreateDirectory(AppDomain.CurrentDomain.GetData("DataDirectory")!.ToString()!);
                    //    var outputFile = new MediaFile { Filename = Path.Combine(dataDirectory, @"To_Save_Image.jpg") };
                    //    engine.GetThumbnail(inputFile, outputFile, options);
                    //    var filePath = Path.Combine(dataDirectory, @"To_Save_Image.jpg");
                    //    base64Image = Globals.ConvertImageBase64String(filePath);
                    //}
                    #endregion Grab thumbnail from a video
                    Base64Image:
                    if (string.IsNullOrEmpty(base64Image)) continue;
                    videoInf.Base64Image = base64Image;
                }
            }

            return new JsonResult(jsonDatas);
        }

        [Route("Home/Video/{index}")]
        public ActionResult Video(int index)
        {
            var videoInfo = Globals.Instance.VideoInfoCollection!.ElementAt(index).DeepCopy();
            ViewBag.Title = videoInfo.VideoName;
            return View(videoInfo);
        }
    }
}

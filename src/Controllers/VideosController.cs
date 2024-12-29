using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net;

namespace phantom.MVC.FileStream.Controllers
{
    public class VideosController : Controller
    {
        int readStreamBufferSize = 65536;
        string videoPath = "F:\\123.webm";
        string[] videoPaths = new string[] { "F:\\sintel-video.mp4", "F:\\frag_bunny.mp4", "F:\\test.mp4" };

        [EnableCors]
        [Route("api/online/{index}")]
        public IActionResult Get(int index)
        {
            videoPath = videoPaths[index];
            var stream = new System.IO.FileStream(videoPath, FileMode.Open, FileAccess.Read); //Got from storage
            if (stream == null) return NotFound();

            Response.Headers["Accept-Ranges"] = "bytes";

            //if there is no range - this is usual request
            var rangeHeaderValue = Request.Headers["Range"].FirstOrDefault();
            if (string.IsNullOrEmpty(rangeHeaderValue))
            {
                var fileStreamResult = new FileStreamResult(stream, "video/mp4");
                Response.ContentLength = stream.Length;
                Response.StatusCode = (int)HttpStatusCode.OK;
                return fileStreamResult;
            }

            if (!TryReadRangeItem(rangeHeaderValue, stream.Length, out long start, out long end))
            {
                return StatusCode((int)HttpStatusCode.RequestedRangeNotSatisfiable);
            }

            Response.Headers["Content-Range"] = $"bytes {start}-{end}/{stream.Length}";
            Response.ContentLength = end - start + 1;
            Response.StatusCode = (int)HttpStatusCode.PartialContent;

            var outStream = new MemoryStream();
            CreatePartialContent(stream, outStream, start, end);
            outStream.Seek(0, SeekOrigin.Begin);
            return new FileStreamResult(outStream, "video/mp4");
        }

        private void CreatePartialContent(Stream inputStream, Stream outputStream, long start, long end)
        {
            var remainingBytes = end - start + 1;
            var buffer = new byte[readStreamBufferSize];
            long position;
            inputStream.Position = start;
            do
            {
                try
                {
                    var count = remainingBytes > readStreamBufferSize ?
                        inputStream.Read(buffer, 0, readStreamBufferSize) :
                        inputStream.Read(buffer, 0, (int)remainingBytes);
                    outputStream.Write(buffer, 0, count);
                }
                catch (Exception error)
                {
                    Debug.WriteLine(error);
                    break;
                }
                position = inputStream.Position;
                remainingBytes = end - position + 1;
            } while (position <= end);
        }

        private bool TryReadRangeItem(string rangeHeaderValue, long contentLength,
            out long start, out long end)
        {
            if (string.IsNullOrEmpty(rangeHeaderValue))
                throw new ArgumentNullException(nameof(rangeHeaderValue));

            start = 0;
            end = contentLength - 1;

            var rangeHeaderSplitted = rangeHeaderValue.Split('=');
            if (rangeHeaderSplitted.Length == 2)
            {
                var range = rangeHeaderSplitted[1].Split('-');
                if (range.Length == 2)
                {
                    if (long.TryParse(range[0], out long startParsed))
                        start = startParsed;
                    if (long.TryParse(range[1], out long endParsed))
                        end = endParsed;
                }
            }

            return start < contentLength && end < contentLength;
        }
    }
}
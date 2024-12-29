namespace DataEntities
{
    public class VideoInfo : IDisposable
    {
        public int? Index { get; set; }
        public string? GroupName { get; set; }
        public string? VideoName { get; set; }
        public string? MineType { get; set; }
        public string? FileName { get; set; }
        public string? Extention { get; set; }
        public string? Base64Image { get; set; }
        public string? FullPath { get; set; }

        public void Dispose()
        {
            Index = null;
            GroupName = VideoName = MineType = FileName = null;
        }
    }
}

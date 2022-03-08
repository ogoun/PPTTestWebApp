using PPTDataLibrary;

var storage = new PPTFilesStorage(@"D:\te");

using (var stream = new FileStream(@"D:\2-40 - 2022 Forecast Shippers_Stephen Spoljaric.pptx", FileMode.Open, FileAccess.Read, FileShare.None, 1024 * 1024 * 24))
{
    var reader = storage.Store("2-40 - 2022 Forecast Shippers_Stephen Spoljaric.pptx", stream).Result;
    foreach (var file in reader.VideoFiles)
    {
        Console.WriteLine($"[{file.SlideId}] {file.VideoFileName}");
        using (var ms = new MemoryStream())
        {
            storage.GetVideoFile(file.FileId, file.VideoId, ms).Wait();
            ms.Position = 0;
            var p = Path.Combine(@"D:\te", file.VideoFileName);
            using (var fs = new FileStream(p, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                ms.CopyTo(fs);
            }
        }
    }
}

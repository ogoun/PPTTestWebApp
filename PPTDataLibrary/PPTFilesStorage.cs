using System.Collections.Concurrent;

namespace PPTDataLibrary
{
    /// <summary>
    /// Provides saving presentation files
    /// </summary>
    public class PPTFilesStorage
    {
        private readonly ConcurrentDictionary<int, PPTReader> _readers;
        private readonly ConcurrentDictionary<int, string> _files;
        private readonly string _baseFolder;
        private int _counter = 0;
        public PPTFilesStorage(string baseFolder)
        {
            if (string.IsNullOrWhiteSpace(baseFolder))
            {
                throw new ArgumentNullException(nameof(baseFolder));
            }
            if (!Directory.Exists(baseFolder))
            {
                Directory.CreateDirectory(baseFolder);
            }
            if (!Directory.Exists(baseFolder))
            {
                throw new ArgumentException(nameof(baseFolder));
            }
            _baseFolder = baseFolder;
            _readers = new ConcurrentDictionary<int, PPTReader>();
            _files = new ConcurrentDictionary<int, string>();
        }
        public async Task<PPTReader> Store(string name, Stream stream)
        {
            var id = Interlocked.Increment(ref _counter);
            var fullPath = Path.Combine(_baseFolder, name);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
            if (stream.CanSeek) stream.Position = 0;
            using (var fs = new FileStream(fullPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 1024 * 1024 * 16, true))
            {
                await stream.CopyToAsync(fs).ConfigureAwait(false);
                await fs.FlushAsync();                
            }
            PPTReader reader = null;
            try
            {
                reader = new PPTReader(id, fullPath);                                
            }
            catch
            {
                File.Delete(fullPath);
                throw;
            }
            if (reader != null)
            {
                _readers[id] = reader;
                _files[id] = fullPath;
            }
            return reader;
        }

        public async Task<string> GetVideoFile(int fileId, int videoId, Stream target)
        {
            if (_readers.TryGetValue(fileId, out var reader))
            {
                var zipPath = reader.GetVideoFilePath(videoId).TrimStart('/');
                if (_files.TryGetValue(fileId, out var pptFile))
                {
                    using (var documentArchive = new System.IO.Compression.ZipArchive(File.OpenRead(pptFile)))
                    {
                        foreach (System.IO.Compression.ZipArchiveEntry entry in documentArchive.Entries)
                        {
                            if (entry.FullName.EndsWith(zipPath, StringComparison.OrdinalIgnoreCase))
                            {
                                await entry.Open().CopyToAsync(target).ConfigureAwait(false);
                                return reader.GetVideoName(videoId);
                            }
                        }
                    }
                }
            }
            return string.Empty;
        }
    }
}

using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;

namespace PPTDataLibrary
{
    public class PPTReader
    {
        /// <summary>
        /// Collection of video files
        /// </summary>
        public IEnumerable<PPTVideoFile> VideoFiles => _meta.Values;

        private Dictionary<int, string> _files = new Dictionary<int, string>();
        private Dictionary<int, PPTVideoFile> _meta = new Dictionary<int, PPTVideoFile>();

        public int Id { get; private set; }
        public PPTReader(int id, string fullPath)
        {
            Id = id;
            Read(fullPath);
        }
        public string GetVideoFilePath(int videoId)
        {
            if (_files.TryGetValue(videoId, out var p)) return p;
            return null;
        }

        public string GetVideoName(int videoId)
        {
            if (_meta.TryGetValue(videoId, out var p)) return p.VideoFileName;
            return null;
        }

        /// <summary>
        /// Search all relationships to video in a presentation file
        /// </summary>
        /// <param name="fullPath"></param>
        public void Read(string fullPath)
        {
            int uid = 0;   
            using (PresentationDocument presentationDocument = PresentationDocument.Open(fullPath, false))
            {
                // Get the presentation part of the presentation document.
                PresentationPart presentationPart = presentationDocument.PresentationPart;
                // Verify that the presentation part and presentation exist.
                if (presentationPart != null && presentationPart.Presentation != null)
                {
                    // Get the Presentation object from the presentation part.
                    Presentation presentation = presentationPart.Presentation;
                    // Verify that the slide ID list exists.
                    if (presentation.SlideIdList != null)
                    {
                        // Get the collection of slide IDs from the slide ID list.
                        DocumentFormat.OpenXml.OpenXmlElementList slideIds =
                            presentation.SlideIdList.ChildElements;
                        // Iterate slides collection
                        for (int slideId = 0; slideId < slideIds.Count(); slideId++)
                        {
                            // Get the relationship ID of the slide.
                            string slidePartRelationshipId = (slideIds[slideId] as SlideId).RelationshipId;
                            // Get the specified slide part from the relationship ID.
                            SlidePart slidePart = (SlidePart)presentationPart.GetPartById(slidePartRelationshipId);
                            if (slidePart != null)
                            {
                                var dataRelationships = slidePart.DataPartReferenceRelationships;
                                if (dataRelationships != null)
                                {
                                    foreach (var rel in dataRelationships)
                                    {
                                        var video_rel = rel as VideoReferenceRelationship;
                                        if (video_rel != null)
                                        {
                                            _files[uid] = video_rel.Uri.OriginalString;
                                            _meta[uid] = new PPTVideoFile
                                            {
                                                FileId = this.Id,
                                                SlideId = slideId,
                                                VideoId = uid,
                                                VideoFileName = Path.GetFileName(video_rel.Uri.OriginalString)
                                            };
                                            uid++;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}

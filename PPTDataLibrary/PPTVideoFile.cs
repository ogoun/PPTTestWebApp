namespace PPTDataLibrary
{
    public class PPTVideoFile
    {
        /// <summary>
        /// Presentation file identity
        /// </summary>
        public int FileId { get; set; }
        /// <summary>
        /// Slide identity in presentation file
        /// </summary>
        public int SlideId { get; set; }
        /// <summary>
        /// Video identity in presentation file
        /// </summary>
        public int VideoId { get;set; }
        /// <summary>
        /// Name of video file
        /// </summary>
        public string VideoFileName { get; set; }
    }
}
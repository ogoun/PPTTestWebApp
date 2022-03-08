using Microsoft.AspNetCore.Mvc;
using PPTDataLibrary;
using System.Net;

namespace PPTWebApp.Controllers
{
    [ApiController]
    public class PPTVideoController
        : ControllerBase
    {
        private readonly ILogger<PPTVideoController> _logger;
        private readonly PPTFilesStorage _storage;
        private readonly string _html;

        public PPTVideoController(PPTFilesStorage storage, ILogger<PPTVideoController> logger)
        {
            _logger = logger;
            _storage = storage;
            _html = System.IO.File.ReadAllText("index.html");
        }

        [HttpGet("/")]
        public ContentResult Get()
        {
            return new ContentResult
            {
                ContentType = "text/html",
                StatusCode = (int)HttpStatusCode.OK,
                Content = _html
            };
        }

        [HttpPost("api/ppt")]
        [DisableRequestSizeLimit()]
        [DisableFormValueModelBinding]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Upload()
        {
            try
            {
                var files = Request?.Form?.Files;
                if (files != null && files.Any())
                {
                    var name = files[0].FileName;
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        name = files[0].Name;
                    }
                    var result = await _storage.Store(name, files[0].OpenReadStream());
                    if (result == null)
                    {
                        return BadRequest();
                    }
                    return Ok(result.VideoFiles);
                }
                return StatusCode(StatusCodes.Status400BadRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ImageStorageController.UploadBackgroundImageAsync]");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("api/ppt")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetVideo([FromQuery] int fileId, [FromQuery] int videoId)
        {
            try
            {
                var ms = new MemoryStream();
                var name = await _storage.GetVideoFile(fileId, videoId, ms);
                if (ms.Length > 0)
                {
                    ms.Position = 0;
                    return new FileStreamResult(ms, "video/mp4");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ImageStorageController.GetBackgroundImage]");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
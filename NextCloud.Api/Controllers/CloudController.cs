using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace NextCloud.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CloudController : ControllerBase
    {
        private static Settings _settings;
        private static NextCloudService _nextCloudService;
        private readonly ILogger<CloudController> _logger;

        public CloudController(ILogger<CloudController> logger, IOptions<Settings> settings)
        {
            _logger = logger;

            _settings = settings.Value;

            List<string> errors = _settings.Validate();

            if (errors.Count > 0)
                throw new ApplicationException(string.Join("\r\n", errors));

            _nextCloudService = new NextCloudService(_settings);
        }

        [HttpPost("{clinicId}/patient/{patientId}/upload")]
        public async Task<IActionResult> Upload([FromForm] IFormFile file, [FromRoute(Name = "clinicId")] Guid clinicId, [FromRoute(Name = "patientId")] Guid patientId)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Nenhum arquivo enviado.");

            using var stream = file.OpenReadStream();

            var path = _settings.Username + $"/{clinicId}/{patientId}";

            var list = await CloudFile.Upload(_nextCloudService, path + $"/{file.FileName}", stream);

            return Ok(list);
        }

        [HttpGet("{clinicId}/patient/{patientId}/files")]
        public async Task<IActionResult> GetClinicFiles([FromRoute(Name = "clinicId")] Guid clinicId, [FromRoute(Name = "patientId")] Guid patientId)
        {
            var list = await CloudFolder.List(_nextCloudService, _settings.Username + $"/{clinicId}/{patientId}", CloudInfo.Properties.All);

            list.ForEach(item =>
            {
                item.Preview = $"{_settings.ServerUri}apps/photos/api/v1/preview/{item.FileId}?etag={item.Tag.Replace("\"", "")}&x=128&y=128";
            });

            return Ok(list);
        }
    }
}
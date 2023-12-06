using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NextCloud.Api.Services;
using Swashbuckle.AspNetCore.Annotations;
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
        [SwaggerOperation(Summary = "Upload file on patient repository")]
        [SwaggerResponse(200, null, typeof(List<Share>))]
        public async Task<IActionResult> Upload([FromForm] List<IFormFile> files, [FromRoute(Name = "clinicId")] Guid clinicId, [FromRoute(Name = "patientId")] Guid patientId)
        {
            if (files == null || files.Count == 0)
                return BadRequest("Nenhum arquivo enviado.");

            List<Share> shareFiles = new();

            foreach (var file in files)
            {
                using (var stream = file.OpenReadStream())
                {
                    var path = _settings.Username + $"/{clinicId}/{patientId}";

                    await CloudFile.Upload(_nextCloudService, path + $"/{file.FileName}", stream);

                    var filePath = $"/{clinicId}/{patientId}/{file.FileName}";

                    var shareFile = await ShareServices.CreatePublicShare(_nextCloudService, filePath);

                    shareFiles.Add(shareFile);
                }
            }
            
            return Ok(shareFiles);
        }
    }
}
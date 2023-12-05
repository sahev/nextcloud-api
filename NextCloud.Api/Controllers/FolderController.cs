using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace NextCloud.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FolderController : ControllerBase
    {
        private static Settings _settings;
        private static NextCloudService _nextCloudService;
        private readonly ILogger<FolderController> _logger;

        public FolderController(ILogger<FolderController> logger, IOptions<Settings> settings)
        {
            _logger = logger;

            _settings = settings.Value;

            List<string> errors = _settings.Validate();

            if (errors.Count > 0)
                throw new ApplicationException(string.Join("\r\n", errors));

            _nextCloudService = new NextCloudService(_settings);
        }

        [HttpGet("{clinicId}")]
        public async Task<IActionResult> GetClinicFiles([FromRoute(Name = "clinicId")] Guid clinicId, [FromQuery(Name = "patientId")] Guid patientId)
        {
            if (patientId == Guid.Empty)
                return Ok(await CloudFolder.List(_nextCloudService, _settings.Username + $"/{clinicId}", CloudInfo.Properties.All));

            return Ok(await CloudFolder.List(_nextCloudService, _settings.Username + $"/{clinicId}/{patientId}", CloudInfo.Properties.All));
        }

        [HttpPost("{clinicId}/create")]
        public async Task<IActionResult> CreateClinicRepository([FromRoute(Name = "clinicId")] Guid clinicId)
        {
            var pathClinic = _settings.Username + $"/{clinicId}";

            try
            {
                await CloudFolder.List(_nextCloudService, pathClinic, CloudInfo.Properties.Id);
            }
            catch
            {
                await CloudFolder.Create(_nextCloudService, pathClinic);
            }

            return Ok();
        }

        [HttpPost("{clinicId}/patient/{patientId}/create")]
        public async Task<IActionResult> CreatePatientRepository([FromRoute(Name = "clinicId")] Guid clinicId, [FromRoute(Name = "patientId")] Guid patientId)
        {
            var pathPatient = _settings.Username + $"/{clinicId}/{patientId}";

            try
            {
                await CloudFolder.List(_nextCloudService, pathPatient, CloudInfo.Properties.Id);
            }
            catch
            {
                await CloudFolder.Create(_nextCloudService, pathPatient);
            }
            
            return Ok();
        }
    }
}
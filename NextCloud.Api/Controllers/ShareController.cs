using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NextCloud.Api.Services;

namespace NextCloud.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ShareController : ControllerBase
    {
        private static Settings _settings;
        private static NextCloudService _nextCloudService;
        private readonly ILogger<ShareController> _logger;

        public ShareController(ILogger<ShareController> logger, IOptions<Settings> settings)
        {
            _logger = logger;

            _settings = settings.Value;

            List<string> errors = _settings.Validate();

            if (errors.Count > 0)
                throw new ApplicationException(string.Join("\r\n", errors));

            _nextCloudService = new NextCloudService(_settings);
        }

        [HttpGet("{clinicId}")]
        public async Task<IActionResult> GetByShareId([FromRoute(Name = "shareId")] string shareId)
        {
            return Ok(await Share.Get(_nextCloudService, shareId));
        }

        [HttpPost("{clinicId}/patient/{patientId}/create")]
        public async Task<IActionResult> CreateClinicShare([FromRoute(Name = "clinicId")] Guid clinicId, [FromRoute(Name = "patientId")] Guid patientId, [FromQuery(Name = "filename")] string filename)
        {
            var path = $"{clinicId}/{patientId}/{filename}";

            return Ok(await ShareServices.CreatePublicShare(_nextCloudService, path));
        }
    }
}
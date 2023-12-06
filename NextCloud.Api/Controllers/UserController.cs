using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;

namespace NextCloud.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private static Settings _settings;
        private static NextCloudService _nextCloudService;
        private readonly ILogger<UserController> _logger;

        public UserController(ILogger<UserController> logger, IOptions<Settings> settings)
        {
            _logger = logger;

            _settings = settings.Value;

            List<string> errors = _settings.Validate();

            if (errors.Count > 0)
                throw new ApplicationException(string.Join("\r\n", errors));

            _nextCloudService = new NextCloudService(_settings);
        }

        [HttpPost()]
        [SwaggerOperation(Summary = "Create user login on cloud")]
        [SwaggerResponse(200)]
        public async Task<IActionResult> CreateUser([FromBody] UserInfo userInfo)
        {
            await NextCloud.User.Create(_nextCloudService, userInfo);

            return Ok();
        }

        [HttpGet()]
        [SwaggerOperation(Summary = "Get cloud users")]
        [SwaggerResponse(200)]
        public async Task<IActionResult> GetUsers()
        {
            return Ok(await NextCloud.User.List(_nextCloudService));
        }
    }
}
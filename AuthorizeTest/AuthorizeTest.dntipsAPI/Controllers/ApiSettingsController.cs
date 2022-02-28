using AuthorizeTest.dntipsAPI.Models;
using AuthorizeTest.dntipsAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AuthorizeTest.dntipsAPI.Controllers
{
    [Route("[controller]")]
    [EnableCors("CorsPolicy")]
    public class ApiSettingsController : Controller
    {
        private readonly IOptionsSnapshot<ApiSettings> _apiSettingsConfig;

        public ApiSettingsController(IOptionsSnapshot<ApiSettings> apiSettingsConfig)
        {
            _apiSettingsConfig = apiSettingsConfig;
            _apiSettingsConfig.CheckArgumentIsNull(nameof(apiSettingsConfig));
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_apiSettingsConfig.Value); // For the Angular Client
        }
    }
}

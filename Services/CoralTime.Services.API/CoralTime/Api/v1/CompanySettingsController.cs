using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using static CoralTime.Common.Constants.Constants.Routes;

namespace CoralTime.Services.API.Api.v1
{
    [Authorize]
    [Route(BaseControllerRoute)]
    public class CompanySettingsController : BaseController<CompanySettingsController, object>
    {
        private readonly IConfiguration _config;

        public CompanySettingsController(ILogger<CompanySettingsController> logger, IConfiguration config) : 
            base(logger)
        {
            _config = config;
        }

        // GET api/v1/CompanySettings/WeekStart
        [HttpGet(CompanySettingsWeekStart)]
        public ActionResult GetWeekStart()
        {
            try
            {
                var startOfWeek = int.Parse(_config["CompanyReportStartOfWeek"]);
                return new JsonResult(new { startOfWeek });
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }
    }
}

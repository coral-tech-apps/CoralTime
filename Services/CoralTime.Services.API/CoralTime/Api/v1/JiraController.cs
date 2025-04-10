using CoralTime.BL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CoralTime.Common.Constants;
using Microsoft.AspNetCore.Authorization;
using CoralTime.ViewModels.JiraSettings;

namespace CoralTime.Services.API.Api.v1
{
    [Authorize]
    [Route(Constants.Routes.BaseControllerRoute)]
    public class JiraController : BaseController<JiraController, IJiraServices>
    {
        public JiraController(IJiraServices service, ILogger<JiraController> logger)
             : base(logger, service)
        {
        }

        // GET: api/v1/Jira
        [HttpGet]
        public IActionResult GetSettings(int memberId)
        {
            return Ok(_service.GetSettings(memberId));
        }

        // POST: api/v1/Jira
        [HttpPost]
        public IActionResult Create([FromBody] JiraSettingsView jiraSettingsView)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid Model");
            }

            _service.Create(jiraSettingsView);
            return Ok();
        }

        // PATCH: api/v1/Jira
        [HttpPatch]
        public IActionResult Update(string id, [FromBody] JiraSettingsView jiraSettingsView)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid Model");
            }

            _service.Update(jiraSettingsView, id);
            return Ok();
        }

        // DELETE: api/v1/Jira
        [HttpDelete]
        public IActionResult Delete(string id)
        {
            _service.DeleteSetting(id);
            return Ok();
        }
    }
}

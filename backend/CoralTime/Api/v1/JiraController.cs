using CoralTime.BL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CoralTime.Common.Constants;
using Microsoft.AspNetCore.Authorization;
using CoralTime.ViewModels.JiraSettings;
using System.Threading.Tasks;
using CoralTime.ViewModels.Jira;

namespace CoralTime.Api.v1
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
        public IActionResult GetSettings()
        {
            return Ok(_service.GetSettings());
        }

        // GET: api/v1/Jira/GetAssignedUsers
        [HttpGet("GetAssignedUsers")]
        public IActionResult GetAssignedUsers(int id)
        {
            return Ok(_service.GetAssignedUsers(id));
        }

        // GET: api/v1/Jira/GetNotAssignedUsers
        [HttpGet("GetNotAssignedUsers")]
        public IActionResult GetNotAssignedUsers(int id)
        {
            return Ok(_service.GetNotAssignedUsers(id));
        }

        // GET: api/v1/Jira/GetMemberSettings
        [HttpGet("GetMemberSettings")]
        public IActionResult GetJiraMemberSetting(int id)
        {
            return Ok(_service.GetMemberSetting(id));
        }

        // POST: api/v1/Jira
        [HttpPost]
        public IActionResult Create([FromBody] JiraSettingsView jiraSettingsView)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid Model");
            }

            _service.CreateSetting(jiraSettingsView);
            return Ok();
        }

        // POST: api/v1/Jira/AssignToIntegration
        [HttpPost("AssignToIntegration")]
        public IActionResult AssignToIntegration(int memberId, int jiraSettingId)
        {
            _service.AssignIntegrationToUser(memberId, jiraSettingId);
            return Ok();
        }

        // PATCH: api/v1/Jira
        [HttpPatch]
        public IActionResult Update(int id, [FromBody] JiraSettingsView jiraSettingsView)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid Model");
            }

            _service.UpdateSetting(jiraSettingsView, id);
            return Ok();
        }

        // PATCH: api/v1/Jira
        [HttpPatch("FillJiraMemberSetting")]
        public IActionResult FillJiraMember(int id, [FromBody] JiraMemberSettingView jiraMemberSettingView)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid Model");
            }

            _service.FillMemberJiraSetting(id, jiraMemberSettingView);
            return Ok();
        }

        // DELETE: api/v1/Jira
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            _service.DeleteSetting(id);
            return Ok();
        }

        // POST: api/v1/Jira/AssignToIntegration
        [HttpDelete("UnAssignToIntegration")]
        public IActionResult UnAssignToIntegration(int memberId, int jiraSettingId)
        {
            _service.UnAssingIntegrationToUser(memberId, jiraSettingId);
            return Ok();
        }
    }
}

using CoralTime.BL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CoralTime.Common.Constants;
using Microsoft.AspNetCore.Authorization;
using CoralTime.ViewModels.JiraSettings;
using System.Threading.Tasks;
using CoralTime.ViewModels.Jira;
using System;

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
        [HttpGet(Constants.Routes.GetAssignedUsers)]
        public IActionResult GetAssignedUsers(int id)
        {
            return Ok(_service.GetAssignedUsers(id));
        }

        // GET: api/v1/Jira/GetNotAssignedUsers
        [HttpGet(Constants.Routes.GetNotAssignedUsers)]
        public IActionResult GetNotAssignedUsers(int id)
        {
            return Ok(_service.GetNotAssignedUsers(id));
        }

        // GET: api/v1/Jira/GetMemberSettings
        [HttpGet(Constants.Routes.GetMemberSettings)]
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

            try
            {
                _service.CreateSetting(jiraSettingsView);
                return Ok();
            }
            catch(Exception e)
            {
                return BadRequest(new { message = e.Message });
            }
        }

        // POST: api/v1/Jira/AssignToIntegration
        [HttpPost(Constants.Routes.AssignToIntegration)]
        public IActionResult AssignToIntegration(int memberId, int jiraSettingId)
        {
            try
            {
                _service.AssignIntegrationToUser(memberId, jiraSettingId);
                return Ok();
            }
            catch(Exception e)
            {
                return BadRequest(new { message = e.Message});
            }
        }

        // PATCH: api/v1/Jira
        [HttpPatch]
        public IActionResult Update(int id, [FromBody] JiraSettingsView jiraSettingsView)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid Model");
            }

            try
            {
                _service.UpdateSetting(jiraSettingsView, id);
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }
        }

        // PATCH: api/v1/Jira
        [HttpPatch(Constants.Routes.FillJiraMemberSetting)]
        public IActionResult FillJiraMember(int id, [FromBody] JiraMemberSettingView jiraMemberSettingView)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid Model");
            }

            try
            {
                _service.FillMemberJiraSetting(id, jiraMemberSettingView);
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }
        }

        // DELETE: api/v1/Jira
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            try
            {
                _service.DeleteSetting(id);
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }
        }

        // POST: api/v1/Jira/AssignToIntegration
        [HttpDelete(Constants.Routes.UnAssignToIntegration)]
        public IActionResult UnAssignToIntegration(int memberId, int jiraSettingId)
        {
            try
            {
                _service.UnAssingIntegrationToUser(memberId, jiraSettingId);
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }
        }
    }
}

using CoralTime.BL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using CoralTime.ViewModels.JiraSettings;
using System.Threading.Tasks;
using CoralTime.ViewModels.Jira;
using System;
using static CoralTime.Common.Constants.Constants;
using static CoralTime.Common.Constants.Constants.Routes;
using ODataRoutes = CoralTime.Common.Constants.Constants.Routes.OData;

namespace CoralTime.Services.API.Api.v1.Odata.Jira
{
    [Authorize]
    [Route(BaseControllerRoute)]
    public class JiraController : BaseODataController<JiraController, IJiraServices>
    {
        public JiraController(IJiraServices service, ILogger<JiraController> logger)
             : base(logger, service)
        {
        }

        // GET: api/v1/odata/Jira/GetSettings()
        // GET: api/v1/Jira
        [HttpGet]
        [HttpGet(ODataRoutes.GetSettings)]
        public IActionResult GetSettings()
        {
            return Ok(_service.GetSettings());
        }

        // GET: api/v1/Jira/GetJiraUserIdStatus?id=7
        [HttpGet(GetJiraUserIdStatusRoute)]
        public async Task<IActionResult> ConnectJiraAsync(int id)
        {
            try
            {
                await _service.FillJiraUserId(id);
                return Ok();
            }
            catch(Exception e)
            {
               return BadRequest(new { message = e.Message });
            }
        }

        // GET: api/v1/odata/Jira/GetAssignedUsers(id=7)
        // GET: api/v1/Jira/GetAssignedUsers?id=7
        [HttpGet(ODataRoutes.GetAssignedUsers)]
        public IActionResult GetAssignedUsers(int id)
        {
            return Ok(_service.GetAssignedUsers(id));
        }

        // GET: api/v1/odata/Jira/GetNotAssignedUsers(id=7)
        // GET: api/v1/Jira/GetNotAssignedUsers?id=7
        [HttpGet(ODataRoutes.GetNotAssignedUsers)]
        public IActionResult GetNotAssignedUsers(int id)
        {
            return Ok(_service.GetNotAssignedUsers(id));
        }

        //GET: api/v1/Jira/GetMemberSetting
        [HttpGet(GetMemberSettingRoute)]
        public IActionResult GetMemberSetting(int id)
        {
            return Ok(_service.GetJiraMemberSetting(id));
        }

        // GET: api/v1/Jira/GetMemberSettings
        [HttpGet(GetJiraMemberSettingsRoute)]
        public IActionResult GetJiraMemberSettings(int id)
        {
            return Ok(_service.GetMemberSettings(id));
        }

        // POST: api/v1/Jira
        [HttpPost]
        [Authorize(Policy = PolicyCreateJiraIntegration)]
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
        [HttpPost(AssignToIntegrationRoute)]
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
        [Authorize(Policy = PolicyEditJiraIntegration)]
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

        // PATCH: api/v1/Jira/FillJiraMemberSetting
        [HttpPatch(FillJiraMemberSettingRoute)]
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

        // DELETE: api/v1/Jira/UnAssignToIntegration
        [HttpDelete(UnAssignToIntegrationRoute)]
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

using CoralTime.BL.Interfaces;
using CoralTime.Common.Constants;
using CoralTime.ViewModels.Jira;
using CoralTime.ViewModels.JiraSettings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CoralTime.Api.v1
{
    [Authorize]
    [Route(Constants.Routes.BaseControllerRoute)]
    public class JiraProjectController : BaseController<JiraProjectController, IJiraProjectService>
    {
        public JiraProjectController(IJiraProjectService service, ILogger<JiraProjectController> logger)
             : base(logger, service)
        {
        }

        // GET: api/v1/JiraProject/GetAllJiraProjectsBySettingId
        [HttpGet(Constants.Routes.GetAllJiraProjectsBySettingId)]
        public IActionResult GetJiraProjects(int jiraSettingId)
        {
            return Ok(_service.GetJiraProjects(jiraSettingId));
        }

        // GET: api/v1/JiraProject/GetUnAssignJiraProject
        [HttpGet(Constants.Routes.GetUnAssignJiraProject)]
        public IActionResult GetUnAssignJiraProject(int jiraSettingId)
        {
            return Ok(_service.GetUnAssignJiraProject(jiraSettingId));
        }

        // GET: api/v1/JiraProject/GetAssignJiraProject
        [HttpGet(Constants.Routes.GetAssignJiraProject)]
        public IActionResult GetAssingJiraProject(int jiraSettingId)
        {
            return Ok(_service.GetAssingJiraProject(jiraSettingId));
        }

        // POST: api/v1/JiraProject/GetAssignJiraProject
        [HttpPost(Constants.Routes.LoadJiraProject)]
        public async Task<IActionResult> LoadJiraProjects(string domain, string apiToken, string email)
        {
            try
            {
               await _service.LoadJiraProject(domain, apiToken, email);
               return Ok();
            }
            catch(Exception ex)
            {
                return BadRequest(ex);
            }
        } 
    }
}


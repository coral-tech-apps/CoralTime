using CoralTime.BL.Interfaces;
using CoralTime.Common.Constants;
using CoralTime.ViewModels.Jira;
using CoralTime.ViewModels.JiraSettings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using static CoralTime.Common.Constants.Constants;
using static CoralTime.Common.Constants.Constants.Routes;
using static CoralTime.Common.Constants.Constants.Routes.OData;

namespace CoralTime.Api.v1.Odata.Jira
{
    [Authorize]
    [Route(BaseODataControllerRoute)]
    public class JiraProjectController : BaseODataController<JiraProjectController, IJiraProjectService>
    {
        public JiraProjectController(IJiraProjectService service, ILogger<JiraProjectController> logger)
             : base(logger, service)
        {
        }

        // GET: api/v1/odata/JiraProject/GetAllJiraProjectsBySettingId
        [ODataRouteComponent(GetAllJiraProjectsBySettingIdRotute)]
        [HttpGet(Constants.Routes.GetAllJiraProjectsBySettingId)]
        public IActionResult GetJiraProjects(int id)
        {
            return Ok(_service.GetJiraProjects(id));
        }

        // GET: api/v1/odata/JiraProject/GetUnAssignJiraProject
        [ODataRouteComponent(GetUnAssignedJiraProjectRoute)]
        [HttpGet(Constants.Routes.GetUnAssignJiraProject)]
        public IActionResult GetUnAssignJiraProject(int id)
        {
            return Ok(_service.GetUnAssignJiraProject(id));
        }

        // GET: api/v1/odata/JiraProject/GetAssignJiraProject
        [ODataRouteComponent(GetAssignedJiraProjectRoute)]
        [HttpGet(Constants.Routes.GetAssignJiraProject)]
        public IActionResult GetAssingJiraProject(int id)
        {
            return Ok(_service.GetAssingJiraProject(id));
        }

        // POST: api/v1/odata/JiraProject/GetAssignJiraProject
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


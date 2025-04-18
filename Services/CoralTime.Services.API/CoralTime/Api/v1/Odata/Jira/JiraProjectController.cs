using CoralTime.BL.Interfaces;
using CoralTime.Common.Constants;
using CoralTime.DAL.Models.Jira;
using CoralTime.DAL.Models;
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
using static Duende.IdentityServer.Models.IdentityResources;

namespace CoralTime.Services.API.Api.v1.Odata.Jira 
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
        public async Task<IActionResult> LoadJiraProjects(int id)
        {
            try
            {
               await _service.LoadJiraProject(id);
               return Ok();
            }
            catch(Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost(Constants.Routes.LinkProjects)]
        public IActionResult LinkProjectJiraProject(int projectId, int jiraProjectId)
        {
            try
            {
                _service.LinkJiraProject(projectId, jiraProjectId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpDelete(Constants.Routes.RemovProjectJiraLink)]
        public IActionResult RemoveProjectJiraLink(int id)
        {
            try
            {
                _service.RemoveProjectJiraLink(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}


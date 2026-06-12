using CoralTime.BL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using ODataRoutes = CoralTime.Common.Constants.Constants.Routes.OData;
using static CoralTime.Common.Constants.Constants.Routes;

namespace CoralTime.Services.API.Api.v1.Odata.Jira 
{
    [Authorize]
    [Route(BaseControllerRoute)]
    public class JiraProjectController : BaseODataController<JiraProjectController, IJiraProjectService>
    {
        public JiraProjectController(IJiraProjectService service, ILogger<JiraProjectController> logger)
             : base(logger, service)
        {
        }

        // GET: api/v1/odata/JiraProject/GetAllJiraProjectsBySettingId(id=7)
        // GET: api/v1/JiraProject/GetAllJiraProjectsBySettingId?id=7
        [HttpGet(ODataRoutes.GetAllJiraProjectsBySettingId)]
        public IActionResult GetAllJiraProjectsBySettingId(int id)
        {
            return Ok(_service.GetJiraProjects(id));
        }

        // GET: api/v1/odata/JiraProject/GetUnAssignJiraProject(id=7)
        // GET: api/v1/JiraProject/GetUnAssignJiraProject?id=7
        [HttpGet(ODataRoutes.GetUnAssignJiraProject)]
        [AllowAnonymous]
        public IActionResult GetUnAssignJiraProject(int id)
        {
            return Ok(_service.GetUnAssignJiraProject(id));
        }

        // GET: api/v1/odata/JiraProject/GetAssignJiraProject(id=7)
        // GET: api/v1/JiraProject/GetAssignJiraProject?id=7
        [HttpGet(ODataRoutes.GetAssignJiraProject)]
        public IActionResult GetAssignJiraProject(int id)
        {
            return Ok(_service.GetAssingJiraProject(id));
        }

        // POST: api/v1/JiraProject/LoadJiraProject
        [HttpPost(LoadJiraProjectRoute)]
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

        [HttpPost(LinkProjectsRoute)]
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

        [HttpDelete(RemovProjectJiraLinkRoute)]
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


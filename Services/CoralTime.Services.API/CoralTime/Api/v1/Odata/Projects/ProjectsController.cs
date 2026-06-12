using CoralTime.BL.Interfaces;
using CoralTime.ViewModels.Projects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using static CoralTime.Common.Constants.Constants;
using ODataRoutes = CoralTime.Common.Constants.Constants.Routes.OData;
using static CoralTime.Common.Constants.Constants.Routes;

namespace CoralTime.Services.API.Api.v1.Odata.Projects
{
    [Authorize]
    [Route(BaseControllerRoute)]
    public class ProjectsController : BaseODataController<ProjectsController, IProjectService>
    {
        public ProjectsController(IProjectService service, ILogger<ProjectsController> logger)
            : base(logger, service) { }

        // TODO: returns isnt IQuerable -> response without Count
        // GET: api/v1/odata/Projects/GetTimeTrackerAllProjects()
        // GET: api/v1/Projects
        [HttpGet]
        [HttpGet(ODataRoutes.GetTimeTrackerAllProjects)]
        public IActionResult GetTimeTrackerAllProjects()
        {
            try
            {
                return Ok(_service.TimeTrackerAllProjects());
            }
            catch (Exception e)
            {
                return SendErrorODataResponse(e);
            }
        }

        // GET: api/v1/odata/Projects/GetMembers(id=2)
        // GET: api/v1/Projects/GetMembers?id=7
        [HttpGet(ODataRoutes.GetProjectMembers)]
        public IActionResult GetMembers(int id)
        {
            try
            {
                return Ok(_service.GetMembers(id));
            }
            catch (Exception e)
            {
                return SendErrorODataResponse(e);
            }
        }

        // GET api/v1/Projects/7
        [HttpGet(IdRoute)]
        public IActionResult GetById(int id)
        {
            try
            {
                var result = _service.GetById(id);
                return new ObjectResult(result);
            }
            catch (Exception e)
            {
                return SendErrorODataResponse(e);
            }
        }

        // POST api/v1/Projects
        [Authorize(Policy = PolicyAddProject)]
        [HttpPost]
        public IActionResult Create([FromBody] ProjectView projectData)
        {
            if (!ModelState.IsValid)
            {
                return SendInvalidModelResponse();
            }

            try
            {
                var result = _service.Create(projectData);
                var locationUri = $"{Request.Host}/{ODataRoutes.BaseODataApiRoute}/Projects({result.Id})";

                return Created(locationUri, result);
            }
            catch (Exception e)
            {
                return SendErrorODataResponse(e);
            }
        }

        // PUT api/v1/Projects/7
        [HttpPut(IdRoute)]
        public IActionResult Update(int id, [FromBody] JsonElement project)
        {
            if (!ModelState.IsValid)
            {
                return SendInvalidModelResponse();
            }

            try
            {
                var result = _service.Update(id, project);
                return new ObjectResult(result);
            }
            catch (Exception e)
            {
                return SendErrorODataResponse(e);
            }
        }

        // PATCH api/v1/Projects/7
        [HttpPatch(IdRoute)]
        public IActionResult Patch(int id, [FromBody] JsonElement project)
        {
            if (!ModelState.IsValid)
            {
                return SendInvalidModelResponse();
            }

            try
            {
                var result = _service.Patch(id, project);
                return new ObjectResult(result);
            }
            catch (Exception e)
            {
                return SendErrorODataResponse(e);
            }
        }

        // DELETE api/v1/Projects/7
        [Authorize(Policy = PolicyEditProject)]
        [HttpDelete(IdRoute)]
        public IActionResult Delete(int id)
        {
            return BadRequest($"Can't delete the project with Id - {id}");
        }
    }
}
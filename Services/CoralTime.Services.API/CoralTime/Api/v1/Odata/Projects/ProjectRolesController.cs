using CoralTime.BL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using static CoralTime.Common.Constants.Constants.Routes;

namespace CoralTime.Services.API.Api.v1.Odata.Projects
{
    [Route(BaseControllerRoute)]
    [Authorize]
    public class ProjectRolesController : BaseODataController<ProjectRolesController, IMemberProjectRoleService>
    {
        public ProjectRolesController(IMemberProjectRoleService service, ILogger<ProjectRolesController> logger)
            : base(logger, service) { }

        // GET: api/v1/odata/ProjectRoles/GetAllProjectRoles()
        // GET: api/v1/ProjectRoles
        [HttpGet]
        [HttpGet(OData.GetAllProjectRoles)]
        public IActionResult GetAllProjectRoles()
        {
            try
            {
                var result = _service.GetProjectRoles();

                return Ok(result);
            }
            catch (Exception e)
            {
                return SendErrorODataResponse(e);
            }
        }
    }
}
using CoralTime.BL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using static CoralTime.Common.Constants.Constants.Routes;
using ODataRoutes = CoralTime.Common.Constants.Constants.Routes.OData;

namespace CoralTime.Services.API.Api.v1.Odata.Projects
{
    [Route(BaseControllerRoute)]
    [Authorize]
    public class ManagerProjectsController : BaseODataController<ManagerProjectsController, IProjectService>
    {
        public ManagerProjectsController(ILogger<ManagerProjectsController> logger, IProjectService service)
            : base(logger, service) { }

        // TODO: returns isnt IQuerable -> response without Count
        // GET: api/v1/odata/ManagerProjects/GetManageProjectsOfManager()
        // GET: api/v1/ManagerProjects
        [HttpGet]
        [HttpGet(ODataRoutes.GetManageProjectsOfManager)]
        public IActionResult GetManageProjectsOfManager()
        {
            try
            {
                return Ok(_service.ManageProjectsOfManager());
            }
            catch (Exception e)
            {
                return SendErrorODataResponse(e);
            }
        }
    }
}
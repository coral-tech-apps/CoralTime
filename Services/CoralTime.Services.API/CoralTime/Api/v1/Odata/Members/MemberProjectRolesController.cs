using CoralTime.BL.Interfaces;
using CoralTime.ViewModels.MemberProjectRoles;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using static CoralTime.Common.Constants.Constants.Routes;
using static CoralTime.Common.Constants.Constants.Routes.OData;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.AspNetCore.OData.Formatter;

namespace CoralTime.Services.API.Api.v1.Odata.Members
{
    [Route(BaseODataControllerRoute)]
    [Authorize]
    public class MemberProjectRolesController : BaseODataController<MemberProjectRolesController, IMemberProjectRoleService>
    {
        public MemberProjectRolesController(ILogger<MemberProjectRolesController> logger, IMemberProjectRoleService service)
            : base(logger, service) { }

        // GET: api/v1/odata/MemberProjectRoles
        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                return Ok(_service.GetAllProjectRoles());
            }
            catch (Exception e)
            {
                return SendErrorODataResponse(e);
            }
        }

        // GET api/v1/odata/MemberProjectRoles(2)
        [ODataRouteComponent(MemberProjectRolesWithIdRoute)]
        [HttpGet(IdRoute)]
        public IActionResult GetById([FromODataUri]int id)
        {
            try
            {
                return new ObjectResult(_service.GetById(id));
            }
            catch (Exception e)
            {
                return SendErrorODataResponse(e);
            }
        }

        // GET api/v1/odata/MemberProjectRoles(2)/members
        [ODataRouteComponent(MemberProjectRolesRouteWithMembers)]
        [HttpGet(IdRouteWithMembers)]
        public IActionResult GetNotAssignMembersAtProjByProjectId([FromODataUri] int id)
        {
            try
            {
                return Ok(_service.GetNotAssignMembersAtProjByProjectId(id));
            }
            catch (Exception e)
            {
                return SendErrorODataResponse(e);
            }
        }

        // GET api/v1/odata/MemberProjectRoles(2)/projects
        [ODataRouteComponent(MemberProjectRolesRouteWithProjects)]
        [HttpGet(IdRouteWithProjects)]
        public IActionResult GetNotAssignMembersAtProjByMemberId([FromODataUri] int id)
        {
            try
            {
                return Ok(_service.GetNotAssignMembersAtProjByMemberId(id));
            }
            catch (Exception e)
            {
                return SendErrorODataResponse(e);
            }
        }

        // POST: api/v1/odata/MemberProjectRoles
        [HttpPost]
        public IActionResult Create([FromBody] MemberProjectRoleView projectRole)
        {
            if (!ModelState.IsValid)
            {
                return SendInvalidModelResponse();
            }

            try
            {
                var value = _service.Create(projectRole);
                var locationUri = $"{Request.Host}/{BaseODataRoute}/MemberProjectRoles({value.Id})";

                return Created(locationUri, value);
            }
            catch (Exception e)
            {
                return SendErrorODataResponse(e);
            }
        }

        // PUT: api/v1/odata/MemberProjectRoles(2)
        [ODataRouteComponent(MemberProjectRolesWithIdRoute)]
        [HttpPut(IdRoute)]
        public IActionResult Update([FromODataUri] int id, [FromBody] MemberProjectRoleView projectRole)
        {
            if (!ModelState.IsValid)
            {
                return SendInvalidModelResponse();
            }

            projectRole.Id = id;
            try
            {
                var value = _service.Update(projectRole);

                return new ObjectResult(value);
            }
            catch (Exception e)
            {
                return SendErrorODataResponse(e);
            }
        }

        // PATCH: api/v1/odata/MemberProjectRoles(2)
        [ODataRouteComponent(MemberProjectRolesWithIdRoute)]
        [HttpPatch(IdRoute)]
        public IActionResult Patch([FromODataUri] int id, [FromBody] MemberProjectRoleView projectRole)
        {
            if (!ModelState.IsValid)
            {
                return SendInvalidModelResponse();
            }

            projectRole.Id = id;

            try
            {
                var value = _service.Patch(projectRole);
                return new ObjectResult(value);
            }
            catch (Exception e)
            {
                return SendErrorODataResponse(e);
            }
        }

        //DELETE :api/v1/odata/MemberProjectRoles(1)
        [ODataRouteComponent(MemberProjectRolesWithIdRoute)]
        [HttpDelete(IdRoute)]
        public IActionResult Delete([FromODataUri] int id)
        {
            try
            {
                _service.Delete(id);

                return new ObjectResult(null);
            }
            catch (Exception e)
            {
                return SendErrorODataResponse(e);
            }
        }
    }
}
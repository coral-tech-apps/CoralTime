using CoralTime.BL.Interfaces;
using CoralTime.ViewModels.MemberProjectRoles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using static CoralTime.Common.Constants.Constants.Routes;
using ODataRoutes = CoralTime.Common.Constants.Constants.Routes.OData;

namespace CoralTime.Services.API.Api.v1.Odata.Members
{
    [Route(BaseControllerRoute)]
    [Authorize]
    public class MemberProjectRolesController : BaseODataController<MemberProjectRolesController, IMemberProjectRoleService>
    {
        public MemberProjectRolesController(ILogger<MemberProjectRolesController> logger, IMemberProjectRoleService service)
            : base(logger, service) { }

        // TODO: returns isnt IQuerable -> response without Count
        // GET: api/v1/odata/MemberProjectRoles/GetAllMemberProjectRoles()
        // GET: api/v1/MemberProjectRoles
        [HttpGet]
        [HttpGet(ODataRoutes.GetAllMemberProjectRoles)]
        public IActionResult GetAllMemberProjectRoles()
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

        // GET api/v1/MemberProjectRoles?id=7
        [HttpGet(IdRoute)]
        public IActionResult GetById(int id)
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

        // GET: api/v1/odata/MemberProjectRoles/GetMembers(id=2)
        // GET: api/v1/MemberProjectRoles/GetMembers?id=7
        [HttpGet(ODataRoutes.GetNotAssignedProjectMembers)]
        public IActionResult GetNotAssignedProjectMembers(int id)
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

        // GET: api/v1/odata/MemberProjectRoles/GetProjects(id=2)
        // GET: api/v1/MemberProjectRoles/GetProjects?id=7
        [HttpGet(ODataRoutes.GetProjects)]
        public IActionResult GetProjects(int id)
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

        // POST: api/v1/MemberProjectRoles
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
                var locationUri = $"{Request.Host}/{ODataRoutes.BaseODataApiRoute}/MemberProjectRoles({value.Id})";

                return Created(locationUri, value);
            }
            catch (Exception e)
            {
                return SendErrorODataResponse(e);
            }
        }

        // PUT: api/v1/MemberProjectRoles/7
        [HttpPut(IdRoute)]
        public IActionResult Update(int id, [FromBody] MemberProjectRoleView projectRole)
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

        // PATCH: api/v1/MemberProjectRoles/7
        [HttpPatch(IdRoute)]
        public IActionResult Patch(int id, [FromBody] MemberProjectRoleView projectRole)
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

        //DELETE :api/v1/MemberProjectRoles/7
        [HttpDelete(IdRoute)]
        public IActionResult Delete(int id)
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
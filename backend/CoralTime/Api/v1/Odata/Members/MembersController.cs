using CoralTime.BL.Interfaces;
using CoralTime.ViewModels.Member;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using static CoralTime.Common.Constants.Constants;
using static CoralTime.Common.Constants.Constants.Routes;
using static CoralTime.Common.Constants.Constants.Routes.OData;
using Microsoft.AspNetCore.OData.Formatter;

namespace CoralTime.Api.v1.Odata.Members
{
    [Authorize]
    [Route(BaseODataControllerRoute)]
    public class MembersController : BaseODataController<MembersController, IMemberService>
    {
        public MembersController(ILogger<MembersController> logger, IMemberService service)
            : base(logger, service) { }

        // GET: api/v1/odata/Members
        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                return Ok(_service.GetAllMembers());
            }
            catch (Exception e)
            {
                return SendErrorODataResponse(e);
            }
        }

        // GET api/v1/odata/Members(5)
        [ODataRouteComponent(MembersWithIdRoute)]
        [HttpGet(IdRoute)]
        public IActionResult GetById([FromODataUri]int id)
        {
            try
            {
                return Ok(_service.GetById(id));
            }
            catch (Exception e)
            {
                return SendErrorODataResponse(e);
            }
        }

        // GET api/v1/odata/Members(2)/projects
        [ODataRouteComponent(MembersRouteWithProjects)]
        [HttpGet(IdRouteWithProjects)]
        public IActionResult GetProjects([FromODataUri]int id)
        {
            try
            {
                return Ok(_service.GetTimeTrackerAllProjects(id));
            }
            catch (Exception e)
            {
                return SendErrorODataResponse(e);
            }
        }

        // POST: api/v1/odata/Members
        [HttpPost]
        [Authorize(Roles = ApplicationRoleAdmin)]
        public async Task<IActionResult> Create([FromBody] MemberView memberView)
        {
            if (!ModelState.IsValid)
            {
                return SendInvalidModelResponse();
            }

            var createdMemberView = await _service.CreateNewUser(memberView, GetBaseUrl());

            var locationUri = $"{Request.Host}/{BaseODataRouteComponent}/Members/{memberView.Id}";

            return base.Created(locationUri, (object)createdMemberView);
        }

        // PUT: api/v1/odata/Members(1)
        [ODataRouteComponent(MembersWithIdRoute)]
        [HttpPut(IdRoute)]
        public async Task<IActionResult> Update([FromODataUri]int id, [FromBody]MemberView memberView)
        {
            if (!ModelState.IsValid)
            {
                return SendInvalidModelResponse();
            }

            memberView.Id = id;
            
            try
            {
                return Ok(await _service.Update(memberView, GetBaseUrl()));
            }
            catch (Exception e)
            {
                return SendErrorODataResponse(e);
            }
        }

        //DELETE :api/v1/odata/Members(1)
        [ODataRouteComponent(MembersWithIdRoute)]
        [HttpDelete(IdRoute)]
        [Authorize(Roles = ApplicationRoleAdmin)]
        public IActionResult Delete([FromODataUri]int id) => BadRequest($"Can't delete the member with Id - {id}");
    }
}
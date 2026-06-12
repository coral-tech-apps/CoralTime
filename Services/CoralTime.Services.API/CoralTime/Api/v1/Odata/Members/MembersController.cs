using CoralTime.BL.Interfaces;
using CoralTime.ViewModels.Member;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using static CoralTime.Common.Constants.Constants;
using static CoralTime.Common.Constants.Constants.Routes;
using ODataRoutes = CoralTime.Common.Constants.Constants.Routes.OData;

namespace CoralTime.Services.API.Api.v1.Odata.Members
{
    [Authorize]
    [Route(BaseControllerRoute)]
    public class MembersController : BaseODataController<MembersController, IMemberService>
    {
        public MembersController(ILogger<MembersController> logger, IMemberService service)
            : base(logger, service) { }

        // TODO: returns isnt IQuerable -> response without Count
        // GET: api/v1/odata/Members/GetAllMembers()
        // GET: api/v1/Members
        [HttpGet]
        [HttpGet(ODataRoutes.GetAllMembers)]
        public IActionResult GetAllMembers()
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

        // GET api/v1/Members/7
        [HttpGet(IdRoute)]
        public IActionResult GetById(int id)
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

        // GET: api/v1/odata/Members/GetProjects(id=2)
        // GET: api/v1/Members/GetProjects?id=2
        [HttpGet(ODataRoutes.GetProjects)]
        public IActionResult GetProjects(int id)
        {
            try
            {
                // TODO: returns not IQuerable -> response without Count
                return Ok(_service.GetTimeTrackerAllProjects(id));
            }
            catch (Exception e)
            {
                return SendErrorODataResponse(e);
            }
        }

        // POST: api/v1/Members
        [HttpPost]
        [Authorize(Policy = PolicyAddMember)]
        public async Task<IActionResult> Create([FromBody] MemberView memberView)
        {
            if (!ModelState.IsValid)
            {
                return SendInvalidModelResponse();
            }

            var createdMemberView = await _service.CreateNewUser(memberView, GetBaseUrl());

            var locationUri = $"{Request.Host}/{ODataRoutes.BaseODataApiRoute}/Members/{memberView.Id}";

            return base.Created(locationUri, createdMemberView);
        }

        // PUT: api/v1/Members/7
        [HttpPut(IdRoute)]
        public async Task<IActionResult> Update(int id, [FromBody]MemberView memberView)
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

        //DELETE :api/v1/Members/7
        [HttpDelete(IdRoute)]
        [Authorize(Policy = PolicyEditMember)]
        public IActionResult Delete(int id) => BadRequest($"Can't delete the member with Id - {id}");

        [HttpGet(ODataRoutes.IsJiraEnableRoute)]
        public IActionResult IsJiraEnable()
        {
            try
            {
                return Ok(_service.IsJiraEnable());
            }
            catch(Exception e)
            {
                return SendErrorODataResponse(e);
            }
        }

        [HttpPost(ChangeJiraFieldRoute)]
        public IActionResult ChangeJiraEnableField(bool jiraSatus)
        {
            try
            {
                return Ok(_service.SetJiraEnableStatus(jiraSatus));
            }
            catch (Exception e)
            {
                return SendErrorODataResponse(e);
            }
        }
    }
}
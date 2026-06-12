using CoralTime.BL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using static CoralTime.Common.Constants.Constants;
using static CoralTime.Common.Constants.Constants.Routes;


namespace CoralTime.Services.API.Api.v1.Odata
{
    [Route(BaseControllerRoute)]
    [Authorize(Roles = ApplicationRoleAdmin)]
    public class MemberActionsController : BaseODataController<MemberActionsController, IMemberActionService>
    {
        public MemberActionsController(IMemberActionService service, ILogger<MemberActionsController> logger)
            : base(logger, service)
        {
        }

        // GET: api/v1/odata/MemberActions/GetAllMemberActions()
        // GET: api/v1/MemberActions
        [HttpGet]
        [HttpGet(OData.GetAllMemberActions)]
        public IActionResult GetAllMemberActions() => Ok(_service.Get());
    }
}
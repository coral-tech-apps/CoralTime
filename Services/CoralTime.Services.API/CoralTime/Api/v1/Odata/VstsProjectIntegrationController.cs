using CoralTime.BL.Interfaces;
using CoralTime.Common.Exceptions;
using CoralTime.ViewModels.Vsts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using static CoralTime.Common.Constants.Constants;
using static CoralTime.Common.Constants.Constants.Routes;
using ODataRoutes = CoralTime.Common.Constants.Constants.Routes.OData;

namespace CoralTime.Services.API.Api.v1.Odata
{
    [Route(BaseControllerRoute)]
    [Authorize(Policy = PolicyViewIntegrationPage)]
    public class VstsProjectIntegrationController : BaseODataController<VstsProjectIntegrationController, IVstsService>
    {
        private readonly IVstsAdminService _vstsAdminService;

        public VstsProjectIntegrationController(IVstsService service, IVstsAdminService vstsAdminService, ILogger<VstsProjectIntegrationController> logger)
            : base(logger, service)
        {
            _vstsAdminService = vstsAdminService;
        }

        // GET: api/v1/odata/VstsProjectIntegration/GetAllVstsProjects()
        // GET: api/v1/VstsProjectIntegration
        [HttpGet]
        [HttpGet(ODataRoutes.GetAllVstsProjects)]
        public IActionResult GetAllVstsProjects() => new ObjectResult(_service.Get());

        // GET: api/v1/odata/VstsProjectIntegration/GetMembers(id=2)
        // GET: api/v1/VstsProjectIntegration/GetMembers&id=2
        [HttpGet(OData.GetVstsProjectMembers)]
        public IActionResult GetMembers(int id)
        {
            try
            {
                return Ok(_service.GetMembersByProjectId(id));
            }
            catch (Exception e)
            {
                return SendErrorODataResponse(e);
            }
        }

        // POST api/v1/VstsProjectIntegration
        [HttpPost]
        public IActionResult Create([FromBody]VstsProjectIntegrationView vstsProjectIntegrationView)
        {
            if (!ModelState.IsValid)
            {
                return SendInvalidModelResponse();
            }

            try
            {
                var vstsProjectIntegrationViewResult = _service.Create(vstsProjectIntegrationView);

                UpdateVstsInfo(vstsProjectIntegrationViewResult);

                var locationUri = $"{Request.Host}/{ODataRoutes.BaseODataApiRoute}/VstsProjectIntegrationView({vstsProjectIntegrationViewResult.Id})";

                return Created(locationUri, vstsProjectIntegrationViewResult);
            }
            catch (Exception e)
            {
                return SendErrorODataResponse(e);
            }
        }

        // PUT api/v1/VstsProjectIntegration/1
        [HttpPut(IdRoute)]
        public IActionResult Update(int id, [FromBody]VstsProjectIntegrationView vstsProjectIntegrationView)
        {
            if (!ModelState.IsValid)
            {
                return SendInvalidModelResponse();
            }

            vstsProjectIntegrationView.Id = id;

            try
            {
                var vstsProjectIntegrationViewResult = _service.Update(vstsProjectIntegrationView);

                UpdateVstsInfo(vstsProjectIntegrationViewResult);

                return new ObjectResult(vstsProjectIntegrationViewResult);
            }
            catch (Exception e)
            {
                return SendErrorODataResponse(e);
            }
        }

        //DELETE :api/v1/VstsProjectIntegration/1
        [HttpDelete(IdRoute)]
        public IActionResult Delete(int id)
        {
            try
            {
                var result = _service.Delete(id);
                return new ObjectResult(null);
            }
            catch (Exception e)
            {
                return SendErrorODataResponse(e);
            }
        }

        private void UpdateVstsInfo(VstsProjectIntegrationView vstsProjectIntegrationViewResult)
        {
            var resultOfUpdateVstsProject = _vstsAdminService.UpdateVstsProject(vstsProjectIntegrationViewResult.Id);

            if (!resultOfUpdateVstsProject)
            {
                throw new CoralTimeSafeEntityException("Error getting VSTS project info");
            }

            var resultOfGettingUserInfo = _vstsAdminService.UpdateVstsUsersByProject(vstsProjectIntegrationViewResult.Id);

            if (!resultOfGettingUserInfo)
            {
                throw new CoralTimeSafeEntityException("Error getting VSTS users info");
            }
        }
    }
}
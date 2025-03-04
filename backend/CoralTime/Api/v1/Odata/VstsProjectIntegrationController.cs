using CoralTime.BL.Interfaces;
using CoralTime.Common.Exceptions;
using CoralTime.ViewModels.Vsts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.Extensions.Logging;
using System;
using static CoralTime.Common.Constants.Constants;
using static CoralTime.Common.Constants.Constants.Routes;
using static CoralTime.Common.Constants.Constants.Routes.OData;
using Microsoft.AspNetCore.OData.Formatter;

namespace CoralTime.Api.v1.Odata
{
    [Route(BaseODataControllerRoute)]
    [Authorize(Roles = ApplicationRoleAdmin)]
    public class VstsProjectIntegrationController : ODataController
    {
        private readonly IVstsService _service;
        private readonly IVstsAdminService _vstsAdminService;
        private readonly ILogger<VstsProjectIntegrationController> _logger;

        public VstsProjectIntegrationController(IVstsService service, IVstsAdminService vstsAdminService, ILogger<VstsProjectIntegrationController> logger)
        {
            _service = service;
            _vstsAdminService = vstsAdminService;
            _logger = logger;
        }

        // GET: api/v1/odata/VstsProjectIntegration
        [HttpGet]
        public IActionResult Get() => Ok(_service.Get());

        // GET api/v1/odata/VstsProjectIntegration(2)/members
        [HttpGet(IdRouteWithMembers)]
        [ODataRouteComponent(VstsProjectIntegrationMembersByProject)]
        public IActionResult GetNotAssignMembersAtProjByProjectId([FromRoute] int id)
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

        // POST api/v1/odata/VstsProjectIntegration
        [HttpPost]
        public IActionResult Create([FromBody] VstsProjectIntegrationView vstsProjectIntegrationView)
        {
            if (!ModelState.IsValid)
            {
                return SendInvalidModelResponse();
            }

            try
            {
                var vstsProjectIntegrationViewResult = _service.Create(vstsProjectIntegrationView);

                UpdateVstsInfo(vstsProjectIntegrationViewResult);

                var locationUri = $"{Request.Host}/{BaseODataRouteComponent}/VstsProjectIntegrationView({vstsProjectIntegrationViewResult.Id})";

                return Created(locationUri, vstsProjectIntegrationViewResult);
            }
            catch (Exception e)
            {
                return SendErrorODataResponse(e);
            }
        }

        // PUT api/v1/odata/VstsProjectIntegration(1)
        [HttpPut(IdRoute)]
        public IActionResult Update([FromODataUri] int id, [FromBody]VstsProjectIntegrationView vstsProjectIntegrationView)
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

                return Ok(vstsProjectIntegrationViewResult);
            }
            catch (Exception e)
            {
                return SendErrorODataResponse(e);
            }
        }

        // DELETE api/v1/odata/VstsProjectIntegration(1)
        [HttpDelete(IdRoute)]
        public IActionResult Delete([FromODataUri] int id)
        {
            try
            {
                var result = _service.Delete(id);
                return NoContent();
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

        private IActionResult SendErrorODataResponse(Exception exception)
        {
            _logger.LogError(exception, exception.Message);
            return StatusCode(500, new { error = exception.Message });
        }

        private IActionResult SendInvalidModelResponse()
        {
            return BadRequest(ModelState);
        }
    }
}
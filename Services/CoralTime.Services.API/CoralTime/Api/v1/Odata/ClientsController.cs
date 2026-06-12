using CoralTime.BL.Interfaces;
using CoralTime.ViewModels.Clients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using static CoralTime.Common.Constants.Constants;
using static CoralTime.Common.Constants.Constants.Routes;
using ODataRoutes = CoralTime.Common.Constants.Constants.Routes.OData;

namespace CoralTime.Services.API.Api.v1.Odata
{
    [Route(BaseControllerRoute)]
    [Authorize]
    public class ClientsController : BaseODataController<ClientsController, IClientService>
    {
        public ClientsController(IClientService service, ILogger<ClientsController> logger)
            : base(logger, service) { }

        // GET: api/v1/odata/Clients/GetAllClients()
        // GET: api/v1/Clients
        [HttpGet]
        [HttpGet(ODataRoutes.GetAllClients)]
        public IActionResult GetAllClients()
        {
            try
            {
                return Ok(_service.GetAllClients());
            }
            catch (Exception e)
            {
                return SendErrorODataResponse(e);
            }
        }

        // POST: api/v1/Clients
        [HttpPost]
        [Authorize(Policy = PolicyAddClient)]
        public IActionResult Create([FromBody] ClientView clientData)
        {
            if (!ModelState.IsValid)
            {
                return SendInvalidModelResponse();
            }

            try
            {
                var result = _service.Create(clientData);

                var locationUri = $"{Request.Host}/{ODataRoutes.BaseODataApiRoute}/Clients({result.Id})";
                return Created(locationUri, result);
            }
            catch (Exception e)
            {
                return SendErrorODataResponse(e);
            }
        }

        // GET api/v1/Clients/2
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

        // PUT: api/v1/Clients/2
        [HttpPut(IdRoute)]
        [Authorize(Policy = PolicyEditClient)]
        public IActionResult Update(int id, [FromBody] JsonElement clientData)
        {
            if (!ModelState.IsValid)
            {
                return SendInvalidModelResponse();
            }

            try
            {
                var result = _service.Update(id, clientData);
                return new ObjectResult(result);
            }
            catch (Exception e)
            {
                return SendErrorODataResponse(e);
            }
        }

        // PATCH: api/v1/Clients/2
        [HttpPatch(IdRoute)]
        [Authorize(Policy = PolicyEditClient)]
        public IActionResult Patch(int id, [FromBody] JsonElement clientData)
        {
            if (!ModelState.IsValid)
            {
                return SendInvalidModelResponse();
            }

            try
            {
                var result = _service.Update(id, clientData);
                return new ObjectResult(result);
            }
            catch (Exception e)
            {
                return SendErrorODataResponse(e);
            }
        }

        //DELETE :api/v1/Clients/1
        [HttpDelete(IdRoute)]
        [Authorize(Policy = PolicyEditClient)]
        public IActionResult Delete(int id)
        {
            return BadRequest($"Can't delete the client with Id - {id}");
        }
    }
}
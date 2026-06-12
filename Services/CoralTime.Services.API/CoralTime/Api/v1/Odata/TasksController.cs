using CoralTime.BL.Interfaces;
using CoralTime.ViewModels.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using static CoralTime.Common.Constants.Constants;
using static CoralTime.Common.Constants.Constants.Routes;
using ODataRouts = CoralTime.Common.Constants.Constants.Routes.OData;

namespace CoralTime.Services.API.Api.v1.Odata
{
    [Route(BaseControllerRoute)]
    [Authorize]
    public class TasksController : BaseODataController<TasksController, ITasksService>
    {
        public TasksController(ITasksService service, ILogger<TasksController> logger)
            : base(logger, service) { }


        // GET: api/v1/odata/Tasks/GetAllTasks()
        // GET: api/v1/Tasks
        // TODO: returns not Iquerable collection -> response without Count
        [HttpGet]
        [HttpGet(ODataRouts.GetAllTasks)]
        public IActionResult GetAllTasks()
        {
            return Ok(_service.Get());
        }

        // GET api/v1/Tasks/2
        [HttpGet(IdRoute)]
        public IActionResult GetById(int id)
        {
            try
            {
                var taskTypeViewResult = _service.GetById(id);
                return new ObjectResult(taskTypeViewResult);
            }
            catch (Exception e)
            {
                return SendErrorODataResponse(e);
            }
        }

        // POST api/v1/Tasks
        [HttpPost]
        public IActionResult Create([FromBody]TaskTypeView taskTypeView)
        {
            if (!ModelState.IsValid)
            {
                return SendInvalidModelResponse();
            }

            try
            {
                var taskTypeViewResult = _service.Create(taskTypeView);
                var locationUri = $"{Request.Host}/{ODataRouts.BaseODataApiRoute}/Tasks({taskTypeViewResult.Id})";

                return Created(locationUri, taskTypeViewResult);
            }
            catch (Exception e)
            {
                return SendErrorODataResponse(e);
            }
        }

        // PUT api/v1/Tasks/1
        [HttpPut(IdRoute)]
        public IActionResult Update(int id, [FromBody]TaskTypeView taskTypeView)
        {
            if (!ModelState.IsValid)
            {
                return SendInvalidModelResponse();
            }

            taskTypeView.Id = id;

            try
            {
                var taskTypeViewResult = _service.Update(taskTypeView);
                return new ObjectResult(taskTypeViewResult);
            }
            catch (Exception e)
            {
                return SendErrorODataResponse(e);
            }
        }

        // PATCH api/v1/Tasks/1
        [HttpPatch(IdRoute)]
        public IActionResult Patch(int id, [FromBody]TaskTypeView taskTypeView)
        {
            if (!ModelState.IsValid)
            {
                return SendInvalidModelResponse();
            }

            taskTypeView.Id = id;

            try
            {
                var taskTypeViewResult = _service.Update(taskTypeView);
                return new ObjectResult(taskTypeViewResult);
            }
            catch (Exception e)
            {
                return SendErrorODataResponse(e);
            }
        }

        //DELETE :api/v1/Tasks/1
        [Authorize(Policy = PolicyEditTask)]
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
    }
}
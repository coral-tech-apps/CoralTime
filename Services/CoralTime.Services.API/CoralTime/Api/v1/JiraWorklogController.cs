using Microsoft.AspNetCore.Mvc;
using CoralTime.BL.Interfaces;
using Microsoft.Extensions.Logging;
using static CoralTime.Common.Constants.Constants.Routes;
using CoralTime.DAL.Models.Jira;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using CoralTime.ViewModels.Jira;
using System;

namespace CoralTime.Services.API.Api.v1
{
    [Authorize]
    [Route(BaseControllerRoute)]
    public class JiraWorklogController :  BaseController<JiraWorklogController, IJiraWorklogSerivce>
    {
        public JiraWorklogController(IJiraWorklogSerivce service, ILogger<JiraWorklogController> logger)
            : base(logger, service) { }

        [HttpPost(GetWorklogs)]
        public async Task<IActionResult> GetWorklogAsync([FromBody] JiraWorklogFilterView filter)
        {
            
            return Ok(await _service.GetWorklogAsync(filter));
        }

        [HttpPost(LoadTimeEntryWorklogs)]
        public IActionResult LoadWorklog([FromBody] JiraWorklogView[] worklogs)
        {
            try
            {
                _service.LoadWorklog(worklogs);
                return Ok();
            }
            catch(Exception e)
            {
                return BadRequest(e);
            }
        }
    }
}

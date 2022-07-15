using HCA.Core.Services;
using HCA.Models;
using HCA.Models.MuleSoft;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HCA.Api.Controllers
{
    [Route("api/[controller]")]
    public class IdentitiesController : Controller
    {
        private readonly IClientIdentityService _clientIdentityService;

        public IdentitiesController(IClientIdentityService clientIdentityService)
        {
            _clientIdentityService = clientIdentityService;
        }

        [HttpPost]
        public async Task<PagenatedCollection<ClientIdentity>> Identities([FromBody] IdentityFilter filter, [FromQuery] int pagNumber = 0, [FromQuery] int recordsPerPage = 20)
        {
            var identitityModels = await _clientIdentityService.GetAll(pagNumber, recordsPerPage);
            return identitityModels;
        }

        [HttpPut("link")]
        public async Task<IActionResult> Link([FromBody] LinkingSources value)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _clientIdentityService.LinkIdentities(value);

            if (result == null)
            {
                return BadRequest("Invalid Input");
            }

            return Ok(result);
        }

        [HttpPut("unlink")]
        public async Task<IActionResult> UnLink([FromBody] UnLinkingSources value)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _clientIdentityService.UnLinkIdentities(value);

            if (result == null)
            {
                return BadRequest("Invalid Input");
            }

            return Ok(result);
        }

        [HttpPut("merge")]
        public async Task<IActionResult> Merge([FromBody] MergingSources value)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _clientIdentityService.MergeIdentites(value);

            if (result == null)
            {
                return BadRequest("Invalid Input");
            }

            return Ok(result);
        }

        [HttpPut("unmerge")]
        public async Task<IActionResult> UnMerge([FromBody] UnMergingSources value)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _clientIdentityService.UnMergeIdentities(value);

            if (result == null)
            {
                return BadRequest("Invalid Input");
            }

            return Ok(result);
        }

        //[HttpDelete("delete")]
        //public async Task<IActionResult> Delete([FromBody] Source value)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    var result = await _clientIdentityService.DeleteIdentity(value);

        //    if (result == null)
        //    {
        //        return BadRequest("Invalid Input");
        //    }

        //    return Ok(result);
        //}
    }
}


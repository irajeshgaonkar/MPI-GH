using HCA.Core.Services;
using HCA.Infrastructure.Exceptions;
using HCA.Infrastructure.Logger;
using HCA.Models;
using HCA.Models.MuleSoft;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HCA.Api.Controllers
{
    [Route("api/[controller]")]
    public class IdentitiesController : Controller
    {
        private readonly IAppLogger _logger;

        private readonly IClientIdentityService _clientIdentityService;

        public IdentitiesController(IClientIdentityService clientIdentityService, IAppLogger logger)
        {
            _clientIdentityService = clientIdentityService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<PagenatedCollection<ClientIdentity>> Identities([FromBody] IdentityFilter filter, [FromQuery] int pagNumber = 0, [FromQuery] int recordsPerPage = 20)
        {
            if (!filter.HasFilter())
            {
                var searchResult = await _clientIdentityService.GetAll(pagNumber, recordsPerPage);
                return searchResult;
            }

            var identitityModels = await _clientIdentityService.Search(pagNumber, recordsPerPage, filter);
            return identitityModels;
        }

        [HttpPut("link")]
        public async Task<IActionResult> Link([FromBody] LinkingSources value)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var result = await _clientIdentityService.LinkIdentities(value);
                if (null == result) return BadRequest("Invalid Input");
                return Ok(result);
            }
            catch (HcaBadRequestException e)
            {
                return BadRequest(e.Message);
            }
            catch (HcaMuleSoftException e)
            {
                return BadRequest(e.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                return StatusCode(500);
            }
        }

        [HttpPut("unlink")]
        public async Task<IActionResult> UnLink([FromBody] UnLinkingSources value)
        {
            try
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
            catch (HcaBadRequestException e)
            {
                return BadRequest(e.Message);
            }
            catch (HcaMuleSoftException e)
            {
                return BadRequest(e.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                return StatusCode(500);
            }
        }

        [HttpPut("merge")]
        public async Task<IActionResult> Merge([FromBody] MergingSources value)
        {
            try
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
            catch (HcaBadRequestException e)
            {
                return BadRequest(e.Message);
            }
            catch (HcaMuleSoftException e)
            {
                return BadRequest(e.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                return StatusCode(500);
            }
        }

        [HttpPut("unmerge")]
        public async Task<IActionResult> UnMerge([FromBody] UnMergingSources value)
        {
            try
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
            catch (HcaBadRequestException e)
            {
                return BadRequest(e.Message);
            }
            catch (HcaMuleSoftException e)
            {
                return BadRequest(e.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                return StatusCode(500);
            }
        }
    }
}


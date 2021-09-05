using System.Collections.Generic;
using System.Threading.Tasks;
using LogProxy.Messages;
using LogProxy.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LogProxy.Controllers
{
    public class BasicAuthorizationAttribute : AuthorizeAttribute
    {
        public BasicAuthorizationAttribute()
        {
            Policy = "BasicAuthentication";
        }
    }
    
    [BasicAuthorizationAttribute]
    [ApiController]
    [Route("[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly ILogProxyService _logProxyService;

        public MessagesController(ILogProxyService logProxyService)
        {
            _logProxyService = logProxyService;
        }
        
        [HttpGet]
        public async Task<IEnumerable<EnrichedTitleAndText>> Get()
        {
            return await _logProxyService.GetFromThirdPartyAsync();
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] params TitleAndText[] titleAndTextCollection)
        {
            await _logProxyService.TransferToThirdPartyAsync(titleAndTextCollection);
            return Ok();
        }
    }
}
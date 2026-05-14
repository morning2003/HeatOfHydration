using Hncdi.HeatOfHydration.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace VolPro.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AiAdviceController : Controller
    {
        private readonly AiAdviceService _service;

        public AiAdviceController(AiAdviceService service)
        {
            _service = service;
        }

        [HttpGet("GetAdvice")]
        public async Task<IActionResult> GetAdvice(long code)
        {
            string result = await _service.GetAiAdviceAsync(code);
            return Ok(new { data = result });
        }

        [HttpPost("Chat")]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            if (request == null || request.messages == null)
                return BadRequest(new { data = "请求参数无效" });

            string result = await _service.ChatAsync(request.code, request.messages ?? new System.Collections.Generic.List<ChatMessage>());
            return Ok(new { data = result });
        }
    }
}

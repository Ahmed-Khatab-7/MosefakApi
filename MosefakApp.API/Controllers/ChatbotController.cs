namespace MosefakApp.API.Controllers;
[ApiController]
[Route("api/[controller]")]
public class ChatbotController : ControllerBase
{
    private readonly IAiIntegrationService _aiService;

    public ChatbotController(IAiIntegrationService aiService)
    {
        _aiService = aiService;
    }

    [HttpPost("ask")]
    [RequiredPermission(Permissions.Chatbot.Ask)]
    public async Task<IActionResult> Ask([FromBody] ChatRequestDto dto)
    {
        // FluentValidation will validate ChatRequestDto
        try
        {
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            // Pass the token to the AI service
            var aiStringReply = await _aiService.AskAiAsync(dto.Question, token);

            var result = new ChatResponseDto
            {
                Success = true,
                Reply = aiStringReply,
                Error = ""
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ChatResponseDto
            {
                Success = false,
                Reply = "",
                Error = ex.Message
            });
        }
    }
}
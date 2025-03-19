using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MosefakApp.Core.Dtos.ChatBot.Requests;
using MosefakApp.Core.Dtos.ChatBot.Responses;

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
    [HasPermission(Permissions.Chatbot.Ask)]
    public async Task<IActionResult> Ask([FromBody] ChatRequestDto dto)
    {
        // FluentValidation will validate ChatRequestDto
        try
        {
            var aiStringReply = await _aiService.AskAiAsync(dto.Question);

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
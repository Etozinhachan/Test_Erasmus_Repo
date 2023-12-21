using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using testingStuff.data;
using testingStuff.models;
using OpenAI_API;
using OpenAI_API.Completions;
using OpenAI_API.Chat;

namespace testingStuff.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OpenAIController : ControllerBase
    {
        
        [HttpPost]
        [Route("getanswer")]
        public async Task<IActionResult> GetResult([FromBody] string prompt)
        {
            // Instances of the APIAuthentication class can be created using your API key.
            var authentication = new APIAuthentication("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");
            // APIAuthentication object used to create an instance of the OpenAIAPI class
            var api = new OpenAIAPI(authentication);

            var result = await api.Chat.CreateChatCompletionAsync(new ChatRequest()
            {
                Model = OpenAI_API.Models.Model.ChatGPTTurbo,
                Temperature = 0.1,
                
                
                Messages = new ChatMessage[] {
                    new ChatMessage(ChatMessageRole.User, prompt)
                }
            });

            var textResult = result.Choices[0].Message.TextContent;

            return Ok(textResult);

            
        }


    }
}
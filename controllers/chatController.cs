using Microsoft.AspNetCore.Mvc;
using testingStuff.data;
using testingStuff.models;

namespace testingStuff.Controllers
{
    [Route("api/[controller]/conversation")]
    [ApiController]
    public class chatController : ControllerBase
    {
        #region constructor thingies
        private readonly DbDataContext _context;
        private readonly int characterLimitPerResponse = 25;

        public chatController(DbDataContext context)
        {
            _context = context;
        }
        #endregion

        #region ApiResponsePool
        private string getApiFullResponse(string prompt){
            prompt = prompt.ToLower();
            string[] acceptableUserPrompts = {"hello", "bye", "good morning"};
            string[] randomResponses = {"The sun was setting behind the mountains, casting a warm glow over the valley below.", "The sound of the waves crashing against the shore was soothing to her ears.", "The old man sat on the bench, watching the children play in the park.", "The smell of freshly baked bread wafted through the air, making her mouth water.", "The city was alive with the sound of honking cars and bustling pedestrians.", "The stars twinkled in the sky, as if they were winking at her.", "The wind howled through the trees, making them sway back and forth.", "The rain pattered against the window, creating a soothing rhythm.", "The fire crackled in the fireplace, warming the room and casting a soft glow.", "The snowflakes fell gently from the sky, blanketing the ground in white."};
            string[] promptResponses = {"Hello, how can i help you?", "Goodbye, hope you come back soon.", "Good morning, how can i help you?"};
            Random random = new Random();
            string returnValue = "";
            returnValue = acceptableUserPrompts.Contains(prompt) ? promptResponses[Array.IndexOf(acceptableUserPrompts, prompt)] : randomResponses[random.Next(randomResponses.Length)];

            return returnValue;
        }

        /* private string getApiPartialResponse(string prompt){
            prompt = prompt.ToLower();
            string fullResponse = getApiFullResponse(prompt);
            
        } */
        #endregion

        #region startChat
        [HttpPost]
        public async Task<ActionResult<ChatResponse>> createChat(UserPrompt userPrompt){
            if (userPrompt == null || userPrompt.prompt.Length == 0){
                return BadRequest(new ChatBadResponse{
                    title = "Bad Request",
                    status = 400,
                    detail = "The request is invalid."
                });
            }

            if (userPrompt.prompt == "test_error"){
                return StatusCode(503, new ChatBadResponse{
                    title = "Service unavailable",
                    status = 503,
                    detail = "The service is currently unavailable."
                });
            }

            var newChat = new Chat{
                id = Guid.NewGuid(),
                user_id = userPrompt.id,
            };

            var chatResponse = new ChatSucessfullResponse{
                conversation_id = Guid.NewGuid(),
                response = getApiFullResponse(prompt: userPrompt.prompt),
                is_final = false 
            };

            userPrompt.conversation_id = chatResponse.conversation_id;
/* 
            newChat.chatPrompts.Add(chatResponse);
            newChat.userPrompts.Add(userPrompt);
 */
            await _context.Chats.AddAsync(newChat);
            await _context.AiResponses.AddAsync(chatResponse);
            await _context.userPrompts.AddAsync(userPrompt);
            return Ok(newChat);

        }


        #endregion

    }
}
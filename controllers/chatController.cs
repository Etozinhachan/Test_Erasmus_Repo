using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly IMapper _mapper;
        private readonly int characterLimitPerResponse = 25;

        public chatController(DbDataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
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
        public async Task<ActionResult<Chat>> createChat(UserPrompt userPrompt){
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
                user_id = userPrompt.user_id,
            };

            var chatResponse = new ChatSucessfullResponse{
                id = Guid.NewGuid(),
                conversation_id = newChat.id,
                response = getApiFullResponse(prompt: userPrompt.prompt),
                is_final = true 
            };

            userPrompt.conversation_id = chatResponse.conversation_id;
            userPrompt.id = Guid.NewGuid();
/* 
            newChat.chatPrompts.Add(chatResponse);
            newChat.userPrompts.Add(userPrompt);
 */
            await _context.Chats.AddAsync(newChat);
            await _context.AiResponses.AddAsync(chatResponse);
            await _context.userPrompts.AddAsync(userPrompt);

            await _context.SaveChangesAsync();

            return Ok(newChat);

        }


        #endregion

        #region continueChatPut
        [HttpPut]
        [Route("{conversation_id:guid}")]
        public async Task<ActionResult<Chat>> continueChatPut([FromRoute] Guid conversation_id, [FromBody] UserPrompt userPrompt){
            
            if (!chatExists(conversation_id)){
                NotFound();
            }

            if (userPrompt.prompt.Length == 0){
                return BadRequest(new ChatBadResponse{
                    title = "Bad Request",
                    status = 400,
                    detail = "The request is invalid."
                });
            }

            if (userPrompt.prompt == "test_error"){
                return BadRequest(new ChatBadResponse{
                    title = "Service Unavailable",
                    status = 503,
                    detail = "The service is currently unavailable."
                });
            }

            var chat = _context.Chats.Where(c => c.id == conversation_id).Include(up => up.userPrompts).Include(cps => cps.chatPrompts).FirstOrDefault();

            // verificar se a ultima ai response eh final ou nao : )

            var chatResponse = new ChatSucessfullResponse{
                conversation_id = conversation_id,
                id = Guid.NewGuid(),
                is_final = false,
                response = getApiFullResponse(userPrompt.prompt)
            };

            userPrompt.conversation_id = conversation_id;
            userPrompt.id = Guid.NewGuid();
            
            await _context.userPrompts.AddAsync(userPrompt);
            await _context.AiResponses.AddAsync(chatResponse);

            return Ok();
        }
        #endregion

        #region getAllChatsFromAUser
        [HttpGet]
        [Route("Chats/{user_id:guid}")] 
        public async Task<ActionResult<IEnumerable<Chat>>> getChats(Guid user_id){
            var searchChats = await _context.Chats.Where(u => u.user.id == user_id).Include(up => up.userPrompts).Include(cps => cps.chatPrompts).OrderBy(c => c.id).ToListAsync();

            if (!ModelState.IsValid){
                return BadRequest(searchChats);
            }

            return searchChats;
        }
        #endregion

        #region getAllChatResponses
        [HttpGet]
        [Route("ChatResponses")]
        public async Task<ActionResult<IEnumerable<ChatSucessfullResponse>>> getChatResponses(){
            var searchChatResponses = await _context.AiResponses.OrderBy(cr => cr.id).ToListAsync();

            if (!ModelState.IsValid){
                return BadRequest(searchChatResponses);
            }

            return searchChatResponses;
        }
        #endregion

        #region getAllUserPrompts
        [HttpGet]
        [Route("UserPrompts")] 
        public async Task<ActionResult<IEnumerable<UserPrompt>>> getUserPrompts(){
            var searchUserPrompts = await _context.userPrompts.OrderBy(up => up.id).ToListAsync();

            if (!ModelState.IsValid){
                return BadRequest(searchUserPrompts);
            }

            return searchUserPrompts;
        }
        #endregion

        #region getChatById
        [HttpGet]
        [Route("{id:guid}")]
        public async Task<ActionResult<Chat>> getChatById([FromRoute] Guid id){
            var searchChat = _context.Chats.Where(c => c.id == id).Include(up => up.userPrompts).Include(cps => cps.chatPrompts).FirstOrDefault();

            if (searchChat == null){
                return NotFound();
            }
            
            return Ok(searchChat);
        }
        #endregion
    
        #region helper methods
        public bool chatExists(Guid id){
            return _context.Chats.Any(c => c.id == id);
        }
        #endregion
    }
}
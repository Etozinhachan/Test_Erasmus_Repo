using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using testingStuff.data;
using testingStuff.Interfaces;
using testingStuff.models;

namespace testingStuff.Controllers
{
    [Route("api/[controller]/conversation")]
    [ApiController]
    public class chatController : ControllerBase
    {
        #region constructor thingies
        private readonly IChatRepository _chatRepository;
        private readonly IMapper _mapper;
        private readonly int characterLimitPerResponse = 25;

        public chatController(IChatRepository chatRepository, IMapper mapper)
        {
            _chatRepository = chatRepository;
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
        public async Task<ActionResult<Chat>> createChat(UserPromptDTO userPromptDTO){
            if (userPromptDTO == null || userPromptDTO.prompt.Length == 0){
                return BadRequest(new ChatBadResponse{
                    title = "Bad Request",
                    status = 400,
                    detail = "The request is invalid."
                });
            }

            if (userPromptDTO.prompt == "test_error"){
                return StatusCode(503, new ChatBadResponse{
                    title = "Service unavailable",
                    status = 503,
                    detail = "The service is currently unavailable."
                });
            }

            var newChat = new Chat{
                id = Guid.NewGuid(),
                user_id = userPromptDTO.user_id,
            };

            var chatResponse = new ChatSucessfullResponse{
                id = Guid.NewGuid(),
                conversation_id = newChat.id,
                response = getApiFullResponse(prompt: userPromptDTO.prompt),
                is_final = true 
            };

            var userPrompt = new UserPrompt{
                id = Guid.NewGuid(),
                conversation_id = chatResponse.conversation_id,
                user_id = userPromptDTO.user_id,
                prompt = userPromptDTO.prompt
            };

/* 
            newChat.chatPrompts.Add(chatResponse);
            newChat.userPrompts.Add(userPrompt);
 */
            _chatRepository.AddChat(newChat);
            _chatRepository.AddAiResponse(chatResponse);
            _chatRepository.AddUserPrompt(userPrompt);

            return Ok(newChat);

        }


        #endregion

        #region continueChatPut
        [HttpPut]
        [Route("{conversation_id:guid}")]
        public async Task<ActionResult<Chat>> continueChatPut([FromRoute] Guid conversation_id, [FromBody] UserPromptDTODTO userPromptDTODTO){
            
            if (!_chatRepository.chatExists(conversation_id)){
                NotFound();
            }

            var chat = _chatRepository.getChatByConvoId(conversation_id);

            if (userPromptDTODTO.prompt.Length == 0 || !_chatRepository.isFinal(_chatRepository.getLastAiResponse(chat))){
                return BadRequest(new ChatBadResponse{
                    title = "Bad Request",
                    status = 400,
                    detail = "The request is invalid."
                });
            }

            if (userPromptDTODTO.prompt == "test_error"){
                return BadRequest(new ChatBadResponse{
                    title = "Service Unavailable",
                    status = 503,
                    detail = "The service is currently unavailable."
                });
            }

            var chatResponse = new ChatSucessfullResponse{
                conversation_id = conversation_id,
                id = Guid.NewGuid(),
                is_final = false,
                response = getApiFullResponse(userPromptDTODTO.prompt)
            };

            var userPrompt = new UserPrompt{
                id = Guid.NewGuid(),
                conversation_id = conversation_id,
                user_id = _chatRepository.getChatByConvoId(conversation_id).user_id,
                prompt = userPromptDTODTO.prompt
            };
            
            _chatRepository.AddUserPrompt(userPrompt);
            _chatRepository.AddAiResponse(chatResponse);

            return Ok(_chatRepository.getChatByConvoId(conversation_id));
        }
        #endregion

        #region getAllChatsFromAUser
        [HttpGet]
        [Route("Chats/{user_id:guid}")] 
        public async Task<ActionResult<IEnumerable<Chat>>> getChats(Guid user_id){
            var searchChats = _chatRepository.getAllUserChats(user_id);

            if (!ModelState.IsValid){
                return BadRequest(searchChats);
            }

            return Ok(searchChats);
        }
        #endregion

        #region getAllChatResponses
        [HttpGet]
        [Route("ChatResponses")]
        public async Task<ActionResult<IEnumerable<ChatSucessfullResponse>>> getChatResponses(){
            var searchChatResponses = _chatRepository.getAllAiResponses;

            if (!ModelState.IsValid){
                return BadRequest(searchChatResponses);
            }

            return Ok(searchChatResponses);
        }
        #endregion

        #region getAllUserPrompts
        [HttpGet]
        [Route("UserPrompts")] 
        public async Task<ActionResult<IEnumerable<UserPrompt>>> getUserPrompts(){
            var searchUserPrompts = _chatRepository.getAllUserPrompts();

            if (!ModelState.IsValid){
                return BadRequest(searchUserPrompts);
            }

            return Ok(searchUserPrompts);
        }
        #endregion

        #region getChatById
        [HttpGet]
        [Route("{id:guid}")]
        public async Task<ActionResult<Chat>> getChatById([FromRoute] Guid id){
            var searchChat = _chatRepository.getChatByConvoId(id);

            if (searchChat == null){
                return NotFound();
            }
            
            return Ok(searchChat);
        }
        #endregion
    

    }
}
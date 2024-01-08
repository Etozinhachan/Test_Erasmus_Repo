using System.IdentityModel.Tokens.Jwt;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using testingStuff.data;
using testingStuff.helper;
using testingStuff.Identity;
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
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _config;
        private readonly IMapper? _mapper;
        private readonly int characterLimitPerResponse = 25;


        public chatController(IChatRepository chatRepository, IUserRepository userRepository, IConfiguration configuration/*, IMapper mapper*/)
        {
            _chatRepository = chatRepository;
            _userRepository = userRepository;
            _config = configuration;
            /*_mapper = mapper;*/
        }
        #endregion

        #region ApiResponsePool
        private (string response, bool isFinal) getApiFullResponse(string prompt, ChatSucessfullResponse? aiResponse)
        {
            if (aiResponse == null){
                aiResponse = new ChatSucessfullResponse{
                    response = ""
                };
            }
            prompt = prompt.ToLower();
            string returnValue = "";
            string[] acceptableUserPrompts = { "hello", "bye", "good morning" };
            string[] randomResponses = { "The sun was setting behind the mountains, casting a warm glow over the valley below.", "The sound of the waves crashing against the shore was soothing to her ears.", "The old man sat on the bench, watching the children play in the park.", "The smell of freshly baked bread wafted through the air, making her mouth water.", "The city was alive with the sound of honking cars and bustling pedestrians.", "The stars twinkled in the sky, as if they were winking at her.", "The wind howled through the trees, making them sway back and forth.", "The rain pattered against the window, creating a soothing rhythm.", "The fire crackled in the fireplace, warming the room and casting a soft glow.", "The snowflakes fell gently from the sky, blanketing the ground in white." };
            string[] promptResponses = { "Hello, how can i help you?", "Goodbye, hope you come back soon.", "Good morning, how can i help you?" };
            bool is_final = false;
            if (aiResponse.response.Length == 0)
            {
                Random random = new Random();
                returnValue = acceptableUserPrompts.Contains(prompt) ? promptResponses[Array.IndexOf(acceptableUserPrompts, prompt)] : randomResponses[random.Next(randomResponses.Length)];
                var returnLength = returnValue.Length;
                returnValue = returnValue.Substring(0, characterLimitPerResponse);
                
                is_final = returnLength <= characterLimitPerResponse;
                return (returnValue, is_final);
            }
            string currentResponse = aiResponse.response;
            string responsePattern = HelperMethods.arrayContains(promptResponses, currentResponse) ? promptResponses[HelperMethods.getResponsePoolIndex(promptResponses, currentResponse)] : randomResponses[HelperMethods.getResponsePoolIndex(randomResponses, currentResponse)];
            string nextResponse = "";
            if (responsePattern.Length - currentResponse.Length > characterLimitPerResponse)
            {
                nextResponse = currentResponse + responsePattern.Substring(currentResponse.Length, characterLimitPerResponse);
            }
            else
            {
                is_final = true;
                nextResponse = currentResponse + responsePattern.Substring(currentResponse.Length, responsePattern.Length - currentResponse.Length);
            }
            return (nextResponse,is_final);

        }

        private (string response, bool is_final) generateAiResponse(Guid chat_id)
        {
            var userPrompt = _chatRepository.getLastUserPrompt(_chatRepository.getChatByConvoId(chat_id));
            ChatSucessfullResponse? aiResponse = _chatRepository.getLastAiResponse(_chatRepository.getChatByConvoId(chat_id));


            string prompt = userPrompt.prompt.ToLower();
            return getApiFullResponse(prompt, aiResponse);
        }

        /* private string getApiPartialResponse(string prompt){
            prompt = prompt.ToLower();
            string fullResponse = getApiFullResponse(prompt);
            
        } */
        #endregion

        #region startChat
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Chat>> createChat(UserPromptDTO userPromptDTO)
        {
            /*
            var myCustomJwtStringB = Request.Headers.Where(h => h.Key == "Authorization").FirstOrDefault().Value.ToString();
            var myCustomJwtString = myCustomJwtStringB?.Substring("Bearer ".Length);
            var myCustomJwt = HelperMethods.ConvertJwtStringToJwtSecurityToken(myCustomJwtString);
            var myCustomJwtData = HelperMethods.DecodeJwt(myCustomJwt);
            */


            var jwtToken = HelperMethods.decodeToken(/*token, SecretKey*/_config, HttpContext);

            var userId = Guid.Parse(jwtToken.Claims.First(x => x.Type == "UserID").Value);

            if (userPromptDTO == null || userPromptDTO.prompt.Length == 0 || !_userRepository.UserExists(userId))
            {
                return BadRequest(new ChatBadResponse
                {
                    title = "Bad Request",
                    status = 400,
                    detail = "The request is invalid."
                });
            }

            if (userPromptDTO.prompt == "test_error")
            {
                return StatusCode(503, new ChatBadResponse
                {
                    title = "Service unavailable",
                    status = 503,
                    detail = "The service is currently unavailable."
                });
            }

            var newChat = new Chat
            {
                id = Guid.NewGuid(),
                user_id = userId,
            };
            
            var userPrompt = new UserPrompt
            {
                id = Guid.NewGuid(),
                conversation_id = newChat.id,
                prompt = userPromptDTO.prompt
            };

            _chatRepository.AddChat(newChat);
            _chatRepository.AddUserPrompt(userPrompt);

/*
            var (response, is_final) = generateAiResponse(newChat.id);

            var chatResponse = new ChatSucessfullResponse
            {
                id = Guid.NewGuid(),
                conversation_id = newChat.id,
                response = response,
                is_final = is_final
            };
*/
            

            /* 
                        newChat.chatPrompts.Add(chatResponse);
                        newChat.userPrompts.Add(userPrompt);
             */
 //           _chatRepository.AddAiResponse(chatResponse);

            return CreatedAtAction(nameof(getChatById), new { id = newChat.id }, newChat);

        }


        #endregion

        #region continueChatPut
        [HttpPut]
        [Route("{conversation_id:guid}")]
        [ActionName("")]
        public async Task<ActionResult<Chat>> continueChatPut([FromRoute] Guid conversation_id, [FromBody] UserPromptDTO userPromptDTODTO)
        {

            if (!_chatRepository.chatExists(conversation_id))
            {
                NotFound();
            }

            var chat = _chatRepository.getChatByConvoId(conversation_id)!;

            var lastAiResponse = _chatRepository.getLastAiResponse(chat)!;

            if (userPromptDTODTO.prompt.Length == 0 || !_chatRepository.isFinal(lastAiResponse))
            {
                return BadRequest(new ChatBadResponse
                {
                    title = "Bad Request",
                    status = 400,
                    detail = "The request is invalid."
                });
            }

            if (userPromptDTODTO.prompt == "test_error")
            {
                return BadRequest(new ChatBadResponse
                {
                    title = "Service Unavailable",
                    status = 503,
                    detail = "The service is currently unavailable."
                });
            }

            

            var chatResponse = new ChatSucessfullResponse
            {
                conversation_id = conversation_id,
                id = Guid.NewGuid(),
                is_final = false,
                response = "",
                response_number = lastAiResponse.response_number + 1
            };

            _chatRepository.AddAiResponse(chatResponse);

            var userPrompt = new UserPrompt
            {
                id = Guid.NewGuid(),
                conversation_id = conversation_id,
                prompt = userPromptDTODTO.prompt,
                prompt_number = _chatRepository.getLastUserPrompt(chat).prompt_number + 1
            };
            _chatRepository.AddUserPrompt(userPrompt);

            var (response, is_final) = generateAiResponse(chatResponse.conversation_id);
            chatResponse.response = response;
            chatResponse.is_final = is_final;
            
            _chatRepository.aiResponseModified(chatResponse);

            return Ok(_chatRepository.getChatByConvoId(conversation_id));
        }
        #endregion

        #region ContinueChatGetMethod

        [HttpGet]
        [Route("{conversation_id:guid}")]
        public async Task<ActionResult<Chat>> getChatContinuation([FromRoute] Guid conversation_id)
        {
            if (!_chatRepository.chatExists(conversation_id))
            {
                return NotFound();
            }

            if (_chatRepository.getLastUserPrompt(_chatRepository.getChatByConvoId(conversation_id)) == null){

                _chatRepository.deleteChat(_chatRepository.getChatByConvoId(conversation_id));
                return NotFound();
            }

            var chat = _chatRepository.getChatByConvoId(conversation_id);
            ChatSucessfullResponse? lastAiResponse = _chatRepository.getLastAiResponse(chat);

            if (lastAiResponse == null){
                lastAiResponse = new ChatSucessfullResponse{
                    response = "",
                    conversation_id = conversation_id,
                    id = Guid.NewGuid(),
                    is_final = false
                };
                _chatRepository.AddAiResponse(lastAiResponse);
            }

            if (_chatRepository.isFinal(lastAiResponse) || lastAiResponse == null)
            {
                return BadRequest(new ChatBadResponse
                {
                    title = "Bad Request",
                    status = 400,
                    detail = "The request is invalid."
                });
            }

            /*
            if (userPromptDTODTO.prompt == "test_error"){
                return BadRequest(new ChatBadResponse{
                    title = "Service Unavailable",
                    status = 503,
                    detail = "The service is currently unavailable."
                });
            }
            */

            var (response, is_final) = generateAiResponse(conversation_id);
/* 
            var chatResponse = new ChatSucessfullResponse
            {
                conversation_id = conversation_id,
                id = _chatRepository.getLastAiResponse(chat).id,
                is_final = is_final,
                response = response
            }; */

            var oldChatResponse = _chatRepository.getLastAiResponse(chat);

            oldChatResponse.is_final = is_final;
            oldChatResponse.response = response;

            _chatRepository.aiResponseModified(oldChatResponse);

            return Ok(_chatRepository.getChatByConvoId(conversation_id));
        }

        #endregion

        #region DeleteChatMethod
        [Authorize(Policy = IdentityData.AdminUserPolicyName)]
        [HttpDelete]
        [Route("{chat_id:guid}")]
        public async Task<ActionResult> deleteChat([FromRoute] Guid chat_id){

            Chat? chat = _chatRepository.getChatByConvoId(chat_id);

            if (chat == null){
                return NotFound();
            }

            _chatRepository.deleteChat(chat);

            return NoContent();
        }
        #endregion

        #region getAllChatsFromAUser
        [HttpGet]
        [Route("Chats/{user_id:guid}")]
        public async Task<ActionResult<IEnumerable<Chat>>> getChats(Guid user_id)
        {
            var searchChats = _chatRepository.getAllUserChats(user_id);

            if (!ModelState.IsValid)
            {
                return BadRequest(searchChats);
            }

            return Ok(searchChats);
        }
        #endregion

        #region getAllChatResponses
        [HttpGet]
        [Route("ChatResponses")]
        public async Task<ActionResult<IEnumerable<ChatSucessfullResponse>>> getChatResponses()
        {
            var searchChatResponses = _chatRepository.getAllAiResponses;

            if (!ModelState.IsValid)
            {
                return BadRequest(searchChatResponses);
            }

            return Ok(searchChatResponses);
        }
        #endregion

        #region getAllUserPrompts
        [HttpGet]
        [Route("UserPrompts")]
        public async Task<ActionResult<IEnumerable<UserPrompt>>> getUserPrompts()
        {
            var searchUserPrompts = _chatRepository.getAllUserPrompts();

            if (!ModelState.IsValid)
            {
                return BadRequest(searchUserPrompts);
            }

            return Ok(searchUserPrompts);
        }
        #endregion

        #region getChatById
        [HttpGet]
        [Route("chat/{id:guid}")]
        public async Task<ActionResult<Chat>> getChatById([FromRoute] Guid id)
        {
            var searchChat = _chatRepository.getChatByConvoId(id);

            if (searchChat == null)
            {
                return NotFound();
            }

            return Ok(searchChat);
        }
        #endregion

    }
}
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
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
            if (aiResponse == null)
            {
                aiResponse = new ChatSucessfullResponse
                {
                    response = ""
                };
            }
            prompt = prompt.ToLower();
            string returnValue = "";
            string[] acceptableUserPrompts = { "hello", "bye", "good morning" };
            string[] randomResponses = { "The sun was setting behind ", "the mountains, casting a ", "warm glow over the valley.", "below.", "The sound of the waves ", "crashing against the shore ", "was soothing to her ears.", "The old man sat on the ", "bench, watching the ", "children play in the park.", "The smell of freshly baked ", "bread wafted through ", "the air, making her mouth water.", "The city was alive with ", "the sound of honking cars.", "and bustling pedestrians ", "The stars twinkled in the ", "sky, as if they were ", "winking at her.", "The wind howled through ", "the trees, making them ", "sway back and forth.", "The rain pattered against ", "the window, creating a soothing rhythm.", "The fire crackled ", "in the fireplace, warming ", "the room and casting a soft glow.", "The snowflakes fell gently ", "from the sky, blanketing the ground ", "in white." };
            string[] promptResponses = { "Hello, how can i help you?", "Goodbye, hope you come back soon.", "Good morning, how can i help you?" };
            bool is_final = false;
            bool isAcceptable = acceptableUserPrompts.Contains(prompt);
            if (aiResponse.response.Length == 0)
            {
                if (prompt.ToLower().Contains("pifaro")){
                    returnValue = "Oh yeah, that human who's coding skills are so bad that i even have a response just from his name being said";
                    is_final = true;
                    return (returnValue, is_final);
                }
                returnValue = isAcceptable ? promptResponses[Array.IndexOf(acceptableUserPrompts, prompt)] : randomResponses[new Random().Next(randomResponses.Length)];               
                
                var returnLength = returnValue.Length;
                if (isAcceptable)
                {
                    returnValue = returnValue.Substring(0, characterLimitPerResponse);
                    is_final = returnLength <= characterLimitPerResponse;
                }
                else
                {
                    is_final = returnValue.ElementAt(returnLength - 1) == '.';
                }

                return (returnValue, is_final);
            }
            string currentResponse = aiResponse.response;
            isAcceptable = HelperMethods.arrayContains(promptResponses, currentResponse);
            string responsePattern = isAcceptable ? promptResponses[HelperMethods.getResponsePoolIndex(promptResponses, currentResponse)] : randomResponses[new Random().Next(randomResponses.Length)];
            string nextResponse;
            if (isAcceptable)
            {
                if (responsePattern.Length - currentResponse.Length > characterLimitPerResponse)
                {
                    nextResponse = currentResponse + responsePattern.Substring(currentResponse.Length, characterLimitPerResponse);
                }
                else
                {
                    is_final = true;
                    nextResponse = currentResponse + responsePattern.Substring(currentResponse.Length, responsePattern.Length - currentResponse.Length);
                }
            }
            else
            {
                nextResponse = currentResponse + responsePattern;
                is_final = nextResponse.ElementAt(nextResponse.Length - 1) == '.';
            }
            return (nextResponse, is_final);

        }

        private async Task<(string response, bool is_final)> getApiCompilationFullResponseAsync(string prompt, ChatSucessfullResponse? aiResponse)
        {
            if (aiResponse == null)
            {
                aiResponse = new ChatSucessfullResponse
                {
                    response = ""
                };
            }

            var inputStart = prompt.IndexOf('|');

            if (inputStart == -1){
                inputStart = prompt.Length;
            }

            var code = prompt.Substring(1, inputStart);
            var input = prompt.Substring(inputStart);

            var CodeToCompile = new Code{
                code = code,
                input = input
            };

            OutputCode outputCode;

            using (var httpClient = new HttpClient())
            {
                StringContent content = new StringContent(JsonConvert.SerializeObject(CodeToCompile), Encoding.UTF8, "application/json");

                using (var response = await httpClient.PostAsync("https://cscompilerapi.onrender.com/api/test/compilecode", content))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    outputCode = JsonConvert.DeserializeObject<OutputCode>(apiResponse)!;
                }
            }

            string output = outputCode.output == "" ? outputCode.error : outputCode.output;

            return (output, true);
        }

        private async Task<(string response, bool is_final)> generateAiResponseAsync(Guid chat_id)
        {
            var userPrompt = _chatRepository.getLastUserPrompt(_chatRepository.getChatByConvoId(chat_id));
            ChatSucessfullResponse? aiResponse = _chatRepository.getLastAiResponse(_chatRepository.getChatByConvoId(chat_id));


            string prompt = userPrompt.prompt.ToLower();
            return prompt.Substring(0, 1) == "?" ? await getApiCompilationFullResponseAsync(prompt, aiResponse) : getApiFullResponse(prompt, aiResponse);
        }

        /* private string getApiPartialResponse(string prompt){
            prompt = prompt.ToLower();
            string fullResponse = getApiFullResponse(prompt);
            
        } */
        #endregion

        #region startChat
        [Authorize]
        [HttpPost]
        [ProducesResponseType(200, Type = typeof(Chat))]
        [ProducesResponseType(400, Type = typeof(BadRequestResult))]
        [ProducesResponseType(401, Type = typeof(UnauthorizedResult))]
//        [ProducesResponseType(403, Type = typeof(ForbidResult))]
        [ProducesResponseType(500, Type = typeof(ProblemHttpResult))]
        public async Task<IActionResult> createChat(UserPromptDTO userPromptDTO)
        {
            try
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

                newChat.chat_number = _chatRepository.getAllUserChats(userId).Count - 1;
                _chatRepository.chatModified(newChat);

                return CreatedAtAction(nameof(getChatById), new { id = newChat.id }, newChat);
            }
            catch (Exception)
            {
                return Problem();
            }
        }


        #endregion

        #region continueChatPut
        [Authorize]
        [HttpPut]
        [Route("{conversation_id:guid}")]
        [ProducesResponseType(200, Type = typeof(ChatSucessfullResponse))]
        [ProducesResponseType(400, Type = typeof(BadRequestResult))]
        [ProducesResponseType(401, Type = typeof(UnauthorizedResult))]
        [ProducesResponseType(403, Type = typeof(ForbidResult))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        [ProducesResponseType(500, Type = typeof(ProblemHttpResult))]
        public async Task<IActionResult> continueChatPut([FromRoute] Guid conversation_id, [FromBody] UserPromptDTO userPromptDTODTO)
        {
            try
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

                var (response, is_final) = await generateAiResponseAsync(chatResponse.conversation_id);
                chatResponse.response = response;
                chatResponse.is_final = is_final;

                _chatRepository.aiResponseModified(chatResponse);

                return Ok(/*_chatRepository.getChatByConvoId(conversation_id)*/ chatResponse);
            }
            catch (Exception)
            {
                return Problem();
            }
        }
        #endregion

        #region ContinueChatGetMethod

        [Authorize]
        [HttpGet]
        [Route("{conversation_id:guid}")]
        [ProducesResponseType(200, Type = typeof(Chat))]
        [ProducesResponseType(400, Type = typeof(BadRequestResult))]
        [ProducesResponseType(401, Type = typeof(UnauthorizedResult))]
        [ProducesResponseType(403, Type = typeof(ForbidResult))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        [ProducesResponseType(500, Type = typeof(ProblemHttpResult))]
        public async Task<IActionResult> getChatContinuation([FromRoute] Guid conversation_id)
        {
            try
            {
                if (!_chatRepository.chatExists(conversation_id))
                {
                    return NotFound();
                }

                if (_chatRepository.getLastUserPrompt(_chatRepository.getChatByConvoId(conversation_id)) == null)
                {

                    _chatRepository.deleteChat(_chatRepository.getChatByConvoId(conversation_id));
                    return NotFound();
                }

                var chat = _chatRepository.getChatByConvoId(conversation_id);
                ChatSucessfullResponse? lastAiResponse = _chatRepository.getLastAiResponse(chat);

                if (lastAiResponse == null)
                {
                    lastAiResponse = new ChatSucessfullResponse
                    {
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

                var (response, is_final) = await generateAiResponseAsync(conversation_id);
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

                return Ok(/*_chatRepository.getChatByConvoId(conversation_id)*/ oldChatResponse);
            }
            catch (Exception)
            {
                return Problem();
            }
        }

        #endregion

        #region DeleteChatMethod
        [Authorize]
        [HttpDelete]
        [Route("{chat_id:guid}")]
        [ProducesResponseType(204, Type = typeof(NoContentResult))]
        [ProducesResponseType(400, Type = typeof(BadRequestResult))]
        [ProducesResponseType(401, Type = typeof(UnauthorizedResult))]
        [ProducesResponseType(403, Type = typeof(ForbidResult))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        [ProducesResponseType(500, Type = typeof(ProblemHttpResult))]
        public async Task<IActionResult> deleteChat([FromRoute] Guid chat_id)
        {
            try
            {

                var jwtToken = HelperMethods.decodeToken(/*token, SecretKey*/_config, HttpContext);

                var userId = Guid.Parse(jwtToken.Claims.First(x => x.Type == "UserID").Value);

                if (!_userRepository.UserExists(userId))
                {
                    return BadRequest(new ChatBadResponse
                    {
                        title = "Bad Request",
                        status = 400,
                        detail = "The request is invalid."
                    });
                }

                var isAdmin = bool.Parse(jwtToken.Claims.First(x => x.Type == "admin").Value);
                var isReallyAdmin = _userRepository.getUser(userId)!.isAdmin;

                Chat? chat = _chatRepository.getChatByConvoId(chat_id);

                if (chat == null)
                {
                    return NotFound();
                }

                if ((!_chatRepository.getAllUserChats(userId).Contains(chat)) && ((!isAdmin && isReallyAdmin) || (isAdmin && !isReallyAdmin) || (!isAdmin && !isReallyAdmin)))
                {
                    return Forbid();
                }

                _chatRepository.deleteChat(chat);

                return NoContent();
            }
            catch (Exception)
            {
                return Problem();
            }
        }
        #endregion

        #region getAllChatsFromAUser
        [Authorize]
        [HttpGet]
        [Route("Chats")]
        [ProducesResponseType(200, Type = typeof(ICollection<Chat>))]
        [ProducesResponseType(400, Type = typeof(BadRequestResult))]
        [ProducesResponseType(401, Type = typeof(UnauthorizedResult))]
        [ProducesResponseType(403, Type = typeof(ForbidResult))]
        [ProducesResponseType(500, Type = typeof(ProblemHttpResult))]
        public async Task<IActionResult> getChats()
        {
            try
            {
                var jwtToken = HelperMethods.decodeToken(/*token, SecretKey*/_config, HttpContext);

                var user_id = Guid.Parse(jwtToken.Claims.First(x => x.Type == "UserID").Value);

                var searchChats = _chatRepository.getAllUserChats(user_id);

                if (!ModelState.IsValid)
                {
                    return BadRequest(searchChats);
                }

                return Ok(searchChats);
            }
            catch (Exception)
            {
                return Problem();
            }
        }
        #endregion

        #region getAllChatResponsesOfAChat
        [Authorize]
        [HttpGet]
        [Route("ChatResponses/{chat_id:guid}")]
        [ProducesResponseType(200, Type = typeof(ICollection<ChatSucessfullResponse>))]
        [ProducesResponseType(400, Type = typeof(BadRequestResult))]
        [ProducesResponseType(401, Type = typeof(UnauthorizedResult))]
        [ProducesResponseType(403, Type = typeof(ForbidResult))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        [ProducesResponseType(500, Type = typeof(ProblemHttpResult))]
        public async Task<ActionResult<IEnumerable<ChatSucessfullResponse>>> getChatResponses(Guid chat_id)
        {
            try
            {
                var jwtToken = HelperMethods.decodeToken(/*token, SecretKey*/_config, HttpContext);

                var user_id = Guid.Parse(jwtToken.Claims.First(x => x.Type == "UserID").Value);

                if (!_chatRepository.chatExists(chat_id) || !_userRepository.UserExists(user_id))
                {
                    return NotFound();
                }

                var isAdmin = bool.Parse(jwtToken.Claims.First(x => x.Type == "admin").Value);
                var chat = _chatRepository.getChatByConvoId(chat_id);



                if (!_chatRepository.getAllUserChats(user_id).Contains(chat!) && !_userRepository.isReallyAdmin(user_id, isAdmin))
                {
                    return Forbid();
                }

                var searchChatResponses = _chatRepository.getAllAiResponses(chat_id);

                if (!ModelState.IsValid)
                {
                    return BadRequest(searchChatResponses);
                }

                return Ok(searchChatResponses);
            }
            catch (Exception)
            {
                return Problem();
            }
        }
        #endregion

        #region getAllUserPromptsOfAChat
        [Authorize]
        [HttpGet]
        [Route("UserPrompts/{chat_id:guid}")]
        [ProducesResponseType(200, Type = typeof(ICollection<UserPrompt>))]
        [ProducesResponseType(400, Type = typeof(BadRequestResult))]
        [ProducesResponseType(401, Type = typeof(UnauthorizedResult))]
        [ProducesResponseType(403, Type = typeof(ForbidResult))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        [ProducesResponseType(500, Type = typeof(ProblemHttpResult))]
        public async Task<IActionResult> getUserPrompts(Guid chat_id)
        {

            try
            {
                var jwtToken = HelperMethods.decodeToken(/*token, SecretKey*/_config, HttpContext);

                var user_id = Guid.Parse(jwtToken.Claims.First(x => x.Type == "UserID").Value);

                if (!_chatRepository.chatExists(chat_id) || !_userRepository.UserExists(user_id))
                {
                    return NotFound();
                }

                var isAdmin = bool.Parse(jwtToken.Claims.First(x => x.Type == "admin").Value);
                var chat = _chatRepository.getChatByConvoId(chat_id);

                if (!_chatRepository.getAllUserChats(user_id).Contains(chat!) && !_userRepository.isReallyAdmin(user_id, isAdmin))
                {
                    return Forbid();
                }

                var searchUserPrompts = _chatRepository.getAllUserPrompts(chat_id);

                if (!ModelState.IsValid)
                {
                    return BadRequest(searchUserPrompts);
                }

                return Ok(searchUserPrompts);
            }
            catch (Exception)
            {
                return Problem();
            }
        }
        #endregion

        #region getChatById
        [Authorize]
        [HttpGet]
        [Route("chat/{id:guid}")]
        [ProducesResponseType(200, Type = typeof(Chat))]
        [ProducesResponseType(401, Type = typeof(UnauthorizedResult))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        [ProducesResponseType(500, Type = typeof(ProblemHttpResult))]
        public async Task<IActionResult> getChatById([FromRoute] Guid id)
        {
            try
            {
                var searchChat = _chatRepository.getChatByConvoId(id);

                if (searchChat == null)
                {
                    return NotFound();
                }

                var jwtToken = HelperMethods.decodeToken(/*token, SecretKey*/_config, HttpContext);

                var userId = Guid.Parse(jwtToken.Claims.First(x => x.Type == "UserID").Value);

                var isAdmin = bool.Parse(jwtToken.Claims.First(x => x.Type == "admin").Value);

                if (!((searchChat.user_id == userId) || _userRepository.isReallyAdmin(userId, isAdmin))){
                    return Forbid();
                }

                return Ok(searchChat);
            }
            catch (Exception)
            {
                return Problem();
            }
        }
        #endregion

    }
}
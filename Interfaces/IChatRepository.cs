using testingStuff.models;

namespace testingStuff.Interfaces;

public interface IChatRepository
{
    public bool chatExists(Guid id);
    public bool isFinal(ChatSucessfullResponse aiResponse);
    public ChatSucessfullResponse? getLastAiResponse(Chat chat);
    public UserPrompt getLastUserPrompt(Chat chat);
    public Chat? getChatByConvoId(Guid id);
    public ICollection<UserPrompt> getAllUserPrompts();
    public ICollection<UserPrompt> getAllUserPrompts(Guid user_id);
    public ICollection<ChatSucessfullResponse> getAllAiResponses();
    public ICollection<ChatSucessfullResponse> getAllAiResponses(Guid user_id);
    public ICollection<Chat> getAllUserChats(Guid user_id);
    public void AddUserPrompt(UserPrompt userPrompt);
    public void AddAiResponse(ChatSucessfullResponse AiResponse);
    public void AddChat(Chat chat);
    public void aiResponseModified(ChatSucessfullResponse chatResponse);
    public void chatModified(Chat chat);
    public void deleteChat(Chat chat);
    public void SaveChanges();


}
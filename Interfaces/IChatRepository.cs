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
    public ICollection<ChatSucessfullResponse> getAllAiResponses();
    public ICollection<Chat> getAllUserChats(Guid user_id);
    public void AddUserPrompt(UserPrompt userPrompt);
    public void AddAiResponse(ChatSucessfullResponse AiResponse);
    public void AddChat(Chat chat);
    public void aiResponseModified(ChatSucessfullResponse chatResponse);
    public void SaveChanges();


}
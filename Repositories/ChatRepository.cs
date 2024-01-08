using Microsoft.EntityFrameworkCore;
using testingStuff.data;
using testingStuff.Interfaces;
using testingStuff.models;

namespace testingStuff.Repositories;

public class ChatRepository : IChatRepository
{
    private readonly DbDataContext _context;

    public ChatRepository(DbDataContext context)
    {
        _context = context;
    }

    public bool chatExists(Guid id)
    {
        return _context.Chats.Any(c => c.id == id);
    }
    public bool isFinal(ChatSucessfullResponse aiResponse)
    {
        return aiResponse.is_final;
    }
    public ChatSucessfullResponse? getLastAiResponse(Chat chat)
    {
        return chat.chatPrompts.LastOrDefault();
    }
    public UserPrompt getLastUserPrompt(Chat chat)
    {
        return chat.userPrompts.LastOrDefault();
    }
    public Chat? getChatByConvoId(Guid id)
    {
        return _context.Chats.Where(c => c.id == id).Include(up => up.userPrompts).Include(cps => cps.chatPrompts).FirstOrDefault();
    }

    public ICollection<UserPrompt> getAllUserPrompts(){
        return _context.userPrompts.OrderBy(up => up.id).ToList();
    }

    public ICollection<ChatSucessfullResponse> getAllAiResponses(){
        return _context.AiResponses.OrderBy(csr => csr.id).ToList();
    }

    public ICollection<Chat> getAllUserChats(Guid user_id){
        return _context.Chats.Where(u => u.user.id == user_id).Include(up => up.userPrompts).Include(cps => cps.chatPrompts).OrderBy(c => c.id).ToList();
    }

    public void AddUserPrompt(UserPrompt userPrompt)
    {
        _context.userPrompts.Add(userPrompt);
        SaveChanges();
    }

    public void AddAiResponse(ChatSucessfullResponse AiResponse)
    {
        _context.AiResponses.Add(AiResponse);
        SaveChanges();
    }

    public void AddChat(Chat chat)
    {
        _context.Chats.Add(chat);
        SaveChanges();
    }

    public void aiResponseModified(ChatSucessfullResponse chatResponse)
    {
        _context.Entry(chatResponse).State = EntityState.Modified;
        SaveChanges();
    }

    public void SaveChanges()
    {
        _context.SaveChanges();
    }
}
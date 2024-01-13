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
        return chat.chatPrompts.OrderBy(csr => csr.response_number).LastOrDefault();
    }
    public UserPrompt getLastUserPrompt(Chat chat)
    {
        return chat.userPrompts.OrderBy(up => up.prompt_number).LastOrDefault();
    }
    public Chat? getChatByConvoId(Guid id)
    {
        return _context.Chats.Where(c => c.id == id).Include(up => up.userPrompts.OrderBy(x => x.prompt_number)).Include(cps => cps.chatPrompts.OrderBy(x => x.response_number)).FirstOrDefault();
    }

    public ICollection<UserPrompt> getAllUserPrompts(){
        return _context.userPrompts.OrderBy(up => up.prompt_number).ToList();
    }

    public ICollection<UserPrompt> getAllUserPrompts(Guid chat_id)
    {
        return _context.userPrompts.Where(c => c.conversation_id == chat_id).OrderBy(csr => csr.prompt_number).ToList();
    }

    public ICollection<ChatSucessfullResponse> getAllAiResponses(){
        return _context.AiResponses.OrderBy(csr => csr.response_number).ToList();
    }

    public ICollection<ChatSucessfullResponse> getAllAiResponses(Guid chat_id)
    {
        return _context.AiResponses.Where(c => c.conversation_id == chat_id).OrderBy(csr => csr.response_number).ToList();
    }

    public ICollection<Chat> getAllUserChats(Guid user_id){
        return _context.Chats.Where(u => u.user.id == user_id).Include(up => up.userPrompts.OrderBy(x => x.prompt_number)).Include(cps => cps.chatPrompts.OrderBy(x => x.response_number)).ToList();
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

    public void deleteChat(Chat chat)
    {
        _context.Remove(chat);
        SaveChanges();
    }

    public void SaveChanges()
    {
        _context.SaveChanges();
    }
}
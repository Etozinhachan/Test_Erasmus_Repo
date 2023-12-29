using System.ComponentModel.DataAnnotations;

namespace testingStuff.models;

public class Chat{

    public Guid id { get; set; }
    public Guid user_id { get; set; }
    public ICollection<UserPrompt> userPrompts { get; }
    public ICollection<ChatSucessfullResponse> chatPrompts { get; }
    public User? user { get; set; }

}
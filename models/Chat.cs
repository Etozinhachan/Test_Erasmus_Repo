using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace testingStuff.models;

public class Chat{

    public Guid id { get; set; }
    public Guid user_id { get; set; }
    public ICollection<UserPrompt> userPrompts { get; }
    public ICollection<ChatSucessfullResponse> chatPrompts { get; }
    public int chat_number { get; set; }
    [JsonIgnore]
    public User? user { get; set; }

}
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace testingStuff.models;

public class UserPrompt{

    public Guid? id { get; set; } = null;
    public Guid conversation_id { get; set; }
    public required string prompt { get; set; }
    [JsonIgnore]
    public int prompt_number { get; set; }
    [JsonIgnore]
    public Chat? chat { get; set; }

}
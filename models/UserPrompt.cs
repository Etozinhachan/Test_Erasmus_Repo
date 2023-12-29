using System.ComponentModel.DataAnnotations;

namespace testingStuff.models;

public class UserPrompt{

    public Guid id { get; set; }
    public Guid conversation_id { get; set; }
    public required string prompt { get; set; }

    public Chat chat { get; set; }

}
using System.ComponentModel.DataAnnotations;

namespace testingStuff.models;

public class UserPrompt{

    [Key]
    public Guid id { get; set; }
    [Key]
    public Guid conversation_id { get; set; }
    public required string prompt { get; set; }

    public Chat chat { get; set; }

}
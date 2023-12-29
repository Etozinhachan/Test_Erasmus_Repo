using System.ComponentModel.DataAnnotations;

namespace testingStuff.models;

public class ChatSucessfullResponse{
    public Guid conversation_id { get; set; }
    public Guid id { get; set; }
    public required string response { get; set; }
    public bool is_final { get; set; }

    public Chat? chat { get; set; }
}
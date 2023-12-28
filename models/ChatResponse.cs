using System.ComponentModel.DataAnnotations;

namespace testingStuff.models;

public class ChatResponse{
    [Key]
    public Guid conversation_id { get; set; }
    [Key]
    public Guid id { get; set; }
    public required string response { get; set; }
    public bool is_final { get; set; }
    public required string title { get; set; }
    public int status { get; set; }
    public required string detail { get; set; }
}
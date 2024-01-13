using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace testingStuff.models;

public class User{

    public Guid id { get; set; }
    public required string UserName { get; set; }
    public required string passHash { get; set; }
    public string? salt { get; set; }
    public bool isAdmin { get; set; } = false;
    public ICollection<Chat>? chats{ get; }

}
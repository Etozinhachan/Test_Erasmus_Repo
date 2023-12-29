using System.ComponentModel.DataAnnotations;

namespace testingStuff.models;

public class User{

    public Guid id {get; set;}
    public required string UserName { get; set; }
    public required string passHash { get; set; }
    public required string salt { get; set; }
    public ICollection<Chat>? chats{ get; }

}
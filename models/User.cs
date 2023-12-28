using System.ComponentModel.DataAnnotations;

namespace testingStuff.models;

public class User{

    [Key]
    public Guid id {get; set;}
    public required string UserName { get; set; }
    public required string passHash { get; set; }

    public ICollection<Chat> chats{ get; }

}
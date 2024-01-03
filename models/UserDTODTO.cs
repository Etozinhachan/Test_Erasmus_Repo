using System.ComponentModel.DataAnnotations;

namespace testingStuff.models;

public class UserDTODTO{

    public Guid id {get; set;}
    public required string UserName { get; set; }
    public required string passHash { get; set; }

}
using System.ComponentModel.DataAnnotations;

namespace testingStuff.models;

public class UserDTO{

    public required string UserName { get; set; }
    public required string passHash { get; set; }


}
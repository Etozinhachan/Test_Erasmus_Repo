using System.ComponentModel.DataAnnotations;

namespace testingStuff.models;

public class User{

    [Key]
    public long Id {get; set;}
    public string UserName { get; set; }
    public string passHash { get; set; }

}
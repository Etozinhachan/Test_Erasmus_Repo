using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace testingStuff.models;

public class UserPromptDTO{

    public Guid user_id { get; set; }
    public required string prompt { get; set; }

}
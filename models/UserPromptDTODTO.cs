using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace testingStuff.models;

public class UserPromptDTODTO{
    public required string prompt { get; set; }

}
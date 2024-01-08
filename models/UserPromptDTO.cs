using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace testingStuff.models;

public class UserPromptDTO{
    public required string prompt { get; set; }
}
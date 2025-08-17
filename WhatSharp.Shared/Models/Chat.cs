using System.ComponentModel.DataAnnotations;

namespace WhatSharp.Shared.Models;

public class Chat
{
    [Key]
    public int Id { get; set; }

    [Required, MinLength(1)]
    public string Name { get; set; } = string.Empty;

    public List<Message> Messages { get; set; } = new();
    
    public List<User> Participants { get; set; } = new();
}
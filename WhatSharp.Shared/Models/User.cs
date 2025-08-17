using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WhatSharp.Shared.Models;

public class User
{
    [Key] 
    public int Id { get; set; }
    public string Login { get; set; }
    public string PasswordHash { get; set; }
    [JsonIgnore] public List<Chat> Chats { get; set; } = new();
}
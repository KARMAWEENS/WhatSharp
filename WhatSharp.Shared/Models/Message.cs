using System.ComponentModel.DataAnnotations;

namespace WhatSharp.Shared.Models;

public class Message
{
    [Key]
    public int Id { get; set; }
    public string Content { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public int UserId { get; set; }
    public User User { get; set; }
    public int ChatId { get; set; }
    public Chat Chat { get; set; }
    public bool IsFromCurrentUser => UserId == Session.CurrentUser?.Id;
}
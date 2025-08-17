namespace WhatSharp.Shared.DTO;

public class MessagesDTO
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
}
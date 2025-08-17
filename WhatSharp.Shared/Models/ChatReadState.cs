namespace WhatSharp.Shared.Models;

public class ChatReadState
{
    public int ChatId { get; set; }
    public int UserId { get; set; }

    public DateTime LastReadAt { get; set; } = DateTime.MinValue;
}
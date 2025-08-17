namespace WhatSharp.Shared.DTO;

public class ChatListItemDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<int> ParticipantIds { get; set; } = new();

    public LastMessageDTO? LastMessage { get; set; }
    public int UnreadCount { get; set; }
}
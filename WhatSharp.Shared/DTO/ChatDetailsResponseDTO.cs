namespace WhatSharp.Shared.DTO;

public class ChatDetailsResponseDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public List<int> ParticipantIds { get; set; } = new();
    public List<string> ParticipantNames { get; set; } = new();

    public List<MessagesDTO> Messages { get; set; } = new();
}
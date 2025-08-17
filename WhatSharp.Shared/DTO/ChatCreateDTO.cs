using System.ComponentModel.DataAnnotations;

namespace WhatSharp.Shared.DTO;

public class ChatCreateDTO
{
    [Required] public string Name { get; set; } = string.Empty;     // pour groupe ; vide possible pour 1:1
    [Required] public List<int> ParticipantIds { get; set; } = new();
}
using System.ComponentModel.DataAnnotations;

namespace WhatSharp.Shared.DTO;

public class AddParticipantDto
{
    [Required] public int UserId { get; set; }
}
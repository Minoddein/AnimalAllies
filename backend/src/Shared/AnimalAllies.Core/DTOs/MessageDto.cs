﻿namespace AnimalAllies.Core.DTOs;

public class MessageDto
{
    public Guid MessageId { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsEdited { get; set; }
    public bool IsRead { get; set; }
    public Guid UserId { get; set; }
    public string FirstName { get; set; }
}
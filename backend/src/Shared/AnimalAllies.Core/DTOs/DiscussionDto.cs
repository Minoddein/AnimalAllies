namespace AnimalAllies.Core.DTOs;

public class DiscussionDto
{
    public Guid Id { get; set; }

    public Guid FirstMember { get; set; }

    public Guid SecondMember { get; set; }

    public Guid RelationId { get; set; }

    public MessageDto[] Messages { get; set; } = [];
}
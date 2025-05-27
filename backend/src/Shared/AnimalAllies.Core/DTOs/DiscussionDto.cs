namespace AnimalAllies.Core.DTOs;

public class DiscussionDto
{
    public Guid Id { get; set; }
    public Guid FirstMember { get; set; }
    public Guid SecondMember { get; set; }
    public Guid RelationId { get; set; }
    public string FirstMemberName { get; set; }
    public string FirstMemberSurname { get; set; }
    public string SecondMemberName { get; set; }
    public string SecondMemberSurname { get; set; }
    public string LastMessage { get; set; }
    public int UnreadMessagesCount { get; set; }
    public MessageDto[] Messages { get; set; } = [];
}
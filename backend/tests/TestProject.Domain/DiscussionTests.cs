using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using Discussion.Domain.Entities;
using Discussion.Domain.ValueObjects;
using FluentAssertions;
using VolunteerRequests.Domain.ValueObjects;

namespace TestProject.Domain;

public class DiscussionTests
{
    [Fact]
    public void Create_Discussion_Successfully()
    {
        // arrange
        var createdAt = CreatedAt.Create(DateTime.Now).Value;
        var discussionId = DiscussionId.NewGuid();
        var users = Users.Create(Guid.NewGuid(), Guid.NewGuid()).Value;
        var relationId = Guid.NewGuid();

        // act
        var result = Discussion.Domain.Aggregate.Discussion.Create(discussionId, users, relationId);

        // assert
        result.IsSuccess.Should().BeTrue();
    }

    
    [Fact]
    public void Closed_Discussion_That_Already_Closed()
    {
        // arrange
        var discussion = InitDiscussion();
        var userId = discussion.Users.FirstMember;

        // act
        discussion.CloseDiscussion(userId);
        var result = discussion.CloseDiscussion(userId);

        // assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Send_Comment_To_Discussion_From_User_Who_Take_Part()
    {
        // arrange
        var createdAt = CreatedAt.Create(DateTime.UtcNow);
        var discussion = InitDiscussion();
        var userId = discussion.Users.FirstMember;
        var message = Message.Create(
            MessageId.NewGuid(),
            Text.Create("test").Value,
            createdAt.Value,
            new IsEdited(false),
            new IsRead(false),
            userId).Value;

        // act
        var result = discussion.SendComment(message);
        
        // assert
        result.IsSuccess.Should().BeTrue();
        discussion.Messages.Should().Contain(message);
    }
    
    
    [Fact]
    public void Send_Comment_To_Discussion_From_User_Who_Doesnt_Take_Part()
    {
        // arrange
        var createdAt = CreatedAt.Create(DateTime.UtcNow);
        var discussion = InitDiscussion();
        var userId = Guid.NewGuid();
        var message = Message.Create(
            MessageId.NewGuid(),
            Text.Create("test").Value,
            createdAt.Value,
            new IsEdited(false),
            new IsRead(false),
            userId).Value;

        // act
        var result = discussion.SendComment(message);
        
        // assert
        result.IsSuccess.Should().BeFalse();
    }
    
    [Fact]
    public void Delete_Comment_From_Discussion_From_User_Who_Created_Message()
    {
        // arrange
        var discussion = InitDiscussionWithComments();
        var message = discussion.Messages.FirstOrDefault()!;
        var userId = message.UserId;

        // act
        var result = discussion.DeleteComment(userId, message.Id);
        
        // assert
        result.IsSuccess.Should().BeTrue();
    }
    
    [Fact]
    public void Delete_Comment_From_Discussion_From_User_Who_Doesnt_Created_Message()
    {
        // arrange
        var discussion = InitDiscussionWithComments();
        var message = discussion.Messages.FirstOrDefault()!;
        var userId = Guid.NewGuid();

        // act
        var result = discussion.DeleteComment(userId, message.Id);
        
        // assert
        result.IsSuccess.Should().BeFalse();
    }
    
    [Fact]
    public void Edit_Comment_From_User_Who_Created_Message()
    {
        // arrange
        var discussion = InitDiscussionWithComments();
        var message = discussion.Messages.FirstOrDefault()!;
        var userId = message.UserId;
        var text = Text.Create("newText").Value;
        
        // act
        var result = discussion.EditComment(userId, message.Id, text);
        
        // assert
        result.IsSuccess.Should().BeTrue();
        message.IsEdited.Value.Should().BeTrue();
        message.Text.Value.Should().Be(text.Value);
    }
    
    [Fact]
    public void Edit_Comment_From_User_Who_Doesnt_Created_Message()
    {
        // arrange
        var discussion = InitDiscussionWithComments();
        var message = discussion.Messages.FirstOrDefault()!;
        var userId = Guid.NewGuid();
        var text = Text.Create("newText").Value;
        
        // act
        var result = discussion.EditComment(userId, message.Id, text);
        
        // assert
        result.IsSuccess.Should().BeFalse();
        message.IsEdited.Value.Should().BeFalse();
        message.Text.Value.Should().NotBe(text.Value);
    }
    
    
    private static Discussion.Domain.Aggregate.Discussion InitDiscussion()
    {
        var createdAt = CreatedAt.Create(DateTime.Now).Value;
        var discussionId = DiscussionId.NewGuid();
        var users = Users.Create(Guid.NewGuid(), Guid.NewGuid()).Value;
        var relationId = Guid.NewGuid();

        var discussion = Discussion.Domain.Aggregate.Discussion.Create(discussionId, users, relationId).Value;
        
        return discussion;
    }
    
    private static Discussion.Domain.Aggregate.Discussion InitDiscussionWithComments()
    {
        var createdAt = CreatedAt.Create(DateTime.UtcNow);
        var discussion = InitDiscussion();
        var message = Message.Create(
            MessageId.NewGuid(),
            Text.Create("test").Value,
            createdAt.Value,
            new IsEdited(false),
            new IsRead(false),
            discussion.Users.FirstMember).Value;
        discussion.SendComment(message);
        
        return discussion;
    }
}
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using Discussion.Domain.Entities;
using Discussion.Domain.ValueObjects;
using FluentAssertions;

namespace TestProject.Domain;

public class DiscussionTests
{
    [Fact]
    public void Create_Discussion_Successfully()
    {
        // arrange
        _ = CreatedAt.Create(DateTime.Now).Value;
        DiscussionId discussionId = DiscussionId.NewGuid();
        Users users = Users.Create(Guid.NewGuid(), Guid.NewGuid()).Value;
        Guid relationId = Guid.NewGuid();

        // act
        Result<Discussion.Domain.Aggregate.Discussion> result =
            Discussion.Domain.Aggregate.Discussion.Create(discussionId, users, relationId);

        // assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Closed_Discussion_That_Already_Closed()
    {
        // arrange
        Discussion.Domain.Aggregate.Discussion discussion = InitDiscussion();
        Guid userId = discussion.Users.FirstMember;

        // act
        discussion.CloseDiscussion(userId);
        Result result = discussion.CloseDiscussion(userId);

        // assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Send_Comment_To_Discussion_From_User_Who_Take_Part()
    {
        // arrange
        Result<CreatedAt> createdAt = CreatedAt.Create(DateTime.UtcNow);
        Discussion.Domain.Aggregate.Discussion discussion = InitDiscussion();
        Guid userId = discussion.Users.FirstMember;
        Message message = Message.Create(
            MessageId.NewGuid(),
            Text.Create("test").Value,
            createdAt.Value,
            new IsEdited(false),
            userId).Value;

        // act
        Result result = discussion.SendComment(message);

        // assert
        result.IsSuccess.Should().BeTrue();
        discussion.Messages.Should().Contain(message);
    }

    [Fact]
    public void Send_Comment_To_Discussion_From_User_Who_Doesnt_Take_Part()
    {
        // arrange
        Result<CreatedAt> createdAt = CreatedAt.Create(DateTime.UtcNow);
        Discussion.Domain.Aggregate.Discussion discussion = InitDiscussion();
        Guid userId = Guid.NewGuid();
        Message message = Message.Create(
            MessageId.NewGuid(),
            Text.Create("test").Value,
            createdAt.Value,
            new IsEdited(false),
            userId).Value;

        // act
        Result result = discussion.SendComment(message);

        // assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Delete_Comment_From_Discussion_From_User_Who_Created_Message()
    {
        // arrange
        Discussion.Domain.Aggregate.Discussion discussion = InitDiscussionWithComments();
        Message message = discussion.Messages.FirstOrDefault()!;
        Guid userId = message.UserId;

        // act
        Result result = discussion.DeleteComment(userId, message.Id);

        // assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Delete_Comment_From_Discussion_From_User_Who_Doesnt_Created_Message()
    {
        // arrange
        Discussion.Domain.Aggregate.Discussion discussion = InitDiscussionWithComments();
        Message message = discussion.Messages.FirstOrDefault()!;
        Guid userId = Guid.NewGuid();

        // act
        Result result = discussion.DeleteComment(userId, message.Id);

        // assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Edit_Comment_From_User_Who_Created_Message()
    {
        // arrange
        Discussion.Domain.Aggregate.Discussion discussion = InitDiscussionWithComments();
        Message message = discussion.Messages.FirstOrDefault()!;
        Guid userId = message.UserId;
        Text text = Text.Create("newText").Value;

        // act
        Result result = discussion.EditComment(userId, message.Id, text);

        // assert
        result.IsSuccess.Should().BeTrue();
        message.IsEdited.Value.Should().BeTrue();
        message.Text.Value.Should().Be(text.Value);
    }

    [Fact]
    public void Edit_Comment_From_User_Who_Doesnt_Created_Message()
    {
        // arrange
        Discussion.Domain.Aggregate.Discussion discussion = InitDiscussionWithComments();
        Message message = discussion.Messages.FirstOrDefault()!;
        Guid userId = Guid.NewGuid();
        Text text = Text.Create("newText").Value;

        // act
        Result result = discussion.EditComment(userId, message.Id, text);

        // assert
        result.IsSuccess.Should().BeFalse();
        message.IsEdited.Value.Should().BeFalse();
        message.Text.Value.Should().NotBe(text.Value);
    }

    private static Discussion.Domain.Aggregate.Discussion InitDiscussion()
    {
        _ = CreatedAt.Create(DateTime.Now).Value;
        DiscussionId discussionId = DiscussionId.NewGuid();
        Users users = Users.Create(Guid.NewGuid(), Guid.NewGuid()).Value;
        Guid relationId = Guid.NewGuid();

        Discussion.Domain.Aggregate.Discussion discussion =
            Discussion.Domain.Aggregate.Discussion.Create(discussionId, users, relationId).Value;

        return discussion;
    }

    private static Discussion.Domain.Aggregate.Discussion InitDiscussionWithComments()
    {
        Result<CreatedAt> createdAt = CreatedAt.Create(DateTime.UtcNow);
        Discussion.Domain.Aggregate.Discussion discussion = InitDiscussion();
        Message message = Message.Create(
            MessageId.NewGuid(),
            Text.Create("test").Value,
            createdAt.Value,
            new IsEdited(false),
            discussion.Users.FirstMember).Value;
        discussion.SendComment(message);

        return discussion;
    }
}
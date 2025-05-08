using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using FluentAssertions;
using VolunteerRequests.Domain.Aggregates;
using VolunteerRequests.Domain.ValueObjects;

namespace TestProject.Domain;

public class VolunteerRequestsTests
{
    [Fact]
    public void Create_Volunteer_Request_And_Approve_Successfully()
    {
        // arrange
        Guid adminId = Guid.NewGuid();
        Guid discussionId = Guid.NewGuid();

        VolunteerRequest volunteerRequest = InitVolunteerRequest();

        // act
        volunteerRequest.TakeRequestForSubmit(adminId, discussionId);
        volunteerRequest.ApproveRequest();

        // assert
        volunteerRequest.RequestStatus.Value.Should().Be(RequestStatus.Approved.Value);
    }

    [Fact]
    public void Create_Volunteer_Request_Send_On_Revision_Edit_Request_Successfully_Approve()
    {
        // arrange
        Guid adminId = Guid.NewGuid();
        Guid discussionId = Guid.NewGuid();
        RejectionComment rejectComment = RejectionComment.Create("Переделай").Value;

        VolunteerRequest volunteerRequest = InitVolunteerRequest();

        // act
        volunteerRequest.TakeRequestForSubmit(adminId, discussionId);
        volunteerRequest.SendRequestForRevision(rejectComment);

        // Что-то поменяли
        volunteerRequest.ResendVolunteerRequest();
        volunteerRequest.ApproveRequest();

        // assert
        volunteerRequest.RequestStatus.Value.Should().Be(RequestStatus.Approved.Value);
    }

    [Fact]
    public void Create_Volunteer_Request_Send_On_Revision_Edit_Request_Not_Successfully_Reject()
    {
        // arrange
        Guid adminId = Guid.NewGuid();
        Guid discussionId = Guid.NewGuid();
        RejectionComment rejectComment = RejectionComment.Create("Переделай").Value;
        RejectionComment rejectionCommentFinally = RejectionComment.Create("Вам отказано").Value;

        VolunteerRequest volunteerRequest = InitVolunteerRequest();

        // act
        volunteerRequest.TakeRequestForSubmit(adminId, discussionId);
        volunteerRequest.SendRequestForRevision(rejectComment);

        // Что-то поменяли
        volunteerRequest.ResendVolunteerRequest();
        volunteerRequest.RejectRequest(rejectionCommentFinally);

        // assert
        volunteerRequest.RequestStatus.Value.Should().Be(RequestStatus.Rejected.Value);
    }

    private static VolunteerRequest InitVolunteerRequest()
    {
        CreatedAt createdAt = CreatedAt.Create(DateTime.Now).Value;

        VolunteerRequestId volunteerRequestId = VolunteerRequestId.NewGuid();

        VolunteerInfo volunteerInfo = new(
            FullName.Create("test", "test", "test").Value,
            Email.Create("test@gmail.com").Value,
            PhoneNumber.Create("+12345678910").Value,
            WorkExperience.Create(10).Value,
            VolunteerDescription.Create("test").Value,
            []);

        Guid userId = Guid.NewGuid();

        VolunteerRequest volunteerRequest = new(volunteerRequestId, createdAt, volunteerInfo, userId);
        return volunteerRequest;
    }
}
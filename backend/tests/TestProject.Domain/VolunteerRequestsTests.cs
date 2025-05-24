using AnimalAllies.SharedKernel.Shared;
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
        var adminId = Guid.NewGuid();
        var discussionId = Guid.NewGuid();
        
        var volunteerRequest = InitVolunteerRequest();

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
        var adminId = Guid.NewGuid();
        var discussionId = Guid.NewGuid();
        var rejectComment = RejectionComment.Create("Переделай").Value;
        
        var volunteerRequest = InitVolunteerRequest();

        // act
        volunteerRequest.TakeRequestForSubmit(adminId, discussionId);
        volunteerRequest.SendRequestForRevision(rejectComment);
        
        //Что-то поменяли

        volunteerRequest.ResendVolunteerRequest();
        volunteerRequest.ApproveRequest();

        // assert
        volunteerRequest.RequestStatus.Value.Should().Be(RequestStatus.Approved.Value);
    }
    
    [Fact]
    public void Create_Volunteer_Request_Send_On_Revision_Edit_Request_Not_Successfully_Reject()
    {
        // arrange
        var adminId = Guid.NewGuid();
        var discussionId = Guid.NewGuid();
        var rejectComment = RejectionComment.Create("Переделай").Value;
        var rejectionCommentFinally = RejectionComment.Create("Вам отказано").Value;
        
        var volunteerRequest = InitVolunteerRequest();

        // act
        volunteerRequest.TakeRequestForSubmit(adminId, discussionId);
        volunteerRequest.SendRequestForRevision(rejectComment);
        
        //Что-то поменяли
        volunteerRequest.ResendVolunteerRequest();
        volunteerRequest.RejectRequest(rejectionCommentFinally);
        
        // assert
        volunteerRequest.RequestStatus.Value.Should().Be(RequestStatus.Rejected.Value);
    }
    
    private static VolunteerRequest InitVolunteerRequest()
    {
        var createdAt = CreatedAt.Create(DateTime.Now).Value;
        
        var volunteerRequestId = VolunteerRequestId.NewGuid();
        
        var volunteerInfo = new VolunteerInfo(
            FullName.Create("test", "test", "test").Value,
            Email.Create("test@gmail.com").Value,
            PhoneNumber.Create("+12345678910").Value,
            WorkExperience.Create(10).Value,
            VolunteerDescription.Create("test").Value);

        var userId = Guid.NewGuid();
        
        var volunteerRequest = new VolunteerRequest(volunteerRequestId,createdAt, volunteerInfo, userId);
        return volunteerRequest;
    }
}
using System.Transactions;
using AnimalAllies.Core.Database;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.SharedKernel.Shared.ValueObjects;
using AnimalAllies.Volunteer.Application.Repository;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Outbox.Abstractions;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Consumers.ApprovedVolunteerRequestEvent;

public class ApprovedVolunteerRequestEventConsumer:
    IConsumer<VolunteerRequests.Contracts.Messaging.ApprovedVolunteerRequestEvent>
{
    private readonly ILogger<ApprovedVolunteerRequestEventConsumer> _logger;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWorkOutbox _unitOfWorkOutbox;
    private readonly IVolunteerRepository _volunteerRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    public ApprovedVolunteerRequestEventConsumer(
        ILogger<ApprovedVolunteerRequestEventConsumer> logger,
        IOutboxRepository outboxRepository,
        IUnitOfWorkOutbox unitOfWorkOutbox,
        IVolunteerRepository volunteerRepository,
        [FromKeyedServices(Constraints.Context.PetManagement)]IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _outboxRepository = outboxRepository;
        _unitOfWorkOutbox = unitOfWorkOutbox;
        _volunteerRepository = volunteerRepository;
        _unitOfWork = unitOfWork;
    }
    //TODO: настроить, чтобы сообщение не дублировалось (сейчас волонтёр создается дважды) 
    public async Task Consume(ConsumeContext<VolunteerRequests.Contracts.Messaging.ApprovedVolunteerRequestEvent> context)
    {
        var message = context.Message;
        
        using var transaction = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        try
        {
            var email = Email.Create(message.Email).Value;
            
            var volunteer = await _volunteerRepository.GetByEmail(email, context.CancellationToken);
            if (volunteer.IsSuccess)
                throw new Exception();
            
            var phoneNumber = PhoneNumber.Create(message.Phone).Value;
        
            var volunteerByPhoneNumber = await _volunteerRepository
                .GetByPhoneNumber(phoneNumber,context.CancellationToken);

            if (!volunteerByPhoneNumber.IsFailure)
                throw new Exception();
        
            var fullName = FullName.Create(message.FirstName, message.SecondName, message.Patronymic).Value;
            var description = VolunteerDescription.Create(message.Description).Value;
            var workExperience = WorkExperience.Create(message.WorkExperience).Value;
            
            var volunteerId = VolunteerId.NewGuid();
        
            var newVolunteer = new Domain.VolunteerManagement.Aggregate.Volunteer(
                volunteerId,
                fullName,
                email,
                description,
                workExperience,
                phoneNumber,
                new ValueObjectList<Requisite>([]));
            
            await _volunteerRepository.Create(newVolunteer, context.CancellationToken);
            
            transaction.Complete();
            
            _logger.LogInformation("Volunteer created with id {id}", volunteerId.Id);
        }
        catch (Exception)
        {
            _logger.LogError("Cannot create volunteer");
        }
    }
}

using AnimalAllies.Accounts.Application.AccountManagement.Commands.CreateVolunteerAccount;
using AnimalAllies.Accounts.Application.AccountManagement.Queries.IsUserExistById;
using AnimalAllies.Accounts.Contracts;

namespace AnimalAllies.Accounts.Presentation;

public class AccountContract: IAccountContract
{
    private readonly IsUserExistByIdHandler _isUserExistByIdHandler;
    private readonly CreateVolunteerAccountHandler _createVolunteerAccountHandler;
    
    public AccountContract(
        IsUserExistByIdHandler isUserExistByIdHandler,
        CreateVolunteerAccountHandler createVolunteerAccountHandler)
    {
        _isUserExistByIdHandler = isUserExistByIdHandler;
        _createVolunteerAccountHandler = createVolunteerAccountHandler;
    }
    
    public async Task<bool> IsUserExistById(Guid userId, CancellationToken cancellationToken = default)
    {
        return (await _isUserExistByIdHandler.Handle(userId, cancellationToken)).Value;
    }

    public async Task ApproveVolunteerRequest(
        Guid userId,
        string firstName, 
        string secondName,
        string? patronymic,
        int workExperience,
        string description,
        string email,
        string phone, 
        CancellationToken cancellationToken = default)
    {
        var command = new CreateVolunteerAccountCommand(
            userId,
            firstName, 
            secondName, 
            patronymic,
            workExperience,
            description,
            email,
            phone);
        
        await _createVolunteerAccountHandler.Handle(command, cancellationToken);
    }
}
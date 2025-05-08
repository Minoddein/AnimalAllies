using System.Data;
using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.Volunteer.Application.Repository;
using AnimalAllies.Volunteer.Contracts.Responses;
using AnimalAllies.Volunteer.Domain.VolunteerManagement.Entities.Pet;
using AnimalAllies.Volunteer.Domain.VolunteerManagement.Entities.Pet.ValueObjects;
using FileService.Communication;
using FileService.Contract.Requests;
using FileService.Contract.Responses;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FileInfo = AnimalAllies.Volunteer.Application.FileProvider.FileInfo;

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.DeletePetPhoto;

public class DeletePetPhotosHandler(
    IVolunteerRepository volunteerRepository,
    ILogger<DeletePetPhotosHandler> logger,
    [FromKeyedServices(Constraints.Context.PetManagement)]
    IUnitOfWork unitOfWork,
    IValidator<DeletePetPhotosCommand> validator,
    FileHttpClient fileHttpClient) : ICommandHandler<DeletePetPhotosCommand, DeletePetPhotosResponse>
{
    private const string BUCKET_NAME = "photos";
    private readonly FileHttpClient _fileHttpClient = fileHttpClient;
    private readonly ILogger<DeletePetPhotosHandler> _logger = logger;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IValidator<DeletePetPhotosCommand> _validator = validator;
    private readonly IVolunteerRepository _volunteerRepository = volunteerRepository;

    public async Task<Result<DeletePetPhotosResponse>> Handle(
        DeletePetPhotosCommand command,
        CancellationToken cancellationToken = default)
    {
        ValidationResult? validationResult =
            await _validator.ValidateAsync(command, cancellationToken).ConfigureAwait(false);

        if (validationResult.IsValid == false)
        {
            return validationResult.ToErrorList();
        }

        IDbTransaction transaction = await _unitOfWork.BeginTransaction(cancellationToken).ConfigureAwait(false);

        try
        {
            Result<Domain.VolunteerManagement.Aggregate.Volunteer> volunteerResult = await _volunteerRepository.GetById(
                VolunteerId.Create(command.VolunteerId), cancellationToken).ConfigureAwait(false);

            if (volunteerResult.IsFailure)
            {
                return volunteerResult.Errors;
            }

            PetId petId = PetId.Create(command.PetId);

            Result<Pet> pet = volunteerResult.Value.GetPetById(petId);

            if (pet.IsFailure)
            {
                return Errors.General.NotFound(petId.Id);
            }

            List<FileInfo> petPreviousPhotos =
            [
                .. pet.Value.PetPhotoDetails
                    .Where(f => !command.FilePaths.Contains(f.Path.Path))
                    .Select(f => new FileInfo(f.Path, BUCKET_NAME))
            ];

            DeletePresignedUrlsRequest request = new(petPreviousPhotos.Select(p =>
                new DeletePresignedUrlRequest(p.BucketName, Path.GetFileNameWithoutExtension(p.FilePath.Path),
                    Path.GetExtension(p.FilePath.Path))));

            GetDeletePresignedUrlsResponse? response =
                await _fileHttpClient.GetDeletePresignedUrlAsync(request, cancellationToken).ConfigureAwait(false);

            if (response is null)
            {
                return Errors.General.Null("response from file service");
            }

            IEnumerable<FilePath> filePaths = command.FilePaths.Select(f =>
                FilePath.Create(Guid.Parse(Path.GetFileNameWithoutExtension(f)), Path.GetExtension(f)).Value);

            pet.Value.DeletePhotos(filePaths);

            DeletePetPhotosResponse deleteUrlResponse = new(response.DeleteUrl);

            await _unitOfWork.SaveChanges(cancellationToken).ConfigureAwait(false);

            transaction.Commit();

            _logger.LogInformation("Files deleted from pet with id - {id}", petId.Id);

            return deleteUrlResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError("Can not delete photo from pet - {id} in transaction", command.PetId);

            transaction.Rollback();

            return Error.Failure("Can not delete photo from pet - {id}", "volunteer.pet.failure");
        }
    }
}
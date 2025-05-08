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

namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.AddPetPhoto;

public class AddPetPhotosHandler(
    IVolunteerRepository volunteerRepository,
    ILogger<AddPetPhotosHandler> logger,
    [FromKeyedServices(Constraints.Context.PetManagement)]
    IUnitOfWork unitOfWork,
    IValidator<AddPetPhotosCommand> validator,
    FileHttpClient fileHttpClient) : ICommandHandler<AddPetPhotosCommand, AddPetPhotosResponse>
{
    private readonly FileHttpClient _fileHttpClient = fileHttpClient;
    private readonly ILogger<AddPetPhotosHandler> _logger = logger;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IValidator<AddPetPhotosCommand> _validator = validator;
    private readonly IVolunteerRepository _volunteerRepository = volunteerRepository;

    public async Task<Result<AddPetPhotosResponse>> Handle(
        AddPetPhotosCommand command,
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

            List<UploadPresignedUrlRequest> uploadPresignedUrlRequests = [];
            uploadPresignedUrlRequests.AddRange(
                command.Photos.Select(file =>
                    new UploadPresignedUrlRequest(file.BucketName, file.FileName, file.ContentType)));

            UploadPresignedUrlsRequest request = new(uploadPresignedUrlRequests);

            IEnumerable<GetUploadPresignedUrlResponse>? response =
                await _fileHttpClient.GetManyUploadPresignedUrlsAsync(
                    request,
                    cancellationToken).ConfigureAwait(false);

            if (response is null)
            {
                return Errors.General.Null();
            }

            List<PetPhoto> photos = [];
            foreach (GetUploadPresignedUrlResponse presignedUrlResponse in response)
            {
                Result<FilePath> path = FilePath.Create(presignedUrlResponse.FileId, presignedUrlResponse.Extension);
                if (path.IsFailure)
                {
                    return path.Errors;
                }

                photos.Add(new PetPhoto(path.Value, false));
            }

            ValueObjectList<PetPhoto> petPhotoList = new(photos);

            Result result = pet.Value.AddPhotos(petPhotoList);

            if (result.IsFailure)
            {
                return result.Errors;
            }

            AddPetPhotosResponse addPetPhotosResponse = new(
                response.Select(r => r.UploadUrl));

            await _unitOfWork.SaveChanges(cancellationToken).ConfigureAwait(false);

            transaction.Commit();

            _logger.LogInformation("Files uploaded to pet with id - {id}", petId.Id);

            return addPetPhotosResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError("Can not add photo to pet - {id} in transaction", command.PetId);

            transaction.Rollback();

            return Error.Failure("Can not add photo to pet - {id}", "volunteer.pet.failure");
        }
    }
}
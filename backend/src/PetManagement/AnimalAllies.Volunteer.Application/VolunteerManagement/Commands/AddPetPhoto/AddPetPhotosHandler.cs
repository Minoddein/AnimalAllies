using AnimalAllies.Core.Abstractions;
using AnimalAllies.Core.Database;
using AnimalAllies.Core.Extension;
using AnimalAllies.SharedKernel.Constraints;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Errors;
using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.Volunteer.Application.Repository;
using AnimalAllies.Volunteer.Contracts.Responses;
using AnimalAllies.Volunteer.Domain.VolunteerManagement.Entities.Pet.ValueObjects;
using FileService.Communication;
using FileService.Contract.Requests;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


namespace AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.AddPetPhoto;

public class AddPetPhotosHandler : ICommandHandler<AddPetPhotosCommand, AddPetPhotosResponse>
{
    private const string BUCKET_NAME = "photos";
    private readonly FileHttpClient _fileHttpClient;
    private readonly IVolunteerRepository _volunteerRepository;
    private readonly ILogger<AddPetPhotosHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<AddPetPhotosCommand> _validator;

    public AddPetPhotosHandler(
        IVolunteerRepository volunteerRepository,
        ILogger<AddPetPhotosHandler> logger,
        [FromKeyedServices(Constraints.Context.PetManagement)]IUnitOfWork unitOfWork,
        IValidator<AddPetPhotosCommand> validator,
        FileHttpClient fileHttpClient)
    {
        _volunteerRepository = volunteerRepository;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _validator = validator;
        _fileHttpClient = fileHttpClient;
    }
    

    public async Task<Result<AddPetPhotosResponse>> Handle(
        AddPetPhotosCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (validationResult.IsValid == false)
        {
            return validationResult.ToErrorList();
        }
        
        var transaction = await _unitOfWork.BeginTransaction(cancellationToken);

        try
        {

            var volunteerResult = await _volunteerRepository.GetById(
                VolunteerId.Create(command.VolunteerId), cancellationToken);

            if (volunteerResult.IsFailure)
                return volunteerResult.Errors;

            var petId = PetId.Create(command.PetId);

            var pet = volunteerResult.Value.GetPetById(petId);

            if (pet.IsFailure)
                return Errors.General.NotFound(petId.Id);

            List<UploadPresignedUrlRequest> uploadPresignedUrlRequests = [];
            uploadPresignedUrlRequests.AddRange(
                command.Photos.Select(file =>
                    new UploadPresignedUrlRequest(file.BucketName, file.FileName, file.ContentType)));

            var request = new UploadPresignedUrlsRequest(uploadPresignedUrlRequests);
            
            var response = await _fileHttpClient.GetManyUploadPresignedUrlsAsync(
                request, 
                cancellationToken);

            if (response is null)
                return Errors.General.Null();

            List<PetPhoto> photos = [];
            var getUploadPresignedUrlResponses = response.ToList();
            foreach (var presignedUrlResponse in getUploadPresignedUrlResponses)
            {
                var path = FilePath.Create(presignedUrlResponse.FileId, presignedUrlResponse.Extension);
                if (path.IsFailure)
                    return path.Errors;
                
                photos.Add(new PetPhoto(path.Value, false));
            }
            
            var petPhotoList = new ValueObjectList<PetPhoto>(photos);

            var result = pet.Value.AddPhotos(petPhotoList);
            
            if (result.IsFailure)
                return result.Errors;
            

            var addPetPhotosResponse = new AddPetPhotosResponse(
                getUploadPresignedUrlResponses.Select(r => r.UploadUrl));

            await _unitOfWork.SaveChanges(cancellationToken);

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
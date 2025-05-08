using AnimalAllies.Core.DTOs;
using AnimalAllies.Core.Models;
using AnimalAllies.Framework;
using AnimalAllies.Framework.Authorization;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.AddPet;
using AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.AddPetPhoto;
using AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.CreateRequisites;
using AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.CreateVolunteer;
using AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.DeletePetForce;
using AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.DeletePetPhoto;
using AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.DeletePetSoft;
using AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.DeleteVolunteer;
using AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.MovePetPosition;
using AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.RestorePet;
using AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.RestoreVolunteer;
using AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.SetMainPhotoOfPet;
using AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.UpdatePet;
using AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.UpdatePetStatus;
using AnimalAllies.Volunteer.Application.VolunteerManagement.Commands.UpdateVolunteer;
using AnimalAllies.Volunteer.Application.VolunteerManagement.Queries.GetFilteredPetsWithPagination;
using AnimalAllies.Volunteer.Application.VolunteerManagement.Queries.GetPetById;
using AnimalAllies.Volunteer.Application.VolunteerManagement.Queries.GetPetsByBreedId;
using AnimalAllies.Volunteer.Application.VolunteerManagement.Queries.GetPetsBySpeciesId;
using AnimalAllies.Volunteer.Application.VolunteerManagement.Queries.GetVolunteerById;
using AnimalAllies.Volunteer.Application.VolunteerManagement.Queries.GetVolunteersWithPagination;
using AnimalAllies.Volunteer.Contracts.Requests;
using AnimalAllies.Volunteer.Contracts.Responses;
using AnimalAllies.Volunteer.Presentation.Requests.Volunteer;
using Microsoft.AspNetCore.Mvc;

namespace AnimalAllies.Volunteer.Presentation;

public class VolunteerController : ApplicationController
{
    [Permission("volunteer.read")]
    [HttpGet]
    public async Task<ActionResult> Get(
        [FromQuery] GetFilteredVolunteersWithPaginationRequest request,
        [FromServices] GetFilteredVolunteersWithPaginationHandler handler,
        CancellationToken cancellationToken = default)
    {
        GetFilteredVolunteersWithPaginationQuery query = request.ToQuery();

        Result<PagedList<VolunteerDto>> result = await handler.Handle(query, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            result.Errors.ToResponse();
        }

        return Ok(result);
    }

    [Permission("volunteer.read")]
    [HttpGet("{volunteerId:guid}")]
    public async Task<ActionResult> GetById(
        [FromRoute] Guid volunteerId,
        [FromServices] GetVolunteerByIdHandler handler,
        CancellationToken cancellationToken = default)
    {
        GetVolunteerByIdQuery query = new(volunteerId);

        Result<VolunteerDto> result = await handler.Handle(query, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            result.Errors.ToResponse();
        }

        return Ok(result);
    }

    [Permission("volunteer.read")]
    [HttpGet("dapper")]
    public async Task<ActionResult> GetDapper(
        [FromQuery] GetFilteredVolunteersWithPaginationRequest request,
        [FromServices] GetFilteredVolunteersWithPaginationHandlerDapper handler,
        CancellationToken cancellationToken = default)
    {
        GetFilteredVolunteersWithPaginationQuery query = request.ToQuery();

        Result<PagedList<VolunteerDto>> result = await handler.Handle(query, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            result.Errors.ToResponse();
        }

        return Ok(result);
    }

    [Permission("volunteer.read")]
    [HttpGet("{volunteerId:guid}/pet-dapper")]
    public async Task<ActionResult> GetPetsDapper(
        [FromRoute] Guid volunteerId,
        [FromQuery] GetFilteredPetsWithPaginationRequest request,
        [FromServices] GetFilteredPetsWithPaginationHandler handler,
        CancellationToken cancellationToken = default)
    {
        GetFilteredPetsWithPaginationQuery query = request.ToQuery(volunteerId);

        Result<PagedList<PetDto>> result = await handler.Handle(query, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            result.Errors.ToResponse();
        }

        return Ok(result);
    }

    [Permission("volunteer.read")]
    [HttpGet("{speciesId:guid}/pet-by-species-id")]
    public async Task<ActionResult> GetPetsBySpeciesIdDapper(
        [FromBody] GetPetsBySpeciesIdRequest request,
        [FromServices] GetPetsBySpeciesIdHandler handler,
        CancellationToken cancellationToken = default)
    {
        GetPetsBySpeciesIdQuery query = new(request.SpeciesId, request.Page, request.PageSize);

        Result<List<PetDto>> result = await handler.Handle(query, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            result.Errors.ToResponse();
        }

        return Ok(result);
    }

    [Permission("volunteer.read")]
    [HttpGet("pet-by-breed-id")]
    public async Task<ActionResult> GetPetsByBreedIdDapper(
        [FromBody] GetPetsByBreedIdRequest request,
        [FromServices] GetPetsByBreedIdHandler handler,
        CancellationToken cancellationToken = default)
    {
        GetPetsByBreedIdQuery query = new(request.BreedId, request.Page, request.PageSize);

        Result<List<PetDto>> result = await handler.Handle(query, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            result.Errors.ToResponse();
        }

        return Ok(result);
    }

    [Permission("volunteer.read")]
    [HttpGet("{petId:guid}/pet-by-id-dapper")]
    public async Task<ActionResult> GetPetByIdDapper(
        [FromRoute] Guid petId,
        [FromServices] GetPetByIdHandler handler,
        CancellationToken cancellationToken = default)
    {
        GetPetByIdQuery query = new(petId);

        Result<PetDto> result = await handler.Handle(query, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            result.Errors.ToResponse();
        }

        return Ok(result);
    }

    [Permission("volunteer.create")]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromServices] CreateVolunteerHandler handler,
        [FromBody] CreateVolunteerRequest request,
        CancellationToken cancellationToken = default)
    {
        CreateVolunteerCommand command = request.ToCommand();

        Result<VolunteerId> result = await handler.Handle(command, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return result.Errors.ToResponse();
        }

        return Ok(result.Value);
    }

    [Permission("volunteer.update")]
    [HttpPut("{id:guid}/main-info")]
    public async Task<IActionResult> UpdateVolunteer(
        [FromRoute] Guid id,
        [FromBody] UpdateVolunteerRequest request,
        [FromServices] UpdateVolunteerHandler handler,
        CancellationToken cancellationToken = default)
    {
        UpdateVolunteerCommand command = request.ToCommand(id);

        Result<VolunteerId> response = await handler.Handle(command, cancellationToken).ConfigureAwait(false);

        if (response.IsFailure)
        {
            return response.Errors.ToResponse();
        }

        return Ok(response.Value);
    }

    [Permission("volunteer.update")]
    [HttpPut("{id:guid}/requisites")]
    public async Task<IActionResult> CreateRequisitesToVolunteer(
        [FromRoute] Guid id,
        [FromBody] CreateRequisitesRequest request,
        [FromServices] CreateRequisitesHandler handler,
        CancellationToken cancellationToken = default)
    {
        CreateRequisitesCommand command = request.ToCommand(id);

        Result<VolunteerId> response = await handler.Handle(command, cancellationToken).ConfigureAwait(false);

        if (response.IsFailure)
        {
            return response.Errors.ToResponse();
        }

        return Ok(response.Value);
    }

    [Permission("volunteer.delete")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteVolunteer(
        [FromRoute] Guid id,
        [FromServices] DeleteVolunteerHandler handler,
        CancellationToken cancellationToken = default)
    {
        DeleteVolunteerCommand command = new(id);

        Result<VolunteerId> response = await handler.Handle(command, cancellationToken).ConfigureAwait(false);

        if (response.IsFailure)
        {
            return response.Errors.ToResponse();
        }

        return Ok(response.Value);
    }

    [Permission("volunteer.create")]
    [HttpPost("{id:guid}/pet")]
    public async Task<ActionResult> AddPet(
        [FromRoute] Guid id,
        [FromBody] AddPetRequest request,
        [FromServices] AddPetHandler handler,
        CancellationToken cancellationToken = default)
    {
        AddPetCommand command = request.ToCommand(id);

        Result<Guid> result = await handler.Handle(command, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return result.Errors.ToResponse();
        }

        return Ok(result.Value);
    }

    [Permission("volunteer.update")]
    [HttpPost("{volunteerId:guid}/{petId:guid}/petPhoto")]
    public async Task<ActionResult> AddPetPhoto(
        [FromRoute] Guid volunteerId,
        [FromRoute] Guid petId,
        [FromBody] AddPetPhotosRequest request,
        [FromServices] AddPetPhotosHandler handler,
        CancellationToken cancellationToken = default)
    {
        AddPetPhotosCommand command = request.ToCommand(volunteerId, petId);

        Result<AddPetPhotosResponse> result = await handler.Handle(command, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return result.Errors.ToResponse();
        }

        return Ok(result.Value);
    }

    [Permission("volunteer.update")]
    [HttpPost("{volunteerId:guid}/{petId:guid}/pet-position")]
    public async Task<ActionResult> MovePetPosition(
        [FromRoute] Guid volunteerId,
        [FromRoute] Guid petId,
        [FromBody] MovePetPositionRequest request,
        [FromServices] MovePetPositionHandler handler,
        CancellationToken cancellationToken = default)
    {
        MovePetPositionCommand command = request.ToCommand(volunteerId, petId);

        Result<VolunteerId> result = await handler.Handle(command, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return result.Errors.ToResponse();
        }

        return Ok(result.Value);
    }

    [Permission("volunteer.update")]
    [HttpPut("{volunteerId:guid}/{petId:guid}/pet")]
    public async Task<ActionResult> UpdatePet(
        [FromRoute] Guid volunteerId,
        [FromRoute] Guid petId,
        [FromBody] UpdatePetRequest request,
        [FromServices] UpdatePetHandler handler,
        CancellationToken cancellationToken = default)
    {
        UpdatePetCommand command = request.ToCommand(volunteerId, petId);

        Result<Guid> result = await handler.Handle(command, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return result.Errors.ToResponse();
        }

        return Ok(result.Value);
    }

    [Permission("volunteer.delete")]
    [HttpPut("{volunteerId:guid}/{petId:guid}/delete-pet-photos")]
    public async Task<ActionResult> DeletePetPhoto(
        [FromRoute] Guid volunteerId,
        [FromRoute] Guid petId,
        [FromBody] DeletePetPhotosRequest request,
        [FromServices] DeletePetPhotosHandler handler,
        CancellationToken cancellationToken = default)
    {
        DeletePetPhotosCommand command = new(volunteerId, petId, request.FilePaths);

        Result<DeletePetPhotosResponse> result = await handler.Handle(command, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return result.Errors.ToResponse();
        }

        return Ok(result.Value);
    }

    [Permission("volunteer.update")]
    [HttpPut("{volunteerId:guid}/{petId:guid}/pet-help-status")]
    public async Task<ActionResult> UpdatePetStatus(
        [FromRoute] Guid volunteerId,
        [FromRoute] Guid petId,
        [FromBody] UpdatePetStatusRequest request,
        [FromServices] UpdatePetStatusHandler handler,
        CancellationToken cancellationToken = default)
    {
        UpdatePetStatusCommand command = request.ToCommand(volunteerId, petId);

        Result<PetId> result = await handler.Handle(command, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return result.Errors.ToResponse();
        }

        return Ok(result.Value);
    }

    // TODO: перенести все реквесты в Contract и убрать ToCommand
    [Permission("volunteer.restore")]
    [HttpPut("{volunteerId:guid}/volunteer-recovery")]
    public async Task<ActionResult> VolunteerRestore(
        [FromRoute] Guid volunteerId,
        [FromServices] RestoreVolunteerHandler handler,
        CancellationToken cancellationToken = default)
    {
        RestoreVolunteerCommand command = new(volunteerId);

        Result<VolunteerId> result = await handler.Handle(command, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return result.Errors.ToResponse();
        }

        return Ok(result.Value);
    }

    [Permission("volunteer.update")]
    [HttpPut("{volunteerId:guid}/{petId:guid}/volunteer-recovery")]
    public async Task<ActionResult> PetRestore(
        [FromRoute] Guid volunteerId,
        [FromRoute] Guid petId,
        [FromServices] RestorePetHandler handler,
        CancellationToken cancellationToken = default)
    {
        RestorePetCommand command = new(volunteerId, petId);

        Result<PetId> result = await handler.Handle(command, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return result.Errors.ToResponse();
        }

        return Ok(result.Value);
    }

    [Permission("volunteer.update")]
    [HttpPut("{volunteerId:guid}/{petId:guid}/pet-main-photo")]
    public async Task<ActionResult> SetMainPhotoOfPet(
        [FromRoute] Guid volunteerId,
        [FromRoute] Guid petId,
        [FromBody] SetMainPhotoOfPetRequest request,
        [FromServices] SetMainPhotoOfPetHandler handler,
        CancellationToken cancellationToken = default)
    {
        SetMainPhotoOfPetCommand command = request.ToCommand(volunteerId, petId);

        Result<PetId> result = await handler.Handle(command, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return result.Errors.ToResponse();
        }

        return Ok(result.Value);
    }

    [Permission("volunteer.delete")]
    [HttpDelete("{volunteerId:guid}/{petId:guid}/pet-removing-soft")]
    public async Task<ActionResult> DeletePetSoft(
        [FromRoute] Guid volunteerId,
        [FromRoute] Guid petId,
        [FromServices] DeletePetSoftHandler handler,
        CancellationToken cancellationToken = default)
    {
        DeletePetSoftCommand command = new(volunteerId, petId);

        Result<PetId> result = await handler.Handle(command, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return result.Errors.ToResponse();
        }

        return Ok(result.Value);
    }

    [Permission("volunteer.delete")]
    [HttpDelete("{volunteerId:guid}/{petId:guid}/pet-removing-force")]
    public async Task<ActionResult> DeletePetForce(
        [FromRoute] Guid volunteerId,
        [FromRoute] Guid petId,
        [FromServices] DeletePetForceHandler handler,
        CancellationToken cancellationToken = default)
    {
        DeletePetForceCommand command = new(volunteerId, petId);

        Result<PetId> result = await handler.Handle(command, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return result.Errors.ToResponse();
        }

        return Ok(result.Value);
    }
}
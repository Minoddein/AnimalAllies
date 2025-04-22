using System.Data;
using AnimalAllies.Core.Models;
using AnimalAllies.Framework;
using AnimalAllies.Framework.Authorization;
using AnimalAllies.Framework.Processors;
using AnimalAllies.SharedKernel.Shared;
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
using AnimalAllies.Volunteer.Presentation.Requests.Volunteer;
using Microsoft.AspNetCore.Mvc;

namespace AnimalAllies.Volunteer.Presentation;

public class VolunteerController: ApplicationController
{
    [Permission("volunteer.read")]
    [HttpGet]
    public async Task<ActionResult> Get(
        [FromQuery] GetFilteredVolunteersWithPaginationRequest request,
        [FromServices] GetFilteredVolunteersWithPaginationHandler handler,
        CancellationToken cancellationToken = default)
    {
        var query = request.ToQuery();

        var result = await handler.Handle(query, cancellationToken);

        if (result.IsFailure)
            result.Errors.ToResponse();

        return Ok(result);
    }
    
    [Permission("volunteer.read")]
    [HttpGet("{volunteerId:guid}")]
    public async Task<ActionResult> GetById(
        [FromRoute] Guid volunteerId,
        [FromServices] GetVolunteerByIdHandler handler,
        CancellationToken cancellationToken = default)
    {
        var query = new GetVolunteerByIdQuery(volunteerId);

        var result = await handler.Handle(query, cancellationToken);

        if (result.IsFailure)
            result.Errors.ToResponse();

        return Ok(result);
    }
    
    [Permission("volunteer.read")]
    [HttpGet("dapper")]
    public async Task<ActionResult> GetDapper(
        [FromQuery] GetFilteredVolunteersWithPaginationRequest request,
        [FromServices] GetFilteredVolunteersWithPaginationHandlerDapper handler,
        CancellationToken cancellationToken = default)
    {
        var query = request.ToQuery();

        var result = await handler.Handle(query, cancellationToken);

        if (result.IsFailure)
            result.Errors.ToResponse();

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
        var query = request.ToQuery(volunteerId);

        var result = await handler.Handle(query, cancellationToken);

        if (result.IsFailure)
            result.Errors.ToResponse();

        return Ok(result);
    }
    
    [Permission("volunteer.read")]
    [HttpGet("{speciesId:guid}/pet-by-species-id")]
    public async Task<ActionResult> GetPetsBySpeciesIdDapper(
        [FromBody] GetPetsBySpeciesIdRequest request,
        [FromServices] GetPetsBySpeciesIdHandler handler,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPetsBySpeciesIdQuery(request.SpeciesId, request.Page, request.PageSize);

        var result = await handler.Handle(query, cancellationToken);

        if (result.IsFailure)
            result.Errors.ToResponse();

        return Ok(result);
    }
    
    [Permission("volunteer.read")]
    [HttpGet("{speciesId:guid}/pet-by-species-id")]
    public async Task<ActionResult> GetPetsByBreedIdDapper(
        [FromBody] GetPetsByBreedIdRequest request,
        [FromServices] GetPetsByBreedIdHandler handler,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPetsByBreedIdQuery(request.BreedId, request.Page, request.PageSize);

        var result = await handler.Handle(query, cancellationToken);

        if (result.IsFailure)
            result.Errors.ToResponse();

        return Ok(result);
    }
    
    [Permission("volunteer.read")]
    [HttpGet("{petId:guid}/pet-by-id-dapper")]
    public async Task<ActionResult> GetPetByIdDapper(
        [FromRoute] Guid petId,
        [FromServices] GetPetByIdHandler handler,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPetByIdQuery(petId);

        var result = await handler.Handle(query, cancellationToken);

        if (result.IsFailure)
            result.Errors.ToResponse();

        return Ok(result);
    }
    
    [Permission("volunteer.create")]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromServices] CreateVolunteerHandler handler,
        [FromBody] CreateVolunteerRequest request,
        CancellationToken cancellationToken = default)
    {

        var command = request.ToCommand();
        
        var result = await handler.Handle(command, cancellationToken);
        
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
        var command = request.ToCommand(id);
        
        var response = await handler.Handle(command, cancellationToken);
        
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
        var command = request.ToCommand(id);
        
        var response = await handler.Handle(command, cancellationToken);
        
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
        var command = new DeleteVolunteerCommand(id);
        
        var response = await handler.Handle(command, cancellationToken);
        
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
        var command = request.ToCommand(id);

        var result = await handler.Handle(command, cancellationToken);

        if (result.IsFailure)
            return result.Errors.ToResponse();

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
        var command = request.ToCommand(volunteerId, petId);

        var result = await handler.Handle(command, cancellationToken);

        if (result.IsFailure)
            return result.Errors.ToResponse();
        
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
        var command = request.ToCommand(volunteerId, petId);

        var result = await handler.Handle(command, cancellationToken);

        if (result.IsFailure)
            return result.Errors.ToResponse();
        
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
        var command = request.ToCommand(volunteerId, petId);

        var result = await handler.Handle(command, cancellationToken);

        if (result.IsFailure)
            return result.Errors.ToResponse();

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
        var command = new DeletePetPhotosCommand(volunteerId, petId, request.FilePaths);

        var result = await handler.Handle(command, cancellationToken);

        if (result.IsFailure)
            return result.Errors.ToResponse();
        
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
        var command = request.ToCommand(volunteerId, petId);

        var result = await handler.Handle(command, cancellationToken);

        if (result.IsFailure)
            return result.Errors.ToResponse();

        return Ok(result.Value);
    }
    
    //TODO: перенести все реквесты в Contract и убрать ToCommand
    
    [Permission("volunteer.restore")]
    [HttpPut("{volunteerId:guid}/volunteer-recovery")]
    public async Task<ActionResult> VolunteerRestore(
        [FromRoute] Guid volunteerId,
        [FromServices] RestoreVolunteerHandler handler,
        CancellationToken cancellationToken = default)
    {
        var command = new RestoreVolunteerCommand(volunteerId);

        var result = await handler.Handle(command, cancellationToken);

        if (result.IsFailure)
            return result.Errors.ToResponse();

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
        var command = new RestorePetCommand(volunteerId, petId);

        var result = await handler.Handle(command, cancellationToken);

        if (result.IsFailure)
            return result.Errors.ToResponse();

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
        var command = request.ToCommand(volunteerId, petId);

        var result = await handler.Handle(command, cancellationToken);

        if (result.IsFailure)
            return result.Errors.ToResponse();

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
        var command = new DeletePetSoftCommand(volunteerId, petId);

        var result = await handler.Handle(command, cancellationToken);

        if (result.IsFailure)
            return result.Errors.ToResponse();

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
        var command = new DeletePetForceCommand(volunteerId, petId);

        var result = await handler.Handle(command, cancellationToken);

        if (result.IsFailure)
            return result.Errors.ToResponse();

        return Ok(result.Value);
    }
}
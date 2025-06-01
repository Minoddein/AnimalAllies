using AnimalAllies.Framework;
using AnimalAllies.Framework.Authorization;
using AnimalAllies.Species.Application.SpeciesManagement.Commands.CreateBreed;
using AnimalAllies.Species.Application.SpeciesManagement.Commands.CreateSpecies;
using AnimalAllies.Species.Application.SpeciesManagement.Commands.DeleteBreed;
using AnimalAllies.Species.Application.SpeciesManagement.Commands.DeleteSpecies;
using AnimalAllies.Species.Application.SpeciesManagement.Queries.GetAllSpeciesWithBreeds;
using AnimalAllies.Species.Application.SpeciesManagement.Queries.GetBreedsBySpeciesId;
using AnimalAllies.Species.Application.SpeciesManagement.Queries.GetSpeciesWithPagination;
using AnimalAllies.Species.Presentation.Requests;
using Microsoft.AspNetCore.Mvc;

namespace AnimalAllies.Species.Presentation;

public class SpeciesController : ApplicationController
{
    [Permission("species.create")]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromServices] CreateSpeciesHandler handler,
        [FromBody] CreateSpeciesRequest request,
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

    [Permission("species.create")]
    [HttpPost("{speciesId:guid}")]
    public async Task<IActionResult> CreateBreed(
        [FromServices] CreateBreedHandler handler,
        [FromRoute] Guid speciesId,
        [FromBody] CreateBreedRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = request.ToCommand(speciesId);

        var result = await handler.Handle(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Errors.ToResponse();
        }

        return Ok(result.Value);
    }

    [Permission("species.delete")]
    [HttpDelete("{speciesId:guid}")]
    public async Task<IActionResult> DeleteSpecies(
        [FromServices] DeleteSpeciesHandler handler,
        [FromRoute] Guid speciesId,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteSpeciesCommand(speciesId);

        var result = await handler.Handle(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Errors.ToResponse();
        }

        return Ok(result.Value);
    }

    [Permission("species.read")]
    [HttpGet("all-species-with-breeds")]
    public async Task<IActionResult> GetSpeciesWithBreeds(
        [FromServices] GetAllSpeciesWithBreedsHandler handler,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllSpeciesWithBreedsQuery();

        var result = await handler.Handle(query, cancellationToken);

        if (result.IsFailure)
        {
            return result.Errors.ToResponse();
        }

        return Ok(result);
    }

    [Permission("species.delete")]
    [HttpDelete("{speciesId:guid}/{breedId:guid}")]
    public async Task<IActionResult> DeleteBreed(
        [FromServices] DeleteBreedHandler handler,
        [FromRoute] Guid speciesId,
        [FromRoute] Guid breedId,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteBreedCommand(speciesId, breedId);

        var result = await handler.Handle(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Errors.ToResponse();
        }

        return Ok(result.Value);
    }

    [Permission("species.read")]
    [HttpGet]
    public async Task<IActionResult> GetSpecies(
        [FromServices] GetSpeciesWithPaginationHandlerDapper handler,
        [FromQuery] GetSpeciesWithPaginationRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = request.ToQuery();

        var result = await handler.Handle(query, cancellationToken);

        if (result.IsFailure)
        {
            return result.Errors.ToResponse();
        }

        return Ok(result);
    }

    [Permission("species.read")]
    [HttpGet("{speciesId:guid}")]
    public async Task<IActionResult> GetBreeds(
        [FromServices] GetBreedsBySpeciesIdWithPaginationHandlerDapper handler,
        [FromRoute] Guid speciesId,
        [FromQuery] GetBreedsBySpeciesIdWithPaginationRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = request.ToQuery(speciesId);

        var result = await handler.Handle(query, cancellationToken);

        if (result.IsFailure)
        {
            return result.Errors.ToResponse();
        }

        return Ok(result);
    }
}
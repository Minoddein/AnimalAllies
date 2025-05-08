using AnimalAllies.Core.DTOs;
using AnimalAllies.Core.Models;
using AnimalAllies.Framework;
using AnimalAllies.Framework.Authorization;
using AnimalAllies.SharedKernel.Shared;
using AnimalAllies.SharedKernel.Shared.Ids;
using AnimalAllies.Species.Application.SpeciesManagement.Commands.CreateBreed;
using AnimalAllies.Species.Application.SpeciesManagement.Commands.CreateSpecies;
using AnimalAllies.Species.Application.SpeciesManagement.Commands.DeleteBreed;
using AnimalAllies.Species.Application.SpeciesManagement.Commands.DeleteSpecies;
using AnimalAllies.Species.Application.SpeciesManagement.Queries.GetBreedsBySpeciesId;
using AnimalAllies.Species.Application.SpeciesManagement.Queries.GetSpeciesWithPagination;
using AnimalAllies.Species.Presentation.Requests;
using Microsoft.AspNetCore.Mvc;

namespace AnimalAllies.Species.Presentation;

public class SpeciesController : ApplicationController
{
    // [Permission("species.create")]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromServices] CreateSpeciesHandler handler,
        [FromBody] CreateSpeciesRequest request,
        CancellationToken cancellationToken = default)
    {
        CreateSpeciesCommand command = request.ToCommand();

        Result<SpeciesId> result = await handler.Handle(command, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return result.Errors.ToResponse();
        }

        return Ok(result.Value);
    }

    // [Permission("species.create")]
    [HttpPost("{speciesId:guid}")]
    public async Task<IActionResult> CreateBreed(
        [FromServices] CreateBreedHandler handler,
        [FromRoute] Guid speciesId,
        [FromBody] CreateBreedRequest request,
        CancellationToken cancellationToken = default)
    {
        CreateBreedCommand command = request.ToCommand(speciesId);

        Result<BreedId> result = await handler.Handle(command, cancellationToken).ConfigureAwait(false);

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
        DeleteSpeciesCommand command = new(speciesId);

        Result<SpeciesId> result = await handler.Handle(command, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return result.Errors.ToResponse();
        }

        return Ok(result.Value);
    }

    [Permission("species.delete")]
    [HttpDelete("{speciesId:guid}/{breedId:guid}")]
    public async Task<IActionResult> DeleteBreed(
        [FromServices] DeleteBreedHandler handler,
        [FromRoute] Guid speciesId,
        [FromRoute] Guid breedId,
        CancellationToken cancellationToken = default)
    {
        DeleteBreedCommand command = new(speciesId, breedId);

        Result<BreedId> result = await handler.Handle(command, cancellationToken).ConfigureAwait(false);

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
        GetSpeciesWithPaginationQuery query = request.ToQuery();

        Result<PagedList<SpeciesDto>> result = await handler.Handle(query, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return result.Errors.ToResponse();
        }

        return Ok(result.Value);
    }

    [Permission("species.read")]
    [HttpGet("{speciesId:guid}")]
    public async Task<IActionResult> GetBreeds(
        [FromServices] GetBreedsBySpeciesIdWithPaginationHandlerDapper handler,
        [FromRoute] Guid speciesId,
        [FromQuery] GetBreedsBySpeciesIdWithPaginationRequest request,
        CancellationToken cancellationToken = default)
    {
        GetBreedsBySpeciesIdWithPaginationQuery query = request.ToQuery(speciesId);

        Result<PagedList<BreedDto>> result = await handler.Handle(query, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return result.Errors.ToResponse();
        }

        return Ok(result.Value);
    }
}
using AnimalAllies.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace AnimalAllies.Framework;

[ApiController]
[Route("api/[controller]")]
public abstract class ApplicationController : ControllerBase
{
    public override OkObjectResult Ok(object? value)
    {
        Envelope envelope = Envelope.Ok(value);

        return base.Ok(envelope);
    }
}
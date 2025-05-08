using AnimalAllies.Core.DTOs.ValueObjects;
using Microsoft.AspNetCore.Http;

namespace AnimalAllies.Framework.Processors;

public class FormFileProcessor : IAsyncDisposable
{
    private readonly List<CreateFileDto> _fileDtos = [];

    public async ValueTask DisposeAsync()
    {
        foreach (CreateFileDto file in _fileDtos)
        {
            await file.Content.DisposeAsync().ConfigureAwait(false);
        }
    }

    public List<CreateFileDto> Process(IFormFileCollection files)
    {
        foreach (IFormFile file in files)
        {
            Stream stream = file.OpenReadStream();
            CreateFileDto fileDto = new(stream, file.FileName);
            _fileDtos.Add(fileDto);
        }

        return _fileDtos;
    }
}
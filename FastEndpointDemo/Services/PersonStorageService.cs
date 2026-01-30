using FastEndpointDemo.Services.Interfaces;
using FastEndpointDemo.Services.Models;
using Microsoft.Extensions.Caching.Memory;

namespace FastEndpointDemo.Services;

public interface IPersonStorageService : IStorageService<PersonModel>
{
}
public class PersonStorageService(IMemoryCache cache) : BaseStorageService<PersonModel>(cache), IPersonStorageService
{
    protected override string Name { get; } = "Person";
}
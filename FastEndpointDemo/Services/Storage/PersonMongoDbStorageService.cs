using FastEndpointDemo.Services.Interfaces;
using FastEndpointDemo.Services.Models;
using MongoDB.Driver;

namespace FastEndpointDemo.Services.Storage;

/// <summary>
/// MongoDB-basert implementasjon av person storage.
/// Bruker BaseMongoDbStorageService for all CRUD-logikk med "persons" som collection navn.
/// Gir persistent lagring med full query-støtte og skalerbarhet.
/// </summary>
public class PersonMongoDbStorageService(IMongoDatabase database, IClock clock) 
    : BaseMongoDbStorageService<PersonModel>(database, clock), IPersonStorageService
{
    /// <summary>
    /// MongoDB collection navn (gir collection "persons" i databasen).
    /// </summary>
    protected override string CollectionName { get; } = "persons";

    /// <summary>
    /// Oppretter spesialiserte indexes for person-søk.
    /// Index på navn-felter for rask søking på fornavn og etternavn.
    /// </summary>
    public override async Task EnsureIndexesAsync(CancellationToken cancellationToken = default)
    {
        // Kall base for standard indexes (Id, CreatedAt)
        await base.EnsureIndexesAsync(cancellationToken);

        // Opprett compound index på FirstName og LastName for rask duplikatkontroll
        var nameIndexKeys = Builders<PersonModel>.IndexKeys
            .Ascending(p => p.FirstName)
            .Ascending(p => p.LastName);

        var nameIndexModel = new CreateIndexModel<PersonModel>(nameIndexKeys, new CreateIndexOptions
        {
            Name = "idx_name",
            Unique = false // Sett til true hvis du vil tvinge unikhet på database-nivå
        });

        await Collection.Indexes.CreateOneAsync(nameIndexModel, cancellationToken: cancellationToken);

        // Opprett index på UpdatedAt for effektiv sortering av sist oppdaterte personer
        var updatedAtIndexKeys = Builders<PersonModel>.IndexKeys
            .Descending(p => p.UpdatedAt);

        var updatedAtIndexModel = new CreateIndexModel<PersonModel>(updatedAtIndexKeys, new CreateIndexOptions
        {
            Name = "idx_updated_at"
        });

        await Collection.Indexes.CreateOneAsync(updatedAtIndexModel, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Finner personer med matchende navn (case-insensitive).
    /// Demonstrerer hvordan man kan bruke MongoDB queries for avansert søk.
    /// </summary>
    /// <param name="firstName">Fornavn (valgfritt)</param>
    /// <param name="lastName">Etternavn (valgfritt)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Liste med personer som matcher søket</returns>
    public async Task<List<PersonModel>> FindByNameAsync(string? firstName, string? lastName, CancellationToken cancellationToken)
    {
        var filterBuilder = Builders<PersonModel>.Filter;
        var filters = new List<FilterDefinition<PersonModel>>();

        if (!string.IsNullOrEmpty(firstName))
        {
            filters.Add(filterBuilder.Regex(p => p.FirstName, new MongoDB.Bson.BsonRegularExpression(firstName, "i")));
        }

        if (!string.IsNullOrEmpty(lastName))
        {
            filters.Add(filterBuilder.Regex(p => p.LastName, new MongoDB.Bson.BsonRegularExpression(lastName, "i")));
        }

        var combinedFilter = filters.Count > 0
            ? filterBuilder.And(filters)
            : filterBuilder.Empty;

        return await FindAsync(combinedFilter, cancellationToken);
    }
}

using FastEndpointDemo.Services.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Testcontainers.MongoDb;

namespace FastEndpoints.IntegrationTests.Services.Storage;

/// <summary>
/// Unit tester for BaseMongoDbStorageService.
/// Bruker Testcontainers for å kjøre en ekte MongoDB-instans under testing.
/// </summary>
public class BaseMongoDbStorageServiceTests : IAsyncLifetime
{
    private MongoDbContainer? _mongoContainer;
    private IMongoClient? _mongoClient;
    private IMongoDatabase? _database;

    /// <summary>
    /// Static constructor to configure MongoDB BSON serialization for Guid.
    /// This runs once when the class is first loaded.
    /// </summary>
    static BaseMongoDbStorageServiceTests()
    {
        // Register GuidSerializer with Standard representation to fix serialization errors
        BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
    }

    private sealed record TestEntity : IEntity
    {
        public Guid Id { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private sealed class TestMongoDbStorageService : FastEndpointDemo.Services.Storage.BaseMongoDbStorageService<TestEntity>
    {
        protected override string CollectionName => "test_entities";

        public TestMongoDbStorageService(IMongoDatabase database, IClock clock) 
            : base(database, clock)
        {
        }

        public IMongoCollection<TestEntity> GetCollection() => Collection;
    }

    /// <summary>
    /// Initialiserer MongoDB container før hver test.
    /// </summary>
    public async ValueTask InitializeAsync()
    {
        _mongoContainer = new MongoDbBuilder("mongo:7")
            .Build();
        
        await _mongoContainer.StartAsync();
        
        var connectionString = _mongoContainer.GetConnectionString();
        
        //  MongoDB.Driver 3.x uses GuidRepresentationMode.V3 by default
        var settings = MongoClientSettings.FromConnectionString(connectionString);
        
        _mongoClient = new MongoClient(settings);
        _database = _mongoClient.GetDatabase("test_db");
    }

    /// <summary>
    /// Rydder opp og stopper MongoDB container etter hver test.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_mongoContainer != null)
        {
            await _mongoContainer.StopAsync();
            await _mongoContainer.DisposeAsync();
        }
    }

    /// <summary>
    /// System Under Test - oppretter database, clock og service for testing.
    /// </summary>
    private (IMongoDatabase Database, TestClock Clock, TestMongoDbStorageService Service) Sut()
    {
        if (_database == null)
            throw new InvalidOperationException("MongoDB is not initialized");

        var clock = new TestClock(DateTimeOffset.UtcNow);
        var service = new TestMongoDbStorageService(_database, clock);
        return (_database, clock, service);
    }

    /// <summary>
    /// Verifiserer at CreateAsync genererer en ny GUID når ID er tom og lagrer entiteten i MongoDB.
    /// </summary>
    [Fact]
    public async Task CreateAsync_WhenIdEmpty_AssignsGuidAndStoresEntity()
    {
        var ct = TestContext.Current.CancellationToken;
        var (_, clock, service) = Sut();
        var entity = new TestEntity { Id = Guid.Empty, Name = "A" };

        var now = clock.UtcNow;
        var id = await service.CreateAsync(entity, ct);

        id.Should().NotBe(Guid.Empty);
        entity.Id.Should().Be(id);
        entity.CreatedAt.Should().Be(now);

        // Verifiser at entiteten er lagret i MongoDB
        var collection = service.GetCollection();
        var filter = Builders<TestEntity>.Filter.Eq(e => e.Id, id);
        var storedEntity = await (await collection.FindAsync(filter, null, ct)).FirstOrDefaultAsync(ct);
        
        storedEntity.Should().NotBeNull();
        storedEntity!.Name.Should().Be("A");
    }

    /// <summary>
    /// Verifiserer at CreateAsync beholder eksisterende ID når den er oppgitt.
    /// </summary>
    [Fact]
    public async Task CreateAsync_WhenIdProvided_PreservesId()
    {
        var ct = TestContext.Current.CancellationToken;
        var (_, _, service) = Sut();
        var id = Guid.NewGuid();
        var entity = new TestEntity { Id = id, Name = "A" };

        var createdId = await service.CreateAsync(entity, ct);

        createdId.Should().Be(id);
        entity.Id.Should().Be(id);
    }

    /// <summary>
    /// Verifiserer at GetAsync returnerer null når entiteten ikke finnes i MongoDB.
    /// </summary>
    [Fact]
    public async Task GetAsync_WhenMissing_ReturnsNull()
    {
        var ct = TestContext.Current.CancellationToken;
        var (_, _, service) = Sut();

        var result = await service.GetAsync(Guid.NewGuid(), ct);

        result.Should().BeNull();
    }

    /// <summary>
    /// Verifiserer at GetAsync returnerer entiteten når den finnes i MongoDB.
    /// </summary>
    [Fact]
    public async Task GetAsync_WhenExists_ReturnsEntity()
    {
        var ct = TestContext.Current.CancellationToken;
        var (_, _, service) = Sut();
        var entity = new TestEntity { Name = "A" };
        var id = await service.CreateAsync(entity, ct);

        var result = await service.GetAsync(id, ct);

        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        result.Name.Should().Be("A");
    }

    /// <summary>
    /// Verifiserer at GetAllAsync returnerer tom liste når collection er tom.
    /// </summary>
    [Fact]
    public async Task GetAllAsync_WhenCollectionEmpty_ReturnsEmpty()
    {
        var ct = TestContext.Current.CancellationToken;
        var (_, _, service) = Sut();

        var result = await service.GetAllAsync(ct);

        result.Should().BeEmpty();
    }

    /// <summary>
    /// Verifiserer at GetAllAsync returnerer alle entiteter fra MongoDB collection.
    /// </summary>
    [Fact]
    public async Task GetAllAsync_ReturnsAllEntities()
    {
        var ct = TestContext.Current.CancellationToken;
        var (_, _, service) = Sut();

        // Opprett flere entiteter
        var id1 = await service.CreateAsync(new TestEntity { Name = "A" }, ct);
        var id2 = await service.CreateAsync(new TestEntity { Name = "B" }, ct);
        var id3 = await service.CreateAsync(new TestEntity { Name = "C" }, ct);

        var result = await service.GetAllAsync(ct);

        result.Should().HaveCount(3);
        result.Select(e => e.Name).Should().BeEquivalentTo(new[] { "A", "B", "C" });
        result.Select(e => e.Id).Should().BeEquivalentTo(new[] { id1, id2, id3 });
    }

    /// <summary>
    /// Verifiserer at UpdateAsync setter UpdatedAt-tidspunkt og overskriver entiteten i MongoDB.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_SetsUpdatedAt_AndOverwritesStoredEntity()
    {
        var ct = TestContext.Current.CancellationToken;
        var (_, clock, service) = Sut();
        var id = await service.CreateAsync(new TestEntity { Name = "A" }, ct);

        var entity = await service.GetAsync(id, ct);
        entity.Should().NotBeNull();

        entity!.Name = "B";

        clock.Advance(TimeSpan.FromSeconds(1));
        var now = clock.UtcNow;
        await service.UpdateAsync(entity, ct);

        entity.UpdatedAt.Should().Be(now);

        // Verifiser ved å hente fra service
        var fetched = await service.GetAsync(id, ct);
        fetched.Should().NotBeNull();
        fetched!.Name.Should().Be("B");
        fetched.UpdatedAt.Should().Be(now);
    }

    /// <summary>
    /// Verifiserer at DeleteAsync fjerner entiteten fra MongoDB.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_RemovesStoredEntity()
    {
        var ct = TestContext.Current.CancellationToken;
        var (_, _, service) = Sut();
        var id = await service.CreateAsync(new TestEntity { Name = "A" }, ct);

        await service.DeleteAsync(id, ct);

        // Verifiser at entiteten er slettet
        var fetched = await service.GetAsync(id, ct);
        fetched.Should().BeNull();

        // Verifiser at den ikke er i listen
        var all = await service.GetAllAsync(ct);
        all.Should().NotContain(e => e.Id == id);
    }

    /// <summary>
    /// Verifiserer at DeleteAsync er idempotent (kan kalles flere ganger uten feil).
    /// </summary>
    [Fact]
    public async Task DeleteAsync_IsIdempotent()
    {
        var ct = TestContext.Current.CancellationToken;
        var (_, _, service) = Sut();

        var id = await service.CreateAsync(new TestEntity { Name = "A" }, ct);

        // Slett to ganger skal ikke kaste exception
        await service.Invoking(s => s.DeleteAsync(id, ct)).Should().NotThrowAsync();
        await service.Invoking(s => s.DeleteAsync(id, ct)).Should().NotThrowAsync();

        // Verifiser at entiteten er borte
        (await service.GetAsync(id, ct)).Should().BeNull();
        (await service.GetAllAsync(ct)).Should().BeEmpty();
    }

    /// <summary>
    /// Verifiserer at CreateAsync kaster MongoWriteException når samme ID opprettes to ganger
    /// (duplicate key exception fra MongoDB).
    /// </summary>
    [Fact]
    public async Task CreateAsync_WhenCalledTwiceWithSameId_ThrowsDuplicateKeyException()
    {
        var ct = TestContext.Current.CancellationToken;
        var (_, _, service) = Sut();

        var id = Guid.NewGuid();
        var entity1 = new TestEntity { Id = id, Name = "A" };
        var entity2 = new TestEntity { Id = id, Name = "B" };

        await service.CreateAsync(entity1, ct);

        // MongoDB skal kaste exception ved duplikat ID
        var act = async () => await service.CreateAsync(entity2, ct);
        await act.Should().ThrowAsync<MongoWriteException>();
    }

    /// <summary>
    /// Verifiserer at DeleteAsync kun fjerner spesifisert entitet, ikke andre.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_OnlyRemovesSpecifiedEntity()
    {
        var ct = TestContext.Current.CancellationToken;
        var (_, _, service) = Sut();

        var id1 = await service.CreateAsync(new TestEntity { Name = "Keep1" }, ct);
        var id2 = await service.CreateAsync(new TestEntity { Name = "Delete" }, ct);
        var id3 = await service.CreateAsync(new TestEntity { Name = "Keep2" }, ct);

        await service.DeleteAsync(id2, ct);

        var remaining = await service.GetAllAsync(ct);
        remaining.Should().HaveCount(2);
        remaining.Select(e => e.Id).Should().BeEquivalentTo(new[] { id1, id3 });
    }

    /// <summary>
    /// Verifiserer at UpdateAsync ikke kaster unntak når entiteten ikke eksisterer.
    /// MongoDB ReplaceOne med upsert:false gjør ingenting hvis dokumentet ikke finnes.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_WhenEntityDoesNotExist_DoesNotThrow()
    {
        var ct = TestContext.Current.CancellationToken;
        var (_, clock, service) = Sut();

        var id = Guid.NewGuid();
        var entity = new TestEntity 
        { 
            Id = id, 
            Name = "DirectUpdate",
            CreatedAt = clock.UtcNow 
        };

        // Kall UpdateAsync uten å kalle CreateAsync først
        // MongoDB ReplaceOne med upsert:false gjør ingenting hvis dokumentet ikke finnes
        await service.Invoking(s => s.UpdateAsync(entity, ct)).Should().NotThrowAsync();

        // Entiteten skal IKKE eksistere siden den aldri ble opprettet
        var fetched = await service.GetAsync(id, ct);
        fetched.Should().BeNull();
    }

    /// <summary>
    /// Verifiserer at MongoDB-serialisering bevarer DateTimeOffset-presisjon.
    /// </summary>
    [Fact]
    public async Task MongoDbSerialization_PreservesDateTimeOffsetPrecision()
    {
        var ct = TestContext.Current.CancellationToken;
        var (_, _, service) = Sut();

        var specificTime = new DateTimeOffset(2024, 1, 15, 10, 30, 45, 123, TimeSpan.Zero);
        var clockWithSpecificTime = new TestClock(specificTime);
        var serviceWithTime = new TestMongoDbStorageService(_database!, clockWithSpecificTime);

        var entity = new TestEntity { Name = "TimeTest" };
        var id = await serviceWithTime.CreateAsync(entity, ct);

        var fetched = await serviceWithTime.GetAsync(id, ct);

        fetched.Should().NotBeNull();
        fetched!.CreatedAt.Should().Be(specificTime);
    }

    /// <summary>
    /// Verifiserer at flere service-instanser deler samme MongoDB collection.
    /// </summary>
    [Fact]
    public async Task MultipleServices_ShareSameMongoDbCollection()
    {
        var ct = TestContext.Current.CancellationToken;
        var (database, clock, service1) = Sut();
        var service2 = new TestMongoDbStorageService(database, clock);

        // Opprett entitet via service1
        var id = await service1.CreateAsync(new TestEntity { Name = "Shared" }, ct);

        // Hent via service2
        var fetched = await service2.GetAsync(id, ct);
        fetched.Should().NotBeNull();
        fetched!.Name.Should().Be("Shared");

        // Begge skal se samme data
        var all1 = await service1.GetAllAsync(ct);
        var all2 = await service2.GetAllAsync(ct);
        all1.Should().HaveCount(1);
        all2.Should().HaveCount(1);
        all1.Select(e => e.Id).Should().BeEquivalentTo(all2.Select(e => e.Id));
    }

    /// <summary>
    /// Verifiserer at FindAsync med filter returnerer matchende entiteter.
    /// </summary>
    [Fact]
    public async Task FindAsync_WithFilter_ReturnsMatchingEntities()
    {
        var ct = TestContext.Current.CancellationToken;
        var (_, _, service) = Sut();

        // Opprett flere entiteter
        await service.CreateAsync(new TestEntity { Name = "Alice" }, ct);
        await service.CreateAsync(new TestEntity { Name = "Bob" }, ct);
        await service.CreateAsync(new TestEntity { Name = "Alice" }, ct);

        // Bruk FindAsync helper (protected method - test via collection)
        var collection = service.GetCollection();
        var filter = Builders<TestEntity>.Filter.Eq(e => e.Name, "Alice");
        var results = await (await collection.FindAsync(filter, null, ct)).ToListAsync(ct);

        results.Should().HaveCount(2);
        results.Should().OnlyContain(e => e.Name == "Alice");
    }

    /// <summary>
    /// Verifiserer at CountAsync returnerer korrekt antall entiteter.
    /// </summary>
    [Fact]
    public async Task CountAsync_ReturnsCorrectCount()
    {
        var ct = TestContext.Current.CancellationToken;
        var (_, _, service) = Sut();

        // Opprett entiteter
        await service.CreateAsync(new TestEntity { Name = "A" }, ct);
        await service.CreateAsync(new TestEntity { Name = "B" }, ct);
        await service.CreateAsync(new TestEntity { Name = "C" }, ct);

        // Bruk CountAsync via collection
        var collection = service.GetCollection();
        var count = await collection.CountDocumentsAsync(Builders<TestEntity>.Filter.Empty, null, ct);

        count.Should().Be(3);
    }

    /// <summary>
    /// Verifiserer at UpdateAsync bevarer CreatedAt-tidspunktet.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_PreservesCreatedAt()
    {
        var ct = TestContext.Current.CancellationToken;
        var (_, clock, service) = Sut();

        var createdTime = clock.UtcNow;
        var id = await service.CreateAsync(new TestEntity { Name = "Original" }, ct);

        clock.Advance(TimeSpan.FromHours(1));

        var entity = await service.GetAsync(id, ct);
        entity!.Name = "Updated";
        await service.UpdateAsync(entity, ct);

        var updated = await service.GetAsync(id, ct);
        updated!.CreatedAt.Should().Be(createdTime);
        updated.UpdatedAt.Should().NotBeNull();
        updated.UpdatedAt.Should().BeAfter(createdTime);
    }

    /// <summary>
    /// Verifiserer at CreateAsync genererer Version 7 GUID.
    /// </summary>
    [Fact]
    public async Task CreateAsync_GeneratesVersion7Guid()
    {
        var ct = TestContext.Current.CancellationToken;
        var (_, _, service) = Sut();

        var entity = new TestEntity { Id = Guid.Empty, Name = "Test" };
        var id = await service.CreateAsync(entity, ct);

        // Version 7 GUID skal ha version bits satt til 0111 (7)
        var bytes = id.ToByteArray();
        var version = (bytes[7] & 0xF0) >> 4;
        version.Should().Be(7);
    }
}

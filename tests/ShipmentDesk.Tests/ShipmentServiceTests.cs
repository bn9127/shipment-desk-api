using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ShipmentDesk.Api.Contracts;
using ShipmentDesk.Api.Data;
using ShipmentDesk.Api.Domain;
using ShipmentDesk.Api.Services;

namespace ShipmentDesk.Tests;

public sealed class ShipmentServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _db;
    private readonly ShipmentService _service;

    public ShipmentServiceTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        _db = new AppDbContext(options);
        _db.Database.EnsureCreated();
        _service = new ShipmentService(_db);
    }

    [Fact]
    public void Workflow_allows_only_the_next_status()
    {
        Assert.True(ShipmentWorkflow.CanAdvance(ShipmentStatus.Booked, ShipmentStatus.Accepted));
        Assert.False(ShipmentWorkflow.CanAdvance(ShipmentStatus.Booked, ShipmentStatus.Departed));
        Assert.False(ShipmentWorkflow.CanAdvance(ShipmentStatus.Delivered, ShipmentStatus.Arrived));
    }

    [Fact]
    public async Task Create_starts_at_booked_and_rejects_a_duplicate_reference()
    {
        var created = await _service.CreateAsync(Sample(), CancellationToken.None);

        Assert.Equal("SD1001", created.Reference);
        Assert.Equal("Booked", created.Status);
        Assert.Single(created.Milestones);

        var duplicate = await Assert.ThrowsAsync<ShipmentRuleException>(() =>
            _service.CreateAsync(Sample(), CancellationToken.None));

        Assert.Contains("already exists", duplicate.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Milestone_must_move_one_step_forward()
    {
        var created = await _service.CreateAsync(Sample(), CancellationToken.None);

        var skipped = await Assert.ThrowsAsync<ShipmentRuleException>(() =>
            _service.AddMilestoneAsync(created.Id, new AddMilestoneRequest
            {
                Status = "Departed",
                Location = "BOM"
            }, CancellationToken.None));

        Assert.Contains("Accepted", skipped.Message, StringComparison.Ordinal);

        var accepted = await _service.AddMilestoneAsync(created.Id, new AddMilestoneRequest
        {
            Status = "Accepted",
            Location = "BOM",
            Note = "Accepted at origin"
        }, CancellationToken.None);

        Assert.Equal("Accepted", accepted.Status);
        Assert.Equal(2, accepted.Milestones.Count);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    private static CreateShipmentRequest Sample() => new()
    {
        Reference = "SD1001",
        Origin = "BOM",
        Destination = "FRA",
        Shipper = "Northwind Traders"
    };
}

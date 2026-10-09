using Microsoft.EntityFrameworkCore;
using ShipmentDesk.Api.Contracts;
using ShipmentDesk.Api.Data;
using ShipmentDesk.Api.Domain;

namespace ShipmentDesk.Api.Services;

public sealed class ShipmentService
{
    private readonly AppDbContext _db;

    public ShipmentService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ShipmentResponse> CreateAsync(CreateShipmentRequest request, CancellationToken cancellationToken)
    {
        var reference = request.Reference.Trim().ToUpperInvariant();
        var origin = request.Origin.Trim().ToUpperInvariant();
        var destination = request.Destination.Trim().ToUpperInvariant();

        if (origin == destination)
        {
            throw new ShipmentRuleException("Origin and destination must differ.");
        }

        var exists = await _db.Shipments.AnyAsync(shipment => shipment.Reference == reference, cancellationToken);
        if (exists)
        {
            throw new ShipmentRuleException($"Reference '{reference}' already exists.");
        }

        var now = DateTimeOffset.UtcNow;
        var shipment = new Shipment
        {
            Reference = reference,
            Origin = origin,
            Destination = destination,
            Shipper = request.Shipper.Trim(),
            Status = ShipmentStatus.Booked,
            CreatedAt = now,
            Milestones =
            [
                new Milestone
                {
                    Status = ShipmentStatus.Booked,
                    Location = origin,
                    Note = "Shipment booked",
                    OccurredAt = now
                }
            ]
        };

        _db.Shipments.Add(shipment);
        await _db.SaveChangesAsync(cancellationToken);
        return ToResponse(shipment);
    }

    public async Task<IReadOnlyList<ShipmentResponse>> ListAsync(string? status, CancellationToken cancellationToken)
    {
        var query = _db.Shipments.AsNoTracking().Include(shipment => shipment.Milestones).AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<ShipmentStatus>(status, ignoreCase: true, out var parsed))
            {
                throw new ShipmentRuleException($"Unknown status '{status}'.");
            }

            query = query.Where(shipment => shipment.Status == parsed);
        }

        var shipments = await query
            .OrderByDescending(shipment => shipment.CreatedAt)
            .ToListAsync(cancellationToken);

        return shipments.Select(ToResponse).ToList();
    }

    public async Task<ShipmentResponse?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var shipment = await _db.Shipments
            .AsNoTracking()
            .Include(item => item.Milestones)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        return shipment is null ? null : ToResponse(shipment);
    }

    public async Task<ShipmentResponse> AddMilestoneAsync(Guid id, AddMilestoneRequest request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<ShipmentStatus>(request.Status, ignoreCase: true, out var nextStatus))
        {
            throw new ShipmentRuleException($"Unknown status '{request.Status}'.");
        }

        var shipment = await _db.Shipments
            .Include(item => item.Milestones)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (shipment is null)
        {
            throw new KeyNotFoundException($"Shipment '{id}' was not found.");
        }

        if (!ShipmentWorkflow.CanAdvance(shipment.Status, nextStatus))
        {
            throw new ShipmentRuleException(
                $"Cannot move from {shipment.Status} to {nextStatus}. Next status must be {ShipmentWorkflow.NextStatusName(shipment.Status)}.");
        }

        shipment.Status = nextStatus;
        shipment.Milestones.Add(new Milestone
        {
            Status = nextStatus,
            Location = request.Location.Trim(),
            Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
            OccurredAt = request.OccurredAt ?? DateTimeOffset.UtcNow
        });

        await _db.SaveChangesAsync(cancellationToken);
        return ToResponse(shipment);
    }

    private static ShipmentResponse ToResponse(Shipment shipment)
    {
        var milestones = shipment.Milestones
            .OrderBy(milestone => milestone.OccurredAt)
            .Select(milestone => new MilestoneResponse(
                milestone.Id,
                milestone.Status.ToString(),
                milestone.Location,
                milestone.Note,
                milestone.OccurredAt))
            .ToList();

        return new ShipmentResponse(
            shipment.Id,
            shipment.Reference,
            shipment.Origin,
            shipment.Destination,
            shipment.Shipper,
            shipment.Status.ToString(),
            shipment.CreatedAt,
            milestones);
    }
}

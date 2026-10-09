namespace ShipmentDesk.Api.Domain;

public sealed class Milestone
{
    public Guid Id { get; set; }

    public Guid ShipmentId { get; set; }

    public ShipmentStatus Status { get; set; }

    public string Location { get; set; } = string.Empty;

    public string? Note { get; set; }

    public DateTimeOffset OccurredAt { get; set; }

    public Shipment? Shipment { get; set; }
}

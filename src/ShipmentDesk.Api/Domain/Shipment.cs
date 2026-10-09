namespace ShipmentDesk.Api.Domain;

public sealed class Shipment
{
    public Guid Id { get; set; }

    public string Reference { get; set; } = string.Empty;

    public string Origin { get; set; } = string.Empty;

    public string Destination { get; set; } = string.Empty;

    public string Shipper { get; set; } = string.Empty;

    public ShipmentStatus Status { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public List<Milestone> Milestones { get; set; } = [];
}

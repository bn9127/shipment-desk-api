using System.ComponentModel.DataAnnotations;

namespace ShipmentDesk.Api.Contracts;

public sealed class CreateShipmentRequest
{
    [Required]
    [StringLength(20, MinimumLength = 4)]
    [RegularExpression("^[A-Za-z0-9-]+$", ErrorMessage = "Reference may contain letters, digits, and hyphens only.")]
    public string Reference { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^[A-Za-z]{3}$", ErrorMessage = "Origin must be a 3-letter code.")]
    public string Origin { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^[A-Za-z]{3}$", ErrorMessage = "Destination must be a 3-letter code.")]
    public string Destination { get; set; } = string.Empty;

    [Required]
    [StringLength(120, MinimumLength = 2)]
    public string Shipper { get; set; } = string.Empty;
}

public sealed class AddMilestoneRequest
{
    [Required]
    public string Status { get; set; } = string.Empty;

    [Required]
    [StringLength(80, MinimumLength = 2)]
    public string Location { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Note { get; set; }

    public DateTimeOffset? OccurredAt { get; set; }
}

public sealed record MilestoneResponse(
    Guid Id,
    string Status,
    string Location,
    string? Note,
    DateTimeOffset OccurredAt);

public sealed record ShipmentResponse(
    Guid Id,
    string Reference,
    string Origin,
    string Destination,
    string Shipper,
    string Status,
    DateTimeOffset CreatedAt,
    IReadOnlyList<MilestoneResponse> Milestones);

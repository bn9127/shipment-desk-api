namespace ShipmentDesk.Api.Domain;

public sealed class ShipmentRuleException : Exception
{
    public ShipmentRuleException(string message) : base(message)
    {
    }
}

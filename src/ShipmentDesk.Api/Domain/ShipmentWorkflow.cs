namespace ShipmentDesk.Api.Domain;

public static class ShipmentWorkflow
{
    public static readonly IReadOnlyList<ShipmentStatus> Order =
    [
        ShipmentStatus.Booked,
        ShipmentStatus.Accepted,
        ShipmentStatus.Departed,
        ShipmentStatus.Arrived,
        ShipmentStatus.Delivered
    ];

    public static bool CanAdvance(ShipmentStatus current, ShipmentStatus next)
    {
        var currentIndex = IndexOf(current);
        var nextIndex = IndexOf(next);
        return nextIndex == currentIndex + 1;
    }

    public static string NextStatusName(ShipmentStatus current)
    {
        var currentIndex = IndexOf(current);
        if (currentIndex < 0 || currentIndex == Order.Count - 1)
        {
            return "none";
        }

        return Order[currentIndex + 1].ToString();
    }

    private static int IndexOf(ShipmentStatus status)
    {
        for (var i = 0; i < Order.Count; i++)
        {
            if (Order[i] == status)
            {
                return i;
            }
        }

        return -1;
    }
}

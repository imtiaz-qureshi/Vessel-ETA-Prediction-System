namespace VesselETA.Infrastructure.Services;

public interface IDistanceCalculator
{
    double CalculateDistanceNauticalMiles(double lat1, double lon1, double lat2, double lon2);
}

public class DistanceCalculator : IDistanceCalculator
{
    private const double EarthRadiusKm = 6371.0;
    private const double KmToNauticalMiles = 0.539957;

    public double CalculateDistanceNauticalMiles(double lat1, double lon1, double lat2, double lon2)
    {
        // Haversine formula
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        var distanceKm = EarthRadiusKm * c;

        return distanceKm * KmToNauticalMiles;
    }

    private static double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }
}
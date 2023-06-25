using System;

namespace Ulift2._0.Helpers
{
    public class Distance
    {
        public static double CalculateDistance(dynamic point1, dynamic point2)
        {
            const double R = 6371e3; // meters
            double phi1 = point1.Lat * Math.PI / 180; // phi, Lambda in radians
            double phi2 = point2.lat * Math.PI / 180;
            double deltaPhi = (point2.lat - point1.Lat) * Math.PI / 180;
            double deltaLambda = (point2.lng - point1.Lng) * Math.PI / 180;

            double a = Math.Sin(deltaPhi / 2) * Math.Sin(deltaPhi / 2) +
                Math.Cos(phi1) * Math.Cos(phi2) *
                Math.Sin(deltaLambda / 2) * Math.Sin(deltaLambda / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            double d = R * c; // in meters
            return d;
        }
    }
}
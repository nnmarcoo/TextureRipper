using System;
using Point = System.Windows.Point;

namespace TextureRipper;

public static class Quad
{
    public static Point[] OrderPointsClockwise(Point[] points)
    {
        // Find the center point of the given points
        double cx = 0;
        double cy = 0;
        foreach (var p in points)
        {
            cx += p.X;
            cy += p.Y;
        }
        cx /= points.Length;
        cy /= points.Length;
        Point center = new Point(cx, cy);

        // Sort the points by their polar angle around the center
        Array.Sort(points, (a, b) =>
        {
            double angleA = Math.Atan2(a.Y - center.Y, a.X - center.X);
            double angleB = Math.Atan2(b.Y - center.Y, b.X - center.X);
            return angleA.CompareTo(angleB);
        });
        return points;
    }

    public static Point CalcRect(Point[] points) // calculate rectangle to map the points to
    {                                            // returns (width, height)
        Point topLeft = points[0];
        Point topRight = points[0];
        Point bottomLeft = points[0];
        Point bottomRight = points[0];

        const double tolerance = 0.0001;

        foreach (Point corner in points) {
            if (corner.X < topLeft.X - tolerance || (Math.Abs(corner.X - topLeft.X) < tolerance && corner.Y < topLeft.Y - tolerance)) {
                topLeft = corner;
            }
            if (corner.X > topRight.X + tolerance || (Math.Abs(corner.X - topRight.X) < tolerance && corner.Y < topRight.Y - tolerance)) {
                topRight = corner;
            }
            if (corner.X < bottomLeft.X - tolerance || (Math.Abs(corner.X - bottomLeft.X) < tolerance && corner.Y > bottomLeft.Y + tolerance)) {
                bottomLeft = corner;
            }
            if (corner.X > bottomRight.X + tolerance || (Math.Abs(corner.X - bottomRight.X) < tolerance && corner.Y > bottomRight.Y + tolerance)) {
                bottomRight = corner;
            }
        }
        
        var topLineLength = Math.Sqrt(Math.Pow(topRight.X - topLeft.X, 2) + Math.Pow(topRight.Y - topLeft.Y, 2));
        var bottomLineLength = Math.Sqrt(Math.Pow(bottomRight.X - bottomLeft.X, 2) + Math.Pow(bottomRight.Y - bottomLeft.Y, 2));
        
        var leftSideLength = Math.Sqrt(Math.Pow(topLeft.X - bottomLeft.X, 2) + Math.Pow(topLeft.Y - bottomLeft.Y, 2));
        var rightSideLength = Math.Sqrt(Math.Pow(topRight.X - bottomRight.X, 2) + Math.Pow(topRight.Y - bottomRight.Y, 2));

        bottomRight.X = topLineLength > bottomLineLength ? bottomLineLength : topLineLength; // overwrite bottomRight
        bottomRight.Y = leftSideLength > rightSideLength ? rightSideLength : leftSideLength; // to save memory

        return bottomRight;
    }

    public static double[][] CalcHomography(double[][] from, double[][] to)
    {



        return new double[1][];
    }
    
    public static double[][] CalcA(Point[] from, Point[] to)
    {



        return new double[1][];
    }
}
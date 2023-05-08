using System;
using System.Windows.Media.Imaging;
using Point = System.Windows.Point;

namespace TextureRipper;


/*todo
 1. calculate arbitrary rectangle to translate to
 
 */

public class Quad
{
    private Point[] _points;
    private BitmapImage _image;

    public Quad(Point[] points, BitmapImage image)
    {
        _points = OrderPointsClockwise(points[0], points[1], points[2], points[3]); //todo
        _image = image;
    }

    public static Point[] OrderPointsClockwise(Point p1, Point p2, Point p3, Point p4)
    {
        // Create an array of the input points
        Point[] points = { p1, p2, p3, p4 };

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
}
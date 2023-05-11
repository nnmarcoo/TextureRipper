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
    
    public static double[,] CalcHomography(Point[] s) // given Ax = B, solve for x
    {
        Point rectSize = CalcRect(s);
        Point[] d = {
            new Point(0,0),
            rectSize with { Y = 0 },
            new Point(rectSize.X,rectSize.Y),
            rectSize with { X = 0 }
        };

        double[,] A = CalcA(s, d);
        double[,] AInv = AInverse(A);
        double[,] B = CalcB(d);

        double[,] h = MatrixMultiply(AInv, B);

        return new double[,] // reshape h to a 3x3
        {
            {h[0,0],h[1,0],h[2,0]},
            {h[3,0],h[4,0],h[5,0]},
            {h[6,0],h[7,0],1},
        };
    }

    public static Point CalcRect(Point[] points) // calculate rectangle to map the points to
    {                                            // returns (width, height)
        var distances = new int[points.Length - 1];

        for (int i = 0; i < points.Length - 1; i++)
        {
            distances[i] = (int)Math.Sqrt(Math.Pow(points[i + 1].X - points[i].X, 2) + Math.Pow(points[i + 1].Y - points[i].Y, 2));
        }

        Array.Sort(distances);

        return new Point(distances[0], distances[1]);
    }

    private static double[,] CalcA(Point[] s, Point[] d) //https://web.archive.org/web/20100801071311/http://alumni.media.mit.edu/~cwren/interpolator/
    {                                                   // in this case, the top left of d will always be the top left of s
        return new double[,]
        {
            {s[0].X, s[0].Y, 1, 0, 0, 0, -d[0].X*s[0].X, -d[0].X*s[0].Y},
            {0, 0, 0, s[0].X, s[0].Y, 1, -d[0].Y*s[0].X, -d[0].Y*s[0].Y},
            
            {s[1].X, s[1].Y, 1, 0, 0, 0, -d[1].X*s[1].X, -d[1].X*s[1].Y},
            {0, 0, 0, s[1].X, s[1].Y, 1, -d[1].Y*s[1].X, -d[1].Y*s[1].Y},
            
            {s[2].X, s[2].Y, 1, 0, 0, 0, -d[2].X*s[2].X, -d[2].X*s[2].Y},
            {0, 0, 0, s[2].X, s[2].Y, 1, -d[2].Y*s[2].X, -d[2].Y*s[2].Y},
            
            {s[3].X, s[3].Y, 1, 0, 0, 0, -d[3].X*s[3].X, -d[3].X*s[3].Y},
            {0, 0, 0, s[3].X, s[3].Y, 1, -d[3].Y*s[3].X, -d[3].Y*s[3].Y},
        };
    }

    private static double[,] CalcB(Point[] d) //format to be multiplied with AInv
    {
        d = OrderPointsClockwise(d);
        return new double[,]
        {
            {d[0].X},
            {d[0].Y},
            {d[1].X},
            {d[1].Y},
            {d[2].X},
            {d[2].Y},
            {d[3].X},
            {d[3].Y}
        };
    }

    private static double[,] AInverse(double[,] A) // Gauss-Jordan elimination method
    {
        int n = A.GetLength(0);
        double[,] B = new double[n, 2 * n];

        // Create an augmented matrix [A|I]
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                B[i, j] = A[i, j];
            }
            B[i, n + i] = 1;
        }

        // Perform row operations to transform [A|I] to [I|A^-1]
        for (var i = 0; i < n; i++)
        {
            // Find the pivot element and swap rows if necessary
            double pivot = B[i, i];
            int pivotRow = i;
            for (int j = i + 1; j < n; j++)
            {
                if (Math.Abs(B[j, i]) > Math.Abs(pivot))
                {
                    pivot = B[j, i];
                    pivotRow = j;
                }
            }
            if (pivotRow != i)
            {
                for (var j = 0; j < 2 * n; j++)
                {
                    (B[i, j], B[pivotRow, j]) = (B[pivotRow, j], B[i, j]);
                }
            }
            // Scale the pivot row to make the pivot element equal to 1
            var scale = 1 / pivot;
            for (var j = 0; j < 2 * n; j++)
            {
                B[i, j] *= scale;
            }
            // Use the pivot row to eliminate the pivot elements in the other rows
            for (var j = 0; j < n; j++)
            {
                if (j != i)
                {
                    double factor = B[j, i];
                    for (int k = 0; k < 2 * n; k++)
                    {
                        B[j, k] -= factor * B[i, k];
                    }
                }
            }
        }
        // Extract the inverse matrix from the augmented matrix
        double[,] AInv = new double[n, n];
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                AInv[i, j] = B[i, n + j];
            }
        }
        return AInv;
    }
    
    public static double[,] MatrixMultiply(double[,] a, double[,] b)
    {
        int aRows = a.GetLength(0);
        int aCols = a.GetLength(1);
        int bRows = b.GetLength(0);
        int bCols = b.GetLength(1);

        if (aCols != bRows)
            throw new ArgumentException("Matrices not compatible");

        double[,] result = new double[aRows, bCols];

        for (var i = 0; i < aRows; i++)
            for (var j = 0; j < bCols; j++)
                for (var k = 0; k < aCols; k++)
                    result[i, j] += a[i, k] * b[k, j];
        return result;
    }
}
using System;
using System.Windows.Media.Imaging;
using Point = System.Windows.Point;
using System.Drawing;

namespace TextureRipper;

/// <summary>
/// Class <c>Quad</c> contains methods for calculating and applying a homography matrix to an image.
/// </summary>
public static class Quad
{
    
    /// <summary>
    /// Orders the given points in clockwise order around their center point.
    /// </summary>
    /// <param name="points">The points to be ordered.</param>
    /// <returns>An ordered array of the given points.</returns>
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

    /// <summary>
    /// Calculates the homography matrix for the given points.
    /// </summary>
    /// <param name="s">The ordered source points.</param>
    /// <returns>The calculated homography matrix.</returns>
    public static double[,] CalcH(Point[] s) // given Ax = B, solve for x
    {
        Point rectSize = CalcRect(s);
        Point[] d = {
            new Point(0,0),
            rectSize with { Y = 0 },
            new Point(rectSize.X,rectSize.Y),
            rectSize with { X = 0 }
        };

        double[,] h = MatrixMultiply(AInverse(CalcA(s, d)), CalcB(d));

        return new[,] // reshape h to a 3x3
        {
            {h[0,0],h[1,0],h[2,0]},
            {h[3,0],h[4,0],h[5,0]},
            {h[6,0],h[7,0],1}
        };
    }
    
    /// <summary>
    /// Calculates the size of the rectangle that the given points should be mapped to.
    /// </summary>
    /// <param name="points">The points to be mapped.</param>
    /// <returns>The size of the rectangle.</returns>
    private static Point CalcRect(Point[] points) // calculate rectangle to map the points to
    {                                            // returns (width, height)
        var distances = new int[points.Length - 1];

        for (int i = 0; i < points.Length - 1; i++)
        {
            distances[i] = (int)Math.Sqrt(Math.Pow(points[i + 1].X - points[i].X, 2) + Math.Pow(points[i + 1].Y - points[i].Y, 2));
        }

        Array.Sort(distances);

        return new Point(distances[0], distances[1]);
    }

    /// <summary>
    /// Calculates the A matrix for the given source and destination points. (Ax=b)
    /// </summary>
    /// <param name="s">The ordered source points.</param>
    /// <param name="d">The ordered destination points.</param>
    /// <returns>The calculated A matrix.</returns>
    private static double[,] CalcA(Point[] s, Point[] d) //https://web.archive.org/web/20100801071311/http://alumni.media.mit.edu/~cwren/interpolator/
    {                                                   // in this case, the top left of d will always be the top left of s
        return new[,]
        {
            {s[0].X, s[0].Y, 1, 0, 0, 0, -d[0].X*s[0].X, -d[0].X*s[0].Y}, // I could loop but I like this visual
            {0, 0, 0, s[0].X, s[0].Y, 1, -d[0].Y*s[0].X, -d[0].Y*s[0].Y},
            
            {s[1].X, s[1].Y, 1, 0, 0, 0, -d[1].X*s[1].X, -d[1].X*s[1].Y},
            {0, 0, 0, s[1].X, s[1].Y, 1, -d[1].Y*s[1].X, -d[1].Y*s[1].Y},
            
            {s[2].X, s[2].Y, 1, 0, 0, 0, -d[2].X*s[2].X, -d[2].X*s[2].Y},
            {0, 0, 0, s[2].X, s[2].Y, 1, -d[2].Y*s[2].X, -d[2].Y*s[2].Y},
            
            {s[3].X, s[3].Y, 1, 0, 0, 0, -d[3].X*s[3].X, -d[3].X*s[3].Y},
            {0, 0, 0, s[3].X, s[3].Y, 1, -d[3].Y*s[3].X, -d[3].Y*s[3].Y},
        };
    }

    /// <summary>
    /// Converts the destination list of points into a 1x8 matrix.
    /// <param name="d">The ordered destination points</param>
    /// <returns>The calculated vector B</returns>
    /// </summary>
    private static double[,] CalcB(Point[] d) //format to be multiplied with AInv
    {
        return new[,]
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
    
    /// <summary>
    /// Calculates the inverse of a given matrix A.
    /// </summary>
    /// <param name="A">The matrix to calculate the inverse of.</param>
    /// <returns>The inverse of matrix A.</returns>
    private static double[,] AInverse(double[,] A) // Gauss-Jordan elimination method todo optimize this
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

    /// <summary>
    /// Multiplies two matrices A and B and returns the resulting matrix C.
    /// </summary>
    /// <param name="a">The first matrix to multiply.</param>
    /// <param name="b">The second matrix to multiply.</param>
    /// <returns>The resulting matrix C from multiplying matrices A and B.</returns>
    /// <exception cref="ArgumentException">Thrown when the matrices are not compatible for multiplication.</exception>
    private static double[,] MatrixMultiply(double[,] a, double[,] b)
    {
        var aRows = a.GetLength(0);
        var aCols = a.GetLength(1);
        var bRows = b.GetLength(0);
        var bCols = b.GetLength(1);

        if (aCols != bRows)
            throw new ArgumentException("Matrices not compatible");

        double[,] result = new double[aRows, bCols];

        for (var i = 0; i < aRows; i++)
            for (var j = 0; j < bCols; j++)
                for (var k = 0; k < aCols; k++)
                    result[i, j] += a[i, k] * b[k, j];
        return result;
    }
    
    /// <summary>
    /// Converts a matrix to a string representation.
    /// </summary>
    /// <param name="matrix">The matrix to convert to a string.</param>
    /// <returns>A string representation of the matrix.</returns>
    public static string MatrixToString(double[,] matrix)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);
        string result = "";

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                result += matrix[i, j].ToString("0.0").PadLeft(10) + " ";
            }
            result += "\n";
        }

        return result;
    }

    /// <summary>
    /// Specific to the texture ripper project. <c>RemapCoords</c> will remap the Canvas coordinate points relative to the source image for homography.
    /// </summary>
    /// <param name="a">Coords of SourceImage.</param>
    /// <param name="b">Array of quad points</param>
    /// <param name="cWidth">Current width of image</param>
    /// <param name="cHeight">Current height of image</param>
    /// <param name="oWidth">Original width of image</param>
    /// <param name="oHeight">Original height of image</param>
    /// <returns>Remapped coordinates of quad.</returns>
    internal static Point[] RemapCoords(Point a, Point[] b, double cWidth, double cHeight, double oWidth, double oHeight)
    {
        b = new Point[] {// (a,b) -> (c,d) = (-a + c, -b + d)
            new (-a.X + b[0].X, -a.Y + b[0].Y),
            new (-a.X + b[1].X, -a.Y + b[1].Y),
            new (-a.X + b[2].X, -a.Y + b[2].Y),
            new (-a.X + b[3].X, -a.Y + b[3].Y)
        };

        for (var i = 0; i < b.Length; i++) // (X,Y) / (currWidth,currHeight) * (origWidth, origHeight) = pos relative to orig image
        {
            b[i].X = (b[i].X / cWidth) * oWidth;
            b[i].Y = (b[i].Y / cHeight) * oHeight;
        }
        return b;
    }

    public static Bitmap WarpImage(BitmapImage image, double[,] H, Point[] crop)
    {
        BitmapSource bitmapSource = image; // convert to BitmapSource to pull color values

        Point size = CalcRect(crop);
        Bitmap output = new Bitmap((int)size.X, (int)size.Y);
        

        int bX = 0;
        int bY = 0;
        
        for (var y = 0; y < image.Height; y++)
        {
            for (var x = 0; x < image.Width; x++)
            {
                Point point = new Point(x, y);

                
                bool isInside = true; 
                for (int i = 0, j = crop.Length - 1; i < crop.Length; j = i++) // Check if the point is inside the crop
                {
                    if ((crop[i].Y > point.Y) != (crop[j].Y > point.Y) &&
                        (point.X < (crop[j].X - crop[i].X) * (point.Y - crop[i].Y) / (crop[j].Y - crop[i].Y) + crop[i].X))
                    {
                        isInside = !isInside;
                    }
                }
                
                if (isInside)
                {
                    output.SetPixel(bX,bY,GetColor(bitmapSource, x, y));
                }
            }
        }

        return output;
    }

    private static Color GetColor(BitmapSource bmp, int x, int y)
    {
        // Get the color of the pixel at position (x, y)
        int bytesPerPixel = (bmp.Format.BitsPerPixel + 7) / 8;
        int stride = bytesPerPixel * bmp.PixelWidth;
        byte[] pixelData = new byte[stride * bmp.PixelHeight];
        bmp.CopyPixels(pixelData, stride, 0);
        int offset = y * stride + x * bytesPerPixel;
        return Color.FromArgb(
            pixelData[offset + 3], // Alpha
            pixelData[offset + 2], // Red
            pixelData[offset + 1], // Green
            pixelData[offset]      // Blue
        );
    }

}
using System; // for rudiementary math and other
using System.Windows.Media.Imaging; // for importing an image
using Point = System.Windows.Point; // for Point object
using System.Drawing;
using System.Windows; // for creating a bitmap (new image)

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
    public static Point CalcRect(Point[] points) // calculate rectangle to map the points to
    {                                            // returns (width, height)

        double topLength;
        double sideLength;
        
        topLength = Point.Subtract(points[0], points[1]).Length > Point.Subtract(points[3], points[2]).Length ? Point.Subtract(points[3], points[2]).Length : Point.Subtract(points[0], points[1]).Length;
        
        sideLength = Point.Subtract(points[1], points[2]).Length > Point.Subtract(points[0], points[3]).Length ? Point.Subtract(points[0], points[3]).Length : Point.Subtract(points[1], points[2]).Length;
        
        return new Point(topLength, sideLength);
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
    /// <param name="a">The matrix to calculate the inverse of.</param>
    /// <returns>The inverse of matrix A.</returns>
    private static double[,] AInverse(double[,] a) // Gauss-Jordan elimination method todo use a faster method
    {
        int n = a.GetLength(0);
        double[,] b = new double[n, 2 * n];

        // Create an augmented matrix [A|I]
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                b[i, j] = a[i, j];
            }
            b[i, n + i] = 1;
        }

        // Perform row operations to transform [A|I] to [I|A^-1]
        for (var i = 0; i < n; i++)
        {
            // Find the pivot element and swap rows if necessary
            double pivot = b[i, i];
            int pivotRow = i;
            for (int j = i + 1; j < n; j++)
            {
                if (Math.Abs(b[j, i]) > Math.Abs(pivot))
                {
                    pivot = b[j, i];
                    pivotRow = j;
                }
            }
            if (pivotRow != i)
            {
                for (var j = 0; j < 2 * n; j++)
                {
                    (b[i, j], b[pivotRow, j]) = (b[pivotRow, j], b[i, j]);
                }
            }
            // Scale the pivot row to make the pivot element equal to 1
            var scale = 1 / pivot;
            for (var j = 0; j < 2 * n; j++)
            {
                b[i, j] *= scale;
            }
            // Use the pivot row to eliminate the pivot elements in the other rows
            for (var j = 0; j < n; j++)
            {
                if (j != i)
                {
                    double factor = b[j, i];
                    for (int k = 0; k < 2 * n; k++)
                    {
                        b[j, k] -= factor * b[i, k];
                    }
                }
            }
        }
        // Extract the inverse matrix from the augmented matrix
        double[,] aInv = new double[n, n];
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                aInv[i, j] = b[i, n + j];
            }
        }
        return aInv;
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
                result += matrix[i, j].ToString("0.000").PadLeft(10) + " ";
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

    public static Bitmap WarpImage(BitmapImage image, double[,] h, Point[] crop)
    {
        Point outres = CalcRect(crop);
        WriteableBitmap bitmapSource = new WriteableBitmap(image); // convert to WriteableBitmap to pull color values

        byte[] pixelData = new byte[bitmapSource.PixelWidth * bitmapSource.PixelHeight * 4]; // array of pixel colors BGRA32 format
        bitmapSource.CopyPixels(pixelData, bitmapSource.PixelWidth * 4, 0);        

        Bitmap output = new Bitmap((int)outres.X, (int)outres.Y);
        
        for (var y = 0; y < bitmapSource.PixelHeight; y++)
        {
            for (var x = 0; x < bitmapSource.PixelWidth; x++)
            {
                double[,] newpos = MatrixMultiply(h, new double[,] {{x},{y},{1}});
                int newX = (int)newpos[0, 0];
                int newY = (int)newpos[1, 0];

                if (newX >= 0 && newX < output.Width && newY >= 0 && newY < output.Height)
                {
                    output.SetPixel(newX, newY, GetArgb(pixelData, bitmapSource, x, y));
                }
            }
        }
        return output;
    }


    private static Color GetArgb(byte[] pixelData, WriteableBitmap image, int x, int y)
    {
        try
        {
            return Color.FromArgb(
                pixelData[4 * x + (y * image.BackBufferStride) + 3],
                pixelData[4 * x + (y * image.BackBufferStride) + 2],
                pixelData[4 * x + (y * image.BackBufferStride) + 1],
                pixelData[4 * x + (y * image.BackBufferStride)]);
        }
        catch
        {
            return Color.Black;
        }
    }

}
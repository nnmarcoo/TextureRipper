using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace TextureRipper;

/// <summary>
/// Class <c>BuildBitmap</c> contains methods to take in multiple bitmaps and neatly combine them into one mosaic bitmap.
/// </summary>
public class BuildBitmap
{
    private readonly List<Bitmap> _inBitmaps;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="inBitmaps">List of bitmaps to be combined</param>
    public BuildBitmap(List<Bitmap> inBitmaps)
    {
        _inBitmaps = inBitmaps;
        OutBitmap = CalcOutBitmap();
    }
    /// <summary>
    /// Getter for the output bitmap.
    /// </summary>
    public Bitmap OutBitmap { get; } = null!;

    /// <summary>
    /// Generates the mosaic bitmap.
    /// </summary>
    /// <returns>The calculated mosaic bitmap.</returns>
    private Bitmap CalcOutBitmap()
    {
        if (_inBitmaps.Count == 1) return _inBitmaps[0];
        
        var dim = CalcOutDim();
        Bitmap outBitmap = new Bitmap(dim.X, dim.Y);
        Point setPos = new Point(0, 0);

        foreach (var bitmap in _inBitmaps)
        {
            using Graphics g = Graphics.FromImage(outBitmap);
            g.DrawImage(bitmap, setPos);
            setPos.X += bitmap.Width + 5;
        }
        return outBitmap;
    }

    /// <summary>
    /// Default Constructor. It will initialize an empty list.
    /// </summary>
    public BuildBitmap()
    {
        _inBitmaps = new List<Bitmap>();
    }
    /// <summary>
    /// Delete the bitmap when object is deleted.
    /// </summary>
    ~BuildBitmap() // is this necessary?
    {
        OutBitmap.Dispose();
    }

    /// <summary>
    /// Add a bitmap to the _inBitmaps.
    /// <param name="bitmap">Desired bitmap.</param>
    /// </summary>
    public void AddBitmap(Bitmap bitmap)
    {
        _inBitmaps.Add(bitmap);
    }

    /// <summary>
    /// Delete all bitmaps from the _inBitmaps list.
    /// </summary>
    public void ClearBitmaps()
    {
        _inBitmaps.Clear();
    }

    /// <summary>
    /// Calculates the output dimensions of the mosaic bitmap.
    /// </summary>
    private Point CalcOutDim()
    {
        var dim = new Point();
        foreach (var bitmap in _inBitmaps)
        {
            dim.X += bitmap.Width;
            dim.X += 5;
        }

        dim.Y = _inBitmaps.Select(bitmap => bitmap.Height).Prepend(0).Max() + 5;
        return dim;
    }
}
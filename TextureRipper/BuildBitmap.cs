using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace TextureRipper;

public class BuildBitmap
{
    private readonly List<Bitmap> _inBitmaps;

    public BuildBitmap(List<Bitmap> inBitmaps)
    {
        _inBitmaps = inBitmaps;
        OutBitmap = CalcOutBitmap();
    }

    public Bitmap OutBitmap { get; }

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

    public BuildBitmap() // Default
    {
        
    }

    ~BuildBitmap() // is this necessary?
    {
        OutBitmap.Dispose();
    }

    public void AddBitmap(Bitmap bitmap)
    {
        _inBitmaps.Add(bitmap);
    }

    public void ClearBitmaps()
    {
        _inBitmaps.Clear();
    }

    private Point CalcOutDim() //todo add margin
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
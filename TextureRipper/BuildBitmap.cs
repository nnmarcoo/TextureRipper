using System.Collections.Generic;
using System.Drawing;

namespace TextureRipper;

public class BuildBitmap
{
    private Bitmap _outBitmap;
    private List<Bitmap> _inBitmaps;

    public BuildBitmap(List<Bitmap> inBitmaps)
    {
        _inBitmaps = inBitmaps;
        _outBitmap = CalcOutBitmap();
    }

    private Bitmap CalcOutBitmap()
    {
        if (_inBitmaps.Count == 1) return _inBitmaps[0];
        
        var dim = CalcOutDim();
        Bitmap outBitmap = new Bitmap(dim.X, dim.Y);

        foreach (var bitmap in _inBitmaps)
        {
            using Graphics g = Graphics.FromImage(outBitmap);
            g.DrawImage(bitmap, new Point(0,0));
        }
        return outBitmap;
    }

    public BuildBitmap() // Default
    {
        
    }

    ~BuildBitmap() // is this necessary?
    {
        _outBitmap.Dispose();
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
            dim.Y += bitmap.Height;
        }
        return dim;
    }
}
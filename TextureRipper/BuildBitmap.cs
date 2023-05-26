using System.Collections.Generic;
using System.Drawing;

namespace TextureRipper;

public class BuildBitmap
{
    private Bitmap _outBitmap;
    private HashSet<Bitmap> _inBitmaps;

    public BuildBitmap(HashSet<Bitmap> inBitmaps)
    {
        _inBitmaps = inBitmaps;
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

    public int CalcOutDim()
    {
        return -1;
    }
}
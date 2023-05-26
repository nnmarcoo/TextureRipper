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

    public BuildBitmap()
    {
        
    }

    ~BuildBitmap() // is this necessary?
    {
        _outBitmap.Dispose();
    }

}
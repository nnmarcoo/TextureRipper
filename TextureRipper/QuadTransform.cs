using System.Drawing;

namespace TextureRipper;

public class QuadTransform
{
    private Point[] src;

    public QuadTransform(Point[] src)
    {
        this.src = src;
    }
}
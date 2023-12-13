using SkiaSharp;

namespace WinterApp;

public record Brick(float X, float Y, float W, float H, SKColor Color)
{
    public bool Collide(float sfX, float sfY)
    {
        if (H >= 0)
            return sfX > X && sfX < X + W && sfY >= Y && sfY <= Y + H;
        else
            return sfX > X && sfX < X + W && sfY >= Y + H && sfY <= Y;
    }

    public float TopY => H >= 0 ? Y : Y + H;
}

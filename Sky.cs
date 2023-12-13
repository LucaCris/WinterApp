using SkiaSharp;

namespace WinterApp;

public class Sky
{
    readonly Random rnd = new();

    public readonly List<SnowFlake> SFList = [];
    public readonly List<SnowFlake> BackSFList = [];
    public readonly List<Brick> BrickList = [];
    public float Width { get; private set; }
    public float Height { get; private set; }
    public float Floor { get; private set; }

    public void Resize(float width, float height)
    {
        (Width, Height) = (width, height);
        Floor = height - SnowFlake.Dim;
        SFList.Clear();
        BackSFList.Clear();
        BrickList.Clear();
        //BrickList.Add(new Brick(Width / 2 - 40, Height / 2 - 10, 80, 20, SKColors.Red));
        AddHome(Width / 2, Floor);
        AddTree(Width / 1.33f, Floor);
    }

    public void AddHome(float x, float y)
    {
        BrickList.Add(new Brick(x, y, 200, -150, SKColors.Gray));
        BrickList.Add(new Brick(x + 80, y, 40, -70, SKColors.Maroon));
        BrickList.Add(new Brick(x + 20, y - 90, 50, -50, SKColors.LightGoldenrodYellow));
        BrickList.Add(new Brick(x + 130, y - 90, 50, -50, SKColors.LightGoldenrodYellow));
        y -= 150;
        x -= 20;
        BrickList.Add(new Brick(x, y, 20, -15, SKColors.Red));
        for (int i = 0; i < 5; i++) {
            x += 20;
            y -= 15;
            BrickList.Add(new Brick(x, y, 20, -15, SKColors.Red));
        }
        y -= 15;
        int w = 0;
        for (int i = 0; i < 6; i++) {
            x += 20;
            y += 15;
            BrickList.Add(new Brick(x, y, 20, -15, SKColors.Red));
            BrickList.Add(new Brick(x, y, -w, -15, SKColors.DarkRed));
            w += 40;
        }
    }

    public void AddTree(float x, float y)
    {
        BrickList.Add(new Brick(x + 75, y, 50, -50, SKColors.Brown));
        y -= 50;
        for (int i = 0; i < 7; i++) {
            BrickList.Add(new Brick(x + i * 15, y - i * 30, 200 - i * 30, -30, SKColors.Green));
        }
    }

    public void AddFlake(bool isBack)
    {
        if (!isBack)
            SFList.Add(new SnowFlake(rnd.Next(0, (int)Width)));
        else
            BackSFList.Add(new SnowFlake(rnd.Next(0, (int)Width)));
    }

    public void Next(float windX)
    {
        float windY = Math.Abs(windX) / 4f;

        foreach (var backsf in BackSFList) {
            backsf.Y += 1 + windY * 2;
        }
        BackSFList.RemoveAll(x => x.Y >= Floor);

        foreach (var sf in SFList) {
            if (sf.IsFalling) {
                sf.Y += rnd.NextSingle() + windY;
                sf.X += rnd.NextSingle() - rnd.NextSingle() + windX;
                if (sf.Y > Floor) {
                    sf.IsFalling = false;
                    sf.Y = Floor;
                    sf.X = (int)((sf.X + SnowFlake.Dim2) / SnowFlake.Dim) * SnowFlake.Dim;
                }
                else {
                    if (SFList.Any(x => !x.IsFalling && Math.Abs(x.X - sf.X) < 1 && Math.Abs(x.Y - sf.Y) < SnowFlake.Dim2))
                        sf.IsFalling = false;
                }

                if (sf.IsFalling) {
                    var coll = BrickList.FirstOrDefault(b => b.Collide(sf.X, sf.Y));
                    if (coll != null) {
                        sf.IsFalling = false;
                        sf.Y = coll.TopY;
                    }
                }

                if (sf.X < -SnowFlake.Dim * 2)
                    sf.X = Width + SnowFlake.Dim;
                else if (sf.X > Width + SnowFlake.Dim * 2)
                    sf.X = -SnowFlake.Dim;
            }
        }

        int n = SFList.Count(x => !x.IsFalling && x.Y >= Floor);
        if (n > Width / SnowFlake.Dim) {
            SFList.RemoveAll(x => !x.IsFalling && x.Y >= Floor);
            Floor -= SnowFlake.Dim2;
        }
    }
}

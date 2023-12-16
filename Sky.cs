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
    public float Footprint { get; private set; }

    public void Resize(float width, float height)
    {
        (Width, Height) = (width, height);
        Floor = height - SnowFlake.Dim;
        SFList.Clear();
        BackSFList.Clear();
        BrickList.Clear();
        Footprint = 0;
        Footprint += AddHome(Width / 2, Floor);
        Footprint += AddTree(Width / 8f, Floor, 1.1f, 1.4f);
        Footprint += AddTree(Width / 1.33f, Floor, 1, 1);
    }

    public float AddHome(float x, float y)
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

        return 240;
    }

    public float AddTree(float x, float y, float zx, float zy)
    {
        BrickList.Add(new Brick(x + 75 * zx, y, 50 * zx, -50 * zy, SKColors.Brown));
        y -= 50 * zy;
        for (int i = 0; i < 7; i++) {
            BrickList.Add(new Brick(x + i * 15 * zx, y - i * 30 * zy, 200 * zx - i * 30 * zx, -30 * zy, SKColors.Green));
        }

        return 200 * zx;
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

        // se finestra molto grande, aumenta velocità discesa gradualmente
        float speedY = (Width * Height) switch
        {
            < 1_000_000 => 0,
            < 2_000_000 => 0.5f,
            < 3_000_000 => 1,
            _ => 2,
        };

        foreach (var backsf in BackSFList)
            backsf.Y += speedY + 1 + windY * 2;

        BackSFList.RemoveAll(x => x.Y >= Floor);

        foreach (var sf in SFList) {
            if (sf.IsFalling) {
                sf.Y += speedY + rnd.NextSingle() + windY;
                sf.X += rnd.NextSingle() - rnd.NextSingle() + windX;
                if (sf.Y > Floor) {
                    sf.IsFalling = false;
                    sf.Y = Floor;
                    sf.X = (int)((sf.X + SnowFlake.Dim2) / SnowFlake.Dim) * SnowFlake.Dim;
                }
                else {
                    if (SFList.Any(x => !x.IsFalling && Math.Abs(x.X - sf.X) < 1 && Math.Abs(x.Y - sf.Y) < SnowFlake.Dim2)) {
                        sf.IsFalling = false;
                        // se collide con altro fiocco, al 25% eliminalo (fusione)
                        if (rnd.NextDouble() < 0.25)
                            sf.DoRemove = true;
                    }
                }

                if (sf.IsFalling) {
                    var coll = BrickList.FirstOrDefault(b => b.Collide(sf.X, sf.Y));
                    if (coll != null) {
                        sf.IsFalling = false;
                        // solo se è vicino alla parte alta del brick, incollalo esattamente sopra, altrimenti si incolla dove è rimasto
                        if (Math.Abs(sf.Y - coll.TopY) < SnowFlake.Dim)
                            sf.Y = coll.TopY;
                    }
                }

                if (sf.X < -SnowFlake.Dim * 2)
                    sf.X = Width + SnowFlake.Dim;
                else if (sf.X > Width + SnowFlake.Dim * 2)
                    sf.X = -SnowFlake.Dim;
            }
        }

        SFList.RemoveAll(x => x.DoRemove);

        int n = SFList.Count(x => !x.IsFalling && x.Y >= Floor);
        if (n > (Width - Footprint) / SnowFlake.Dim) {
            SFList.RemoveAll(x => !x.IsFalling && x.Y >= Floor);
            Floor -= SnowFlake.Dim2;
        }
    }
}

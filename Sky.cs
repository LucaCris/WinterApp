using SkiaSharp;

namespace WinterApp;

public class Sky
{
    readonly Random rnd = new();

    public readonly List<SnowFlake> SFList = [];
    public readonly List<SnowFlake> BackSFList = [];
    public readonly List<RainDrop> DropList = [];
    public readonly List<Brick> BrickList = [];
    public float Width { get; private set; }
    public float Height { get; private set; }
    public float Floor { get; private set; }
    public float Footprint { get; private set; }
    private DateTime LastFloorFused;

    public void Resize(float width, float height)
    {
        (Width, Height) = (width, height);
        Floor = height - SnowFlake.Dim;
        SFList.Clear();
        BackSFList.Clear();
        DropList.Clear();
        BrickList.Clear();
        Footprint = 0;
        Footprint += AddHome(Width / 2, Floor);
        Footprint += AddTree(Width / 8f, Floor, 1.1f, 1.4f);
        Footprint += AddTree(Width / 1.33f, Floor, 1, 1);
        Footprint += AddCar(Width / 4f, Floor);
        Footprint += AddPerson(Width / 1.5f, Floor);
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

    public float AddCar(float x, float y)
    {
        // Car body
        BrickList.Add(new Brick(x, y - 5, 160, -40, SKColors.Red));

        // Left wheel
        BrickList.Add(new Brick(x + 20, y, 30, -30, SKColors.Gray));

        // Right wheel
        BrickList.Add(new Brick(x + 110, y, 30, -30, SKColors.Gray));

        // Car cabin (upper part)
        BrickList.Add(new Brick(x + 35, y - 40, 90, -35, SKColors.DarkRed));

        // Left window
        BrickList.Add(new Brick(x + 40, y - 45, 25, -20, SKColors.LightCyan));

        // Right window
        BrickList.Add(new Brick(x + 95, y - 45, 25, -20, SKColors.LightCyan));

        // Windshield
        BrickList.Add(new Brick(x + 70, y - 50, 25, -15, SKColors.LightCyan));

        return 160;
    }

    public float AddPerson(float x, float y)
    {
        // Body
        BrickList.Add(new Brick(x + 15, y - 25, 40, -25, SKColors.Blue));

        // Head
        BrickList.Add(new Brick(x + 20, y - 50, 30, -30, SKColors.BurlyWood));

        // Hat
        BrickList.Add(new Brick(x + 15, y - 80, 40, -15, SKColors.Black));

        // Hat brim
        BrickList.Add(new Brick(x + 10, y - 80, 50, -5, SKColors.DarkGray));

        // Left arm
        BrickList.Add(new Brick(x + 5, y - 15, 10, -30, SKColors.BurlyWood));

        // Right arm
        BrickList.Add(new Brick(x + 55, y - 15, 10, -30, SKColors.BurlyWood));

        // Scarf (wrapped around neck area)
        BrickList.Add(new Brick(x + 18, y - 48, 34, -8, SKColors.Red));

        // Scarf hanging
        BrickList.Add(new Brick(x + 12, y - 48, 8, -25, SKColors.Red));

        // Left leg
        BrickList.Add(new Brick(x + 20, y, 12, -25, SKColors.Brown));

        // Right leg
        BrickList.Add(new Brick(x + 38, y, 12, -25, SKColors.Brown));

        return 70;
    }

    public void AddFlake(bool isBack)
    {
        if (!isBack)
            SFList.Add(new SnowFlake(rnd.Next(0, (int)Width)));
        else
            BackSFList.Add(new SnowFlake(rnd.Next(0, (int)Width)));
    }

    public void AddDrop()
    {
        DropList.Add(new RainDrop(rnd.Next(0, (int)Width)));
    }

    public void Next(float windX, bool snowMode)
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
        //speedY = 6;

        foreach (var drop in DropList) {
            drop.Y += 5 + speedY * 2 + rnd.NextSingle() + windY * 2;
            drop.X += windX * 8;
        }

        DropList.RemoveAll(x => x.Y >= Floor);

        foreach (var backsf in BackSFList)
            backsf.Y += speedY + 1 + windY * 2;

        BackSFList.RemoveAll(x => x.Y >= Floor);

        bool dropFused = false;

        foreach (var sf in SFList) {
            if (sf.IsFalling) {
                sf.Y += speedY + rnd.NextSingle() + windY;
                sf.X += rnd.NextSingle() - rnd.NextSingle() + windX;
                if (sf.Y > Floor) {
                    sf.IsFalling = false;
                    sf.Y = Floor;
                    sf.X = (int)((sf.X + SnowFlake.Dim2) / SnowFlake.Dim) * SnowFlake.Dim;
                } else {
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
            } else if (!snowMode && !dropFused) {
                if (rnd.NextDouble() < 0.01) {
                    if (!SFList.Any(other => !other.IsFalling && other.Y < sf.Y && Math.Abs(other.X - sf.X) < SnowFlake.Dim2)) {
                        sf.DoRemove = true;
                        dropFused = true;
                    }
                }
            }
        }

        SFList.RemoveAll(x => x.DoRemove);

        int n = SFList.Count(x => !x.IsFalling && x.Y >= Floor);
        if (Footprint < Width && n > (Width - Footprint) / SnowFlake.Dim) {
            SFList.RemoveAll(x => !x.IsFalling && x.Y >= Floor);
            Floor -= SnowFlake.Dim2;
            LastFloorFused = DateTime.Now;
        } else if (n == 0 && !snowMode && Floor < Height - SnowFlake.Dim && rnd.NextDouble() < 0.05 && (DateTime.Now - LastFloorFused).TotalSeconds > 10) {
            Floor += SnowFlake.Dim2;
            LastFloorFused = DateTime.Now;
        }
    }

    public void Draw(SKCanvas canvas)
    {
        // Draw only animated elements (snowflakes, raindrops) without bricks
        var bgPaint = new SKPaint { ColorF = SKColors.Black };
        bgPaint.ColorF = bgPaint.ColorF.WithAlpha(0);
        canvas.DrawRect(0, 0, Width, Height, bgPaint);

        var p = new SKPaint { Color = SKColors.FloralWhite, StrokeWidth = 1 };
        foreach (var sf in BackSFList)
            canvas.DrawPoint(sf.X, sf.Y, p);

        p = new SKPaint { Color = SKColors.White, StrokeWidth = SnowFlake.Dim };
        foreach (var sf in SFList)
            canvas.DrawPoint(sf.X - SnowFlake.Dim2, sf.Y - SnowFlake.Dim2, p);

        p = new SKPaint { Color = SKColors.LightBlue.WithAlpha(200), StrokeWidth = 2 };
        foreach (var sf in DropList)
            canvas.DrawLine(sf.X, sf.Y, sf.X, sf.Y + 2, p);
    }
}

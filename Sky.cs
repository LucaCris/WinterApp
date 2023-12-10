namespace WinterApp;

public class Sky
{
    readonly Random rnd = new();

    public readonly List<SnowFlake> SFList = [];
    public float Width { get; private set; }
    public float Height { get; private set; }
    public float Floor { get; private set; }

    public void Resize(float width, float height)
    {
        (Width, Height) = (width, height);
        Floor = height - SnowFlake.Dim;
        SFList.Clear();
    }

    public void AddFlake()
    {
        SFList.Add(new SnowFlake(rnd.Next(0, (int)Width)));
    }

    public void Next(float windX)
    {
        float windY = Math.Abs(windX) / 4f;
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

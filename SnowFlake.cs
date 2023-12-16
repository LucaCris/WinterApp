namespace WinterApp;

public class SnowFlake(float x)
{
    public const float Dim = 5; 
    public const float Dim2 = Dim/2;

    public float X { get; set; } = x;
    public float Y { get; set; } = 0;
    public bool IsFalling { get; set; } = true;
    public bool DoRemove { get; set; } = false;
}

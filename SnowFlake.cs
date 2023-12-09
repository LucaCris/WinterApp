namespace WinterApp;

public class SnowFlake
{
    public const float Dim = 5; 
    public const float Dim2 = Dim/2;
    
    public SnowFlake(float x)
    {
        X = x;
        Y = 0;
        IsFalling = true;
        Remove = false;
    }

    public float X { get; set; }
    public float Y { get; set; }
    public bool IsFalling { get; set; }
    public bool Remove { get; set; }

}

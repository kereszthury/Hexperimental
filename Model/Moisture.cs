namespace Hexperimental.Model;

public readonly struct Moisture
{
    public readonly float clouds, moisture;

    public Moisture(float clouds = 0, float moisture = 0)
    {
        this.clouds = clouds;
        this.moisture = moisture;
    }
}

namespace PXResources.Shared.Resources;


public class PublicPixStats
{
    public int Hp { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int Speed { get; set; }


    public PublicPixStats()
    {
        this.Hp = 0;
        this.Attack = 0;
        this.Defense = 0;
        this.Speed = 0;
    }


    public PublicPixStats(int hp, int attack, int defense, int speed)
    {
        this.Hp = hp;
        this.Attack = attack;
        this.Defense = defense;
        this.Speed = speed;
    }


    public PublicPixStats(int max)
    {
        this.Hp = Random.Shared.Next(max);
        this.Attack = Random.Shared.Next(max);
        this.Defense = Random.Shared.Next(max);
        this.Speed = Random.Shared.Next(max);
    }
}
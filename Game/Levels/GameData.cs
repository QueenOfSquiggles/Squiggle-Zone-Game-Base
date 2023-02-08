public class GameData
{
    public float some_float = 0.123f;
    public int some_int = 123;
    public string some_str = "123";

    public override string ToString()
    {
        return $"GameData:-\n\t{some_float}\n\t{some_int}\n\t{some_str}";
    }
}
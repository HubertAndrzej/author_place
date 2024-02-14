namespace AuthorPlace.Models.ValueObjects;

public class Sql
{
    private Sql(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static explicit operator Sql(string value) => new(value);

    public override string ToString()
    {
        return this.Value;
    }
}

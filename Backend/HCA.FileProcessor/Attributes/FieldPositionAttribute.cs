namespace HCA.FileProcessor.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class FieldPositionAttribute : Attribute
{
    public int Position;

    public FieldPositionAttribute(int position = -1)
    {
        Position = position;
    }
}


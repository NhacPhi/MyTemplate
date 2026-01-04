using System;


public enum AttributeType
{
    HP
}

public class AttributeTypeExtentions
{
    public const string HP = "HP";

    public static string GetName(AttributeType type)
    {
        switch (type)
        {
            case AttributeType.HP:
                return HP;
        }

        return string.Empty;
    }
}

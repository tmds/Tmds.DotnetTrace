namespace Tmds.Babeltrace
{
    public enum FieldType
    {
        UnknownField = -1,
        Unknown = 0,
        Integer,
        Float,
        Enum,
        String,
        Struct,
        UntaggedVariant,
        Variant,
        Array,
        Sequence
    }
}
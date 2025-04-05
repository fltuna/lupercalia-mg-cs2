namespace LupercaliaMGCore.model;

public static class CenterHtmlSizeExtensions
{
    public static string ToLowerString(this CenterHtmlSize size)
    {
        return size.ToString().ToLower();
    }
}


public enum CenterHtmlSize
{
    XXXL,
    XXL,
    XL,
    ML,
    M,
    SM,
    S,
    XS,
}
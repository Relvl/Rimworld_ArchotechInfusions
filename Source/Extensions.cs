using System.Text;

namespace ArchotechInfusions;

public static class Extensions
{
    public static StringBuilder TrimEnd(this StringBuilder sb)
    {
        if (sb == null || sb.Length == 0) return sb;
        var i = sb.Length - 1;
        for (; i >= 0; i--)
            if (!char.IsWhiteSpace(sb[i]))
                break;
        if (i < sb.Length - 1)
            sb.Length = i + 1;
        return sb;
    }
}
using System.Text;
using Verse;

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

    public static float PercentOfRange(this float input, float min, float max) => (input - min) / (max - min);
    public static float Denormalize(this float input, float min, float max) => input * (max - min) + min;

    public static Rot4 CopyAndRotate(this Rot4 input, RotationDirection direction)
    {
        var copy = input;
        copy.Rotate(direction);
        return copy;
    }
}
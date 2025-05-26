using System.Diagnostics.CodeAnalysis;
using Verse;

namespace ArchotechInfusions;

/// <summary>
///     Welp... Yea. It's a thing. I guess... At least, that's better than another Harmony patch.
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class ExternalUrlDef : ThingDef
{
    public string url;
}
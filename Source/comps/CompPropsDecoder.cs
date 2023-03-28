using Verse;

namespace ArchotechInfusions.comps;

// ReSharper disable once ClassNeverInstantiated.Global
public class CompPropsDecoder : CompProperties
{
    public float StartupTicks;
    public FloatRange DecodeTicks;

    public CompPropsDecoder() => compClass = typeof(CompDecoder);
}
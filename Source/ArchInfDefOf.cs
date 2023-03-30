using RimWorld;
using Verse;

namespace ArchotechInfusions;

[DefOf]
public static class ArchInfSoundDefOf
{
    public static SoundDef ArchInfTransceiverStart;
    public static SoundDef ArchInfTransceiverReceive;
    public static SoundDef ArchInfTransceiverRecharge;

    static ArchInfSoundDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(ArchInfSoundDefOf));
}

[DefOf]
public class JobDriverDefOf
{
    public static JobDef ArchInf_GenerateKey;
    public static JobDef ArchInf_RefuelContainer;

    static JobDriverDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(JobDriverDefOf));
}
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
public class WorkGiverDefOf
{
    public static WorkGiverDef ArchInf_GenerateKey;

    static WorkGiverDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(WorkGiverDefOf));
}

[DefOf]
public class JobDriverDefOf
{
    public static JobDef ArchInf_GenerateKey;

    static JobDriverDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(JobDriverDefOf));
}
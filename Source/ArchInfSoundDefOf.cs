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
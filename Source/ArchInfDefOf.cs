using System.Diagnostics.CodeAnalysis;
using RimWorld;
using Verse;

namespace ArchotechInfusions;

[DefOf]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
public static class ArchInfSoundDefOf
{
    public static SoundDef ArchInfTransceiverStart;
    public static SoundDef ArchInfTransceiverReceive;
    public static SoundDef ArchInfTransceiverRecharge;
    public static SoundDef ArchInfDecoderStart;

    static ArchInfSoundDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(ArchInfSoundDefOf));
}

[DefOf]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class JobDriverDefOf
{
    public static JobDef ArchInf_GenerateKey;
    public static JobDef ArchInf_RefuelContainer;
    public static JobDef ArchInf_RepairInventory;

    static JobDriverDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(JobDriverDefOf));
}
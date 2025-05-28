using System.Diagnostics.CodeAnalysis;
using RimWorld;
using Verse;

namespace ArchotechInfusions.defOf;

[DefOf]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
public static class ArchInfSoundDefOf
{
    public static SoundDef ArchInfTransceiverStart;
    public static SoundDef ArchInfTransceiverReceive;
    public static SoundDef ArchInfTransceiverRecharge;
    public static SoundDef ArchInfDecoderStart;

    static ArchInfSoundDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(ArchInfSoundDefOf));
    }
}
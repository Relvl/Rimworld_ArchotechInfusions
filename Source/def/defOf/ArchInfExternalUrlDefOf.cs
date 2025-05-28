using System.Diagnostics.CodeAnalysis;
using RimWorld;

namespace ArchotechInfusions.defOf;

[DefOf]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class ArchInfExternalUrlDefOf
{
    public static ExternalUrlDef ArchInf_ExternalUrl_Integrity_Wiki;

    static ArchInfExternalUrlDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(ArchInfExternalUrlDefOf));
    }
}
using System.Diagnostics.CodeAnalysis;
using RimWorld;

namespace ArchotechInfusions.defOf;

[DefOf]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class ArchInfStatCategoryDefOf
{
    public static StatCategoryDef JAI_Pawn_Affected;

    static ArchInfStatCategoryDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(ArchInfStatCategoryDefOf));
    }
}
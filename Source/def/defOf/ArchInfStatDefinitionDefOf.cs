using System.Diagnostics.CodeAnalysis;
using RimWorld;

namespace ArchotechInfusions.defOf;

[DefOf]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
public class ArchInfStatDefinitionDefOf
{
    public static StatDefinitionDef Unbreakable;

    static ArchInfStatDefinitionDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(ArchInfStatDefinitionDefOf));
    }
}
using RimWorld;

namespace ArchotechInfusions;

[DefOf]
public class ArchInfStatDefinitionDefOf
{
    public static StatDefinitionDef Unbreakable;

    static ArchInfStatDefinitionDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(ArchInfStatDefinitionDefOf));
    }
}
using System.Diagnostics.CodeAnalysis;
using ArchotechInfusions.comps.comp_base;
using Verse;

namespace ArchotechInfusions.comps;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
[SuppressMessage("ReSharper", "ConvertToConstant.Global")]
public class DatabaseProps : CompProperties
{
    public int MaxSpace = 1000;

    public DatabaseProps()
    {
        compClass = typeof(Comp);
    }

    public class Comp : AGridComp<DatabaseProps>;
}
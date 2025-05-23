using System.Diagnostics.CodeAnalysis;
using ArchotechInfusions.comps.comp_base;

namespace ArchotechInfusions.comps;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
[SuppressMessage("ReSharper", "ConvertToConstant.Global")]
public class CompProps_Database : CompPropertiesBase_Grid
{
    public int MaxSpace = 1000;

    public CompProps_Database()
    {
        compClass = typeof(Comp_Database);
    }
}

public class Comp_Database : CompBase_Grid<CompProps_Database>;
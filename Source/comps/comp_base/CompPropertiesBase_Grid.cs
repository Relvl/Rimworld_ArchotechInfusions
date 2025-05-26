using System.Diagnostics.CodeAnalysis;
using ArchotechInfusions.grid.graphic;
using Verse;

namespace ArchotechInfusions.comps.comp_base;

[SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
[SuppressMessage("ReSharper", "ConvertToConstant.Global")]
public abstract class CompPropertiesBase_Grid : CompProperties
{
    /// <summary>
    /// </summary>
    public GridVisibility Visibility = GridVisibility.Allways;
}
using System.Diagnostics.CodeAnalysis;
using ArchotechInfusions.comps.comp_base;

namespace ArchotechInfusions.comps;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
public class CompProps_ArchiteRepairer : CompPropertiesBase_Grid
{
    public float ArchitePerHp;
    public float EnergyPerHp;
    public float HpPerTick;

    public CompProps_ArchiteRepairer()
    {
        compClass = typeof(Comp_ArchiteRepairer);
    }
}

public class Comp_ArchiteRepairer : CompBase_Grid<CompProps_ArchiteRepairer>;
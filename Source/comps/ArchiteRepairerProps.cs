using System.Diagnostics.CodeAnalysis;
using ArchotechInfusions.comps.comp_base;
using Verse;

namespace ArchotechInfusions.comps;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
public class ArchiteRepairerProps : CompProperties
{
    public float ArchitePerHp;
    public float EnergyPerHp;
    public float HpPerTick;

    public ArchiteRepairerProps()
    {
        compClass = typeof(Comp);
    }

    public class Comp : AGridComp<ArchiteRepairerProps>;
}
using Verse;

namespace ArchotechInfusions.comps;

// ReSharper disable UnassignedField.Global
// ReSharper disable once ClassNeverInstantiated.Global
public class CompPropsAccumulator : CompProperties
{
    public float MaxStored;

    public CompPropsAccumulator() => compClass = typeof(CompAccumulator);
}
using System.Diagnostics.CodeAnalysis;
using RimWorld;
using Verse;

namespace ArchotechInfusions;

[DefOf]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class JobDriverDefOf
{
    public static JobDef ArchInf_GenerateKey;
    public static JobDef ArchInf_RefuelContainer;
    public static JobDef ArchInf_RepairInventory;
    public static JobDef ArchInf_Print;

    static JobDriverDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(JobDriverDefOf));
}
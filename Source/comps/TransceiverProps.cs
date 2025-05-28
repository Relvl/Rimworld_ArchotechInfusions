using System.Diagnostics.CodeAnalysis;
using ArchotechInfusions.comps.comp_base;
using Verse;

namespace ArchotechInfusions.comps;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
public class TransceiverProps : CompProperties
{
    public float ReceivePowerGain;
    public int ReceiveTicks;
    public float TransmitPowerGain;
    public int TransmitTicks;

    public TransceiverProps()
    {
        compClass = typeof(Comp);
    }

    public class Comp : AGridComp<TransceiverProps>;
}
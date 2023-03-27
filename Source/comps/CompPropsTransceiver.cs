using Verse;

namespace ArchotechInfusions.comps;

// ReSharper disable once ClassNeverInstantiated.Global
public class CompPropsTransceiver : CompProperties
{
    public float RechargeTicks;

    public float ReceiveConsumption;
    public float TranscieveConsumption;

    public float ReceiveTicks;
    public float TransceiveTicks;

    public CompPropsTransceiver() => compClass = typeof(CompTransceiver);
}
using System;
using System.Collections.Generic;
using ArchotechInfusions.grid;
using RimWorld;
using Verse;

namespace ArchotechInfusions.comps;

public class ProgressCompBase : ThingComp
{
    private CompPowerTrader _power;
    private GridMemberComp _member;
    private int _progress;
    private bool _mute;

    protected CompPowerTrader Power => _power ??= parent.TryGetComp<CompPowerTrader>();
    protected GridMemberComp Member => _member ??= parent.TryGetComp<GridMemberComp>();

    protected int Progress
    {
        get => _progress;
        set => _progress = value;
    }

    protected bool Mute
    {
        get => _mute;
        set => _mute = value;
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        if (Power is null) throw new Exception("ArchInf: CompTransceiver can't work without CompPowerTrader");
        if (Member is null) throw new Exception("ArchInf: CompTransceiver can't work without GridMemberComp");
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref _progress, "progress");
        Scribe_Values.Look(ref _mute, "mute");
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        yield return new Command_Action
        {
            defaultLabel = Mute ? "Unmute" : "Mute", //
            action = () => Mute = !Mute,
        };
    }

    public class State
    {
        public readonly string Name;
        public readonly int Ticks;
        public readonly Action OnFinal;

        public State(string name, int ticks, Action onFinal)
        {
            Name = name;
            Ticks = ticks;
            OnFinal = onFinal;
        }
    }
}
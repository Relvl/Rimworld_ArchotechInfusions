using Verse;

namespace ArchotechInfusions.comps;

// ReSharper disable UnassignedField.Global,FieldCanBeMadeReadOnly.Global,InconsistentNaming,ClassNeverInstantiated.Global -- def reflective
public class CompProps_Database : CompProperties
{
    public int MaxSpace = 1000;

    public CompProps_Database() => compClass = typeof(Comp_Database);
}

public class Comp_Database : CompBase_Membered<CompProps_Database>
{
}
namespace ArchotechInfusions.comps.comp_base;

public abstract class CompBase_GridState<T, TP> : CompBase_Grid<TP>
    where TP : CompPropertiesBase_Grid
    where T : CompBase_GridState<T, TP>;
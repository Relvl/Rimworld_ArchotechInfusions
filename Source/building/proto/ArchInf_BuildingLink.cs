using ArchotechInfusions.graphic;
using Verse;

namespace ArchotechInfusions.building.proto;

// ReSharper disable once InconsistentNaming
public abstract class ArchInf_BuildingLink : AddInf_Building
{
    private GraphicGridLink _graphic;
    private GraphicGridLink GraphicLinked => _graphic ??= new GraphicGridLink(GraphicDatabase.Get<Graphic_Single>(def.graphicData.texPath, ShaderDatabase.Transparent));

    /// <summary>
    ///     Disable native render to layer, we will use our's own.
    ///     In the original code of the game, it is very difficult to create custom graphics that would link correctly... =(
    /// </summary>
    public override void Print(SectionLayer layer)
    {
    }

    public virtual void PrintLinkable(SectionLayer layer)
    {
        GraphicLinked.PrintLinkable(layer, this);
    }
}
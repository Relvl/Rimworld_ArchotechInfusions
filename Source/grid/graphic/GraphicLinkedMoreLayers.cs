using UnityEngine;
using Verse;

namespace ArchotechInfusions.grid.graphic;

public class GraphicLinkedMoreLayers : Graphic_Linked
{
    private const LinkFlags AllCustomFlags = LinkFlags.Custom1 |
                                             LinkFlags.Custom2 |
                                             LinkFlags.Custom3 |
                                             LinkFlags.Custom4 |
                                             LinkFlags.Custom5 |
                                             LinkFlags.Custom6 |
                                             LinkFlags.Custom7 |
                                             LinkFlags.Custom8 |
                                             LinkFlags.Custom9 |
                                             LinkFlags.Custom10;

    public GraphicLinkedMoreLayers()
    {
    }

    public GraphicLinkedMoreLayers(Graphic subGraphic) : base(subGraphic)
    {
    }

    public override void Init(GraphicRequest req)
    {
        subGraphic = GraphicDatabase.Get<Graphic_Single>(req.graphicData.texPath, ShaderDatabase.Transparent);
        data = subGraphic.data;
    }

    public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
    {
        return new GraphicLinkedMoreLayers(subGraphic.GetColoredVersion(newShader, newColor, newColorTwo)) { data = data };
    }

    public override bool ShouldLinkWith(IntVec3 c, Thing parent)
    {
        if (!parent.Spawned) return false;
        if (c.InBounds(parent.Map))
        {
            Log.Warning($"ShouldLinkWith {c} => {parent.Map.linkGrid.LinkFlagsAt(c) & AllCustomFlags} == {parent.def.graphicData.linkFlags}");
            return parent.def.graphicData.linkFlags == (parent.Map.linkGrid.LinkFlagsAt(c) & AllCustomFlags);
        }

        return (parent.def.graphicData.linkFlags & LinkFlags.MapEdge) != 0;
    }
}
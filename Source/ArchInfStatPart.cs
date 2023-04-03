using RimWorld;

namespace ArchotechInfusions;

/// <summary>
/// This stat part adds to every StatDef that can be changed/affected by this mod.
/// TransformValue can be called for the pawn or for the gear (and maybe for something else?). So we need to detect for what exactly it called. 
/// </summary>
public class ArchInfStatPart : StatPart
{
    public override void TransformValue(StatRequest req, ref float val)
    {
        throw new System.NotImplementedException();
    }

    public override string ExplanationPart(StatRequest req)
    {
        throw new System.NotImplementedException();
    }
}
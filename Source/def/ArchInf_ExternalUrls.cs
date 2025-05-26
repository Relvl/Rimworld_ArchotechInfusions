using RimWorld;

namespace ArchotechInfusions;

[DefOf]
public class ArchInf_ExternalUrls
{
    public static ExternalUrlDef ArchInf_ExternalUrl_Integrity_Wiki;

    static ArchInf_ExternalUrls()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(ArchInf_ExternalUrls));
    }
}
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace ArchotechInfusions;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class ArchotechInfusionsMod : Mod
{
    public ArchotechInfusionsMod(ModContentPack content) : base(content)
    {
        new Harmony("johnson1893.archotech.infusions")
            .PatchAll(Assembly.GetExecutingAssembly());
    }
}
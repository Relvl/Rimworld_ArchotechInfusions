using System.Reflection;
using HarmonyLib;
using Verse;

namespace ArchotechInfusions;

// ReSharper disable once UnusedType.Global
public class ArchotechInfusionsMod : Mod
{
    public ArchotechInfusionsMod(ModContentPack content) : base(content)
    {
        new Harmony("io.github.Relvl.Rimworld.ArchotechInfusions").PatchAll(Assembly.GetExecutingAssembly());
    }
}
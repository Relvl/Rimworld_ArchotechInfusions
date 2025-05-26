using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using HarmonyLib;
using RimWorld;
using Verse;

namespace ArchotechInfusions.harmony;

[HarmonyPatch(typeof(Dialog_InfoCard.Hyperlink))]
[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public static class Patch_DialogInfocard_Hyperlink
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(Dialog_InfoCard.Hyperlink.ActivateHyperlink))]
    public static bool ActivateHyperlink_Prefix(Dialog_InfoCard.Hyperlink __instance)
    {
        if (__instance.IsHidden) return true;
        if (__instance.def is ExternalUrlDef urlDef)
        {
            ShowUrlDialog(urlDef.url);
            return false;
        }

        return true;
    }

    private static void ShowUrlDialog(string url)
    {
        Find.WindowStack.Add(new Dialog_MessageBox(
            "JAI.OpenUrl.Question".Translate() + url,
            "JAI.OpenUrl".Translate(),
            () =>
            {
                try
                {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = url,
                            UseShellExecute = true
                        });
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                        Process.Start("open", url);
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                        Process.Start("xdg-open", url);
                    else
                        throw new PlatformNotSupportedException("Unknown platform");
                }
                catch (Exception ex)
                {
                    Log.Error("JAI: Failed to open URL: " + ex.Message);
                    Messages.Message("JAI.OpenUrl.FailMessage".Translate(), MessageTypeDefOf.RejectInput, false);
                }
            },
            "Cancel".Translate()
        ));
    }
}
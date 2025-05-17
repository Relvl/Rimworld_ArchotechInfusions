using System;
using System.Collections.Generic;
using System.Linq;
using ArchotechInfusions.def;
using RimWorld;
using Verse;

namespace ArchotechInfusions.statcollectors;

public static class StatCollector
{
    private static readonly HashSet<string> IgnoredDefs = [];
    private static readonly HashSet<string> NegatedDefs = [];

    private static IEnumerable<StatCollectionElement> _statCache;
    public static IEnumerable<StatCollectionElement> StatCache => _statCache ??= GetStatCache();

    private static IEnumerable<StatCollectionElement> GetStatCache()
    {
        CollectDefsTo<StatIgnoreDef>(IgnoredDefs, def => def.IgnoredStats);
        CollectDefsTo<StatNegateDef>(NegatedDefs, def => def.NegatedStats);

        var usedStats = new Dictionary<string, StatCollectionElement>();

        // Doing all the collectors
        foreach (var collectorDef in DefDatabase<StatCollectorDef>.AllDefs)
        foreach (var collectorClass in collectorDef.CollectorClasses)
        {
            if (Activator.CreateInstance(collectorClass) is not IStatCollector instance)
            {
                Log.Error($"ArchInf: cannot create stat collector: {collectorClass.Name}");
                continue;
            }

            foreach (var element in instance.Collect())
            {
                if (IgnoredDefs.Contains(element.StatDef.defName)) continue;
                if (NegatedDefs.Contains(element.StatDef.defName)) element.Modifier.IsNegated = true;
                ProcessElement(element, usedStats);
            }
        }

        foreach (var overrideDef in DefDatabase<StatModDef>.AllDefs)
        {
            var statDef = DefDatabase<StatDef>.GetNamed(overrideDef.defName, false);
            if (statDef is null) continue;
            var element = new StatCollectionElement(statDef) { Modifier = overrideDef };
            ProcessElement(element, usedStats, true);
        }

        foreach (var element in usedStats.Values)
        {
            if (element.IsPassive) continue;
            yield return element;
        }
    }

    private static void ProcessElement(StatCollectionElement element, Dictionary<string, StatCollectionElement> usedStats, bool force = false)
    {
        if (!force && usedStats.ContainsKey(element.StatDef.defName))
        {
            usedStats[element.StatDef.defName].UpdateFrom(element);
            return;
        }

        usedStats.Remove(element.StatDef.defName);
        usedStats.Add(element.StatDef.defName, element);
    }

    private static void CollectDefsTo<T>(ICollection<string> list, Func<T, List<string>> provider) where T : Def
    {
        list.Clear();
        foreach (var def in DefDatabase<T>.AllDefs)
        foreach (var stat in provider(def))
            if (stat.EndsWith("*"))
                foreach (var statDef in DefDatabase<StatDef>.AllDefs.Where(s => s.defName.StartsWith(stat.Substring(0, stat.Length - 1))))
                    list.Add(statDef.defName);
            else if (stat.StartsWith("*"))
                foreach (var statDef in DefDatabase<StatDef>.AllDefs.Where(s => s.defName.StartsWith(stat.Substring(0, stat.Length - 1))))
                    list.Add(statDef.defName);
            else
                list.Add(DefDatabase<StatDef>.GetNamed(stat, false)?.defName ?? "");
        list.Remove("");
    }

    public static Instruction GenerateNewInstruction() => StatCache.RandomElementByWeight(element => short.MaxValue - element.Order()).Generate();
}
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._Ares.Stats;

public sealed partial class TempStatModifierSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<TempStatModifierComponent>();
        var curTime = _timing.CurTime;

        while (query.MoveNext(out var uid, out var comp))
        {
            var modified = false;

            foreach (var (statId, mods) in comp.Modifiers)
            {
                for (var i = mods.Count - 1; i >= 0; i--)
                {
                    if (curTime < mods[i].EndTime)
                        continue;

                    mods.RemoveAt(i);
                    modified = true;
                }
            }

            if (modified)
            {
                Dirty(uid, comp);
                RemoveEmptyEntries(comp);
            }
        }
    }

    public void AddModifier(EntityUid uid, ProtoId<StatPrototype> statId, int delta, TimeSpan duration, string? source = null)
    {
        var comp = EnsureComp<TempStatModifierComponent>(uid);

        if (!comp.Modifiers.TryGetValue(statId, out var mods))
        {
            mods = new List<TempStatMod>();
            comp.Modifiers[statId] = mods;
        }

        if (source != null)
        {
            for (var i = 0; i < mods.Count; i++)
            {
                if (mods[i].Source == source)
                {
                    mods[i] = new TempStatMod
                    {
                        Delta = delta,
                        EndTime = _timing.CurTime + duration,
                        Source = source
                    };
                    Dirty(uid, comp);
                    return;
                }
            }
        }

        mods.Add(new TempStatMod
        {
            Delta = delta,
            EndTime = _timing.CurTime + duration,
            Source = source
        });
        Dirty(uid, comp);
    }

    private static void RemoveEmptyEntries(TempStatModifierComponent comp)
    {
        var toRemove = new List<ProtoId<StatPrototype>>();
        foreach (var (statId, mods) in comp.Modifiers)
        {
            if (mods.Count == 0)
                toRemove.Add(statId);
        }
        foreach (var statId in toRemove)
        {
            comp.Modifiers.Remove(statId);
        }
    }
}

// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Server._Ares.Perks;
using Content.Shared._Ares.Oddity;
using Content.Shared._Ares.Perks;
using Content.Shared._Ares.Sanity.Components;
using Content.Shared._Ares.Stats;
using Content.Shared.Examine;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._Ares.Oddity;

public sealed partial class OdditySystem : EntitySystem
{
    [Dependency] private readonly AresStatsSystem _stats = default!;
    [Dependency] private readonly PerkSystem _perks = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<OddityComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<OddityComponent, GetVerbsEvent<AlternativeVerb>>(AddUseVerb);
        SubscribeLocalEvent<OddityComponent, ExaminedEvent>(OnExamined);
    }

    private void OnMapInit(Entity<OddityComponent> ent, ref MapInitEvent args)
    {
        RollStats(ent.Comp);
        RollPerk(ent.Comp);
    }

    private void OnExamined(Entity<OddityComponent> ent, ref ExaminedEvent args)
    {
        var comp = ent.Comp;
        if (comp.GrantedStats.Count == 0 && string.IsNullOrEmpty(comp.RolledPerk))
            return;

        foreach (var (statId, value) in comp.GrantedStats)
        {
            if (!_prototypes.TryIndex(statId, out var stat))
                continue;

            args.PushMarkup(Loc.GetString("oddity-examine-stat",
                ("stat", Loc.GetString(stat.Name)),
                ("value", value),
                ("color", stat.Color.ToHex())));
        }

        if (!string.IsNullOrEmpty(comp.RolledPerk)
            && _prototypes.TryIndex(comp.RolledPerk, out var perk))
        {
            args.PushMarkup(Loc.GetString("oddity-examine-perk",
                ("perk", Loc.GetString(perk.Name))));
        }
    }

    private void RollStats(OddityComponent comp)
    {
        comp.GrantedStats.Clear();

        foreach (var range in comp.StatRanges)
        {
            if (!_prototypes.HasIndex(range.Stat))
                continue;

            var value = range.Max > range.Min
                ? _random.Next(range.Min, range.Max + 1)
                : range.Min;

            comp.GrantedStats[range.Stat] = value;
        }
    }

    private void RollPerk(OddityComponent comp)
    {
        comp.RolledPerk = string.Empty;

        if (comp.PerkChance <= 0 || !_random.Prob(comp.PerkChance))
            return;

        var oddityPerks = _prototypes.EnumeratePrototypes<PerkPrototype>()
            .Where(p => p.IsOddity)
            .Select(p => new ProtoId<PerkPrototype>(p.ID))
            .ToList();

        if (oddityPerks.Count > 0)
            comp.RolledPerk = oddityPerks[_random.Next(oddityPerks.Count)];
    }

    private void AddUseVerb(Entity<OddityComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanInteract || args.Hands == null)
            return;

        var user = args.User;

        if (!TryComp<RestComponent>(user, out var rest) || !rest.LevelUpPending)
            return;

        if (ent.Comp.GrantedStats.Count == 0 && string.IsNullOrEmpty(ent.Comp.RolledPerk))
            return;

        args.Verbs.Add(new AlternativeVerb
        {
            Act = () => UseOddity(ent, user),
            Text = Loc.GetString("oddity-use-verb"),
            Message = Loc.GetString("oddity-use-verb-desc", ("grants", DescribeGrants(ent.Comp))),
            Priority = 2,
        });
    }

    private string DescribeGrants(OddityComponent comp)
    {
        var parts = new List<string>();
        foreach (var (statId, value) in comp.GrantedStats)
        {
            if (!_prototypes.TryIndex(statId, out var stat))
                continue;

            parts.Add($"{Loc.GetString(stat.Name)} +{value}");
        }

        if (!string.IsNullOrEmpty(comp.RolledPerk)
            && _prototypes.TryIndex(comp.RolledPerk, out var perk))
        {
            parts.Add(Loc.GetString("oddity-grant-perk", ("perk", Loc.GetString(perk.Name))));
        }

        return string.Join(", ", parts);
    }

    private void UseOddity(Entity<OddityComponent> ent, EntityUid user)
    {
        if (!Exists(ent) || !HasComp<OddityComponent>(ent))
            return;

        if (!TryComp<RestComponent>(user, out var rest) || !rest.LevelUpPending)
            return;

        rest.LevelUpPending = false;
        Dirty(user, rest);

        foreach (var (statId, value) in ent.Comp.GrantedStats)
            _stats.ModifyStatLevel(user, statId, value);

        if (!string.IsNullOrEmpty(ent.Comp.RolledPerk))
            _perks.ApplyPerk(user, ent.Comp.RolledPerk);

        _popup.PopupEntity(
            Loc.GetString("oddity-popup-used", ("grants", DescribeGrants(ent.Comp))),
            user,
            user);

        RemCompDeferred<OddityComponent>(ent);
    }
}
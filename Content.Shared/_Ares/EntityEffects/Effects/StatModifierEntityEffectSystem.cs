// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Shared._Ares.Stats;

public sealed partial class StatModifierEntityEffectSystem : EntityEffectSystem<MetaDataComponent, StatModifier>
{
    [Dependency] private readonly TempStatModifierSystem _mods = default!;

    protected override void Effect(Entity<MetaDataComponent> entity, ref EntityEffectEvent<StatModifier> args)
    {
        var time = TimeSpan.FromTicks((long)(args.Effect.Time.Ticks * args.Scale));
        _mods.AddModifier(entity, args.Effect.Stat, args.Effect.Amount, time, args.Effect.Source);
    }
}

public sealed partial class StatModifier : EntityEffectBase<StatModifier>
{
    [DataField(required: true)]
    public ProtoId<StatPrototype> Stat;

    [DataField(required: true)]
    public int Amount;

    [DataField]
    public TimeSpan Time = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Source identifier for grouping. Modifiers with the same stat and source will refresh duration instead of stacking.
    /// Typically set to the reagent ID in YAML via $id.
    /// </summary>
    [DataField]
    public string? Source;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        if (!prototype.TryIndex(Stat, out var statProto))
            return null;

        var statColor = statProto.Color.ToHex();
        var amountColor = Amount >= 0 ? "#00cc44" : "#cc2222";
        var sign = Amount >= 0 ? "+" : "";

        return Loc.GetString("ares-effect-guidebook-stat-modifier",
            ("chance", Probability),
            ("amount", $"{sign}{Amount}"),
            ("stat", Loc.GetString(statProto.Name)),
            ("statColor", statColor),
            ("amountColor", amountColor),
            ("time", Time.TotalSeconds));
    }
}

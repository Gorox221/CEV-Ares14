// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Oddity;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Prototypes;

namespace Content.Client._Ares.Oddity.Systems;

public sealed class OddityVisualsSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<OddityComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<OddityComponent, AfterAutoHandleStateEvent>(OnHandleState);
        SubscribeLocalEvent<OddityComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(Entity<OddityComponent> ent, ref ComponentStartup args)
    {
        Apply(ent, ent.Comp);
    }

    private void OnHandleState(Entity<OddityComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        Apply(ent, ent.Comp);
    }

    private void OnShutdown(EntityUid uid, OddityComponent comp, ComponentShutdown args)
    {
        if (TryComp<SpriteComponent>(uid, out var sprite))
            sprite.PostShader = null;
    }

    private void Apply(EntityUid uid, OddityComponent comp)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        if (_proto.TryIndex<ShaderPrototype>(comp.Shader, out var proto))
            sprite.PostShader = proto.InstanceUnique();
    }
}

// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Text;
using Content.Shared._Ares.Sanity.Components;
using Robust.Client.GameObjects;
using Robust.Client.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Client._Ares.Sanity.Systems;

public sealed partial class FabricBreakdownClientSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _playerMan = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;
    [Dependency] private readonly IPrototypeManager _protos = default!;

    private static readonly char[] GarbleSymbols = { '&', '!', '#', '*', '(', '%', '@', '$', '?' };

    private readonly Dictionary<NetEntity, GhostSpriteSwap> _swaps = new();

    public FabricBreakdownClientSystem()
    {
        UpdatesAfter.Add(typeof(AppearanceSystem));
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);
        UpdateSwaps();
    }

    private void UpdateSwaps()
    {
        if (_playerMan.LocalEntity is not { } local || !TryComp<TheFabricBreakdownComponent>(local, out var fabric))
        {
            RestoreAll();
            return;
        }

        var desired = new HashSet<NetEntity>(fabric.Targets);

        foreach (var (netEnt, swap) in _swaps.ToArray())
        {
            if (desired.Contains(netEnt))
                continue;

            _swaps.Remove(netEnt);
            RestoreSwap(netEnt, swap);
        }

        foreach (var netEnt in desired)
        {
            if (_swaps.TryGetValue(netEnt, out var swap))
            {
                RefreshSwap(netEnt, swap);
                continue;
            }

            var ent = GetEntity(netEnt);
            if (!ent.IsValid() || !EntityManager.EntityExists(ent))
                continue;

            if (!TryComp<SpriteComponent>(ent, out _))
                continue;

            var textureIds = fabric.Textures;
            if (textureIds.Count == 0)
                continue;

            var texId = textureIds[Random.Shared.Next(textureIds.Count)];
            if (!_protos.TryIndex(texId, out var texture))
                continue;

            var ghostSwap = new GhostSpriteSwap(texture.RsiPath, texture.RsiState);

            _swaps[netEnt] = ghostSwap;
            RefreshSwap(netEnt, ghostSwap);
        }
    }

    private void RefreshSwap(NetEntity netEnt, GhostSpriteSwap swap)
    {
        var ent = GetEntity(netEnt);
        if (!ent.IsValid() || !EntityManager.EntityExists(ent) || !TryComp<SpriteComponent>(ent, out var sprite))
        {
            _swaps.Remove(netEnt);
            return;
        }

        swap.Refresh((ent, sprite), _sprite);
    }

    private void RestoreSwap(NetEntity netEnt, GhostSpriteSwap swap)
    {
        var ent = GetEntity(netEnt);
        if (!ent.IsValid() || !EntityManager.EntityExists(ent) || !TryComp<SpriteComponent>(ent, out var sprite))
            return;

        swap.Restore((ent, sprite), _sprite);
    }

    private void RestoreAll()
    {
        if (_swaps.Count == 0)
            return;

        foreach (var (netEnt, swap) in _swaps.ToArray())
            RestoreSwap(netEnt, swap);

        _swaps.Clear();
    }

    public bool HasFabric()
    {
        return _playerMan.LocalEntity is { } local && HasComp<TheFabricBreakdownComponent>(local);
    }

    public bool IsHallucinated(EntityUid target)
    {
        if (_playerMan.LocalEntity is not { } local)
            return false;

        if (!TryComp<TheFabricBreakdownComponent>(local, out var fabric))
            return false;

        var net = GetNetEntity(target);
        return fabric.Targets.Contains(net);
    }

    public string? Garble(string? wrappedMessage)
    {
        if (string.IsNullOrEmpty(wrappedMessage))
            return wrappedMessage;

        if (!FormattedMessage.TryFromMarkup(wrappedMessage, out var parsed, out _))
            return wrappedMessage;

        var rebuilt = new FormattedMessage();
        foreach (var node in parsed.Nodes)
        {
            if (node.Name == null && node.Value.StringValue is { } text)
                rebuilt.AddText(GarbleText(text));
            else
                rebuilt.PushTag(node);
        }

        return rebuilt.ToMarkup();
    }

    private static string GarbleText(string text)
    {
        var builder = new StringBuilder(text.Length);
        var changed = false;

        foreach (var ch in text)
        {
            if (char.IsWhiteSpace(ch))
            {
                builder.Append(ch);
                continue;
            }

            builder.Append(GarbleSymbols[Random.Shared.Next(GarbleSymbols.Length)]);
            changed = true;
        }

        return changed ? builder.ToString() : text;
    }

    private sealed class GhostSpriteSwap
    {
        private readonly string _rsiPath;
        private readonly string _rsiState;

        private readonly List<(int Index, bool Visible)> _original = new();
        private SpriteComponent.Layer? _ghostLayer;

        public GhostSpriteSwap(string rsiPath, string rsiState)
        {
            _rsiPath = rsiPath;
            _rsiState = rsiState;
        }

        public void Refresh(Entity<SpriteComponent?> ent, SpriteSystem sprite)
        {
            var comp = ent.Comp;
            if (comp == null)
                return;

            var ghostIndex = IndexOf(comp, _ghostLayer);
            if (ghostIndex >= 0)
            {
                var index = 0;
                foreach (var layer in comp.AllLayers)
                {
                    if (layer is SpriteComponent.Layer concrete && index != ghostIndex)
                        sprite.LayerSetVisible(ent, index, false);
                    index++;
                }

                return;
            }

            Apply(ent, comp, sprite);
        }

        private void Apply(Entity<SpriteComponent?> ent, SpriteComponent comp, SpriteSystem sprite)
        {
            _original.Clear();
            _ghostLayer = null;

            var index = 0;
            foreach (var layer in comp.AllLayers)
            {
                if (layer is not SpriteComponent.Layer concrete)
                {
                    index++;
                    continue;
                }

                _original.Add((index, concrete.Visible));
                sprite.LayerSetVisible(ent, index, false);
                index++;
            }

            var ghostIndex = sprite.AddRsiLayer(ent, _rsiState, new ResPath(_rsiPath));
            if (ghostIndex >= 0)
                sprite.TryGetLayer(ent, ghostIndex, out _ghostLayer, logMissing: false);
        }

        public void Restore(Entity<SpriteComponent?> ent, SpriteSystem sprite)
        {
            var comp = ent.Comp;
            if (comp == null)
                return;

            if (_ghostLayer != null)
            {
                var ghostIndex = IndexOf(comp, _ghostLayer);
                if (ghostIndex >= 0)
                    sprite.RemoveLayer(ent, ghostIndex, logMissing: false);

                _ghostLayer = null;
            }

            foreach (var (index, visible) in _original)
            {
                if (!sprite.LayerExists(ent, index))
                    continue;

                sprite.LayerSetVisible(ent, index, visible);
            }

            _original.Clear();
        }

        private static int IndexOf(SpriteComponent sprite, SpriteComponent.Layer? target)
        {
            if (target == null)
                return -1;

            var index = 0;
            foreach (var layer in sprite.AllLayers)
            {
                if (ReferenceEquals(layer, target))
                    return index;
                index++;
            }

            return -1;
        }
    }
}

// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Client.Resources;
using Content.Client.Stylesheets;
using Content.Shared._Ares.LockHack;
using Content.Shared.Wires;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.Input;
using static Robust.Client.UserInterface.Controls.BoxContainer;

namespace Content.Client._Ares.LockHack.UI;

public sealed class LockHackMenu : BaseWindow
{
    [Dependency] private readonly IResourceCache _resourceCache = default!;

    private readonly Control _wiresHBox;
    private readonly Label _nameLabel;

    public event Action<int>? OnPulse;

    public LockHackMenu()
    {
        IoCManager.InjectDependencies(this);

        var rootContainer = new LayoutContainer { Name = "LockHackRoot" };
        AddChild(rootContainer);

        MouseFilter = MouseFilterMode.Stop;

        var panelTex = _resourceCache.GetTexture("/Textures/Interface/Nano/button.svg.96dpi.png");
        var back = new StyleBoxTexture
        {
            Texture = panelTex,
            Modulate = Color.FromHex("#25252A"),
        };
        back.SetPatchMargin(StyleBox.Margin.All, 10);

        var topPanel = new PanelContainer
        {
            PanelOverride = back,
            MouseFilter = MouseFilterMode.Pass
        };
        var bottomWrap = new LayoutContainer
        {
            Name = "BottomWrap"
        };
        var bottomPanel = new PanelContainer
        {
            PanelOverride = back,
            MouseFilter = MouseFilterMode.Pass
        };

        var wrappingHBox = new BoxContainer
        {
            Orientation = LayoutOrientation.Horizontal
        };
        _wiresHBox = new BoxContainer
        {
            Orientation = LayoutOrientation.Horizontal,
            SeparationOverride = 4,
            VerticalAlignment = VAlignment.Bottom
        };

        wrappingHBox.AddChild(new Control { MinSize = new Vector2(20, 0) });
        wrappingHBox.AddChild(_wiresHBox);
        wrappingHBox.AddChild(new Control { MinSize = new Vector2(20, 0) });

        bottomWrap.AddChild(bottomPanel);

        LayoutContainer.SetAnchorPreset(bottomPanel, LayoutContainer.LayoutPreset.BottomWide);
        LayoutContainer.SetMarginTop(bottomPanel, -55);

        bottomWrap.AddChild(wrappingHBox);
        LayoutContainer.SetAnchorPreset(wrappingHBox, LayoutContainer.LayoutPreset.Wide);
        LayoutContainer.SetMarginBottom(wrappingHBox, -4);

        rootContainer.AddChild(topPanel);
        rootContainer.AddChild(bottomWrap);

        LayoutContainer.SetAnchorPreset(topPanel, LayoutContainer.LayoutPreset.Wide);
        LayoutContainer.SetMarginBottom(topPanel, -80);

        LayoutContainer.SetAnchorPreset(bottomWrap, LayoutContainer.LayoutPreset.VerticalCenterWide);
        LayoutContainer.SetGrowHorizontal(bottomWrap, LayoutContainer.GrowDirection.Both);

        var topContainerWrap = new BoxContainer
        {
            Orientation = LayoutOrientation.Vertical,
            Children =
            {
                (_nameLabel = new Label
                {
                    Text = Loc.GetString("ares-lockhack-title"),
                    FontOverride = _resourceCache.GetFont("/Fonts/Boxfont-round/Boxfont Round.ttf", 13),
                    StyleClasses = { StyleClass.LabelKeyText },
                    Margin = new Thickness(4, 2, 12, 2),
                }),
                new Control { MinSize = new Vector2(0, 95) }
            }
        };

        rootContainer.AddChild(topContainerWrap);

        LayoutContainer.SetAnchorPreset(topContainerWrap, LayoutContainer.LayoutPreset.Wide);

        SetHeight = 200;
        MinWidth = 400;
    }

    public void Populate(LockHackBoundUserInterfaceState state)
    {
        _wiresHBox.RemoveAllChildren();

        var random = new Random(state.Wires.Length);
        foreach (var wire in state.Wires)
        {
            if (wire.Id >= 16)
                break;

            var mirror = random.Next(2) == 0;
            var flip = random.Next(2) == 0;
            var type = random.Next(2);
            var control = new WireHackControl(wire.Color, wire.Letter, wire.Pulsed, flip, mirror, type, _resourceCache)
            {
                VerticalAlignment = VAlignment.Bottom
            };
            _wiresHBox.AddChild(control);

            var wireId = wire.Id;
            control.ContactsClicked += () =>
            {
                OnPulse?.Invoke(wireId);
            };
        }
    }

    protected override DragMode GetDragModeFor(Vector2 relativeMousePos)
    {
        return DragMode.Move;
    }

    protected override bool HasPoint(Vector2 point)
    {
        return false;
    }

    private sealed class WireHackControl : Control
    {
        private readonly IResourceCache _resourceCache;

        private const string TextureContact = "/Textures/Interface/WireHacking/contact.svg.96dpi.png";

        public event Action? ContactsClicked;

        public WireHackControl(WireColor color, WireLetter letter, bool pulsed, bool flip, bool mirror, int type,
            IResourceCache resourceCache)
        {
            _resourceCache = resourceCache;

            HorizontalAlignment = HAlignment.Center;
            MouseFilter = MouseFilterMode.Stop;

            var layout = new LayoutContainer();
            AddChild(layout);

            var greek = new Label
            {
                Text = letter.Letter().ToString(),
                VerticalAlignment = VAlignment.Bottom,
                HorizontalAlignment = HAlignment.Center,
                Align = Label.AlignMode.Center,
                FontOverride = _resourceCache.GetFont("/Fonts/NotoSansDisplay/NotoSansDisplay-Bold.ttf", 12),
                FontColorOverride = pulsed ? Color.FromHex("#44FF44") : Color.Gray,
                ToolTip = letter.Name(),
                MouseFilter = MouseFilterMode.Stop
            };

            layout.AddChild(greek);
            LayoutContainer.SetAnchorPreset(greek, LayoutContainer.LayoutPreset.BottomWide);
            LayoutContainer.SetGrowVertical(greek, LayoutContainer.GrowDirection.Begin);
            LayoutContainer.SetGrowHorizontal(greek, LayoutContainer.GrowDirection.Both);

            var contactTexture = _resourceCache.GetTexture(TextureContact);
            var contact1 = new TextureRect
            {
                Texture = contactTexture,
                Modulate = pulsed ? Color.FromHex("#44FF44") : Color.FromHex("#E1CA76")
            };

            layout.AddChild(contact1);
            LayoutContainer.SetPosition(contact1, new Vector2(0, 0));

            var contact2 = new TextureRect
            {
                Texture = contactTexture,
                Modulate = pulsed ? Color.FromHex("#44FF44") : Color.FromHex("#E1CA76")
            };

            layout.AddChild(contact2);
            LayoutContainer.SetPosition(contact2, new Vector2(0, 60));

            var wire = new WireRender(color, pulsed, flip, mirror, type, _resourceCache);

            layout.AddChild(wire);
            LayoutContainer.SetPosition(wire, new Vector2(2, 16));

            ToolTip = Loc.GetString("ares-lockhack-wire-tooltip", ("color", color.Name()), ("letter", letter.Name()));
            MinSize = new Vector2(20, 102);
        }

        protected override void KeyBindDown(GUIBoundKeyEventArgs args)
        {
            base.KeyBindDown(args);

            if (args.Function != EngineKeyFunctions.UIClick)
                return;

            ContactsClicked?.Invoke();
        }

        protected override bool HasPoint(Vector2 point)
        {
            return base.HasPoint(point) && point.Y <= 80;
        }

        private sealed class WireRender : Control
        {
            private readonly WireColor _color;
            private readonly bool _pulsed;
            private readonly bool _flip;
            private readonly bool _mirror;
            private readonly int _type;

            private static readonly string[] TextureNormal =
            {
                "/Textures/Interface/WireHacking/wire_1.svg.96dpi.png",
                "/Textures/Interface/WireHacking/wire_2.svg.96dpi.png"
            };

            private static readonly string[] TextureCut =
            {
                "/Textures/Interface/WireHacking/wire_1_cut.svg.96dpi.png",
                "/Textures/Interface/WireHacking/wire_2_cut.svg.96dpi.png",
            };

            private readonly IResourceCache _resourceCache;

            public WireRender(WireColor color, bool pulsed, bool flip, bool mirror, int type,
                IResourceCache resourceCache)
            {
                _resourceCache = resourceCache;
                _color = color;
                _pulsed = pulsed;
                _flip = flip;
                _mirror = mirror;
                _type = type;

                SetSize = new Vector2(16, 50);
            }

            protected override void Draw(DrawingHandleScreen handle)
            {
                var tex = _resourceCache.GetTexture(_pulsed ? TextureCut[_type] : TextureNormal[_type]);

                var l = 0f;
                var r = tex.Width + l;
                var t = 0f;
                var b = tex.Height + t;

                if (_flip)
                    (t, b) = (b, t);

                if (_mirror)
                    (l, r) = (r, l);

                l *= UIScale;
                r *= UIScale;
                t *= UIScale;
                b *= UIScale;

                var rect = new UIBox2(l, t, r, b);
                handle.DrawTextureRect(tex, rect, _pulsed ? Color.LimeGreen : _color.ColorValue());
            }
        }
    }
}

// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using System.Numerics; // Ares-tweak
using Content.Client.CharacterInfo;
using Content.Client.Gameplay;
using Content.Client.Stylesheets;
using Content.Client.UserInterface.Controls;
using Content.Client.UserInterface.Systems.Character.Controls;
using Content.Client.UserInterface.Systems.Character.Windows;
using Content.Client.UserInterface.Systems.Objectives.Controls;
using Content.Shared.Input;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Roles;
using Content.Shared._Ares.Perks; // Ares-tweak
using Content.Shared._Ares.Sanity.Components; // Ares-tweak
using Content.Shared._Ares.Sanity.Events; // Ares-tweak
using Content.Shared._Ares.Stats; // Ares-tweak
using JetBrains.Annotations;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Input.Binding;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using static Content.Client.CharacterInfo.CharacterInfoSystem;
using static Robust.Client.UserInterface.Controls.BaseButton;

namespace Content.Client.UserInterface.Systems.Character;

[UsedImplicitly]
public sealed class CharacterUIController : UIController, IOnStateEntered<GameplayState>, IOnStateExited<GameplayState>, IOnSystemChanged<CharacterInfoSystem>
{
    [Dependency] private readonly IEntityManager _ent = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    [UISystemDependency] private readonly CharacterInfoSystem _characterInfo = default!;
    [UISystemDependency] private readonly SpriteSystem _sprite = default!;
    [UISystemDependency] private readonly AresStatsSystem _stats = default!; // Ares-tweak

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<MindRoleTypeChangedEvent>(OnRoleTypeChanged);
    }

    private CharacterWindow? _window;
    private MenuButton? CharacterButton => UIManager.GetActiveUIWidgetOrNull<MenuBar.Widgets.GameTopMenuBar>()?.CharacterButton;

    public void OnStateEntered(GameplayState state)
    {
        DebugTools.Assert(_window == null);

        _window = UIManager.CreateWindow<CharacterWindow>();
        LayoutContainer.SetAnchorPreset(_window, LayoutContainer.LayoutPreset.CenterTop);

       // Ares-tweak start
        _window.TabContainer.SetTabTitle(0, Loc.GetString("character-info-tab-info"));
        _window.TabContainer.SetTabTitle(1, Loc.GetString("character-info-tab-stats"));
        // Ares-tweak end

        _window.OnClose += DeactivateButton;
        _window.OnOpen += ActivateButton;

        CommandBinds.Builder
            .Bind(ContentKeyFunctions.OpenCharacterMenu,
                InputCmdHandler.FromDelegate(_ => ToggleWindow()))
            .Register<CharacterUIController>();
    }

    public void OnStateExited(GameplayState state)
    {
        if (_window != null)
        {
            _window.Close();
            _window = null;
        }

        CommandBinds.Unregister<CharacterUIController>();
    }

    public void OnSystemLoaded(CharacterInfoSystem system)
    {
        system.OnCharacterUpdate += CharacterUpdated;
        _player.LocalPlayerDetached += CharacterDetached;
    }

    public void OnSystemUnloaded(CharacterInfoSystem system)
    {
        system.OnCharacterUpdate -= CharacterUpdated;
        _player.LocalPlayerDetached -= CharacterDetached;
    }

    public void UnloadButton()
    {
        if (CharacterButton == null)
        {
            return;
        }

        CharacterButton.OnPressed -= CharacterButtonPressed;
    }

    public void LoadButton()
    {
        if (CharacterButton == null)
        {
            return;
        }

        CharacterButton.OnPressed += CharacterButtonPressed;
    }

    private void DeactivateButton()
    {
        if (CharacterButton == null)
        {
            return;
        }

        CharacterButton.Pressed = false;
    }

    private void ActivateButton()
    {
        if (CharacterButton == null)
        {
            return;
        }

        CharacterButton.Pressed = true;
    }

    private void CharacterUpdated(CharacterData data)
    {
        if (_window == null)
        {
            return;
        }

        var (entity, job, objectives, briefing, entityName, memories) = data;

        _window.SpriteView.SetEntity(entity);

        UpdateRoleType();

        _window.NameLabel.Text = entityName;
        _window.SubText.Text = job;
        _window.Objectives.RemoveAllChildren();
        _window.Memories.RemoveAllChildren();
        _window.ObjectivesLabel.Visible = objectives.Any();

        foreach (var (groupId, conditions) in objectives)
        {
            var objectiveControl = new CharacterObjectiveControl
            {
                Orientation = BoxContainer.LayoutOrientation.Vertical,
                Modulate = Color.Gray
            };


            var objectiveText = new FormattedMessage();
            objectiveText.TryAddMarkup(groupId, out _);

            var objectiveLabel = new RichTextLabel
            {
                StyleClasses = { StyleClass.TooltipTitle }
            };
            objectiveLabel.SetMessage(objectiveText);

            objectiveControl.AddChild(objectiveLabel);

            foreach (var condition in conditions)
            {
                var conditionControl = new ObjectiveConditionsControl();
                conditionControl.ProgressTexture.Texture = _sprite.Frame0(condition.Icon);
                conditionControl.ProgressTexture.Progress = condition.Progress;
                var titleMessage = new FormattedMessage();
                var descriptionMessage = new FormattedMessage();
                titleMessage.AddText(condition.Title);
                descriptionMessage.AddText(condition.Description);

                conditionControl.Title.SetMessage(titleMessage);
                conditionControl.Description.SetMessage(descriptionMessage);

                objectiveControl.AddChild(conditionControl);
            }

            _window.Objectives.AddChild(objectiveControl);
        }

        foreach (var (memoryName, memoryValue) in memories)
        {
            var memoryControl = new BoxContainer()
            {
                Orientation = BoxContainer.LayoutOrientation.Vertical,
                Modulate = Color.Gray
            };
            var text = Loc.TryGetString(memoryName, out var t, ("value", memoryValue))
                ? t
                : $"{memoryName}: {memoryValue}";

            memoryControl.AddChild(new Label
            {
                Text = text,
            });
            _window.Memories.AddChild(memoryControl);
        }

        if (briefing != null)
        {
            var briefingControl = new ObjectiveBriefingControl();
            var text = new FormattedMessage();
            text.PushColor(Color.Yellow);
            text.AddText(briefing);
            briefingControl.Label.SetMessage(text);
            _window.Objectives.AddChild(briefingControl);
        }

        var controls = _characterInfo.GetCharacterInfoControls(entity);
        foreach (var control in controls)
        {
            _window.Objectives.AddChild(control);
        }

        _window.RolePlaceholder.Visible = briefing == null && !controls.Any() && !objectives.Any();

    // Ares-tweak start
        PopulateStats();
    }

    private void PopulateStats()
    {
        if (_window == null)
            return;

        var player = _player.LocalEntity;
        if (player == null)
            return;

        _window.StatsContainer.RemoveAllChildren();

        // Ares-tweak start
        var statsPanel = new PanelContainer
        {
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = new Color(0.11f, 0.13f, 0.17f, 0.95f),
                BorderColor = new Color(0.45f, 0.6f, 0.95f, 0.4f),
                BorderThickness = new Thickness(1),
                ContentMarginLeftOverride = 10,
                ContentMarginRightOverride = 10,
                ContentMarginTopOverride = 6,
                ContentMarginBottomOverride = 8,
            },
        };
        var statsBox = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            HorizontalExpand = true,
        };

        var statsHeader = new Label
        {
            Text = Loc.GetString("character-info-stats-header"),
            StyleClasses = { StyleNano.StyleClassLabelHeadingBigger },
            FontColorOverride = new Color(0.55f, 0.7f, 1f),
        };
        statsBox.AddChild(statsHeader);

        foreach (var stat in _prototypeManager.EnumeratePrototypes<StatPrototype>())
        {
            var level = _stats.GetStatLevel(player.Value, new ProtoId<StatPrototype>(stat.ID));

            var row = new BoxContainer
            {
                Orientation = BoxContainer.LayoutOrientation.Horizontal,
                Margin = new Thickness(0, 4),
            };

            var nameLabel = new Label
            {
                Text = Loc.GetString(stat.Name),
                FontColorOverride = stat.Color,
                MinSize = new Vector2(130, 0),
                ToolTip = stat.Description != null ? Loc.GetString(stat.Description) : null,
            };
            row.AddChild(nameLabel);

            var valueLabel = new Label
            {
                Text = level.ToString(),
                Margin = new Thickness(4, 0, 0, 0),
                FontColorOverride = stat.Color,
            };
            row.AddChild(valueLabel);

            statsBox.AddChild(row);
        }

        statsPanel.AddChild(statsBox);
        _window.StatsContainer.AddChild(statsPanel);
        // Ares-tweak end

        // Ares-tweak start
        if (_ent.TryGetComponent(player.Value, out PerksComponent? perksComp) && perksComp.Perks.Count > 0)
        {
            var perksPanel = new PanelContainer
            {
                Margin = new Thickness(0, 8, 0, 0),
                PanelOverride = new StyleBoxFlat
                {
                    BackgroundColor = new Color(0.12f, 0.12f, 0.15f, 0.95f),
                    BorderColor = new Color(0.85f, 0.7f, 0.2f, 0.45f),
                    BorderThickness = new Thickness(1),
                    ContentMarginLeftOverride = 10,
                    ContentMarginRightOverride = 10,
                    ContentMarginTopOverride = 6,
                    ContentMarginBottomOverride = 8,
                },
            };
            var perksBox = new BoxContainer
            {
                Orientation = BoxContainer.LayoutOrientation.Vertical,
                HorizontalExpand = true,
            };

            var perksHeader = new Label
            {
                Text = Loc.GetString("character-info-perks-label"),
                StyleClasses = { StyleNano.StyleClassLabelHeadingBigger },
            };
            perksBox.AddChild(perksHeader);

            var perksScroll = new ScrollContainer
            {
                MinSize = new Vector2(0, 150),
                HorizontalExpand = true,
                Margin = new Thickness(0, 4, 0, 0),
            };
            var perksList = new BoxContainer
            {
                Orientation = BoxContainer.LayoutOrientation.Vertical,
                HorizontalExpand = true,
            };

            foreach (var perkId in perksComp.Perks)
            {
                if (!_prototypeManager.TryIndex(perkId, out PerkPrototype? perk))
                    continue;

                var perkLabel = new Label
                {
                    Text = $"- {Loc.GetString(perk.Name)}",
                    FontColorOverride = new Color(0.85f, 0.8f, 0.65f),
                    Margin = new Thickness(0, 2, 0, 0),
                    MouseFilter = Control.MouseFilterMode.Stop,
                    ToolTip = perk.Description != null ? Loc.GetString(perk.Description) : null,
                };
                perksList.AddChild(perkLabel);
            }

            perksScroll.AddChild(perksList);
            perksBox.AddChild(perksScroll);
            perksPanel.AddChild(perksBox);
            _window.StatsContainer.AddChild(perksPanel);
        }
        // Ares-tweak end

        // Ares-tweak start
        if (_ent.TryGetComponent(player.Value, out InsightComponent? insight))
        {
            var insightPanel = new PanelContainer
            {
                Margin = new Thickness(0, 8, 0, 0),
                PanelOverride = new StyleBoxFlat
                {
                    BackgroundColor = new Color(0.1f, 0.14f, 0.18f, 0.95f),
                    BorderColor = new Color(0.4f, 0.7f, 1f, 0.45f),
                    BorderThickness = new Thickness(1),
                    ContentMarginLeftOverride = 10,
                    ContentMarginRightOverride = 10,
                    ContentMarginTopOverride = 6,
                    ContentMarginBottomOverride = 8,
                },
            };
            var insightBox = new BoxContainer
            {
                Orientation = BoxContainer.LayoutOrientation.Vertical,
                HorizontalExpand = true,
            };

            var insightHeader = new Label
            {
                Text = Loc.GetString("character-info-insight-label"),
                StyleClasses = { StyleNano.StyleClassLabelHeadingBigger },
                FontColorOverride = new Color(0.6f, 0.85f, 1f),
            };
            insightBox.AddChild(insightHeader);

            var insightBar = new ProgressBar
            {
                MaxValue = insight.MaxInsight,
                MinValue = 0,
                Value = insight.CurrentInsight,
                HorizontalExpand = true,
                MinSize = new Vector2(0, 20),
                Margin = new Thickness(0, 4, 0, 0),
            };
            insightBox.AddChild(insightBar);

            if (_ent.TryGetComponent(player.Value, out RestComponent? rest) && rest.LevelUpPending)
            {
                var levelUpLabel = new Label
                {
                    Text = Loc.GetString("rest-levelup-ui"),
                    FontColorOverride = Color.Gold,
                    Margin = new Thickness(0, 4, 0, 0),
                };
                insightBox.AddChild(levelUpLabel);

                var btnContainer = new BoxContainer
                {
                    Orientation = BoxContainer.LayoutOrientation.Vertical,
                    Margin = new Thickness(0, 4, 0, 0),
                };

                var internalizeBtn = new Button
                {
                    Text = Loc.GetString("rest-button-internalize"),
                    MinSize = new Vector2(0, 30),
                    HorizontalExpand = true,
                    Margin = new Thickness(0, 2, 0, 0),
                    ToolTip = Loc.GetString("rest-button-internalize-desc"),
                };
                var uid = player.Value;
                var netEnt = _ent.GetNetEntity(uid);
                internalizeBtn.OnPressed += _ =>
                {
                    if (_ent.EntityNetManager != null)
                        _ent.EntityNetManager.SendSystemNetworkMessage(new RestLevelUpRequestEvent(netEnt, "Internalize"));
                };
                btnContainer.AddChild(internalizeBtn);

                var oddityHint = new RichTextLabel
                {
                    Text = Loc.GetString("rest-oddity-hint"),
                    StyleClasses = { StyleNano.StyleClassLabelSubText },
                    HorizontalExpand = true,
                    Margin = new Thickness(0, 2, 0, 0),
                };
                btnContainer.AddChild(oddityHint);

                insightBox.AddChild(btnContainer);
            }

            insightPanel.AddChild(insightBox);
            _window.StatsContainer.AddChild(insightPanel);
        }
        // Ares-tweak end

        // Ares-tweak start
        if (!_ent.HasComponent<InsightComponent>(player.Value)
            && _ent.TryGetComponent(player.Value, out RestComponent? restComp)
            && restComp.LevelUpPending)
        {
            var levelUpSeparator = new Control
            {
                MinSize = new Vector2(0, 8),
            };
            _window.StatsContainer.AddChild(levelUpSeparator);

            var levelUpLabel = new Label
            {
                Text = Loc.GetString("rest-levelup-ui"),
                FontColorOverride = Color.Gold,
            };
            _window.StatsContainer.AddChild(levelUpLabel);

            var btnContainer = new BoxContainer
            {
                Orientation = BoxContainer.LayoutOrientation.Horizontal,
                Margin = new Thickness(0, 4, 0, 0),
            };

            var internalizeBtn = new Button
            {
                Text = Loc.GetString("rest-button-internalize"),
                MinSize = new Vector2(120, 30),
                ToolTip = Loc.GetString("rest-button-internalize-desc"),
            };
            var uid = player.Value;
            var netId = _ent.GetNetEntity(uid);
            internalizeBtn.OnPressed += _ =>
            {
                if (_ent.EntityNetManager != null)
                    _ent.EntityNetManager.SendSystemNetworkMessage(new RestLevelUpRequestEvent(netId, "Internalize"));
            };
            btnContainer.AddChild(internalizeBtn);

            var oddityHint = new RichTextLabel
            {
                Text = Loc.GetString("rest-oddity-hint"),
                StyleClasses = { StyleNano.StyleClassLabelSubText },
                HorizontalExpand = true,
                VerticalAlignment = Control.VAlignment.Center,
                Margin = new Thickness(8, 0, 0, 0),
            };
            btnContainer.AddChild(oddityHint);

            _window.StatsContainer.AddChild(btnContainer);
        }
        // Ares-tweak end
    }

    private void OnRoleTypeChanged(MindRoleTypeChangedEvent ev, EntitySessionEventArgs _)
    {
        UpdateRoleType();
    }

    private void UpdateRoleType()
    {
        if (_window == null || !_window.IsOpen)
            return;

        if (!_ent.TryGetComponent<MindContainerComponent>(_player.LocalEntity, out var container)
            || container.Mind is null)
            return;

        if (!_ent.TryGetComponent<MindComponent>(container.Mind.Value, out var mind))
            return;

        if (!_prototypeManager.TryIndex(mind.RoleType, out var proto))
            Log.Error($"Player '{_player.LocalSession}' has invalid Role Type '{mind.RoleType}'. Displaying default instead");

        _window.RoleType.Text = Loc.GetString(proto?.Name ?? "role-type-crew-aligned-name");
        _window.RoleType.FontColorOverride = proto?.Color ?? Color.White;
    }

    private void CharacterDetached(EntityUid uid)
    {
        CloseWindow();
    }

    private void CharacterButtonPressed(ButtonEventArgs args)
    {
        ToggleWindow();
    }

    private void CloseWindow()
    {
        _window?.Close();
    }

    private void ToggleWindow()
    {
        if (_window == null)
            return;

        CharacterButton?.SetClickPressed(!_window.IsOpen);

        if (_window.IsOpen)
        {
            CloseWindow();
        }
        else
        {
            _characterInfo.RequestCharacterInfo();
            _window.Open();
        }
    }
}
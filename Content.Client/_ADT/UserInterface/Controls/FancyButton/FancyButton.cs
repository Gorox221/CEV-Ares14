using Robust.Client.Graphics;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Maths;

namespace Content.Client._ADT.UserInterface.Controls.FancyButton;

public sealed class FancyButton : ContainerButton
{
    private Color _color = Color.FromHex("#141F2F");
    private Color _borderColor = Color.FromHex("#4972A1");
    private Color _hoveredColor = Color.FromHex("#4972A1");
    private Color _disabledColor = Color.FromHex("#3f3f3fff");

    public ContainerButton Button => this;

    public Color Color
    {
        get => _color;
        set { _color = value; UpdateStyleBox(); }
    }

    public Color BorderColor
    {
        get => _borderColor;
        set { _borderColor = value; UpdateStyleBox(); }
    }

    public Color HoveredColor
    {
        get => _hoveredColor;
        set { _hoveredColor = value; UpdateStyleBox(); }
    }

    public Color DisabledColor
    {
        get => _disabledColor;
        set { _disabledColor = value; UpdateStyleBox(); }
    }

    public FancyButton()
    {
        UpdateStyleBox();
    }

    public void UpdateColor()
    {
        UpdateStyleBox();
    }

    private void UpdateStyleBox()
    {
        switch (DrawMode)
        {
            case DrawModeEnum.Normal:
                StyleBoxOverride = new StyleBoxFlat(_color)
                {
                    BorderColor = _borderColor,
                    BorderThickness = new Thickness(2),
                };
                break;
            case DrawModeEnum.Hover:
                StyleBoxOverride = new StyleBoxFlat(_hoveredColor)
                {
                    BorderColor = _borderColor,
                    BorderThickness = new Thickness(2),
                };
                break;
            case DrawModeEnum.Pressed:
                StyleBoxOverride = new StyleBoxFlat(_hoveredColor)
                {
                    BorderColor = _borderColor,
                    BorderThickness = new Thickness(2),
                };
                break;
            case DrawModeEnum.Disabled:
                StyleBoxOverride = new StyleBoxFlat(_disabledColor)
                {
                    BorderColor = _borderColor,
                    BorderThickness = new Thickness(2),
                };
                break;
        }
    }

    protected override void DrawModeChanged()
    {
        UpdateStyleBox();
    }
}

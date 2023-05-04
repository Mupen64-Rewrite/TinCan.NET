using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Media.TextFormatting;
using Avalonia.Themes.Simple;
using TinCan.NET.Helpers;
using Brushes = Avalonia.Media.Brushes;
using Pen = Avalonia.Media.Pen;

namespace TinCan.NET.Controls;

public class Joystick : Avalonia.Controls.Control
{
    private static readonly IPen BlackPen = new ImmutablePen(Colors.Black.ToUint32(), 1.0);
    private static readonly IPen RedPen = new ImmutablePen(Colors.Red.ToUint32(), 3.0);

    static Joystick()
    {
        AffectsRender<Joystick>(JoyXProperty, JoyYProperty);
    }

    public static StyledProperty<sbyte> JoyXProperty = AvaloniaProperty.Register<Joystick, sbyte>(nameof(JoyX));

    public sbyte JoyX
    {
        get => GetValue(JoyXProperty);
        set => SetValue(JoyXProperty, value);
    }
    
    public static StyledProperty<sbyte> JoyYProperty = AvaloniaProperty.Register<Joystick, sbyte>(nameof(JoyY));
    public sbyte JoyY
    {
        get => GetValue(JoyYProperty);
        set => SetValue(JoyYProperty, value);
    }
    

    public override void Render(DrawingContext c)
    {
        Point center = Bounds.Center;
        c.FillRectangle(Brushes.White, Bounds);
        c.DrawEllipse(null, BlackPen, Bounds);
        c.DrawLine(BlackPen, new Point(Bounds.Left, center.Y), new Point(Bounds.Right, center.Y));
        c.DrawLine(BlackPen, new Point(center.X, Bounds.Top), new Point(center.X, Bounds.Bottom));

        Point joyPos = new(
            MathHelpers.Lerp(Bounds.Left, Bounds.Right, ((double) JoyX + 128) / 256),
            MathHelpers.Lerp(Bounds.Bottom, Bounds.Top, ((double) JoyY + 128) / 256));
        
        c.DrawLine(RedPen, center, joyPos);
        c.DrawEllipse(Brushes.Blue, null, joyPos, 5.0, 5.0);
    }

    private void UpdatePosition(Point mousePos)
    {
        double normX = mousePos.X / Bounds.Width;
        double normY = mousePos.Y / Bounds.Height;

        JoyX = (sbyte) (Math.Clamp((int) normX * 256, 0, 255) - 128);
        JoyY = (sbyte) (Math.Clamp((int) normY * 256, 0, 255) - 128);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        UpdatePosition(e.GetPosition(this));
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        UpdatePosition(e.GetPosition(this));
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        UpdatePosition(e.GetPosition(this));
    }
}
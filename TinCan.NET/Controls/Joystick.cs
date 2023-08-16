using System;
using System.Numerics;
using Avalonia;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using TinCan.NET.Helpers;
using Brushes = Avalonia.Media.Brushes;
using Vector = Avalonia.Vector;

namespace TinCan.NET.Controls;

public class Joystick : Avalonia.Controls.Control
{
    private static readonly IPen OutlinePen = new ImmutablePen(Colors.Black.ToUInt32());
    private static readonly IPen LinePen = new ImmutablePen(Colors.Blue.ToUInt32(), 3.0);
    private static readonly IBrush TipBrush = new ImmutableSolidColorBrush(Colors.Red);

    static Joystick()
    {
        AffectsRender<Joystick>(JoyXProperty, JoyYProperty);
    }

    public static readonly StyledProperty<sbyte> JoyXProperty = AvaloniaProperty.Register<Joystick, sbyte>(
        nameof(JoyX), defaultBindingMode: BindingMode.TwoWay);

    public sbyte JoyX
    {
        get => GetValue(JoyXProperty);
        set => SetValue(JoyXProperty, value);
    }

    public static readonly StyledProperty<sbyte> JoyYProperty = AvaloniaProperty.Register<Joystick, sbyte>(
        nameof(JoyY), defaultBindingMode: BindingMode.TwoWay);

    public sbyte JoyY
    {
        get => GetValue(JoyYProperty);
        set => SetValue(JoyYProperty, value);
    }


    public override void Render(DrawingContext c)
    {
        Point center = Bounds.Center;
        c.DrawEllipse(Brushes.White, null, Bounds);

        c.DrawEllipse(null, OutlinePen, Bounds);
        c.DrawLine(OutlinePen, new Point(Bounds.Left, center.Y), new Point(Bounds.Right, center.Y));
        c.DrawLine(OutlinePen, new Point(center.X, Bounds.Top), new Point(center.X, Bounds.Bottom));

        Point joyPos = new(
            MathHelpers.Lerp(Bounds.Left, Bounds.Right, ((double) JoyX + 128) / 256),
            MathHelpers.Lerp(Bounds.Bottom, Bounds.Top, ((double) JoyY + 128) / 256));

        c.DrawLine(LinePen, center, joyPos);
        c.DrawEllipse(TipBrush, null, joyPos, 5.0, 5.0);
    }

    private void UpdatePosition(Point mousePos)
    {
        // Translate point so that center is at the origin
        var relPos = mousePos - Bounds.Center;
        // determine the "real" relative bounds (i.e. with the top right slightly clipped off as it only goes to 127 and not 128)
        var relBounds = new Rect(Bounds.Width / -2.0, Bounds.Height * (127.0 / -256.0), Bounds.Width * (255.0 / 256.0),
            Bounds.Height * (255.0 / 256.0));
        // Used to normalize distance to joystick range
        var relXDist = Bounds.Width / 2.0;
        var relYDist = Bounds.Height / 2.0;

        if (!relBounds.Contains(relPos))
        {
            // determine which side will be clipped against, then intersect the line from (0, 0) to relPos with
            // that side.
            // Notes: 
            // - PerpDot(A, B) > 0 if rotating from A -> B is closer CCW, and < 0 if closer CW
            // - The intersection formulas can be derived by considering any point on the line segment to be t * v where
            //   0 <= t <= 1. We can then solve for t using the known axis, then determine the unknown axis using the
            //   known value of t.

            // center to TR/BL are still a 45 degree line
            if (relPos.Y > -relPos.X)
            {
                // BottomRight is the dividing line between Q2 and Q3
                if (MathHelpers.PerpDot(relPos, relBounds.BottomRight) < 0.0)
                {
                    // Q2 (bottom)
                    relPos = new Point(relBounds.Bottom * relPos.X / relPos.Y, relBounds.Bottom);
                }
                else
                {
                    // Q3 (right)
                    relPos = new Point(relBounds.Right, relBounds.Right * relPos.Y / relPos.X);
                }
            }
            else
            {
                // TopLeft is the dividing line between Q0 and Q1
                if (MathHelpers.PerpDot(relPos, relBounds.TopLeft) < 0.0)
                {
                    // Q0 (top)
                    relPos = new Point(relBounds.Top * relPos.X / relPos.Y, relBounds.Top);
                }
                else
                {
                    // Q1 (left)
                    relPos = new Point(relBounds.Left, relBounds.Left * relPos.Y / relPos.X);
                }
            }
        }

        var relX = relPos.X * 128 / relXDist;
        var relY = relPos.Y * -128 / relYDist;
        if (relX is > -8 and < 8)
            relX = 0;
        if (relY is > -8 and < 8)
            relY = 0;
        JoyX = (sbyte) relX;
        JoyY = (sbyte) relY;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        PointerPoint ptrPoint = e.GetCurrentPoint(this);
        if (ptrPoint.Properties.IsLeftButtonPressed)
            UpdatePosition(ptrPoint.Position);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        PointerPoint ptrPoint = e.GetCurrentPoint(this);
        if (ptrPoint.Properties.IsLeftButtonPressed)
            UpdatePosition(ptrPoint.Position);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        PointerPoint ptrPoint = e.GetCurrentPoint(this);
        if (ptrPoint.Properties.IsLeftButtonPressed)
            UpdatePosition(ptrPoint.Position);
    }
}
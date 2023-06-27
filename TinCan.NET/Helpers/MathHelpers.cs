using System;
using Avalonia;

namespace TinCan.NET.Helpers;

/// <summary>
/// Provides math utilities not provided by <see cref="System.Math"/> or
/// <see cref="Avalonia.Utilities.MathUtilities"/>.
/// </summary>
public static class MathHelpers
{
    /// <summary>
    /// Linearly interpolates between two values.
    /// </summary>
    /// <param name="x">the first value</param>
    /// <param name="y"></param>
    /// <param name="t">some value, preferably between 0 and 1, but not always</param>
    /// <returns></returns>
    public static double Lerp(double x, double y, double t)
    {
        return x * (1.0 - t) + y * t;
    }

    public static Point Lerp(Point a, Point b, double t)
    {
        return new Point(Lerp(a.X, b.X, t), Lerp(a.Y, b.Y, t));
    }

    public static double PerpDot(Point a, Point b)
    {
        // same as this: a.X * b.Y - a.Y * b.X, but faster
        return Math.FusedMultiplyAdd(-a.Y, b.X, a.X * b.Y);
    }

    /// <summary>
    /// Intersects a line from (0, 0)
    /// </summary>
    /// <param name="p"></param>
    /// <param name="n"></param>
    /// <returns></returns>
    public static Point IntersectVectorToHorizontal(Point p, double n)
    {
        return new Point(n * p.X / p.Y, n);
    }
}
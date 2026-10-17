using System.Globalization;
using Astrarium.Port.Core;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace Astrarium.Avalonia.Views;

public sealed class SkyMapControl : Control
{
    public static readonly StyledProperty<SkyScene?> SceneProperty =
        AvaloniaProperty.Register<SkyMapControl, SkyScene?>(nameof(Scene));

    public static readonly StyledProperty<SkyChartObject?> SelectedObjectProperty =
        AvaloniaProperty.Register<SkyMapControl, SkyChartObject?>(nameof(SelectedObject), defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);

    public static readonly StyledProperty<bool> ShowStarsProperty =
        AvaloniaProperty.Register<SkyMapControl, bool>(nameof(ShowStars), true);

    public static readonly StyledProperty<bool> ShowConstellationsProperty =
        AvaloniaProperty.Register<SkyMapControl, bool>(nameof(ShowConstellations), true);

    private static readonly IBrush SkyFill = new SolidColorBrush(Color.Parse("#0B1424"));
    private static readonly IPen HorizonPen = new Pen(new SolidColorBrush(Color.Parse("#54BDAE")), 1.4);
    private static readonly IPen GridPen = new Pen(new SolidColorBrush(Color.Parse("#31445C")), 0.8);
    private static readonly IPen ConstellationPen = new Pen(new SolidColorBrush(Color.Parse("#54BDAE"), 0.33), 1.15);
    private static readonly IPen SelectionPen = new Pen(new SolidColorBrush(Color.Parse("#7DE1CF"), 0.92), 1.5);
    private static readonly IBrush BodyLabelBrush = new SolidColorBrush(Color.Parse("#F3F7FF"));
    private static readonly IBrush CompassBrush = new SolidColorBrush(Color.Parse("#8FA5C0"));
    private static readonly int[][] AsterismSegments =
    [
        [746, 3179], [3179, 4427], [4427, 6686], [6686, 8886],
        [25336, 25930], [25336, 27989], [25930, 26311], [26311, 26727], [26727, 27366], [27366, 24436], [24436, 25930],
        [54061, 53910], [53910, 58001], [58001, 59774], [59774, 62956], [62956, 65378], [65378, 67301], [59774, 54061],
        [91262, 102098], [102098, 97649], [97649, 91262]
    ];

    private static readonly Typeface MapTypeface = new("Inter");

    static SkyMapControl()
    {
        AffectsRender<SkyMapControl>(SceneProperty, SelectedObjectProperty, ShowStarsProperty, ShowConstellationsProperty);
    }

    public SkyScene? Scene
    {
        get => GetValue(SceneProperty);
        set => SetValue(SceneProperty, value);
    }

    public SkyChartObject? SelectedObject
    {
        get => GetValue(SelectedObjectProperty);
        set => SetValue(SelectedObjectProperty, value);
    }

    public bool ShowStars
    {
        get => GetValue(ShowStarsProperty);
        set => SetValue(ShowStarsProperty, value);
    }

    public bool ShowConstellations
    {
        get => GetValue(ShowConstellationsProperty);
        set => SetValue(ShowConstellationsProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var bounds = Bounds;
        context.DrawRectangle(new SolidColorBrush(Color.Parse("#08101D")), null, bounds, 20, 20);

        var diameter = Math.Max(0, Math.Min(bounds.Width - 44, bounds.Height - 44));
        if (diameter < 80)
            return;

        var radius = diameter / 2;
        var center = new Point(bounds.Width / 2, bounds.Height / 2 + 5);
        context.DrawEllipse(SkyFill, HorizonPen, center, radius, radius);

        DrawGrid(context, center, radius);
        DrawCompass(context, center, radius);

        var scene = Scene;
        if (scene is null)
            return;

        var starsByHip = scene.Objects
            .Where(item => item.Kind == SkyObjectKind.Star && item.Id.StartsWith("hip:", StringComparison.Ordinal))
            .ToDictionary(item => item.Id[4..], item => item, StringComparer.Ordinal);
        if (ShowConstellations)
            DrawConstellations(context, starsByHip, center, radius);

        if (ShowStars)
        {
            foreach (var item in scene.Objects)
            {
                if (item.Kind != SkyObjectKind.Star || item.AltitudeDegrees < 0)
                    continue;

                var point = ToScreen(item, center, radius);
                var size = Math.Clamp(5.2 - item.Magnitude * 0.62, 1.15, 5.7);
                var color = StarColor(item.ColorIndex);
                var brush = new SolidColorBrush(color);
                context.DrawEllipse(brush, null, point, size / 2, size / 2);

                if (item.Magnitude <= 1.45 || item.Id == SelectedObject?.Id)
                    DrawLabel(context, item.Name, new Point(point.X + size + 4, point.Y - 6), 10.5, item.Id == SelectedObject?.Id);
            }
        }

        DrawBodies(context, scene, center, radius);
        DrawSelectedInfo(context, center, radius);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (Scene is null)
            return;

        var pointer = e.GetPosition(this);
        var diameter = Math.Max(0, Math.Min(Bounds.Width - 44, Bounds.Height - 44));
        var radius = diameter / 2;
        var center = new Point(Bounds.Width / 2, Bounds.Height / 2 + 5);
        var nearest = Scene.Objects
            .Where(item => item.AltitudeDegrees >= 0)
            .Select(item => (Item: item, Point: ToScreen(item, center, radius)))
            .Select(pair => (pair.Item, Distance: Math.Sqrt(Math.Pow(pair.Point.X - pointer.X, 2) + Math.Pow(pair.Point.Y - pointer.Y, 2))))
            .Where(pair => pair.Distance <= (pair.Item.Kind == SkyObjectKind.Star ? 12 : 18))
            .OrderBy(pair => pair.Distance)
            .FirstOrDefault();

        if (nearest.Item is not null)
        {
            SetCurrentValue(SelectedObjectProperty, nearest.Item);
            e.Handled = true;
        }
    }

    private static void DrawGrid(DrawingContext context, Point center, double radius)
    {
        foreach (var altitude in new[] { 30.0, 60.0 })
        {
            var ringRadius = radius * (90 - altitude) / 90;
            context.DrawEllipse(null, GridPen, center, ringRadius, ringRadius);
        }

        foreach (var azimuth in new[] { 45.0, 90.0, 135.0, 180.0, 225.0, 270.0, 315.0 })
        {
            var angle = Angle.ToRadians(azimuth);
            var edge = new Point(center.X + radius * Math.Sin(angle), center.Y - radius * Math.Cos(angle));
            context.DrawLine(GridPen, center, edge);
        }

        context.DrawEllipse(null, HorizonPen, center, radius, radius);
        DrawText(context, "30° ALT", new Point(center.X + 9, center.Y - radius * 0.68), 9, CompassBrush);
        DrawText(context, "60°", new Point(center.X + 9, center.Y - radius * 0.36), 9, CompassBrush);
    }

    private static void DrawCompass(DrawingContext context, Point center, double radius)
    {
        DrawText(context, "N", new Point(center.X - 4, center.Y - radius - 22), 12, BodyLabelBrush);
        DrawText(context, "E", new Point(center.X + radius + 9, center.Y - 6), 12, BodyLabelBrush);
        DrawText(context, "S", new Point(center.X - 4, center.Y + radius + 7), 12, BodyLabelBrush);
        DrawText(context, "W", new Point(center.X - radius - 20, center.Y - 6), 12, BodyLabelBrush);
        DrawText(context, "ZENITH", new Point(center.X - 20, center.Y - 4), 8, CompassBrush);
    }

    private static void DrawConstellations(
        DrawingContext context,
        IReadOnlyDictionary<string, SkyChartObject> starsByHip,
        Point center,
        double radius)
    {
        foreach (var segment in AsterismSegments)
        {
            var firstId = segment[0].ToString(CultureInfo.InvariantCulture);
            var secondId = segment[1].ToString(CultureInfo.InvariantCulture);
            if (!starsByHip.TryGetValue(firstId, out var first) || !starsByHip.TryGetValue(secondId, out var second) ||
                first.AltitudeDegrees < 0 || second.AltitudeDegrees < 0)
                continue;

            context.DrawLine(ConstellationPen, ToScreen(first, center, radius), ToScreen(second, center, radius));
        }
    }

    private void DrawBodies(DrawingContext context, SkyScene scene, Point center, double radius)
    {
        foreach (var body in scene.Objects.Where(item => item.Kind != SkyObjectKind.Star && item.AltitudeDegrees >= 0))
        {
            var point = ToScreen(body, center, radius);
            var color = body.Kind switch
            {
                SkyObjectKind.Sun => Color.Parse("#FFC968"),
                SkyObjectKind.Moon => Color.Parse("#D9E8FF"),
                SkyObjectKind.Planet when body.Name == "Mars" => Color.Parse("#FF907C"),
                SkyObjectKind.Planet when body.Name is "Jupiter" or "Saturn" => Color.Parse("#E5C58D"),
                _ => Color.Parse("#74E0D1")
            };
            var selected = body.Id == SelectedObject?.Id;
            var halo = new SolidColorBrush(color, 0.16);
            var brush = new SolidColorBrush(color);

            context.DrawEllipse(halo, null, point, selected ? 12 : 9, selected ? 12 : 9);
            context.DrawEllipse(brush, selected ? SelectionPen : null, point, selected ? 4.3 : 3.2, selected ? 4.3 : 3.2);
            DrawLabel(context, body.Name, new Point(point.X + 10, point.Y - 8), 11.5, selected);
        }
    }

    private void DrawSelectedInfo(DrawingContext context, Point center, double radius)
    {
        if (SelectedObject is null || SelectedObject.AltitudeDegrees < 0)
            return;

        var point = ToScreen(SelectedObject, center, radius);
        context.DrawEllipse(null, SelectionPen, point, 8.8, 8.8);
    }

    private static Point ToScreen(SkyChartObject item, Point center, double radius)
    {
        var distance = radius * (90 - Math.Clamp(item.AltitudeDegrees, 0, 90)) / 90;
        var azimuth = Angle.ToRadians(item.AzimuthDegrees);
        return new Point(center.X + distance * Math.Sin(azimuth), center.Y - distance * Math.Cos(azimuth));
    }

    private static Color StarColor(double? colorIndex)
    {
        if (colorIndex is null)
            return Color.Parse("#DDE8FF");
        if (colorIndex < 0.1)
            return Color.Parse("#A9C5FF");
        if (colorIndex < 0.55)
            return Color.Parse("#DDE8FF");
        if (colorIndex < 1.0)
            return Color.Parse("#FFD7A1");
        return Color.Parse("#FFAB7A");
    }

    private static void DrawLabel(DrawingContext context, string text, Point point, double size, bool emphasized)
    {
        var color = emphasized ? Color.Parse("#8CE8D8") : Color.Parse("#CBD7EB");
        var brush = new SolidColorBrush(color);
        DrawText(context, text, point, size, brush);
    }

    private static void DrawText(DrawingContext context, string text, Point point, double size, IBrush brush)
    {
        var formatted = new FormattedText(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, MapTypeface, size, brush);
        context.DrawText(formatted, point);
    }

    private static class Angle
    {
        public static double ToRadians(double degrees) => degrees * Math.PI / 180.0;
    }
}

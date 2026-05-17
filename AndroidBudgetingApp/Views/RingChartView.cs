using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;

namespace AndroidBudgetingApp.Views;

/// <summary>
/// Custom ring/donut chart view matching the Analytics prototype.
/// </summary>
public class RingChartView : View
{
    private readonly Paint _paint = new();
    private readonly List<(float Percentage, Color Color)> _segments = new();
    private const float StartAngle = -90f; // Start from top

    public RingChartView(Context context) : base(context) { }
    public RingChartView(Context context, IAttributeSet? attrs) : base(context, attrs) { }

    public void SetData(List<(float Percentage, string ColorHex)> data)
    {
        _segments.Clear();
        foreach (var (pct, hex) in data)
            _segments.Add((pct, Color.ParseColor(hex)));
        Invalidate();
    }

    protected override void OnDraw(Canvas? canvas)
    {
        if (canvas == null) return;
        base.OnDraw(canvas);
        if (_segments.Count == 0) return;

        var density = Resources?.DisplayMetrics?.Density ?? 3f;
        _paint.AntiAlias = true;
        _paint.SetStyle(Paint.Style.Stroke);
        _paint.StrokeWidth = 16f * density;

        var padding = _paint.StrokeWidth / 2f;
        var rect = new RectF(padding, padding, Width - padding, Height - padding);

        // Draw background ring
        _paint.Color = Color.ParseColor("#F5F5F5");
        canvas.DrawArc(rect, 0, 360, false, _paint);

        // Draw segments
        var currentAngle = StartAngle;
        foreach (var (pct, color) in _segments)
        {
            var sweep = pct / 100f * 360f;
            _paint.Color = color;
            canvas.DrawArc(rect, currentAngle, sweep, false, _paint);
            currentAngle += sweep;
        }
    }
}

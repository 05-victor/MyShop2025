using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MyShop.Client.Views.Components.Skeletons;

/// <summary>
/// Base skeleton loader with shimmer animation.
/// Usage:
/// <skeletons:SkeletonLoader Width="200" Height="20" Shape="Rectangle"/>
/// <skeletons:SkeletonLoader Width="40" Height="40" Shape="Circle"/>
/// </summary>
public sealed partial class SkeletonLoader : UserControl
{
    public SkeletonLoader()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    public static readonly DependencyProperty ShapeProperty =
        DependencyProperty.Register(
            nameof(Shape),
            typeof(SkeletonShape),
            typeof(SkeletonLoader),
            new PropertyMetadata(SkeletonShape.Rectangle, OnShapeChanged));

    public SkeletonShape Shape
    {
        get => (SkeletonShape)GetValue(ShapeProperty);
        set => SetValue(ShapeProperty, value);
    }

    private static void OnShapeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SkeletonLoader skeleton)
        {
            skeleton.UpdateShape();
        }
    }

    private void UpdateShape()
    {
        if (Shape == SkeletonShape.Circle)
        {
            // Make it circular
            var size = Math.Min(Width, Height);
            if (!double.IsNaN(size))
            {
                SkeletonRectangle.RadiusX = size / 2;
                SkeletonRectangle.RadiusY = size / 2;
            }
        }
        else
        {
            SkeletonRectangle.RadiusX = 4;
            SkeletonRectangle.RadiusY = 4;
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        PulseStoryboard.Begin();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        PulseStoryboard.Stop();
    }
}

public enum SkeletonShape
{
    Rectangle,
    Circle
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Enigma.D3.MapHack
{
	public static class ControlHelper
	{
		public static UIElement SetOpacity(this UIElement element, double opacity)
		{
			element.Opacity = opacity;
			return element;
		}

		public static UIElement AnimateOpacity(this UIElement element, double fromValue, double toValue, double durationInSeconds)
		{
			return element.AnimateOpacity(fromValue, toValue, TimeSpan.FromSeconds(durationInSeconds));
		}

		public static UIElement AnimateOpacity(this UIElement element, double fromValue, double toValue, TimeSpan duration)
		{
			element.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(fromValue, toValue, new Duration(duration)) { RepeatBehavior = RepeatBehavior.Forever, AutoReverse = true });
			return element;
		}

		public static UIElement AnimateScale(this UIElement element, double fromValue, double toValue, double durationInSeconds)
		{
			return element.AnimateScale(fromValue, toValue, TimeSpan.FromSeconds(durationInSeconds));
		}

		public static UIElement AnimateScale(this UIElement element, double fromValue, double toValue, TimeSpan duration)
		{
			var scale = new ScaleTransform(fromValue, fromValue);
			element.AddRenderTransform(scale);
			scale.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(fromValue, toValue, new Duration(duration)) { RepeatBehavior = RepeatBehavior.Forever, AutoReverse = true });
			scale.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(fromValue, toValue, new Duration(duration)) { RepeatBehavior = RepeatBehavior.Forever, AutoReverse = true });
			return element;
		}

		public static UIElement SpinRight(this UIElement element, double revolutionsPerSecond)
		{
			return element.AnimateRotation(0, -360, 1d / revolutionsPerSecond, false);
		}

		public static UIElement SpinLeft(this UIElement element, double revolutionsPerSecond)
		{
			return element.AnimateRotation(0, 360, 1d / revolutionsPerSecond, false);
		}

		public static UIElement AnimateRotation(this UIElement element, double fromAngle, double toAngle, double durationInSeconds, bool autoReverse = false)
		{
			return element.AnimateRotation(fromAngle, toAngle, TimeSpan.FromSeconds(durationInSeconds));
		}

		public static UIElement AnimateRotation(this UIElement element, double fromAngle, double toAngle, TimeSpan duration, bool autoReverse = false)
		{
			var scale = new RotateTransform(fromAngle);
			element.AddRenderTransform(scale);
			scale.BeginAnimation(RotateTransform.AngleProperty, new DoubleAnimation(fromAngle, toAngle, new Duration(duration)) { RepeatBehavior = RepeatBehavior.Forever, AutoReverse = autoReverse });
			scale.BeginAnimation(RotateTransform.AngleProperty, new DoubleAnimation(fromAngle, toAngle, new Duration(duration)) { RepeatBehavior = RepeatBehavior.Forever, AutoReverse = autoReverse });
			return element;
		}

		public static void AddLayoutTransform(this FrameworkElement element, Transform transform)
		{
			if (element.LayoutTransform == null)
			{
				element.LayoutTransform = transform;
			}
			else
			{
				var group = element.LayoutTransform as TransformGroup;
				if (group == null)
				{
					group = new TransformGroup();
					group.Children.Add(element.RenderTransform);
				}
				group.Children.Add(transform);
				element.LayoutTransform = group;
			}
		}

		public static void AddRenderTransform(this UIElement element, Transform transform)
		{
			if (element.RenderTransform == null)
			{
				element.RenderTransform = transform;
			}
			else
			{
				var group = element.RenderTransform as TransformGroup;
				if (group == null)
				{
					group = new TransformGroup();
					group.Children.Add(element.RenderTransform);
				}
				group.Children.Add(transform);
				element.RenderTransform = group;
			}
		}


		public static UIElement If(this UIElement element, bool condition, Func<UIElement, UIElement> func)
		{
			if (condition)
				return func.Invoke(element);
			return element;
		}

		public static UIElement Do(this UIElement element, Action<UIElement> func)
		{
			func.Invoke(element);
			return element;
		}


		public static UIElement BindVisibilityTo<T>(this UIElement element, T source, Expression<Func<T,bool>> propertySelector)
		{
			var memberExpression = propertySelector.Body as MemberExpression;
			var propertyName = memberExpression.Member.Name;
			BindingOperations.SetBinding(element, UIElement.VisibilityProperty, new Binding(propertyName) { Source = source, Converter = new BooleanToVisibilityConverter() });
			return element;
		}


		public static Ellipse CreateCircle(double diameter, Brush fill, Brush stroke = null, double strokeThickness = double.NaN)
		{
			diameter -= strokeThickness / 2;

			var control = new Ellipse();
			control.BeginInit();

			control.Width = diameter;
			control.Height = diameter;
			control.Stroke = stroke;
			control.StrokeThickness = strokeThickness;
			control.Fill = fill;

			var translateTransform = new TranslateTransform();
			BindingOperations.SetBinding(translateTransform, TranslateTransform.XProperty, new Binding() { Source = control, Path = new PropertyPath(Ellipse.ActualWidthProperty), Converter = new HalfConverter() });
			BindingOperations.SetBinding(translateTransform, TranslateTransform.YProperty, new Binding() { Source = control, Path = new PropertyPath(Ellipse.ActualHeightProperty), Converter = new HalfConverter() });
			control.RenderTransform = new TransformGroup
			{
				Children = new TransformCollection { translateTransform }
			};

			control.EndInit();
			return control;
		}

        public static Ellipse CreateEllipse(double width, double height, Brush fill, Brush stroke = null, double strokeThickness = double.NaN)
        {
            var control = new Ellipse();
            control.BeginInit();

            control.Width = width;
            control.Height = height;
            control.Stroke = stroke;
            control.StrokeThickness = strokeThickness;
            control.Fill = fill;

            var translateTransform = new TranslateTransform();
            BindingOperations.SetBinding(translateTransform, TranslateTransform.XProperty, new Binding() { Source = control, Path = new PropertyPath(Ellipse.ActualWidthProperty), Converter = new HalfConverter() });
            BindingOperations.SetBinding(translateTransform, TranslateTransform.YProperty, new Binding() { Source = control, Path = new PropertyPath(Ellipse.ActualHeightProperty), Converter = new HalfConverter() });
            control.RenderTransform = new TransformGroup
            {
                Children = new TransformCollection { translateTransform }
            };

            control.EndInit();
            return control;
        }

        public static Path CreateCross(double size, Brush stroke, double strokeThickness)
		{
			var geometry = new PathGeometry(new[]
			{ 
				new PathFigure(new Point(0, size / 2), new[] { new LineSegment(new Point(size, size / 2), true) }, false),
				new PathFigure(new Point(size / 2, 0), new[] { new LineSegment(new Point(size / 2, size), true) }, false)
			});
			return new Path() { Data = geometry, StrokeThickness = strokeThickness, Stroke = stroke, RenderTransform = new TranslateTransform(-size / 2, -size / 2) };
		}

		public static SolidColorBrush CreateAnimatedBrush(Color fromValue, Color toValue, double durationInSeconds)
		{
			return CreateAnimatedBrush(fromValue, toValue, TimeSpan.FromSeconds(durationInSeconds));
		}

		public static SolidColorBrush CreateAnimatedBrush(Color fromValue, Color toValue, TimeSpan duration)
		{
			var brush = new SolidColorBrush(Colors.Purple);
			brush.BeginAnimation(SolidColorBrush.ColorProperty, new ColorAnimation(fromValue, toValue, new Duration(duration)) { RepeatBehavior = RepeatBehavior.Forever, AutoReverse = true });
			return brush;
		}
	}
}

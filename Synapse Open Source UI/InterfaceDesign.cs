using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Synapse_X_UI
{
    /// <summary>
    /// title: InterfaceDesign.cs
    /// description: Interface design class, contains ui animation methods
    /// author: brack4712
    /// </summary>
    class InterfaceDesign : Window
    {
        private TimeSpan duration { get; set; } = TimeSpan.FromSeconds(1);
        private IEasingFunction ease { get; set; } = new QuarticEase { EasingMode = EasingMode.EaseInOut };

        public InterfaceDesign(TimeSpan? timeSpan = null, IEasingFunction easingFunction = null)
        {
            if (timeSpan != null)
            {
                duration = (TimeSpan)timeSpan;
            }

            if (easingFunction != null)
            {
                ease = easingFunction;
            }
        }

        public void FadeIn(DependencyObject element)
        {
            DoubleAnimation fadeAnimation = new DoubleAnimation()
            {
                From = 0,
                To = 1,
                Duration = duration,
                EasingFunction = ease
            };

            Storyboard.SetTarget(fadeAnimation, element);
            Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath(OpacityProperty));

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(fadeAnimation);
            //storyboard.FillBehavior = FillBehavior.Stop;
            storyboard.Begin();
        }

        public void FadeOut(DependencyObject element)
        {
            DoubleAnimation fadeAnimation = new DoubleAnimation()
            {
                From = 1,
                To = 0,
                Duration = duration,
                EasingFunction = ease
            };

            Storyboard.SetTarget(fadeAnimation, element);
            Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath(OpacityProperty));

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(fadeAnimation);
            //storyboard.FillBehavior = FillBehavior.Stop;
            storyboard.Begin();
        }

        public void Shift(DependencyObject element, Thickness from, Thickness to)
        {
            ThicknessAnimation shiftAnimation = new ThicknessAnimation()
            {
                From = from,
                To = to,
                Duration = duration,
                EasingFunction = ease
            };

            Storyboard.SetTarget(shiftAnimation, element);
            Storyboard.SetTargetProperty(shiftAnimation, new PropertyPath(MarginProperty));

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(shiftAnimation);
            //storyboard.FillBehavior = FillBehavior.Stop;
            storyboard.Begin();
        }

        public void ShiftWindow(Window window, double leftFrom, double topFrom, double leftTo, double topTo)
        {
            DoubleAnimation leftAnimation = new DoubleAnimation()
            {
                From = leftFrom,
                To = leftTo,
                Duration = duration,
                EasingFunction = ease
            };

            DoubleAnimation topAnimation = new DoubleAnimation()
            {
                From = topFrom,
                To = topTo,
                Duration = duration,
                EasingFunction = ease
            };

            Storyboard.SetTarget(leftAnimation, window);
            Storyboard.SetTargetProperty(leftAnimation, new PropertyPath(LeftProperty));

            Storyboard.SetTarget(topAnimation, window);
            Storyboard.SetTargetProperty(topAnimation, new PropertyPath(TopProperty));

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(leftAnimation);
            storyboard.Children.Add(topAnimation);
            storyboard.FillBehavior = FillBehavior.Stop;
            storyboard.Begin();
        }

        public void Resize(DependencyObject element, double height, double width)
        {
            DoubleAnimation heightAnimation = new DoubleAnimation()
            {
                From = (double)element.GetValue(ActualHeightProperty),
                To = height,
                Duration = duration,
                EasingFunction = ease
            };

            DoubleAnimation widthAnimation = new DoubleAnimation()
            {
                From = (double)element.GetValue(ActualWidthProperty),
                To = width,
                Duration = duration,
                EasingFunction = ease
            };


            Storyboard.SetTarget(heightAnimation, element);
            Storyboard.SetTargetProperty(heightAnimation, new PropertyPath(HeightProperty));

            Storyboard.SetTarget(widthAnimation, element);
            Storyboard.SetTargetProperty(widthAnimation, new PropertyPath(WidthProperty));


            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(heightAnimation);
            storyboard.Children.Add(widthAnimation);
            //storyboard.FillBehavior = FillBehavior.Stop;
            storyboard.Begin();
        }

        public void FontColor(DependencyObject element, string from, string to)
        {
            ColorAnimation colorAnimation = new ColorAnimation()
            {
                From = (Color)ColorConverter.ConvertFromString(from),
                To = (Color)ColorConverter.ConvertFromString(to),
                Duration = duration,
                EasingFunction = ease
            };

            Storyboard.SetTarget(colorAnimation, element);
            Storyboard.SetTargetProperty(colorAnimation, new PropertyPath("(Label.Foreground).(SolidColorBrush.Color)"));

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(colorAnimation);
            //storyboard.FillBehavior = FillBehavior.Stop;
            storyboard.Begin();
        }

        public void ButtonColor(DependencyObject element, string from, string to)
        {
            ColorAnimation colorAnimation = new ColorAnimation()
            {
                From = (Color)ColorConverter.ConvertFromString(from),
                To = (Color)ColorConverter.ConvertFromString(to),
                Duration = duration,
                EasingFunction = ease
            };

            Storyboard.SetTarget(colorAnimation, element);
            Storyboard.SetTargetProperty(colorAnimation, new PropertyPath("(Button.Background).(SolidColorBrush.Color)"));

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(colorAnimation);
            //storyboard.FillBehavior = FillBehavior.Stop;
            storyboard.Begin();
        }
    }
}

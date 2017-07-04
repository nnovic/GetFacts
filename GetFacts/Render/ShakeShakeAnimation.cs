using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace GetFacts.Render
{
    class ShakeShakeAnimation:ThicknessAnimationUsingKeyFrames
    {
        private readonly Thickness originalThickness;

        public ShakeShakeAnimation(Thickness original)
        {
            originalThickness = original;
            const double amplitude = 0.02;
            double tLeft, tRight;
            tLeft = original.Left;
            tRight = original.Right;
            double shakeQuantity = Math.Min(tLeft, tRight);

            ThicknessKeyFrame kf1 = new LinearThicknessKeyFrame()
            {
                Value = new Thickness()
                {
                    Left = originalThickness.Left-shakeQuantity,
                    Top = originalThickness.Top,
                    Right = originalThickness.Right+shakeQuantity,
                    Bottom = originalThickness.Bottom
                },
                KeyTime = KeyTime.FromPercent(1* amplitude)
            };

            ThicknessKeyFrame kf2 = new LinearThicknessKeyFrame()
            {
                Value = new Thickness()
                {
                    Left = originalThickness.Left + shakeQuantity,
                    Top = originalThickness.Top,
                    Right = originalThickness.Right - shakeQuantity,
                    Bottom = originalThickness.Bottom
                },
                KeyTime = KeyTime.FromPercent(3* amplitude)
            };

            ThicknessKeyFrame kf3 = new LinearThicknessKeyFrame()
            {
                Value = new Thickness()
                {
                    Left = originalThickness.Left - shakeQuantity,
                    Top = originalThickness.Top,
                    Right = originalThickness.Right + shakeQuantity,
                    Bottom = originalThickness.Bottom
                },
                KeyTime = KeyTime.FromPercent(5* amplitude)
            };

            ThicknessKeyFrame kf4 = new LinearThicknessKeyFrame()
            {
                Value = new Thickness()
                {
                    Left = originalThickness.Left,
                    Top = originalThickness.Top,
                    Right = originalThickness.Right,
                    Bottom = originalThickness.Bottom
                },
                KeyTime = KeyTime.FromPercent(6* amplitude)
            };

            Duration = new Duration(new TimeSpan(0, 0, 2));
            KeyFrames.Add(kf1);
            KeyFrames.Add(kf2);
            KeyFrames.Add(kf3);
            KeyFrames.Add(kf4);
            RepeatBehavior = RepeatBehavior.Forever;
        }
    }
}

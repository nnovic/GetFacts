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
                KeyTime = KeyTime.FromPercent(0.05)
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
                KeyTime = KeyTime.FromPercent(0.15)
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
                KeyTime = KeyTime.FromPercent(0.25)
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
                KeyTime = KeyTime.FromPercent(0.30)
            };

            Duration = new Duration(new TimeSpan(0, 0, 1));
            KeyFrames.Add(kf1);
            KeyFrames.Add(kf2);
            KeyFrames.Add(kf3);
            KeyFrames.Add(kf4);
            RepeatBehavior = RepeatBehavior.Forever;
        }
    }
}

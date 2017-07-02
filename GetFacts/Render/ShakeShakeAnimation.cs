using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace GetFacts.Render
{
    class ShakeShakeAnimation:ThicknessAnimation
    {
        public ShakeShakeAnimation()
        {
            From = new Thickness(10);
            To = new Thickness(0);
            Duration = new Duration(new TimeSpan(0,0,1));
            AutoReverse = true;
            RepeatBehavior = RepeatBehavior.Forever;
        }
    }
}

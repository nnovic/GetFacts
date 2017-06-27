using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetFacts.Render
{
    interface ICanDock
    {
        void Undock(MediaDisplay md);

        void Dock(MediaDisplay md);
    }
}

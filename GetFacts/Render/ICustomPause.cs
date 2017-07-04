using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetFacts.Render
{
    public interface ICustomPause
    {
        /// <summary>
        /// (en secondes)
        /// </summary>
        int MaxPageDisplayDuration
        {
            get;
        }

        /// <summary>
        /// (en secondes)
        /// </summary>
        int MinPageDisplayDuration
        {
            get;
        }



    }
}

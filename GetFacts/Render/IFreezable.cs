﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetFacts.Render
{
    public interface IFreezable
    {
        event EventHandler<FreezeEventArgs> Frozen;
        event EventHandler<FreezeEventArgs> Unfrozen;
    }

    public class FreezeEventArgs : EventArgs
    {
        private readonly CauseOfFreezing cause;
        public FreezeEventArgs(CauseOfFreezing cause)
        {
            this.cause = cause;
        }
        public CauseOfFreezing Cause
        {
            get { return cause; }
        }
    }

    public enum CauseOfFreezing
    {
        ZoomOnMedia,
        CursorOnArticle
    }
}

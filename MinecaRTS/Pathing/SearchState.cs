using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecaRTS
{
    public enum SearchState
    {
        Incomplete,
        Smoothing,
        Retracing,
        Complete,
        Failed
    }
}

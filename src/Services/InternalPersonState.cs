using System;

namespace covidSim.Services
{
    [Flags]
    public enum InternalPersonState
    {
        Healthy,
        Sick,
        Bored,
        Dead,
        NeedDeleted
    }
}
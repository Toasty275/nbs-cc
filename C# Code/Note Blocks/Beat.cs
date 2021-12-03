using System;
using System.Collections.Generic;

namespace Note_Blocks
{
    class Beat
    {
        public short Jumps { get; }
        public List<int> Instruments { get; }
        public List<int> Pitches { get; }
        public Beat(short jumps, List<int> instruments, List<int> pitches)
        {
            this.Jumps = jumps;
            this.Instruments = instruments;
            this.Pitches = pitches;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Note_Blocks
{
    class Beat
    {
        private short jumps;
        private List<int> instruments;
        private List<int> pitches;
        public short Jumps
        {
            get { return jumps; }
        }
        public List<int> Instruments
        {
            get { return instruments; }
        }
        public List<int> Pitches
        {
            get { return pitches; }
        }
        public Beat(short jumps, List<int> instruments, List<int> pitches)
        {
            this.jumps = jumps;
            this.instruments = instruments;
            this.pitches = pitches;
        }
    }
}

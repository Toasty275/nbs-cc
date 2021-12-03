using System;
using System.Collections.Generic;
using System.IO;

namespace Note_Blocks
{
    class Song
    {
        private BinaryReader reader;
        public string Title { get; }
        public string Description { get; }
        public decimal Bpm { get; }
        public List<string> Header { get; }

        public Song(string file)
        {
            reader = new BinaryReader(File.Open(file, FileMode.Open));
            Header = new List<string>();
            Header.Add("zero: " + reader.ReadInt16().ToString()); //zero
            Header.Add("nbs version: " + reader.ReadByte().ToString()); //nbs version
            Header.Add("instrument count: " + reader.ReadByte().ToString()); //vanilla instrument count
            Header.Add("song length: " + reader.ReadInt16().ToString()); //song length
            Header.Add("layer count: " + reader.ReadInt16().ToString()); //layer count
            Title = new string(reader.ReadChars(reader.ReadInt32()));
            Header.Add("title: " + Title); //song name
            Header.Add("author: " + new string(reader.ReadChars(reader.ReadInt32()))); //author name
            Header.Add("song author: " + new string(reader.ReadChars(reader.ReadInt32()))); //original author
            Description = new string(reader.ReadChars(reader.ReadInt32()));
            Header.Add("description: " + Description); //song description
            Bpm = reader.ReadInt16();
            Header.Add("tempo: " + Bpm.ToString()); //tempo
            Header.Add("auto save: " + reader.ReadByte().ToString()); //auto save
            Header.Add("auto save duration: " + reader.ReadByte().ToString()); //auto save duration
            Header.Add("time signature: " + reader.ReadByte().ToString()); //time signature
            Header.Add("minutes spent: " + reader.ReadInt32().ToString()); //minutes spent
            Header.Add("left clicks: " + reader.ReadInt32().ToString()); //left clicks
            Header.Add("right clicks: " + reader.ReadInt32().ToString()); //right clicks
            Header.Add("note blocks added: " + reader.ReadInt32().ToString()); //note blocks added
            Header.Add("note blocks removed: " + reader.ReadInt32().ToString()); //note blocks removed
            Header.Add("midi name: " + new string(reader.ReadChars(reader.ReadInt32()))); //midi name
            Header.Add("loop: " + reader.ReadByte().ToString()); //loop
            Header.Add("max loop count: " + reader.ReadByte().ToString()); //max loop count
            Header.Add("loop start tick: " + reader.ReadInt16().ToString()); //loop start tick
        }
        public Beat NextBeat()
        {
            short jumps = reader.ReadInt16();
            if (jumps == 0) return new Beat(0, null, null);
            List<int> instruments = new List<int>();
            List<int> pitches = new List<int>();
            while (true)
            {
                short nextLayer = reader.ReadInt16();
                if (nextLayer == 0) break;
                instruments.Add(reader.ReadByte());
                pitches.Add(reader.ReadByte() - 33);
                reader.ReadByte();
                reader.ReadByte();
                reader.ReadInt16();
            }
            return new Beat(jumps, instruments, pitches);
        }
        public void Dispose()
        {
            reader.Dispose();
        }
    }
}

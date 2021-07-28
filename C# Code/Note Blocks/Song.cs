using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Note_Blocks
{
    class Song
    {
        private BinaryReader reader;
        private string title;
        private decimal bpm;
        private List<string> header;

        public string Title
        {
            get { return title; }
        }
        public decimal Bpm
        {
            get { return bpm; }
        }
        public List<string> Header
        {
            get { return header; }
        }
        public Song(string file)
        {
            reader = new BinaryReader(File.Open(file, FileMode.Open));
            header = new List<string>();
            header.Add("zero: " + reader.ReadInt16().ToString()); //zero
            header.Add("nbs version: " + reader.ReadByte().ToString()); //nbs version
            header.Add("instrument count: " + reader.ReadByte().ToString()); //vanilla instrument count
            header.Add("song length: " + reader.ReadInt16().ToString()); //song length
            header.Add("layer count: " + reader.ReadInt16().ToString()); //layer count
            title = new string(reader.ReadChars(reader.ReadInt32()));
            header.Add("title: " + title); //song name
            header.Add("author: " + new string(reader.ReadChars(reader.ReadInt32()))); //author name
            header.Add("song author: " + new string(reader.ReadChars(reader.ReadInt32()))); //original author
            header.Add("description: " + new string(reader.ReadChars(reader.ReadInt32()))); //song description
            bpm = reader.ReadInt16();
            header.Add("tempo: " + bpm.ToString()); //tempo
            header.Add("auto save: " + reader.ReadByte().ToString()); //auto save
            header.Add("auto save duration: " + reader.ReadByte().ToString()); //auto save duration
            header.Add("time signature: " + reader.ReadByte().ToString()); //time signature
            header.Add("minutes spent: " + reader.ReadInt32().ToString()); //minutes spent
            header.Add("left clicks: " + reader.ReadInt32().ToString()); //left clicks
            header.Add("right clicks: " + reader.ReadInt32().ToString()); //right clicks
            header.Add("note blocks added: " + reader.ReadInt32().ToString()); //note blocks added
            header.Add("note blocks removed: " + reader.ReadInt32().ToString()); //note blocks removed
            header.Add("midi name: " + new string(reader.ReadChars(reader.ReadInt32()))); //midi name
            header.Add("loop: " + reader.ReadByte().ToString()); //loop
            header.Add("max loop count: " + reader.ReadByte().ToString()); //max loop count
            header.Add("loop start tick: " + reader.ReadInt16().ToString()); //loop start tick
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

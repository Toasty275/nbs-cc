using System;
using System.Collections.Generic;
using System.IO;

namespace Note_Blocks
{
    public class Nbs
    {
        public enum Name
        {
            Harp,
            DoubleBass,
            BassDrum,
            Snare,
            Click,
            Guitar,
            Flute,
            Bell,
            Chime,
            Xylophone,
            IronXylophone,
            CowBell,
            Didgeridoo,
            Bit,
            Banjo,
            Pling,
            TempoChanger,
            SongTick
        }
        public readonly struct Note
        {
            public int Pitch { get; }
            public Name Instrument { get; }
            public Note(int Pitch, Name Instrument)
            {
                this.Pitch = Pitch;
                this.Instrument = Instrument;
            }
            public override int GetHashCode()
            {
                return (sbyte)Pitch << 8 + (byte)Instrument;
            }
            public override bool Equals(object obj)
            {
                if (obj is not Note) return base.Equals(obj);
                Note n = (Note)obj;
                return Pitch == n.Pitch && Instrument == n.Instrument;
            }

            public static bool operator ==(Note left, Note right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(Note left, Note right)
            {
                return !(left == right);
            }
        }
        public readonly struct Beat
        {
            public int Jumps { get; }
            public List<Note> RawNotes { get; }
            public Beat(int Jumps, List<Note> RawNotes)
            {
                this.Jumps = Jumps;
                this.RawNotes = RawNotes;
            }
            public Beat()
            {
                Jumps = 0;
                RawNotes = new();
            }
            public Beat(int Jumps)
            {
                this.Jumps = Jumps;
                RawNotes = new();
            }

            public static Beat Empty
            {
                get
                {
                    Beat beat = new(0, new());
                    return beat;
                }
            }
            public List<Note> Notes
            {
                get
                {
                    List<Note> temp = RawNotes;
                    foreach (Note note in Duplicates)
                    {
                        temp.Remove(note);
                    }
                    foreach (Note note in OutOfRange)
                    {
                        temp.Remove(note);
                    }
                    return temp;
                }
            }
            public List<Note> Duplicates
            {
                get
                {
                    List<Note> seen = new();
                    List<Note> duplicates = new();
                    foreach (Note x in RawNotes)
                    {
                        if (seen.Contains(x))
                        {
                            if (!duplicates.Contains(x)) duplicates.Add(x);
                        }
                        else seen.Add(x);
                    }
                    return duplicates;
                }
            }
            public List<Note> OutOfRange
            {
                get
                {
                    List<Note> temp = new();
                    foreach (Note x in RawNotes)
                    {
                        if (x.Pitch < -24 || x.Pitch > 48) temp.Add(x);
                    }
                    return temp;
                }
            }
        }

        private readonly BinaryReader reader;
        public List<Beat> Song { get; private set; }
        public List<string> Header { get; private set; }
        public string Title { get; private set; }
        public string Description { get; private set; }
        public int Bpm { get; private set; }
        public Nbs(string path)
        {
            reader = new(File.OpenRead(path));

            #region Header
            Header = new();
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
            #endregion

            #region Notes
            Song = new();
            while (true)
            {
                short jumps = reader.ReadInt16();
                if (jumps == 0) break;
                Beat beat = new(jumps, new());
                while (true)
                {
                    short nextLayer = reader.ReadInt16();
                    if (nextLayer == 0) break;
                    Name name = (Name)reader.ReadByte();
                    sbyte pitch = (sbyte)(reader.ReadByte() - 33);
                    reader.ReadByte(); //velocity
                    reader.ReadByte(); //panning
                    reader.ReadInt16(); //pitch offset (cents)
                    Note note = new(pitch, name);
                    beat.RawNotes.Add(note);
                }

                Song.Add(beat);
            }
            #endregion
            reader.Dispose();
        }
    }
}
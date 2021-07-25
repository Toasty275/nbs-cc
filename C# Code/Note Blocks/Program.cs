using System;
using System.IO;
using System.Collections.Generic;

namespace Note_Blocks
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Toast's Note Block Studio (.nbs) decoder");
            BinaryReader reader = new BinaryReader(File.Open("Input.nbs", FileMode.Open));
            List<string> header = new List<string>();
            decimal bpm;
            //Part 1
            header.Add("zero: " + reader.ReadInt16().ToString()); //zero
            header.Add("nbs version: " + reader.ReadByte().ToString()); //nbs version
            header.Add("instrument count: " + reader.ReadByte().ToString()); //vanilla instrument count
            header.Add("song length: " + reader.ReadInt16().ToString()); //song length
            header.Add("layer count: " + reader.ReadInt16().ToString()); //layer count
            string title = new string(reader.ReadChars(reader.ReadInt32()));
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

            foreach (string x in header) Console.WriteLine(x);
            Console.WriteLine();
            //part 2
            int index = 0;
            string lines = null;
            lines += "\ttemp[\"tps\"] = " + (bpm / 100);
            lines += "\n\ttemp[\"title\"] = \"" + title + "\"";

            string[] instNames = new string[]
            {
                "Piano",
                "Double bass",
                "Bass drum",
                "Snare",
                "Click",
                "Guitar",
                "Flute",
                "Bell",
                "Chime",
                "Xylophone",
                "Iron xylo",
                "Cow bell",
                "Didgeridoo",
                "Bit",
                "Banjo",
                "Pling"
            };
            int[] instGoTo = new int[instNames.Length];
            instGoTo[2] = 1;
            instGoTo[3] = 2;
            instGoTo[4] = 3;
            byte[] instNums = new byte[instNames.Length];
            List<int> drumNotes = new List<int>();
            List<int> snareNotes = new List<int>();
            List<int> clickNotes = new List<int>();
            while (true)
            {
                short jumps = reader.ReadInt16();
                if (jumps == 0) break;
                index++;
                for (int x = index + jumps; index != x - 1; index++)
                {
                    lines += "\n\ttemp[" + index + "] = {}";
                }
                string line = null;

                while (true) //main thingey
                {
                    short nextLayer = reader.ReadInt16();
                    if (nextLayer == 0) break;
                    int instrument = reader.ReadByte();
                    int pitch = reader.ReadByte() - 33;

                    int temp = instNums[instrument]; //low mid hi
                    if (pitch < 0)
                    {
                        if (temp - 4 >= 0) temp -= 4;
                        if (temp - 2 >= 0) temp -= 2;
                        if (temp - 1 < 0) instNums[instrument] += 1;
                    }
                    else if (pitch > 24)
                    {
                        if (temp - 4 < 0) instNums[instrument] += 4;
                    }
                    else
                    {
                        if (temp - 4 >= 0) temp -= 4;
                        if (temp - 2 < 0) instNums[instrument] += 2;
                    }

                    if (instrument == 2) //percussion note counts
                    {
                        if (!drumNotes.Contains(pitch))
                        {
                            drumNotes.Add(pitch);
                        }
                        pitch = drumNotes.IndexOf(pitch);
                    }
                    else if (instrument == 3)
                    {
                        if (!snareNotes.Contains(pitch))
                        {
                            snareNotes.Add(pitch);
                        }
                        pitch = snareNotes.IndexOf(pitch);
                    }
                    else if (instrument == 4)
                    {
                        if (!clickNotes.Contains(pitch))
                        {
                            clickNotes.Add(pitch);
                        }
                        pitch = clickNotes.IndexOf(pitch);
                    }

                    if (instGoTo[instrument] != 0)
                    {
                        instrument = instGoTo[instrument];
                    }
                    else
                    {
                        int max = 0;
                        foreach (int x in instGoTo)
                        {
                            if (max < x) max = x;
                        }
                        max++;
                        instGoTo[instrument] = max;
                        instrument = max;
                    }

                    line += "{" + instrument + ", " + pitch + "}, ";
                    reader.ReadByte();
                    reader.ReadByte();
                    reader.ReadInt16();
                }
                lines += "\n\ttemp[" + index + "] = {" + line + "}";
            }
            File.WriteAllText("output.txt", "function makeSong()\n\ttemp = {}\n" + lines + "\n\n\treturn temp\nend");
            lines = "Instrument scales:\n";
            for (int x = 0; x != instNames.Length; x++)
            {
                if (instNums[x] == 0) continue;
                lines += "\n" + instNames[x] + $"({instGoTo[x]}): ";
                string line = null;
                if (instNums[x] - 4 >= 0)
                {
                    line = "hi " + line;
                    instNums[x] -= 4;
                }
                if (instNums[x] - 2 >= 0)
                {
                    line = "mid " + line;
                    instNums[x] -= 2;
                }
                if (instNums[x] - 1 >= 0)
                {
                    line = "low " + line;
                }
                lines += line;
            }
            lines += "\n\nPercussion assignments:\n";
            if (drumNotes.Count > 0)
            {
                lines += "\nBass Drum notes: ";
                for (int x = 0; x != drumNotes.Count; x++) lines += $"{x}={drumNotes[x]} ";
            }
            if (snareNotes.Count > 0)
            {
                lines += "\nSnare Drum notes: ";
                for (int x = 0; x != snareNotes.Count; x++) lines += $"{x}={snareNotes[x]} ";
            }
            if (clickNotes.Count > 0)
            {
                lines += "\nClick notes: ";
                for (int x = 0; x != clickNotes.Count; x++) lines += $"{x}={clickNotes[x]} ";
            }
            Console.WriteLine(lines);
            reader.Dispose();
            //Console.ReadKey();
        }
    }
}
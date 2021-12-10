using System;
using System.IO;
using System.Collections.Generic;

namespace Note_Blocks
{
    class Program
    {
        static void Main(string[] args)
        {
            #region Arguments
            string input = null;
            string output = null;
            bool showHeader = false;
            bool logOutput = true;
            int art = 0;
            for (int x = 0; x != args.Length; x++)
            {
                string arg = args[x];
                if (arg.StartsWith('-'))
                {
                    switch (arg[1..])
                    {
                        case "i":
                            input = args[x + 1];
                            x++;
                            break;
                        case "h":
                            string[] helpLines =
                            {
                                "Usage: [args] outputFile(optional)",
                                "-h: Help",
                                "-i: Input file",
                                "-H: Show nbs header",
                                "-s: Don't output to log (silent)",
                                "-a: Generate art(0) call every n ticks"
                            };
                            foreach (string l in helpLines) Console.WriteLine(l);
                            Environment.Exit(0);
                            break;
                        case "H":
                            showHeader = true;
                            break;
                        case "s":
                            logOutput = false;
                            break;
                        case "a":
                            art = int.Parse(args[x + 1]);
                            x++;
                            break;
                        default:
                            throw new ArgumentException("Invalid parameter. Use -h for help.", arg);
                    }
                    continue;
                }
                else if (output == null) output = arg;
                else throw new ArgumentException("More than one output supplied");
            }
            if (input == null) throw new ArgumentException("No input file specified.");
            #endregion

            Console.WriteLine("Toast's Note Block Studio (.nbs) decoder\n");
            List<string> Log = new() { "Toast's Note Block Studio (.nbs) decoder" };

            Nbs song = new(input);
            Console.WriteLine("File: " + input);
            Log.Add("File: " + input);
            decimal bpm = song.Bpm;
            string title = song.Title;
            List<string> header = song.Header;
            if (showHeader) foreach (string x in header) Console.WriteLine(x);
            Log.AddRange(header);
            Console.WriteLine();
            //part 2
            int index = 0;
            string lines = null;
            lines += "\ttemp[\"tps\"] = " + (bpm / 100);
            lines += "\n\ttemp[\"title\"] = \"" + title + "\"";

            List<int> drumNotes = new();
            List<int> snareNotes = new();
            List<int> clickNotes = new();
            bool usingTempoChange = false;
            List<int> tempos = new();
            if (int.TryParse(song.Description.Split("\n")[0], out int temptempochanger))
            {
                usingTempoChange = true;
                for (int x = 1; x != song.Description.Split("\n").Length; x++)
                {
                    tempos.Add(int.Parse(song.Description.Split("\n")[x]));
                }
            }
            Nbs.Name tempoChanger = (Nbs.Name)temptempochanger;
            int[] instNums = new int[16];
            foreach (Nbs.Beat beat in song.Song)
            {
                //get the next beat
                int jumps = beat.Jumps;
                index++;
                for (int x = index + jumps; index != x - 1; index++)
                {
                    string temp = null;
                    if (art > 0 && index - 1 % art == 0) temp = "-2, 0";
                    lines += "\n\ttemp[" + index + "] = {" + temp + "}";
                }
                string line = null;
                //warn about invalid notes
                foreach (Nbs.Note note in beat.Duplicates)
                {
                    Console.WriteLine($"[{input}] Duplicate note on tick {index - 1}. Inst {note.instrument}, Pitch {note.pitch}");
                    Log.Add($"[{input}] Duplicate note on tick {index - 1}. Inst {note.instrument}, Pitch {note.pitch}");
                }
                foreach (Nbs.Note note in beat.OutOfRange)
                {
                    Console.WriteLine($"[{input}] Note out of range on tick {index - 1}. Inst {note.instrument}, Pitch {note.pitch}");
                    Log.Add($"[{input}] Note out of range on tick {index - 1}. Inst {note.instrument}, Pitch {note.pitch}");
                }

                for (int i = 0; i != beat.Notes.Count; i++) //main thingey
                {
                    Nbs.Name instrument = beat.Notes[i].instrument;
                    int pitch = beat.Notes[i].pitch;
                    
                    if (usingTempoChange && instrument == tempoChanger)
                    {
                        instrument = Nbs.Name.TempoChanger;
                        pitch = tempos[pitch];
                    }
                    else
                    {
                        int temp = instNums[(int)instrument]; //low mid hi
                        if (pitch < 0)
                        {
                            if (temp - 4 >= 0) temp -= 4;
                            if (temp - 2 >= 0) temp -= 2;
                            if (temp - 1 < 0) instNums[(int)instrument] += 1;
                        }
                        else if (pitch > 24)
                        {
                            if (temp - 4 < 0) instNums[(int)instrument] += 4;
                        }
                        else
                        {
                            if (temp - 4 >= 0) temp -= 4;
                            if (temp - 2 < 0) instNums[(int)instrument] += 2;
                        }

                        if (instrument == Nbs.Name.BassDrum) //percussion note counts
                        {
                            if (!drumNotes.Contains(pitch))
                            {
                                drumNotes.Add(pitch);
                            }
                            pitch = drumNotes.IndexOf(pitch);
                        }
                        else if (instrument == Nbs.Name.Snare)
                        {
                            if (!snareNotes.Contains(pitch))
                            {
                                snareNotes.Add(pitch);
                            }
                            pitch = snareNotes.IndexOf(pitch);
                        }
                        else if (instrument == Nbs.Name.Click)
                        {
                            if (!clickNotes.Contains(pitch))
                            {
                                clickNotes.Add(pitch);
                            }
                            pitch = clickNotes.IndexOf(pitch);
                        }
                    }
                    line += "{" + (int)instrument + ", " + pitch + "}, ";
                }
                if (art > 0 && (index - 1) % art == 0) line += "{-2, 0}";
                lines += "\n\ttemp[" + index + "] = {" + line + "}";
            }
            if (output != null) File.WriteAllText(output, "function makeSong()\n\ttemp = {}\n" + lines + "\n\n\treturn temp\nend");
            lines = "Instrument scales:\n";
            for (int x = 0; x != 16; x++)
            {
                if (instNums[x] == 0) continue;
                lines += "\n" + (Nbs.Name)x + $"({x}): ";
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
            Log.Add(lines);
            if (logOutput) File.WriteAllLines("log.txt", Log);
        }
    }
}
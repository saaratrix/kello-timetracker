using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace KelloTimetracker.Classes
{
    class UpworkTimePrinter
    {
        /// <summary>
        /// Takes a folder path in the form of "u {folder}"
        /// </summary>
        /// <param name="foldersWithCommand"></param>
        public static void PrintTimes(string foldersWithCommand)
        {     
            if (foldersWithCommand.Length <= 2)
            {
                Program.WriteLineMessage("expected input: 'u folder'", ConsoleColor.Red);
                return;
            }

            try
            {
                
                // IEnumerable<string> files = Directory.EnumerateFiles(folder);
                var filesWithFolder = GetFiles(foldersWithCommand);

                UpworkTimeFixer timeFixer = new UpworkTimeFixer();
                double totalTime = 0;

                foreach (var (folder, files) in filesWithFolder)
                {
                    foreach (string filename in files)
                    {
                        string date = filename.Substring(folder.Length + 1).Split('.')[0];

                        Program.WriteLineMessage("Date: " + date, ConsoleColor.Yellow);

                        using (StreamReader sr = new StreamReader(filename))
                        {
                            DateTime start;
                            TimeSpan duration;
                            double dailyTotal = 0;
                            double restTotal = 0;


                            List<UpworkTimeEntry> entries = new List<UpworkTimeEntry>();

                            while (!sr.EndOfStream)
                            {
                                string line = sr.ReadLine();

                                if (line.StartsWith("start"))
                                {
                                    string startStr = line.Split(' ')[1];
                                    start = DateTime.Parse(startStr);
                                    // Read next line
                                    duration = TimeSpan.Parse(sr.ReadLine().Split(' ')[1]);

                                    UpworkTimeEntry entry = new UpworkTimeEntry(start, duration);
                                    entries.Add(entry);
                                }
                            }

                            if (entries.Count > 0)
                            {
                                // BUG if 1 entry has less than 10 minutes work then the totalRest will be fucked
                                entries = timeFixer.FixEntries(entries);

                                foreach (UpworkTimeEntry entry in entries)
                                {
                                    entry.PrintTime();
                                    dailyTotal += entry.DurationSeconds;
                                }

                                restTotal = timeFixer.RestSeconds;

                                int hours = (int)(dailyTotal / 3600);
                                int minutes = (int)((dailyTotal - (hours * 3600)) / 60);

                                Program.WriteLineMessage("Daily total: " + hours + "h " + minutes + "m",
                                    ConsoleColor.Cyan);

                                hours = (int)(restTotal / 3600);
                                minutes = (int)((restTotal - (hours * 3600)) / 60);

                                Program.WriteLineMessage("Daily overflow:" + hours + "h " + minutes + "m",
                                    ConsoleColor.Red);

                                totalTime += dailyTotal;

                                Console.WriteLine();
                            }
                            else
                            {
                                Console.WriteLine("NO ENTRIES FOUND");
                            }
                        }
                    }
                }

                int totalHours = (int)(totalTime / 3600);
                int totalMinutes = (int)((totalTime - (totalHours * 3600)) / 60);
                    
                Program.WriteLineMessage("Total time: " + totalHours + "h " + totalMinutes + "m", ConsoleColor.Magenta);

                totalHours = (int)(timeFixer.RestSeconds / 3600);
                totalMinutes = (int)((timeFixer.RestSeconds - (totalHours * 3600)) / 60);

                Program.WriteLineMessage("Total overflow:" + totalHours + "h " + totalMinutes + "m", ConsoleColor.DarkGreen);
            }
            catch (Exception e)
            {
                Program.WriteLineMessage("Unexpected error: " + e.Message, ConsoleColor.Red);
            }               
        }

        private static List<(string week, List<string> files)> GetFiles(string command)
        {
            // Remove the "u " part.
            string folders = command.Substring(2);   
            var parts = folders.Split(' ');

            var output = new List<(string week, List<string> files)>();
            foreach (var part in parts)
            {
                var files = Directory.EnumerateFiles(part).ToList();
                output.Add((part, files));
            }

            return output;
        }
    }
}

using System.Collections.Generic;
using System.IO;

namespace GeneticSearch
{
    static class CommandProcessor
    {
        public static void ProcessCommands(string inputFile, string outputFile, List<Protein> proteins)
        {
            if (!File.Exists(inputFile)) return;

            using (StreamReader reader = new StreamReader(inputFile))
            using (StreamWriter writer = new StreamWriter(outputFile))
            {
                writer.WriteLine("Дюрдь Игорь");
                writer.WriteLine("Genetic Searching");
                writer.WriteLine(new string('-', 80));

                int commandCounter = 1;

                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    string[] parts = line.Split('\t');
                    if (parts.Length < 4)
                    {
                        string commandName = parts[0].Trim();

                        string p1 = parts.Length > 1 ? parts[1].Trim() : string.Empty;
                        string p2 = parts.Length > 2 ? parts[2].Trim() : string.Empty;

                        CommandExecutor.Execute(commandName, p1, p2, proteins, writer, commandCounter);

                        writer.WriteLine(new string('-', 80));
                        commandCounter++;
                    }
                    else writer.WriteLine("Неправильная команда");
                }
            }
        }
    }
}
using System.Collections.Generic;

namespace GeneticSearch
{
    class Program
    {
        static void Main(string[] args)
        {
            const string InformationFile = "sequences.0.txt";
            const string CommandsFile = "commands.0.txt";
            const string ResultFile = "genedata.0.txt";

            List<Protein> data = DataReader.ReadData(InformationFile);
            CommandProcessor.ProcessCommands(CommandsFile, ResultFile, data);
        }
    }
}
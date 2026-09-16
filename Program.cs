using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticSearch
{
    class Program
    {
        struct Protein
        {
            public string protein_name; 
            public string organism_name; 
            public string code_amino_acid; 
        }

        struct Commands
        {
            public string name;
            public string parameter1;
            public string parameter2;
        }

        static List<Commands> ReadCommands(string filename)
        {
            StreamReader reader = new StreamReader(filename);
            List<Commands> commands = new List<Commands>();

            Commands command;
            command.name = String.Empty;
            command.parameter1 = String.Empty;
            command.parameter2 = String.Empty;

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] parts = line.Split('\t');

                if (parts.Length == 2)
                {
                    command.name = parts[0];
                    command.parameter1 = parts[1];
                    command.parameter2 = String.Empty;
                }
                else
                {
                    command.name = parts[0];
                    command.parameter1 = parts[1];
                    command.parameter2 = parts[2];
                }
                commands.Add(command);
            }
            return commands;
        }

        static List<Protein> ReadData(string filename)
        {
            
            StreamReader reader = new StreamReader(filename);

            
            List<Protein> data = new List<Protein>();

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] parts = line.Split('\t');
                Protein protein;
                protein.protein_name = parts[0];
                protein.organism_name = parts[1];
                protein.code_amino_acid = parts[2];
                data.Add(protein);
            }
            return data;
        }

        static string Encoding(string code_amino_acid)
        {
            string encoded = String.Empty;
            for (int i = 0; i < code_amino_acid.Length; i++)
            {
                char ch = code_amino_acid[i];
                int count = 1;
                while (i < code_amino_acid.Length - 1 && code_amino_acid[i + 1] == ch)
                {
                    count++;
                    i++;
                }
                if (count > 2) encoded = encoded + count + ch;
                if (count == 1) encoded = encoded + ch;
                if (count == 2) encoded = encoded + ch + ch;
            }
            return encoded;
        }

        static string Decoding(string code_amino_acid)
        {
            string decoded = String.Empty;
            for (int i = 0; i < code_amino_acid.Length; i++)
            {   // 8ATA3TCGC4T....
                char ch = code_amino_acid[i];
                if (char.IsDigit(ch))  // '8' -> int 8
                {
                    char letter = code_amino_acid[i + 1];
                    int count = ch - '0'; // '8' - '0' = 8
                    for (int j = 1; j < count; j++)
                        decoded = decoded + letter;
                }
                else decoded = decoded + ch;
            }
            return decoded;
        }

        static void PrintData(List<Protein> data)
        {
            for (int i = 0; i < data.Count; i++)
            {
                Console.WriteLine("Protein " + (i + 1));
                Console.WriteLine(data[i].protein_name);
                Console.WriteLine(data[i].organism_name);
                Console.WriteLine(data[i].code_amino_acid);
                Console.WriteLine("========================");
            }
        }

        static void PrintCommands(List<Commands> commands)
        {
            for (int i = 0; i < commands.Count; i++)
            {
                Console.WriteLine("Command " + (i + 1));
                Console.WriteLine(commands[i].name);
                Console.WriteLine(commands[i].parameter1);
                Console.WriteLine(commands[i].parameter2);
                Console.WriteLine("========================");
            }
        }

        static void CommandHandler(List<Protein> proteins, List<Commands> commands)
        {
            for (int i = 0; i < commands.Count; i++)
            {
                if (commands[i].name == "search") { }
                if (commands[i].name == "diff") { }
                if (commands[i].name == "mode") { }
            }
        }

        static void Main(string[] args)
        {
            //reding protein data
            List<Protein> data = ReadData("sequences.0.txt");
            PrintData(data);

            //reading commands
            List<Commands> commands = ReadCommands("commands.0.txt");
            PrintCommands(commands);
        }
    }
}








using System.Text;


namespace GeneticSearch
{
    class Program
    {
        struct Protein
        {
            private string protein_name; 
            private string organism_name; 
            private string code_amino_acid;

            public string ProteinName
            {
                get { return protein_name; }
                set { protein_name = value ?? string.Empty; }
            }

            public string OrganismName
            {
                get { return organism_name; }
                set { organism_name = value ?? string.Empty; }
            }

            public string CodeAminoAcid
            {
                get { return code_amino_acid; }
                set { code_amino_acid = value ?? string.Empty; }
            }
        }

        struct Commands
        {
            private string name;
            private string parameter1;
            private string parameter2;

            public string Name
            {
                get { return name; }
                set { name = value ?? string.Empty; }
            }

            public string Parameter1
            {
                get { return parameter1; }
                set { parameter1 = value ?? string.Empty; }
            }

            public string Parameter2
            {
                get { return parameter2; }
                set { parameter2 = value ?? string.Empty; }
            }
            public Commands()
            {
                name = String.Empty;
                parameter1 = String.Empty;
                parameter2 = String.Empty;
            }
        }

        static List<Protein> ReadData(string filename)
        {
            List<Protein> data = new List<Protein>();
            using (StreamReader reader = new StreamReader(filename))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    string[] parts = line.Split('\t');
                    if (parts.Length < 3)
                    {
                        Console.WriteLine($"Пропущена некорректная строка: {line}");
                        continue;
                    }
                    Protein protein= new Protein();
                    protein.ProteinName = parts[0].Trim();
                    protein.OrganismName = parts[1].Trim();
                    protein.CodeAminoAcid = parts[2].Trim();
                    data.Add(protein);
                }
            }
            return data;
        }
        static void ExecuteCommand(string command, string p1, string p2, List<Protein> proteins, StreamWriter writer, int counter)
        {
            string prefix = $"{counter} {command}";

            if (command == "search")
            {
                writer.WriteLine($"{prefix}\t{p1}");
                string searchSequence = RLDecoding(p1);
                bool found = false;

                writer.WriteLine("organism\tprotein");

                foreach (var protein in proteins)
                {
                    if (protein.CodeAminoAcid.Contains(searchSequence))
                    {
                        writer.WriteLine($"{protein.OrganismName}\t{protein.ProteinName}");
                        found = true;
                    }
                }

                if (!found)
                {
                    writer.WriteLine("NOT FOUND");
                }
            }
            else if (command == "diff")
            {
                writer.WriteLine($"{prefix}\t{p1}\t{p2}");

                int index1 = proteins.FindIndex(p => p.ProteinName == p1);
                int index2 = proteins.FindIndex(p => p.ProteinName == p2);

                if (index1 == -1 || index2 == -1)
                {
                    writer.WriteLine("MISSING:");
                    if (index1 == -1) writer.WriteLine(p1);
                    if (index2 == -1) writer.WriteLine(p2);
                }
                else
                {
                    string seq1 = proteins[index1].CodeAminoAcid;
                    string seq2 = proteins[index2].CodeAminoAcid;

                    int differences = Math.Abs(seq1.Length - seq2.Length);
                    int minLength = Math.Min(seq1.Length, seq2.Length);

                    for (int i = 0; i < minLength; i++)
                    {
                        if (seq1[i] != seq2[i])
                        {
                            differences++;
                        }
                    }

                    writer.WriteLine("amino-acids difference:");
                    writer.WriteLine(differences);
                }
            }
            else if (command == "mode")
            {
                writer.WriteLine($"{prefix}\t{p1}");

                int index = proteins.FindIndex(p => p.ProteinName == p1);

                if (index == -1)
                {
                    writer.WriteLine("amino-acid occurs:");
                    writer.WriteLine($"MISSING:\t{p1}");
                }
                else
                {
                    string seq = proteins[index].CodeAminoAcid;

                    var mostFrequent = seq
                        .GroupBy(c => c)
                        .OrderByDescending(g => g.Count())
                        .ThenBy(g => g.Key)
                        .FirstOrDefault();

                    writer.WriteLine("amino-acid\toccurs:");
                    if (mostFrequent != null)
                    {
                        writer.WriteLine($"{mostFrequent.Key}\t{mostFrequent.Count()}");
                    }
                }
            }
        }

        static string RLDecoding(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < input.Length; i++)
            {
                if (char.IsDigit(input[i]))
                {
                    int count = input[i] - '0';
                    if (i + 1 < input.Length)
                    {
                        char nextChar = input[i + 1];
                        sb.Append(nextChar, count);
                        i++;
                    }
                }
                else
                {
                    sb.Append(input[i]);
                }
            }

            return sb.ToString();
        }

        static void ProcessCommands(string inputFile, string outputFile, List<Protein> proteins)
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
                    string commandName = parts[0].Trim();

                    string p1 = parts.Length > 1 ? parts[1].Trim() : string.Empty;
                    string p2 = parts.Length > 2 ? parts[2].Trim() : string.Empty;

                    ExecuteCommand(commandName, p1, p2, proteins, writer, commandCounter);

                    writer.WriteLine(new string('-', 80));
                    commandCounter++;
                }
            }
        }
        static void Main(string[] args)
        {
            const String InformationFile = "sequences.0.txt";
            const String CommandsFile = "commands.0.txt";
            const String ResultFile = "genedata.0.txt";

            List<Protein> data = ReadData(InformationFile);
            ProcessCommands(CommandsFile, ResultFile, data);

        }
    }
}








using System;


namespace GeneticSearch
{
    static class CommandExecutor
    {
        public static void Execute(string command, string p1, string p2,
                                   List<Protein> proteins, StreamWriter writer, int counter)
        {
            string prefix = $"{counter} {command}";

            if (command == "search")
            {
                writer.WriteLine($"{prefix}\t{p1}");
                string searchSequence = RLDecoder.Decode(p1);
                bool found = false;

                writer.WriteLine("organism\t\t\tprotein");

                foreach (var protein in proteins)
                {
                    string name = RLDecoder.Decode(protein.CodeAminoAcid);
                    if (name.Contains(searchSequence))
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
            else
            {
                writer.WriteLine($"Unknown command number\t{counter}");
            }
        }
    }
}
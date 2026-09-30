using System;
using System.Collections.Generic;
using System.IO;

namespace GeneticSearch
{
    public static class DataReader
    {
        public static List<Protein> ReadData(string filename)
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
                    Protein protein = new Protein();
                    protein.ProteinName = parts[0].Trim();
                    protein.OrganismName = parts[1].Trim();
                    protein.CodeAminoAcid = parts[2].Trim();
                    data.Add(protein);
                }
            }
            return data;
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;
using GeneticSearch;

namespace GeneticSearch.Tests
{
    
    public class RLDecoderTests
    {
        [Fact]
        public void Decode_Null_ReturnsEmpty()
        {
            Assert.Equal(string.Empty, RLDecoder.Decode(null));
        }

        [Fact]
        public void Decode_EmptyString_ReturnsEmpty()
        {
            Assert.Equal(string.Empty, RLDecoder.Decode(""));
        }

        [Theory]
        [InlineData("3A", "AAA")]
        [InlineData("2B3C", "BBCCC")]
        [InlineData("1A1B1C", "ABC")]
        [InlineData("AB", "AB")]
        [InlineData("A3B", "ABBB")]
        [InlineData("0A", "")]          
        [InlineData("9X", "XXXXXXXXX")] 
        public void Decode_VariousInputs_ReturnsExpected(string input, string expected)
        {
            Assert.Equal(expected, RLDecoder.Decode(input));
        }

        [Fact]
        public void Decode_DigitAtEnd_IgnoresTrailingDigit()
        {
            Assert.Equal("AAA", RLDecoder.Decode("3A2"));
        }
    }

    // ============================================================
    // 2. Тесты для DataReader.ReadData
    // ============================================================
    public class DataReaderTests : IDisposable
    {
        private readonly string _tempFile;

        public DataReaderTests()
        {
            _tempFile = Path.GetTempFileName();
        }

        public void Dispose()
        {
            if (File.Exists(_tempFile)) File.Delete(_tempFile);
        }

        [Fact]
        public void ReadData_ValidFile_ParsesAllRows()
        {
            File.WriteAllText(_tempFile,
                "Prot1\tHomo sapiens\tMKV\n" +
                "Prot2\tE. coli\t2A3B\n");

            var result = DataReader.ReadData(_tempFile);

            Assert.Equal(2, result.Count);
            Assert.Equal("Prot1", result[0].ProteinName);
            Assert.Equal("Homo sapiens", result[0].OrganismName);
            Assert.Equal("MKV", result[0].CodeAminoAcid);
            Assert.Equal("Prot2", result[1].ProteinName);
            Assert.Equal("E. coli", result[1].OrganismName);
            Assert.Equal("2A3B", result[1].CodeAminoAcid);
        }

        [Fact]
        public void ReadData_EmptyLines_Skipped()
        {
            File.WriteAllText(_tempFile,
                "Prot1\tHomo sapiens\tMKV\n" +
                "\n" +
                "   \n" +
                "Prot2\tE. coli\t2A3B\n");

            var result = DataReader.ReadData(_tempFile);

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void ReadData_MalformedLine_Skipped()
        {
            File.WriteAllText(_tempFile,
                "Prot1\tHomo sapiens\tMKV\n" +
                "OnlyOneColumn\n" +
                "Prot2\tE. coli\t2A3B\n");

            var result = DataReader.ReadData(_tempFile);

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void ReadData_WhitespaceAroundValues_Trimmed()
        {
            File.WriteAllText(_tempFile,
                "  Prot1  \t  Homo sapiens  \t  MKV  \n");

            var result = DataReader.ReadData(_tempFile);

            Assert.Single(result);
            Assert.Equal("Prot1", result[0].ProteinName);
            Assert.Equal("Homo sapiens", result[0].OrganismName);
            Assert.Equal("MKV", result[0].CodeAminoAcid);
        }
    }

    public class CommandProcessorIntegrationTests : IDisposable
    {
        private readonly string _commandsFile;
        private readonly string _outputFile;

        public CommandProcessorIntegrationTests()
        {
            _commandsFile = Path.GetTempFileName();
            _outputFile = Path.GetTempFileName();
        }

        public void Dispose()
        {
            if (File.Exists(_commandsFile)) File.Delete(_commandsFile);
            if (File.Exists(_outputFile)) File.Delete(_outputFile);
        }

        [Fact]
        public void ProcessCommands_FullScenario_WritesAllResults()
        {
            
            var proteins = new List<Protein>
            {
                new Protein { ProteinName = "P1", OrganismName = "Human",  CodeAminoAcid = "MKV" },
                new Protein { ProteinName = "P2", OrganismName = "Mouse",  CodeAminoAcid = "MKR" },
            };

            
            File.WriteAllText(_commandsFile,
                "search\tMKV\n" +
                "diff\tP1\tP2\n" +
                "mode\tP1\n");

            
            CommandProcessor.ProcessCommands(_commandsFile, _outputFile, proteins);

            string output = File.ReadAllText(_outputFile);

            
            Assert.Contains("Дюрдь Игорь", output);
            Assert.Contains("Genetic Searching", output);
            Assert.Contains("1 search", output);
            Assert.Contains("2 diff", output);
            Assert.Contains("3 mode", output);
            Assert.Contains("Human", output);
            Assert.Contains("amino-acids difference:", output);
            Assert.Contains("amino-acid", output);
        }

        [Fact]
        public void ProcessCommands_EmptyCommandLines_Skipped()
        {
            var proteins = new List<Protein>
            {
                new Protein { ProteinName = "P1", OrganismName = "Human", CodeAminoAcid = "MKV" }
            };

            File.WriteAllText(_commandsFile,
                "search\tMKV\n" +
                "\n" +
                "   \n" +
                "mode\tP1\n");

            CommandProcessor.ProcessCommands(_commandsFile, _outputFile, proteins);
            string output = File.ReadAllText(_outputFile);

            
            Assert.Contains("1 search", output);
            Assert.Contains("2 mode", output);
        }
    }
}
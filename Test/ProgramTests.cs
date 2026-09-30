using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;
using GeneticSearch;

namespace GeneticSearch.Tests
{
    // ============================================================
    // 1. Тесты для RLDecoder.Decode
    // ============================================================
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
        [InlineData("0A", "")]          // 0 повторов → пусто
        [InlineData("9X", "XXXXXXXXX")] // максимальная цифра
        public void Decode_VariousInputs_ReturnsExpected(string input, string expected)
        {
            Assert.Equal(expected, RLDecoder.Decode(input));
        }

        [Fact]
        public void Decode_DigitAtEnd_IgnoresTrailingDigit()
        {
            // "3A2" — двойка в конце без следующего символа, должна быть проигнорирована
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

    // ============================================================
    // 3. Тесты для CommandExecutor.Execute
    // ============================================================
    public class CommandExecutorTests
    {
        // Хелпер: собрать список белков для тестов
        private static List<Protein> MakeProteins()
        {
            return new List<Protein>
            {
                new Protein { ProteinName = "P1", OrganismName = "Human",  CodeAminoAcid = "2A3B" }, // AABBB
                new Protein { ProteinName = "P2", OrganismName = "Mouse",  CodeAminoAcid = "MKV"  }, // MKV
                new Protein { ProteinName = "P3", OrganismName = "E.coli", CodeAminoAcid = "AABB" }, // AABB
            };
        }

        // Хелпер: выполнить команду и вернуть вывод
        private static string RunCommand(string command, string p1, string p2,
                                         List<Protein> proteins, int counter = 1)
        {
            using var ms = new MemoryStream();
            using (var writer = new StreamWriter(ms, new UTF8Encoding(false), 1024, true))
            {
                CommandExecutor.Execute(command, p1, p2, proteins, writer, counter);
                writer.Flush();
            }
            return Encoding.UTF8.GetString(ms.ToArray());
        }

        // ---------- search ----------

        [Fact]
        public void Search_FindsSubstring_ReturnsOrganismAndProtein()
        {
            var proteins = MakeProteins();
            string output = RunCommand("search", "AAB", "", proteins);

            Assert.Contains("Human", output);
            Assert.Contains("P1", output);
            Assert.DoesNotContain("NOT FOUND", output);
        }

        [Fact]
        public void Search_NoMatch_ReturnsNotFound()
        {
            var proteins = MakeProteins();
            string output = RunCommand("search", "ZZZ", "", proteins);

            Assert.Contains("NOT FOUND", output);
        }

        [Fact]
        public void Search_RunLengthEncodedPattern_DecodesFirst()
        {
            var proteins = MakeProteins();
            // "3B" → "BBB" — есть в P1 (AABBB)
            string output = RunCommand("search", "3B", "", proteins);

            Assert.Contains("Human", output);
            Assert.DoesNotContain("NOT FOUND", output);
        }

        // ---------- diff ----------

        [Fact]
        public void Diff_EqualLength_CountsMismatches()
        {
            var proteins = MakeProteins();
            // P1 = 2A3B → AABBB (5), P3 = AABB (4). Разница длин = 1
            // Общая длина 4: A==A, A==A, B==B, B==B → 0 мисматчей, +1 за длину = 1
            string output = RunCommand("diff", "P1", "P3", proteins);

            Assert.Contains("amino-acids difference:", output);
            Assert.Contains("1", output);
        }

        [Fact]
        public void Diff_Identical_ReturnsZero()
        {
            var proteins = MakeProteins();
            string output = RunCommand("diff", "P1", "P1", proteins);

            Assert.Contains("amino-acids difference:", output);
            Assert.Contains("0", output);
        }

        [Fact]
        public void Diff_MissingProtein_ReportsMissing()
        {
            var proteins = MakeProteins();
            string output = RunCommand("diff", "P1", "UNKNOWN", proteins);

            Assert.Contains("MISSING:", output);
            Assert.Contains("UNKNOWN", output);
        }

        // ---------- mode ----------

        [Fact]
        public void Mode_MostFrequentChar_Returned()
        {
            var proteins = MakeProteins();
            // P1 = 2A3B → AABBB; B встречается 3 раза, A — 2. Должен победить B.
            string output = RunCommand("mode", "P1", "", proteins);

            Assert.Contains("amino-acid", output);
            Assert.Contains("B", output);
            Assert.Contains("3", output);
        }

        [Fact]
        public void Mode_TieBreaker_Alphabetical()
        {
            var proteins = new List<Protein>
            {
                // AABB: A=2, B=2 → тай-брейк по алфавиту → A
                new Protein { ProteinName = "Tie", OrganismName = "X", CodeAminoAcid = "AABB" }
            };
            string output = RunCommand("mode", "Tie", "", proteins);

            Assert.Contains("A", output);
            Assert.Contains("2", output);
        }

        [Fact]
        public void Mode_MissingProtein_ReportsMissing()
        {
            var proteins = MakeProteins();
            string output = RunCommand("mode", "UNKNOWN", "", proteins);

            Assert.Contains("MISSING:", output);
            Assert.Contains("UNKNOWN", output);
        }

        // ---------- неизвестная команда ----------

        [Fact]
        public void UnknownCommand_WritesErrorMessage()
        {
            var proteins = MakeProteins();
            string output = RunCommand("foobar", "x", "y", proteins, counter: 5);

            Assert.Contains("Unknown command number", output);
            Assert.Contains("5", output);
        }
    }

    // ============================================================
    // 4. Интеграционный тест: ProcessCommands от начала до конца
    // ============================================================
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
            // Исходные белки
            var proteins = new List<Protein>
            {
                new Protein { ProteinName = "P1", OrganismName = "Human",  CodeAminoAcid = "MKV" },
                new Protein { ProteinName = "P2", OrganismName = "Mouse",  CodeAminoAcid = "MKR" },
            };

            // Файл команд
            File.WriteAllText(_commandsFile,
                "search\tMKV\n" +
                "diff\tP1\tP2\n" +
                "mode\tP1\n");

            // Запуск
            CommandProcessor.ProcessCommands(_commandsFile, _outputFile, proteins);

            string output = File.ReadAllText(_outputFile);

            // Проверки
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
        public void ProcessCommands_MissingInputFile_DoesNothing()
        {
            string nonExistent = Path.Combine(Path.GetTempPath(), "no_such_file_" + Guid.NewGuid() + ".txt");
            var proteins = new List<Protein>();

            // Не должно бросить исключение
            CommandProcessor.ProcessCommands(nonExistent, _outputFile, proteins);

            Assert.True(File.Exists(_outputFile));
            // Файл либо пуст, либо не создан — главное, что не упало
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

            // Должно быть ровно 2 команды, счётчик дошёл до 2
            Assert.Contains("1 search", output);
            Assert.Contains("2 mode", output);
        }
    }
}
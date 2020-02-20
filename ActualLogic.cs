using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Hashcode2k20
{
    class InputFile
    {
        public int NumberOfDifferentBooks { get; set; }
        public int NumberOfLibraries { get; set; }
        public int NumberOfMaxDays { get; set; }

        /// <summary>
        /// Index is ID of Book
        /// </summary>
        public (int Index, int Score)[] BookScores { get; set; }

        public List<Library> Libraries { get; set; }
    }

    class Library
    {
        public int NumberOfBooksInLibrary { get; set; }
        public int NumberOfDaysForSignup { get; set; }
        public int NumberOfShipmentsPossiblePerDay { get; set; }

        public int[] IdsOfBooksInLibrary { get; set; }
    }




    class ActualLogic
    {
        private InputFile _input = new InputFile();
        private string _outputFile;
        public ActualLogic(string inputFileName, string outputFolder)
        {
            _outputFile = Path.Combine(outputFolder, $"{Path.GetFileNameWithoutExtension(inputFileName)}.out");

            string[] inputFileContents = File.ReadAllText(inputFileName).Split('\n');
            string[] lineOne = inputFileContents[0].Split(' ');

            // -->
        }

        /// <summary>
        /// GOAL: Order as many slices as possible, but not more than the maximum number
        /// </summary>
        public string Process()
        {
            var sbOut = new StringBuilder($"\n");


            File.WriteAllText(_outputFile, sbOut.ToString());
            return _outputFile;
        }
    }
}

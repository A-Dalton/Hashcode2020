using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Hashcode2k20
{
    class InputFile
    {
        public int NumberOfDifferentBooks { get; set; }
        public int NumberOfLibraries { get; set; }
        public int NumberOfMaxDays { get; set; }

        /// <summary>
        /// Key is ID of Book
        /// </summary>
        public Dictionary<int, int> BookWithScore { get; set; } = new Dictionary<int, int>();

        public Library[] Libraries { get; set; }
    }

    class Library
    {
        public int LibraryId { get; set; }
        public int NumberOfBooksInLibrary { get; set; }
        public int NumberOfDaysForSignup { get; set; }
        public int NumberOfShipmentsPossiblePerDay { get; set; }

        public HashSet<int> IdsOfBooksInLibrary { get; set; }

        public long ScoreOfAllBooks { get; set; }

        // Some kind of priority? / Related To "NumberOfShipmentsPossiblePerDay" + "NumberOfDaysForSignup"
    }

    class LibraryOutput
    {
        public Library Lib { get; set; }
        public int LibraryId { get; set; }

        public List<int> BookIdsToScan { get; set; }

        // Internal use 1
        public int SignedUpOnDay { get; set; }

        // Internal use 2
        public int CurrentShippingDay { get; set; }
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

            // Data filling from line 1
            _input.NumberOfDifferentBooks = int.Parse(lineOne[0]);
            _input.NumberOfLibraries = int.Parse(lineOne[1]);
            _input.NumberOfMaxDays = int.Parse(lineOne[2]);
            _input.Libraries = new Library[_input.NumberOfLibraries];

            // Data filling from line 2
            int i = 0;
            foreach (int bookId in inputFileContents[1].Split(' ').Select(x => int.Parse(x)))
                _input.BookWithScore.Add(i++, bookId);

            // Data filling of libraries starting from line 3
            int iLine = 2; // Current line in input file
            for (int iLib = 0; iLib < _input.NumberOfLibraries; iLib++)
            {
                _input.Libraries[iLib] = new Library() { LibraryId = iLib };

                string[] curLine = inputFileContents[iLine].Split(' ');

                _input.Libraries[iLib].NumberOfBooksInLibrary = int.Parse(curLine[0]);
                _input.Libraries[iLib].NumberOfDaysForSignup = int.Parse(curLine[1]);
                _input.Libraries[iLib].NumberOfShipmentsPossiblePerDay = int.Parse(curLine[2]);

                iLine++; // Onto the next line
                _input.Libraries[iLib].IdsOfBooksInLibrary = inputFileContents[iLine].Split(' ').Select(x => int.Parse(x)).ToHashSet();
                iLine++; // Onto the lext library
            }
        }

        /// <summary>
        /// GOAL: Order as many slices as possible, but not more than the maximum number
        /// </summary>
        public string Process()
        {
            // Track already used books
            HashSet<int> usedBooksIds = new HashSet<int>();

            // Try some sort of order where to start
            foreach (Library l in _input.Libraries)
                l.ScoreOfAllBooks = _input.BookWithScore.Where(x => l.IdsOfBooksInLibrary.Contains(x.Key)).Sum(x => x.Value);

            int totalSignUpDayCounter = 0;
            var librariesToUse = new List<LibraryOutput>();

            // First off - only handle the sign-up process for the libraries - try to have the better libraries at first
            foreach (Library l in _input.Libraries.OrderByDescending(x => x.ScoreOfAllBooks).OrderBy(x => x.NumberOfDaysForSignup))
            {
                if (totalSignUpDayCounter > _input.NumberOfMaxDays)
                    break;

                // Entry for the output file
                librariesToUse.Add(new LibraryOutput() { Lib = l, LibraryId = l.LibraryId, BookIdsToScan = new List<int>(), SignedUpOnDay = totalSignUpDayCounter, CurrentShippingDay = totalSignUpDayCounter });

                totalSignUpDayCounter += l.NumberOfDaysForSignup + 1; // +1 as the next library can start on the next day
            }

            // Within the library, scan as much books as possible
            foreach (var l in librariesToUse)
            {
                int dayCounter = 0;

                if (l.CurrentShippingDay > _input.NumberOfMaxDays)
                    break;

                dayCounter += l.Lib.NumberOfShipmentsPossiblePerDay;

                int currentyDayProcessingCounter = 0;
                // Books with the highest score first :)
                foreach (var bookId in l.Lib.IdsOfBooksInLibrary.Where(x => !usedBooksIds.Contains(x)).OrderByDescending(x => _input.BookWithScore[x]))
                {
                    if (currentyDayProcessingCounter == l.Lib.NumberOfShipmentsPossiblePerDay)
                    {
                        l.CurrentShippingDay++;
                        currentyDayProcessingCounter = 0;
                    }

                    if (l.CurrentShippingDay > _input.NumberOfMaxDays)
                        break;

                    l.BookIdsToScan.Add(bookId);
                    usedBooksIds.Add(bookId);
                }
            }

            // Line 1:                      Single integer (number of libraries to sign up)
            // Line 2 / 4 / 6 ... :         Id of library + Number of books from library
            // Line 3 / 5 / 7 ... :         Ids of books to scan (separated by space)
            var sbOut = new StringBuilder($"{librariesToUse.Count}\n");
            foreach (var iLib in librariesToUse)
            {
                if (iLib.BookIdsToScan.Count == 0)
                    continue;

                sbOut.Append($"{iLib.LibraryId} {iLib.BookIdsToScan.Count}\n");
                foreach (int bookId in iLib.BookIdsToScan)
                {
                    sbOut.Append(bookId);
                    sbOut.Append(' ');
                }
                sbOut.Append("\n");
            }

            File.WriteAllText(_outputFile, sbOut.ToString());
            return _outputFile;
        }
    }
}

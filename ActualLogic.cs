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
        public Dictionary<int, int> BooksWithActualScore { get; set; } = new Dictionary<int, int>();

        public bool PriorizizeScore { get; set; }

        public Library[] Libraries { get; set; }

        /// <summary>
        /// Key is ID of Book, value is number of times the book is used across all libraries
        /// </summary>
        public Dictionary<int, int> StockAcrossLibraries { get; set; } = new Dictionary<int, int>();

        /// <summary>
        /// Helper property for sorting
        /// </summary>
        public Dictionary<int, int> BookWithScoreNormalizedByNumberOfDuplicates { get; set; } = new Dictionary<int, int>();
    }

    class Library
    {
        public int LibraryId { get; set; }
        public int NumberOfBooksInLibrary { get; set; }
        public int NumberOfDaysForSignup { get; set; }
        public int NumberOfShipmentsPossiblePerDay { get; set; }

        public HashSet<int> IdsOfBooksInLibrary { get; set; }

        public long ScoreOfAllBooks { get; set; }
        public long ScoreOfAllBooksNormalizedByNumberOfDuplicates { get; set; }

        // Some kind of priority? / Related To "NumberOfShipmentsPossiblePerDay" + "NumberOfDaysForSignup"

        // Total days it would take from Signup to scanning all the books
        // x.NumberOfDaysForSignup + (x.NumberOfBooksInLibrary / x.NumberOfShipmentsPossiblePerDay)
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
            foreach (int score in inputFileContents[1].Split(' ').Select(x => int.Parse(x)))
                _input.BooksWithActualScore.Add(i++, score);

            // _input.PriorizizeScore = _input.BookWithScore.Values.Distinct().Count() > (_input.NumberOfDifferentBooks / 100);

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

        public void PopulateHelperProperties()
        {
            // Count how many times a book is in stock across libraries
            for (int bookId = 0; bookId < _input.NumberOfDifferentBooks; bookId++)
            {
                foreach (var lib in _input.Libraries.Where(x => x.IdsOfBooksInLibrary.Contains(bookId)))
                {
                    if (_input.StockAcrossLibraries.TryGetValue(bookId, out int count))
                        _input.StockAcrossLibraries[bookId]++;
                    else
                        _input.StockAcrossLibraries.Add(bookId, 1);
                }

                // Reflect books which are not available in any library
                if (!_input.StockAcrossLibraries.ContainsKey(bookId))
                    _input.StockAcrossLibraries.Add(bookId, 0);
            }

            // Normalize the actual score based on the number of duplicate books
            for (int bookId = 0; bookId < _input.NumberOfDifferentBooks; bookId++)
            {
                int bookCountAcrossLibraries = _input.StockAcrossLibraries[bookId];
                _input.BookWithScoreNormalizedByNumberOfDuplicates.Add(bookId, bookCountAcrossLibraries == 0 ? 0 : _input.BooksWithActualScore[bookId] / _input.StockAcrossLibraries[bookId]);
            }

            // Track the total score of all books available in the library
            foreach (Library l in _input.Libraries)
            {
                l.ScoreOfAllBooks = _input.BooksWithActualScore.Where(x => l.IdsOfBooksInLibrary.Contains(x.Key)).Sum(x => x.Value);
                l.ScoreOfAllBooksNormalizedByNumberOfDuplicates = _input.BookWithScoreNormalizedByNumberOfDuplicates.Where(x => l.IdsOfBooksInLibrary.Contains(x.Key)).Sum(x => x.Value);
            }
        }

        public List<LibraryOutput> DoBookScanningCalculation()
        {
            HashSet<int> usedBooksIds = new HashSet<int>();
            int totalSignUpDayCounter = 0;
            var librariesToUse = new List<LibraryOutput>();

            // As the order is the most important part of the calculation, here are other ideas to improve on the mechanism:
            // "_input.Libraries.OrderByDescending(x => x.ScoreOfAllBooks).OrderBy(x => x.NumberOfDaysForSignup))" had a relatively good score
            // var libOrder = _input.Libraries.OrderByDescending(x => x.ScoreOfAllBooks).OrderBy(x => (x.NumberOfDaysForSignup + (x.NumberOfBooksInLibrary / x.NumberOfShipmentsPossiblePerDay)));

            var libOrder = _input.Libraries.OrderByDescending(x => x.ScoreOfAllBooksNormalizedByNumberOfDuplicates).OrderBy(x => x.NumberOfDaysForSignup);

            foreach (Library l in libOrder)
            {
                if (totalSignUpDayCounter > _input.NumberOfMaxDays)
                    break;

                // Entry for the output file
                librariesToUse.Add(new LibraryOutput() { Lib = l, LibraryId = l.LibraryId, BookIdsToScan = new List<int>(), SignedUpOnDay = totalSignUpDayCounter, CurrentShippingDay = (totalSignUpDayCounter + l.NumberOfDaysForSignup) });

                totalSignUpDayCounter += l.NumberOfDaysForSignup; // 'totalSignUpDayCounter' now points to the start day for the next library
            }

            // Within the library, scan as much books as possible
            List<LibraryOutput> zeroBooks = new List<LibraryOutput>();
            foreach (var l in librariesToUse)
            {
                // Books with the highest score first :)
                int booksPerDayCounter = 0;
                foreach (var bookId in l.Lib.IdsOfBooksInLibrary.OrderByDescending(x => _input.BookWithScoreNormalizedByNumberOfDuplicates[x]))
                {
                    // Do not re-use books
                    if (usedBooksIds.Contains(bookId))
                        continue;

                    if (booksPerDayCounter == l.Lib.NumberOfShipmentsPossiblePerDay)
                    {
                        l.CurrentShippingDay++; // Ship the next book on the next day
                        if (l.CurrentShippingDay == _input.NumberOfMaxDays)
                            break; // D–1 is the last day during which books can be shipped
                        booksPerDayCounter = 0; // Start again with book 0 that day
                    }

                    l.BookIdsToScan.Add(bookId);
                    usedBooksIds.Add(bookId);

                    booksPerDayCounter++;
                }

                // Manually remove libraries with no books to scan...
                if (l.BookIdsToScan.Count == 0)
                    zeroBooks.Add(l);
            }

            foreach (var l in zeroBooks)
                librariesToUse.Remove(l);

            return librariesToUse;
        }

        /// <summary>
        /// GOAL: Scan as many books as possible - with the highest score possible
        /// </summary>
        public string Process()
        {
            // Populate some properties which are only meant to help in the actual calcuation
            PopulateHelperProperties();

            var output = DoBookScanningCalculation();

            // Line 1:                      Single integer (number of libraries to sign up)
            // Line 2 / 4 / 6 ... :         Id of library + Number of books from library
            // Line 3 / 5 / 7 ... :         Ids of books to scan (separated by space)
            var sbOut = new StringBuilder($"{output.Count}\n");
            foreach (var iLib in DoBookScanningCalculation())
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

using MovieAPI.Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace MovieServiceAPITests_NUnit
{
    public class MovieServiceTests: MovieAPI.Services.MovieService
    {
        private const string testName = "test name";
        private const long testLanguage = 1;
        private const long testID = 2021;
        private static MovieDTO GetValidItem()
        {
            return new MovieDTO()
            {
                ID = testID,
                Name = testName,
                Language = testLanguage,
                FilmingStarted = new DateTime(2020, 10, 10),
                FilmingEnded = new DateTime(2020, 11, 10)
            };
        }

        [TestCase(null, ExpectedResult = false,Description = "NULL")]
        [TestCase("", ExpectedResult = false, Description = "Empty")]
        [TestCase("Valid name", ExpectedResult = true, Description = "Valid input")]
        public bool NameValidationItemTest(string nameValue)
        {
            var testData = GetValidItem();
            testData.Name = nameValue;
            return NameValidationItem.Validate(testData, new List<Movie>());
        }


        [TestCase(-1, ExpectedResult = false, Description ="OutsideRange: -ve")]
        [TestCase(1000, ExpectedResult = false, Description = "OutsideRange: +ve")]
        [TestCase(1, ExpectedResult = true, Description = "Within range")]
        public bool LanguageValidationItemTest(long languageID)
        {
            var testData = GetValidItem();
            testData.Language = languageID;
            return LanguageValidationItem.Validate(testData, new List<Movie>());
        }

        [TestCase(null, null, null, null, null, null, ExpectedResult =true, Description ="Start & end dates null")]
        [TestCase(2010, 10,10, null, null, null, ExpectedResult =true, Description = "End date null")]
        [TestCase(null, null, null, 2019, 12, 01, ExpectedResult = true, Description = "Start date null")]
        [TestCase(2019, 10, 10, 2018, 10,10, ExpectedResult = false, Description = "End date before start date")]
        [TestCase(2017, 10, 10, 2018, 10, 10, ExpectedResult = true, Description = "Start date before end date")]
        [TestCase(2018, 10, 10, 2018, 10, 10, ExpectedResult = true, Description = "Start date == end date")]
        public bool FilmingDateValidationItemTest(int? startDateYear, int? startDateMonth, int? startDateDay, int? endDateYear, int? endDateMonth, int? endDateDay)
        {
            var testData = GetValidItem();
            testData.FilmingStarted = startDateYear.HasValue ? new DateTime(startDateYear.Value, startDateMonth.Value, startDateDay.Value) : null;
            testData.FilmingEnded = endDateYear.HasValue ? new DateTime(endDateYear.Value, endDateMonth.Value, endDateDay.Value) : null;
            return FilmingDateValidationItem.Validate(testData, new List<Movie>());
        }

        [TestCase(testID, testName, testLanguage, ExpectedResult =true, Description="Duplicate, same ID")]
        [TestCase(testID + 1, testName, testLanguage, ExpectedResult = false, Description = "Duplicate, diff ID")]
        [TestCase(testID, "Unique name", testLanguage, ExpectedResult = true, Description = "Diff name, same ID")]
        [TestCase(testID + 1, "Unique name", testLanguage, ExpectedResult = true, Description = "Diff name, diff ID")]
        [TestCase(testID, testName, 2, ExpectedResult = true, Description = "Diff language, same ID")]
        [TestCase(testID + 1, testName, 2, ExpectedResult = true, Description = "Diff language, diff ID")]
        [TestCase(testID, "Unique name", 2, ExpectedResult = true, Description = "Diff name & language, same ID")]
        [TestCase(testID + 1, "Unique name", 2, ExpectedResult = true, Description = "Diff name & language, diff ID")]
        public bool UniqueRecordValidationItemTest(long ID, string name, long language)
        {
            var existingDTO = GetValidItem();
            List<Movie> existingData = new()
            {
                new Movie()
                {
                    ID = existingDTO.ID,
                    Name = existingDTO.Name,
                    Language = existingDTO.Language,
                    FilmingStarted = existingDTO.FilmingStarted,
                    FilmingEnded = existingDTO.FilmingEnded
                }
            };

            var toTest = GetValidItem();
            toTest.ID = ID; 
            toTest.Language = language;
            toTest.Name = name;
            return UniqueRecordValidationItem.Validate(toTest, existingData);
        }
        
    }
}
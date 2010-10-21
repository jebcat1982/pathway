﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using NUnit.Framework;
using SIL.PublishingSolution;
using SIL.Tool;


namespace Test.LiftPrepare
{
    [TestFixture]
    public class LiftTest
    {

        #region Private Variables
        private string _inputPath;
        private string _outputPath;
        private string _expectedPath;
        private PublicationInformation _projInfo;
        private ExportOpenOffice exportOpenOffice;
        #endregion Private Variables
        #region SetUp

        [TestFixtureSetUp]
        protected void SetUp()
        {
            string testPath = PathPart.Bin(Environment.CurrentDirectory, "/LiftPrepare/TestFiles");
            _inputPath = Common.PathCombine(testPath, "Input");
            _outputPath = Common.PathCombine(testPath, "Output");
            _expectedPath = Common.PathCombine(testPath, "Expected");
            Common.SupportFolder = "";
            Common.ProgInstall = PathPart.Bin(Environment.CurrentDirectory, "/../PsSupport");
            exportOpenOffice = new ExportOpenOffice();

        }
        #endregion

        [Test]
        public void EntryFilter1()
        {
            string inputLift = "EntryFilter.lift";
            string inputXhtml= "EntryFilter1.xhtml";

            string input =Path.Combine(_inputPath,inputLift);
            string expected = Path.Combine(_expectedPath, inputXhtml);
            string output = Path.Combine(_outputPath, inputXhtml);

            string ouputCopy = Path.Combine(_outputPath, Path.ChangeExtension(inputXhtml, ".lift"));
            File.Copy(input, ouputCopy, true);
            _projInfo = new PublicationInformation();

            _projInfo.IsEntryFilter = true;
            _projInfo.EntryFilterKey = "at start";
            _projInfo.EntryFilterString = "STU";
            _projInfo.DefaultXhtmlFileWithPath = ouputCopy;
            exportOpenOffice.SetPublication(_projInfo);
            
            exportOpenOffice.TransformLiftToXhtml(_projInfo);
            XmlAssert.AreEqual(expected,output,"Entry 1 Filter Failed");
        }


        [Test]
        public void EntryFilter2()
        {
            string inputLift = "EntryFilter.lift";
            string inputXhtml = "EntryFilter2.xhtml";

            string input = Path.Combine(_inputPath, inputLift);
            string expected = Path.Combine(_expectedPath, inputXhtml);
            string output = Path.Combine(_outputPath, inputXhtml);

            string ouputCopy = Path.Combine(_outputPath, Path.ChangeExtension(inputXhtml, ".lift"));
            File.Copy(input, ouputCopy, true);
            _projInfo = new PublicationInformation();

            _projInfo.IsEntryFilter = true;
            _projInfo.EntryFilterKey = "AnyWhere";
            _projInfo.EntryFilterString = "dent";
            _projInfo.DefaultXhtmlFileWithPath = ouputCopy;
            exportOpenOffice.SetPublication(_projInfo);

            exportOpenOffice.TransformLiftToXhtml(_projInfo);
            XmlAssert.AreEqual(expected, output, "Entry 2 Filter Failed");
        }

        [Test]
        public void EntryFilter3()
        {
            string inputLift = "EntryFilter.lift";
            string inputXhtml = "EntryFilter3.xhtml";

            string input = Path.Combine(_inputPath, inputLift);
            string expected = Path.Combine(_expectedPath, inputXhtml);
            string output = Path.Combine(_outputPath, inputXhtml);

            string ouputCopy = Path.Combine(_outputPath, Path.ChangeExtension(inputXhtml, ".lift"));
            File.Copy(input, ouputCopy, true);
            _projInfo = new PublicationInformation();

            _projInfo.IsEntryFilter = true;
            _projInfo.EntryFilterKey = "Whole Item";
            _projInfo.EntryFilterString = "fruit";
            _projInfo.DefaultXhtmlFileWithPath = ouputCopy;
            exportOpenOffice.SetPublication(_projInfo);

            exportOpenOffice.TransformLiftToXhtml(_projInfo);
            XmlAssert.AreEqual(expected, output, "Entry 3 Filter Failed");
        }

        [Test]
        public void EntryFilter4()
        {
            string inputLift = "EntryFilter.lift";
            string inputXhtml = "EntryFilter4.xhtml";

            string input = Path.Combine(_inputPath, inputLift);
            string expected = Path.Combine(_expectedPath, inputXhtml);
            string output = Path.Combine(_outputPath, inputXhtml);

            string ouputCopy = Path.Combine(_outputPath, Path.ChangeExtension(inputXhtml, ".lift"));
            File.Copy(input, ouputCopy, true);
            _projInfo = new PublicationInformation();

            _projInfo.IsEntryFilter = true;
            _projInfo.EntryFilterKey = "At End";
            _projInfo.EntryFilterString = "uit";
            _projInfo.DefaultXhtmlFileWithPath = ouputCopy;
            exportOpenOffice.SetPublication(_projInfo);

            exportOpenOffice.TransformLiftToXhtml(_projInfo);
            XmlAssert.AreEqual(expected, output, "Entry 4 Filter Failed");
        }

        [Test]
        public void EntryFilter5()
        {
            string inputLift = "EntryFilter.lift";
            string inputXhtml = "EntryFilter5.xhtml";

            string input = Path.Combine(_inputPath, inputLift);
            string expected = Path.Combine(_expectedPath, inputXhtml);
            string output = Path.Combine(_outputPath, inputXhtml);

            string ouputCopy = Path.Combine(_outputPath, Path.ChangeExtension(inputXhtml, ".lift"));
            File.Copy(input, ouputCopy, true);
            _projInfo = new PublicationInformation();

            _projInfo.IsEntryFilter = true;
            _projInfo.EntryFilterKey = "at start";
            _projInfo.EntryFilterString = "stu";
            _projInfo.IsEntryFilterMatchCase = false; // exact match
            _projInfo.DefaultXhtmlFileWithPath = ouputCopy;
            exportOpenOffice.SetPublication(_projInfo);

            exportOpenOffice.TransformLiftToXhtml(_projInfo);
            XmlAssert.AreEqual(expected, output, "Entry 5 Filter Failed");
        }
        [Test]
        public void EntryFilter6()
        {
            string inputLift = "EntryFilter.lift";
            string inputXhtml = "EntryFilter6.xhtml";

            string input = Path.Combine(_inputPath, inputLift);
            string expected = Path.Combine(_expectedPath, inputXhtml);
            string output = Path.Combine(_outputPath, inputXhtml);

            string ouputCopy = Path.Combine(_outputPath, Path.ChangeExtension(inputXhtml, ".lift"));
            File.Copy(input, ouputCopy, true);
            _projInfo = new PublicationInformation();

            _projInfo.IsEntryFilter = true;
            _projInfo.EntryFilterKey = "Not Equal";
            _projInfo.EntryFilterString = "fruit";
            _projInfo.DefaultXhtmlFileWithPath = ouputCopy;
            exportOpenOffice.SetPublication(_projInfo);

            exportOpenOffice.TransformLiftToXhtml(_projInfo);
            XmlAssert.AreEqual(expected, output, "Entry 6 Filter Failed");
        }

        [Test]
        public void SenseFilter1()
        {
            string inputLift = "EntryFilter.lift";
            string inputXhtml = "SenseFilter1.xhtml";

            string input = Path.Combine(_inputPath, inputLift);
            string expected = Path.Combine(_expectedPath, inputXhtml);
            string output = Path.Combine(_outputPath, inputXhtml);

            string ouputCopy = Path.Combine(_outputPath, Path.ChangeExtension(inputXhtml, ".lift"));
            File.Copy(input, ouputCopy, true);
            _projInfo = new PublicationInformation();

            _projInfo.IsSenseFilter = true;
            _projInfo.SenseFilterKey= "at start";
            _projInfo.SenseFilterString = "banana";
            _projInfo.IsSenseFilterMatchCase = false; // any case 
            _projInfo.DefaultXhtmlFileWithPath = ouputCopy;
            exportOpenOffice.SetPublication(_projInfo);

            exportOpenOffice.TransformLiftToXhtml(_projInfo);
            XmlAssert.AreEqual(expected, output, "Sense Filter1 Failed");
        }

        [Test]
        public void SenseFilter2()
        {
            string inputLift = "EntryFilter.lift";
            string inputXhtml = "SenseFilter2.xhtml";

            string input = Path.Combine(_inputPath, inputLift);
            string expected = Path.Combine(_expectedPath, inputXhtml);
            string output = Path.Combine(_outputPath, inputXhtml);

            string ouputCopy = Path.Combine(_outputPath, Path.ChangeExtension(inputXhtml, ".lift"));
            File.Copy(input, ouputCopy, true);
            _projInfo = new PublicationInformation();

            _projInfo.IsSenseFilter = true;
            _projInfo.SenseFilterKey = "at start";
            _projInfo.SenseFilterString = "banana";
            _projInfo.IsSenseFilterMatchCase = true; // exact match
            _projInfo.DefaultXhtmlFileWithPath = ouputCopy;
            exportOpenOffice.SetPublication(_projInfo);

            exportOpenOffice.TransformLiftToXhtml(_projInfo);
            XmlAssert.AreEqual(expected, output, "Sense Filter2 Failed");
        }

        [Test]
        public void EntrySenseFilter1()
        {
            string inputLift = "EntryFilter.lift";
            string inputXhtml = "EntrySenseFilter1.xhtml";

            string input = Path.Combine(_inputPath, inputLift);
            string expected = Path.Combine(_expectedPath, inputXhtml);
            string output = Path.Combine(_outputPath, inputXhtml);

            string ouputCopy = Path.Combine(_outputPath, Path.ChangeExtension(inputXhtml, ".lift"));
            File.Copy(input, ouputCopy, true);
            _projInfo = new PublicationInformation();

            _projInfo.IsEntryFilter = true;
            _projInfo.EntryFilterKey = "at start";
            _projInfo.EntryFilterString = "stu";
            _projInfo.IsEntryFilterMatchCase = true; // exact match

            _projInfo.IsSenseFilter = true;
            _projInfo.SenseFilterKey = "at start";
            _projInfo.SenseFilterString = "GiR";
            _projInfo.IsSenseFilterMatchCase = true; // any case 
            _projInfo.DefaultXhtmlFileWithPath = ouputCopy;
            exportOpenOffice.SetPublication(_projInfo);

            exportOpenOffice.TransformLiftToXhtml(_projInfo);
            XmlAssert.AreEqual(expected, output, "Sense Filter1 Failed");
        }

        [Test]
        public void EntrySenseFilter2()
        {
            string inputLift = "EntryFilter.lift";
            string inputXhtml = "EntrySenseFilter2.xhtml";

            string input = Path.Combine(_inputPath, inputLift);
            string expected = Path.Combine(_expectedPath, inputXhtml);
            string output = Path.Combine(_outputPath, inputXhtml);

            string outputCopy = Path.Combine(_outputPath, Path.ChangeExtension(inputXhtml, ".lift"));
            File.Copy(input, outputCopy, true);
            _projInfo = new PublicationInformation();

            _projInfo.IsEntryFilter = true;
            _projInfo.EntryFilterKey = "at start";
            _projInfo.EntryFilterString = "sTu";
            _projInfo.IsEntryFilterMatchCase = false; // exact match

            _projInfo.IsSenseFilter = true;
            _projInfo.SenseFilterKey = "at start";
            _projInfo.SenseFilterString = "GiR";
            _projInfo.IsSenseFilterMatchCase = false; // any case 
            _projInfo.DefaultXhtmlFileWithPath = outputCopy;
            exportOpenOffice.SetPublication(_projInfo);

            exportOpenOffice.TransformLiftToXhtml(_projInfo);
            XmlAssert.AreEqual(expected, output, "Sense Filter1 Failed");
        }

        [Test]
        public void LanguageFilter1()
        {
            string inputLift = "EntryFilter.lift";
            string inputXhtml = "LanguageFilter1.xhtml";

            string input = Path.Combine(_inputPath, inputLift);
            string expected = Path.Combine(_expectedPath, inputXhtml);
            string output = Path.Combine(_outputPath, inputXhtml);

            string outputCopy = Path.Combine(_outputPath, Path.ChangeExtension(inputXhtml, ".lift"));
            File.Copy(input, outputCopy, true);
            _projInfo = new PublicationInformation();

            _projInfo.IsLanguageFilter = true;
            _projInfo.LanguageFilterKey = "Whole Item";
            _projInfo.LanguageFilterString= "tPi";
            _projInfo.IsLanguageFilterMatchCase = false; // exact match

            _projInfo.DefaultXhtmlFileWithPath = outputCopy;
            exportOpenOffice.SetPublication(_projInfo);

            exportOpenOffice.TransformLiftToXhtml(_projInfo);
            XmlAssert.AreEqual(expected, output, "Language Filter Failed");
        }
        [Test]
        public void LanguageSort()
        {
            string inputLift = "LanguageSort.lift";

            string input = Path.Combine(_inputPath, inputLift);
            string expected = Path.Combine(_expectedPath, inputLift);
            string output = Path.Combine(_outputPath, inputLift);

            File.Copy(input, output, true);

            exportOpenOffice.LiftSortWritingSys(input,output);
            TextFileAssert.AreEqual(expected, output, "Sense Filter1 Failed");
        }

        [Test]
        //[Ignore] //Palaso library fails when loading coallator LiftEntrySorter.cs line 32 (prepLiftForSort)
        public void AkooseSorting()
        {
            string inputLift = "EntrySort.lift";
            string input = Path.Combine(_inputPath, inputLift);
            string expected = Path.Combine(_expectedPath, inputLift);
            string output = Path.Combine(_outputPath, inputLift);

            File.Copy(input, output, true);

            LiftPreparer liftPreparer = new LiftPreparer();
            liftPreparer.loadLift(output);
            liftPreparer.applySort();
            TextFileAssert.AreEqual(expected, output);

        }

        [Test]
        public void LoadCoallator()
        {
            var collator = new Palaso.WritingSystems.Collation.IcuRulesCollator("[alternate shifted]");
            //If this test fails, platform may not be set to x86
        }
    }
}
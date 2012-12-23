﻿#region License
// Copyright (c) 2012 Raif Atef Wasef
// This source file is licensed under the  MIT license.
// 
// Permission is hereby granted, free of charge, to any person 
// obtaining a copy of this software and associated documentation 
// files (the "Software"), to deal in the Software without restriction, including 
// without limitation the rights to use, copy, modify, merge, publish, distribute, 
// sublicense, and/or sell copies of the Software, and to permit persons to whom 
// the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included 
// in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY 
// KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE 
// WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR 
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS 
// OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR 
// OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT 
// OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH 
// THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using FunkyGrep.Engine;
using FunkyGrep.Engine.Specifications;
using NUnit.Framework;

namespace FunkyGrep.Tests.Engine.Specifications
{
    [TestFixture]
    public class FileSpecificationTests
    {
        string _tempPath;
        string _tempSubfolder;

        [TestFixtureSetUp]
        public void Setup()
        {
            var random = new Random();

            string tempPath = Path.Combine(Path.GetTempPath(), "FNGREP_" + random.Next());
            if (Directory.Exists(tempPath)) Directory.Delete(tempPath, true);

            Directory.CreateDirectory(tempPath);
            this._tempPath = tempPath;

            string tempSubfolder = Path.Combine(
                tempPath, random.Next().ToString(Thread.CurrentThread.CurrentCulture));
            Directory.CreateDirectory(tempSubfolder);
            this._tempSubfolder = tempSubfolder;

            const string dummyFileContent = "Just a temp file for unit tests";
            var topLevelFileNames = new[] { "temp1.css", "temp2.txt" };
            var subLevelFileNames = new[] { "temp3.asp", "temp4.bmp" };

            foreach (string fileName in topLevelFileNames)
            {
                File.WriteAllText(Path.Combine(tempPath, fileName), dummyFileContent);
            }

            foreach (string fileName in subLevelFileNames)
            {
                File.WriteAllText(Path.Combine(tempSubfolder, fileName), dummyFileContent);
            }
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            Directory.Delete(this._tempPath, true);
        }

        [Test]
        public void Enumeration_IncludeSubdirectories()
        {
            var fileSpec = new FileSpecification(this._tempPath, true, null, null);
            List<IDataSource> files = fileSpec.EnumerateFiles().ToList();

            Assert.That(files.Count, Is.EqualTo(4));
            Assert.That(
                files.Any(x => x.Identifier == Path.Combine(this._tempPath, "temp1.css")), Is.True);
            Assert.That(
                files.Any(x => x.Identifier == Path.Combine(this._tempPath, "temp2.txt")), Is.True);
            Assert.That(
                files.Any(x => x.Identifier == Path.Combine(this._tempSubfolder, "temp3.asp")),
                Is.True);
            Assert.That(
                files.Any(x => x.Identifier == Path.Combine(this._tempSubfolder, "temp4.bmp")),
                Is.True);
        }

        [Test]
        public void Enumeration_Toplevel_Only()
        {
            var fileSpec = new FileSpecification(this._tempPath, false, null, null);
            List<IDataSource> files = fileSpec.EnumerateFiles().ToList();

            Assert.That(files.Count, Is.EqualTo(2));
            Assert.That(
                files.Any(x => x.Identifier == Path.Combine(this._tempPath, "temp1.css")), Is.True);
            Assert.That(
                files.Any(x => x.Identifier == Path.Combine(this._tempPath, "temp2.txt")), Is.True);
        }

        [Test]
        public void Enumeration_excluding_patterns_Returns_CorrectResult()
        {
            var fileSpec = new FileSpecification(
                this._tempPath, true, null, new[] { "*.asp", "*.bmp", "*.txt" });
            List<IDataSource> files = fileSpec.EnumerateFiles().ToList();

            Assert.That(files.Count(), Is.EqualTo(1));
            Assert.That(
                files.Any(x => x.Identifier == Path.Combine(this._tempPath, "temp1.css")), Is.True);
        }

        [Test]
        public void Enumeration_matching_filePatterns_Returns_CorrectResult()
        {
            var fileSpec = new FileSpecification(
                this._tempPath, true, new[] { "temp1.css", "temp2.txt" }, null);
            List<IDataSource> files = fileSpec.EnumerateFiles().ToList();

            Assert.That(files.Count(), Is.EqualTo(2));
            Assert.That(
                files.Any(x => x.Identifier == Path.Combine(this._tempPath, "temp1.css")), Is.True);
            Assert.That(
                files.Any(x => x.Identifier == Path.Combine(this._tempPath, "temp2.txt")), Is.True);
        }

        [Test]
        public void Enumeration_none_matching_filePattern_Returns_EmptyResult()
        {
            var fileSpec = new FileSpecification(this._tempPath, true, new[] { "*.xyz" }, null);
            List<IDataSource> files = fileSpec.EnumerateFiles().ToList();

            Assert.That(files.Count(), Is.EqualTo(0));
        }
    }
}
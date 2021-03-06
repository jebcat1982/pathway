﻿// --------------------------------------------------------------------------------------------
// <copyright file="RampFile.cs" from='2013' to='2014' company='SIL International'>
//      Copyright ( c ) 2014, SIL International. 
//    
//      Distributable under the terms of either the Common Public License or the
//      GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright> 
// <author>Greg Trihus</author>
// <email>greg_trihus@sil.org</email>
// Last reviewed: 
// 
// <remarks>
// Prepare RAMP file objects
// </remarks>
// --------------------------------------------------------------------------------------------

namespace SIL.Tool
{
    /// <summary>
    /// Combines all css into a single file by implementing @import
    /// </summary>
    public class RampFile
    {
        private string _fileName;
        private string _fileDescription;
        private string _fileRelationship;
        private string _fileIsPrimary;
        private string _fileSilPublic;

        public string FileName
        {
            get { return _fileName; }
            set { _fileName = value; }
        }

        public string FileDescription
        {
            get { return _fileDescription; }
            set { _fileDescription = value; }
        }

        public string FileRelationship
        {
            get { return _fileRelationship; }
            set { _fileRelationship = value; }
        }

        public string FileIsPrimary
        {
            get { return _fileIsPrimary; }
            set { _fileIsPrimary = value; }
        }

        public string FileSilPublic
        {
            get { return _fileSilPublic; }
            set { _fileSilPublic = value; }
        }
    }
}

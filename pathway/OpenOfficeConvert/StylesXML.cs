// --------------------------------------------------------------------------------------------
// <copyright file="StylesXML.cs" from='2009' to='2009' company='SIL International'>
//      Copyright © 2009, SIL International. All Rights Reserved.   
//    
//      Distributable under the terms of either the Common Public License or the
//      GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright> 
// <author>Greg Trihus</author>
// <email>greg_trihus@sil.org</email>
// Last reviewed: 
// 
// <remarks>
// Creates the ODT Styles 
// </remarks>
// --------------------------------------------------------------------------------------------

#region Using
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;
using SIL.Tool;

#endregion Using
namespace SIL.PublishingSolution
{
    #region Class StylesXML
    public class StylesXML
    {
        #region Private Variable
        Styles _styleName = new Styles();  // Contains all the Css Informations to be used in XHTML

        Dictionary<string, string> _allParagraphProperty;
        Dictionary<string, string> _allTextProperty;
        Dictionary<string, string> _allPageLayoutProperty;
        Dictionary<string, string> _allColumnProperty;
        Dictionary<string, string> _paragraphProperty;
        Dictionary<string, string> _textProperty;
        Dictionary<string, string> _pageLayoutProperty;
        Dictionary<string, string> _columnProperty;
        Dictionary<string, string> _sectionProperty;
        Dictionary<string, string> _firstPageLayoutProperty;
        Dictionary<string, string>[] _pageHeaderFooter;
        Dictionary<string, string> _columnSep;
        ClassInfo _selectorClass;
        ArrayList _multiClass = new ArrayList();
        ArrayList _tagName = new ArrayList();  // for merge style in CloseODTStyles() function
        ArrayList _baseTagName = new ArrayList();  // for insert tagName
        ArrayList _allTagName = new ArrayList();  // for all tagName

        Dictionary<string, Dictionary<string, string>> _tagProperty = new Dictionary<string, Dictionary<string, string>>();

        StyleAttribute _attributeInfo;
        string _className = string.Empty;
        bool _borderAdded = false;

        Dictionary<string, string> _attribute;
        Dictionary<string, string> _dispClassName = new Dictionary<string, string>();
        XmlTextWriter _writer;
        MapProperty _mapProperty = new MapProperty();
        bool _pageFirst = false;
        bool _isDictionary = false;
        bool _pseudoClassName = false;
        string _styleFilePath;
        string _attribClassName = string.Empty;
        ArrayList _firstPageContentNone = new ArrayList();
        bool isMirrored = false; //TD-410

        const string _parentSeperator = ".";

        //Marks_crop - Declaration
        private const double _lineLength = 0.8;
        private const double _gapToMargin = .2;
        float _PageWidth = 0.00F;
        float _leftMargin = 0.00F;
        float _rightMargin = 0.00F;
        float _rightPosition = 0.00F;
        float _PageHeight = 0.00F;
        float _topMargin = 0.00F;
        float _bottomMargin = 0.00F;
        float _bottomPosition = 0.00F;
        bool _isFirstpageDimensionChanged = false; // TD-190(marks:crop)
        private VerboseClass _verboseWriter = VerboseClass.GetInstance();
        #endregion

        #region public Variable
        //public static bool ShowError = false;
        //public static StreamWriter ErrorFile = null;
        //public static bool ErrorWritten = false;
        //private static string fileName;
        public bool IsPosition = false;
        public static bool IsCropMarkChecked; // TD-190(marks:crop)
        public static bool IsMarginChanged; // TD-190(marks:crop)
        public static string HeaderRule; // TD-1007(Add a ruling line to the bottom of the header.)
        #endregion

        #region Constructor
        public StylesXML()
        {
            _verboseWriter.ErrorWritten = false;
            _verboseWriter.ShowError = false;
            IsCropMarkChecked = false;
            HeaderRule = string.Empty;
        }
        #endregion

        #region Public Methods
        /// -------------------------------------------------------------------------------------------
        /// <summary>
        /// Generate Styles.xml body from .css
        /// 
        /// <list> 
        /// </list>
        /// </summary>
        /// <param name="sourceFile">Input .CSS file</param>
        /// <param name="targetFile">Styles.xml</param>
        /// <param name="outputFile">Output File</param>
        /// <param name="fromNunit">Is From NUnit</param>
        /// <returns>"style name" collection, which has relative values </returns>
        /// -------------------------------------------------------------------------------------------
        public Styles CreateStyles(string sourceFile, string targetFile, string outputFile, bool fromNunit)
        {
            InitializeObject(outputFile); // Creates new Objects
            LoadAllProperty();  // Loads all properties
            LoadSpellCheck();
            CreateODTStyles(targetFile); // Create Styles.xml for odt
            var cssTree = new CssParser();
            TreeNode node = cssTree.BuildTree(sourceFile);
            //To show errors to user to edit and save the CSS file.
            //if (cssTree.ErrorList.Count > 0)
            //{
            //    var errForm = new CSSError(cssTree.ErrorList, Path.GetDirectoryName(sourceFile));
            //    errForm.ShowDialog();
            //    cssTree = new CSSParser();
            //    node = cssTree.BuildTree(sourceFile);
            //}
            ProcessCSSTree(node, outputFile);
            AddTagStyle(); // Add missing tags in styles.xml (h1,h2,..)
            CloseODTStyles();  // Close Styles.xml for odt
            MergeTag(); // Merge tags in styles.xml (h1,h2,..)
            return _styleName;
        }

        /// <summary>
        /// Return a list of font names used in the css file.
        /// </summary>
        /// <param name="name">used to name temporary files.</param>
        /// <param name="cssFile">where css info is stored</param>
        /// <returns>an array list of font names</returns>
        //public ArrayList GetFontList(string name, string cssFile)
        //{
        //    ArrayList fontList = new ArrayList();
        //    string xhtmlFile = name + ".xhtml";
        //    string targetFile = Common.PathCombine(Common.GetTempFolderPath(), name + ".xml");
        //    Styles styleName = CreateStyles(cssFile, targetFile, xhtmlFile, false);

        //    for (int i = 0; i < styleName.UsedFontsList.Count; i++)
        //    {
        //        fontList.Add(styleName.UsedFontsList[i].ToString());
        //    }
        //    return fontList;
        //}

        //private void GetNewFont(string OutputFile)
        //{
        //    ArrayList ss = _mapProperty.NewFonts;
        //    var projInfo = new PublicationInformation();
        //    var folder = new DirectoryInfo(Path.GetDirectoryName(OutputFile));
        //    FileInfo[] deFiles = folder.GetFiles("*.de");
        //    if (deFiles.Length > 0)
        //    {
        //        string deFile = deFiles[0].FullName;
        //        projInfo.LoadProjectFile(deFile);
        //        projInfo.AddFolderToXML(Path.GetDirectoryName(deFile) + "/fonts", "SolutionExplorer");
        //        foreach (string usedFont in ss)
        //        {
        //            string fontPath = Path.GetDirectoryName(Environment.GetFolderPath(Environment.SpecialFolder.System))+ "/fonts/";
        //            projInfo.FullPath = Path.GetDirectoryName(deFile) + "/";
        //            projInfo.SubPath = "/";
        //            projInfo.AddFileToXML(fontPath + usedFont, "false", false, "fonts", false, true);
        //        }
                
        //    }
        //}

        private void LoadSpellCheck()
        {
            const string sKey = @"SOFTWARE\classes\.odt";
            RegistryKey key;
            try
            {
                key = Registry.LocalMachine.OpenSubKey(sKey);
            }
            catch (Exception)
            {
                key = null;
            }
            // Check to see if Open Office Installed
            if (key == null)
                return;
            object value = key.GetValue("");
            if (value == null)
                return;
            string documentType = value.ToString();

            string sKey2 = string.Format(@"SOFTWARE\Classes\{0}\shell\open\command", documentType);
            RegistryKey key2;
            try
            {
                key2 = Registry.LocalMachine.OpenSubKey(sKey2);
            }
            catch (Exception)
            {
                key2 = null;
            }
            if (key2 == null)
                return;

            string launchCommand = key2.GetValue("").ToString();
            Match m = Regex.Match(launchCommand, "\"(.*)program");

            string spellPath = Common.PathCombine("share", "autocorr");
            string openOfficePath = Common.PathCombine(m.Groups[1].Value, "basis");
            openOfficePath = Directory.Exists(openOfficePath)
                                 ? Common.PathCombine(openOfficePath, spellPath)
                                 : Common.PathCombine(m.Groups[1].Value, spellPath);
            if (!Directory.Exists(openOfficePath)) return;

            string[] spellFiles = Directory.GetFiles(openOfficePath, "acor_*.dat");
            foreach (string fileName in spellFiles)
            {
                string fName = Path.GetFileNameWithoutExtension(fileName);
                string[] lang_coun = fName.Substring(5).Split('-');
                if (lang_coun.Length == 2)
                {
                    string lang = lang_coun[0];
                    string coun = lang_coun[1];

                    if (_styleName.SpellCheck.ContainsKey(lang))
                    {
                        _styleName.SpellCheck[lang].Add(coun);
                    }
                    else
                    {
                        ArrayList arLang = new ArrayList();
                        //arLang = _styleName.AttribAncestor[coun];
                        arLang.Add(coun);
                        _styleName.SpellCheck[lang] = arLang;
                    }
                }
            }
        }

        public string GetPseudoDetails(TreeNode node)
        {
            string clsName = string.Empty;
            string tagName = string.Empty;
            string pseudoName = string.Empty;
            string pseudoContent;
            string attribName = string.Empty;
            string propertyName = string.Empty;
            string hasValue = string.Empty;
            string styleNameN = string.Empty;
            string footerClassName = string.Empty;

            foreach (TreeNode nodeItem in node.Nodes)
            {
                switch (nodeItem.Text)
                {
                    case "ANY":
                        //TODO
                        break;
                    case "TAG":
                        tagName = "." + nodeItem.FirstNode.Text;
                        break;
                    case "CLASS":
                        clsName = nodeItem.FirstNode.Text;
                        foreach (TreeNode attribNode in nodeItem.Nodes)
                        {
                            if (attribNode.Text == "ATTRIB" && attribNode.FirstNode.NextNode.Text == "HASVALUE")
                            {
                                hasValue = attribNode.Nodes[2].Text;
                                hasValue = hasValue.Replace("-", "");
                                hasValue = " " + RemoveHyphenAndUnderscore(hasValue);
                            }
                            else if (attribNode.Text == "ATTRIB" && attribNode.FirstNode.NextNode.Text == "ATTRIBEQUAL")
                            {
                                attribName = attribNode.Nodes[2].Text;
                                attribName = "_." + RemoveHyphenAndUnderscore(attribName);
                            }
                        }
                        clsName = clsName + hasValue + attribName;
                        if (_styleName.PseudoPosition.Contains(clsName))
                        {
                            _styleName.PseudoPosition.Remove(clsName);
                        }

                        _styleName.PseudoPosition.Add(clsName);
                        break;
                    case "PSEUDO":
                        pseudoName = nodeItem.FirstNode.Text;
                        if (string.Compare(pseudoName, "contains") == 0)
                        {
                            pseudoContent = RemoveHyphenAndUnderscore(nodeItem.Nodes[2].Text);
                            clsName = clsName + "-" + pseudoContent.Replace(" ", "");
                            _styleName.ClassContainsSelector.Add(clsName + tagName, pseudoContent);
                        }
                        break;
                    case "PARENTOF":
                        if (clsName.Trim().Length != 0)
                        {
                            styleNameN = styleNameN + clsName + ".";
                            clsName = string.Empty;
                        }
                        break;
                    case "PRECEDEDS":
                        if (clsName.Trim().Length != 0)
                        {
                            styleNameN = styleNameN + clsName + "_";
                            clsName = string.Empty;
                        }
                        break;
                    case "PROPERTY":
                        if (nodeItem.FirstNode.Text == "content")
                        {
                            bool added = false;
                            var content = new StringBuilder();
                            int childCount = nodeItem.Nodes.Count;
                            string stringResult = string.Empty;
                            for (int i = 0; i < childCount; i++)
                            {
                                TreeNode childNode = nodeItem.Nodes[i];
                                if (childNode.Text == "string")
                                {
                                    TreeNode childNodeNext;

                                    int next = i;
                                    while (++next < childCount)
                                    {
                                        childNodeNext = nodeItem.Nodes[next];
                                        if (childNodeNext.Text == "chapter" ||
                                            childNodeNext.Text == "chapterx")
                                        {
                                            stringResult += "#ChapterNumber";
                                        }
                                        else if (childNodeNext.Text == "verse" ||
                                            childNodeNext.Text == "versex")
                                        {
                                            stringResult += "#VerseNumber";
                                        }
                                        else if (childNodeNext.Text == ")")
                                        {
                                            i = next;
                                            content.Append(stringResult);
                                            stringResult = string.Empty;
                                        }
                                        else if (!(childNodeNext.Text == "(" ||
                                            childNodeNext.Text == " "))
                                        {
                                            break;
                                        }
                                    }

                                }
                                else if (string.Compare(childNode.Text, "content") != 0)
                                {
                                    if (!added)
                                    {
                                        content.Append('\u0b83');
                                    }
                                    content.Append(childNode.Text);
                                    if (string.Compare(childNode.Text, "counter") == 0)
                                    {
                                        added = true;
                                    }
                                    else if (childNode.Text == ")" && added)
                                    {
                                        added = false;
                                    }
                                }
                                else
                                {
                                    added = true;
                                }
                            }
                            propertyName = content.ToString();
                            //propertyName = propertyName.Replace("\"", "");
                            if (propertyName == "normal" || propertyName == "none")
                            {
                                propertyName = string.Empty;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            if (pseudoName == "after")
            {
                _styleName.PseudoClassAfter[clsName] = propertyName;
            }
            else if (pseudoName == "before")
            {

                _styleName.PseudoClassBefore[clsName] = propertyName;
                if (_styleName.PseudoPosition.Contains(clsName))
                {
                    _styleName.PseudoPosition.Remove(clsName);
                }
                _styleName.PseudoPosition.Add(clsName);
            }
            else if (pseudoName == "footnote-call")
            {
                footerClassName = "Footnote anchor";
                _styleName.FootNoteCall[clsName] = propertyName.Replace('\u0b83', ' ');
            }
            else if (pseudoName == "footnote-marker")
            {
                footerClassName = "Footnote Symbol";
                _styleName.FootNoteMarker[clsName] = propertyName.Replace('\u0b83', ' ').Trim();
            }


            if (pseudoName == "footnote-call" || pseudoName == "footnote-marker")
            {
                styleNameN = footerClassName;
            }
            else if (pseudoName == "" && propertyName != string.Empty)
            {
                string currNode = CheckParent(node);
                if (currNode == "yes")
                {
                    _styleName.ClassContent.Add(clsName, propertyName.Replace("'", ""));
                }
                styleNameN = styleNameN + clsName + tagName;
            }
            else if (!string.IsNullOrEmpty(pseudoName))
            {
                if (string.Compare(pseudoName, "contains") == -1)
                {
                    pseudoName = "-" + pseudoName;
                    styleNameN = styleNameN + clsName + tagName + pseudoName;
                }
                else
                {
                    styleNameN = clsName.Replace("-", "") + tagName;
                }
                //styleName = styleName + clsName + tagName + pseudoName;
            }
            return styleNameN;
        }

        /// <summary>
        /// To remove the hyphen "'" and "_" from the attrib value
        /// </summary>
        /// <param name="attribName">Attrib value</param>
        /// <returns>return attribname</returns>
        private static string RemoveHyphenAndUnderscore(string attribName)
        {
            if (attribName.IndexOf("'") >= 0)
            {
                attribName = attribName.Replace("'", "");
            }
            else if (attribName.IndexOf("\"") >= 0)
            {
                attribName = attribName.Replace("\"", "");
            }
            return attribName;
        }

        public string GetPseudoParent(TreeNode node)
        {
            string clsName = string.Empty;
            string parentName = string.Empty;
            string precedeName = string.Empty;
            string insertValue = string.Empty;
            string pseudoName = string.Empty;
            string attribName = string.Empty;
            string precedesAttribName = string.Empty;
            string parentAttribName = string.Empty;
            string firstClassName = string.Empty;

            try
            {
                foreach (TreeNode nodeItem in node.Nodes)
                {
                    if (nodeItem.Text == "CLASS")
                    {
                        clsName = nodeItem.Nodes[0].Text;
                        if (nodeItem.Nodes.Count >= 2)
                        {
                            if (nodeItem.FirstNode.NextNode.Text == "ATTRIB")
                            {
                                if (nodeItem.FirstNode.NextNode.FirstNode.NextNode.Text == "ATTRIBEQUAL")
                                {
                                    attribName = nodeItem.FirstNode.NextNode.Nodes[2].Text;
                                    if (attribName.IndexOf("\'") > 0)
                                        attribName = "_." + attribName.Replace("\'", "");
                                    else if (attribName.IndexOf("\"") > 0)
                                        attribName = attribName.Replace("\"", "");
                                    if (attribName != "")
                                    {
                                        attribName = "_." + attribName;
                                    }
                                }
                            }
                        }
                    }
                    if (nodeItem.Text == "PARENTOF")
                    {
                        if (firstClassName == string.Empty)
                            firstClassName = nodeItem.Text;

                        parentName = node.Nodes[nodeItem.Index - 1].FirstNode.Text;
                        if (node.Nodes[nodeItem.Index - 1].FirstNode.NextNode != null)
                        {
                            attribName = string.Empty;
                            parentAttribName = "_." + node.Nodes[nodeItem.Index - 1].FirstNode.NextNode.LastNode.Text.Replace("\"", "");
                        }

                    }
                    else if (nodeItem.Text == "PRECEDES")
                    {
                        if (firstClassName == string.Empty)
                            firstClassName = nodeItem.Text;

                        precedeName = node.Nodes[nodeItem.Index - 1].FirstNode.Text;
                        if (node.Nodes[nodeItem.Index - 1].FirstNode.NextNode != null)
                            precedesAttribName = "_." + node.Nodes[nodeItem.Index - 1].FirstNode.NextNode.LastNode.Text.Replace("\"", "");
                        attribName = string.Empty;
                    }
                    else if (nodeItem.Text == "PSEUDO")
                    {
                        pseudoName = nodeItem.FirstNode.Text;
                    }
                    else if (nodeItem.Text == "PROPERTY")
                    {
                        if (nodeItem.FirstNode.Text == "content")
                        {
                            insertValue = nodeItem.FirstNode.NextNode.Text;
                            if (insertValue == "normal" || insertValue == "none")
                            {
                                insertValue = string.Empty;
                            }
                        }
                    }
                }

                _styleName.PseudoClass.Add(precedeName + "+" + clsName);

                if (firstClassName.ToUpper() == "PARENTOF")
                {
                    clsName = clsName + attribName + "_" + precedeName + precedesAttribName + "." + parentName + parentAttribName;
                }
                else if (firstClassName.ToUpper() == "PRECEDES")
                {
                    clsName = clsName + attribName + "." + parentName + parentAttribName + "_" + precedeName + precedesAttribName;
                }

                if (pseudoName == "after")
                {
                    //_styleName.PseudoAttrib.Add(clsName, concatValue);
                }
                else if (pseudoName == "before")
                {
                    _styleName.PseudoAncestorBefore[clsName] = insertValue;
                }
                else if (pseudoName == "" && insertValue != string.Empty)
                {
                    _styleName.ClassContent.Add(clsName, insertValue.Replace("\"", ""));
                }
                if (pseudoName != "")
                {
                    clsName = clsName + "-" + pseudoName;
                }
                return clsName;
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                return clsName;
            }
        }

        /// -------------------------------------------------------------------------------------------
        /// <summary>
        /// To get the contents of the Header and the Footer
        /// 
        /// <list> 
        /// </list>
        /// </summary>
        /// <param name="node">TreeNode</param>
        /// <param name="pageName"> Name of the page whether it is first/ left / right </param>
        /// <returns></returns>
        /// -------------------------------------------------------------------------------------------
        /// 

        //public void PageHeaderFooter(TreeNode node, bool isAllPage)
        public void PageHeaderFooter(TreeNode node, string pageName)
        {
            TreeNode getNode = new TreeNode();
            StyleAttribute styleAttributeInfo = new StyleAttribute();
            Dictionary<string, string> attributeInfo = new Dictionary<string, string>();
            string[] searchKey = { "top-left", "top-center", "top-right", "bottom-left", "bottom-center", "bottom-right" };
            string prefix;
            int headerFooterIndex = 0;
            if (pageName == "PAGE")
            {
                headerFooterIndex = 6;
            }
            else if (pageName.IndexOf("left") > 0)
            {
                headerFooterIndex = 12;
            }
            else if (pageName.IndexOf("right") > 0)
            {
                headerFooterIndex = 18;
            }
            try
            {
                for (int i = 0; i < 6; i++)
                {
                    getNode = FindNodeProperty(node, searchKey[i]);
                    if (getNode != null)
                    {
                        foreach (TreeNode chileNode in getNode.Nodes)
                        {
                            if (chileNode.Text == "PROPERTY")
                            {
                                styleAttributeInfo = Properties(chileNode);
                                styleAttributeInfo.ClassName = node.Text.ToString();
                                if (styleAttributeInfo.Name.ToLower() == "content")
                                {
                                    string writingString = styleAttributeInfo.StringValue.Replace("\"", "");
                                    writingString = styleAttributeInfo.StringValue.Replace("'", "");
                                    if (writingString.ToLower() == "normal" || writingString.ToLower() == "none")
                                    {
                                        if (pageName != "PAGE")
                                        {
                                            _firstPageContentNone.Add(i); // avoiding first page content:normal or none.
                                        }
                                        continue;
                                    }
                                    _pageHeaderFooter[i + headerFooterIndex][styleAttributeInfo.Name] = writingString;
                                }
                                else if (styleAttributeInfo.Name == "color" || styleAttributeInfo.Name == "background-color")
                                {
                                    prefix = _allTextProperty[styleAttributeInfo.Name].ToString();
                                    styleAttributeInfo = _mapProperty.MapSingleValue(styleAttributeInfo);
                                    _pageHeaderFooter[i + headerFooterIndex][prefix + styleAttributeInfo.Name] = styleAttributeInfo.StringValue.Replace("'", "");

                                }
                                else if (styleAttributeInfo.Name.ToLower() == "direction")
                                {
                                    attributeInfo = _mapProperty.MapMultipleValue(styleAttributeInfo); // direction: ltr
                                    foreach (KeyValuePair<string, string> para in attributeInfo)
                                    {
                                        _pageHeaderFooter[i + headerFooterIndex]["style:" + para.Key] = para.Value;
                                    }
                                }
                                else if (styleAttributeInfo.Name == "margin-top"
                                || styleAttributeInfo.Name == "margin-bottom"
                                || styleAttributeInfo.Name == "margin-left"
                                || styleAttributeInfo.Name == "margin-right")
                                {
                                    prefix = _allParagraphProperty[styleAttributeInfo.Name].ToString();
                                    styleAttributeInfo = _mapProperty.MapSingleValue(styleAttributeInfo);
                                    _pageHeaderFooter[i + headerFooterIndex][prefix + styleAttributeInfo.Name] = styleAttributeInfo.StringValue.Replace("'", "");
                                }
                                else
                                {
                                    prefix = _allTextProperty[styleAttributeInfo.Name].ToString();
                                    styleAttributeInfo = _mapProperty.MapSingleValue(styleAttributeInfo); // convert attributes to Open Office format
                                    _pageHeaderFooter[i + headerFooterIndex][prefix + styleAttributeInfo.Name] = styleAttributeInfo.StringValue.Replace("'", "");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
        }

        /// -------------------------------------------------------------------------------------------
        /// <summary>
        /// Finds the node and returns Propert node
        /// 
        /// <list> 
        /// </list>
        /// </summary>
        /// <param name="node">TreeNode</param>
        /// <returns>TreeNode</returns>
        /// -------------------------------------------------------------------------------------------
        /// 
        public TreeNode FindNodeProperty(TreeNode node, string keyText)
        {
            TreeNode returnValue = new TreeNode();
            returnValue = null;
            try
            {
                if (node.Text == keyText)
                {
                    return node.Parent;
                }

                foreach (TreeNode chileNode in node.Nodes)
                {
                    returnValue = FindNodeProperty(chileNode, keyText);
                    if (returnValue != null)
                    {
                        return returnValue;
                    }
                }
                return returnValue;
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                return returnValue;
            }
        }

        /// -------------------------------------------------------------------------------------------
        /// <summary>
        /// Finds the node 
        /// 
        /// <list> 
        /// </list>
        /// </summary>
        /// <param name="node">TreeNode</param>
        /// <returns>TreeNode</returns>
        /// -------------------------------------------------------------------------------------------

        public TreeNode FindNode(TreeNode node, string keyText)
        {
            TreeNode returnValue = new TreeNode();
            returnValue = null;
            try
            {
                if (node.Text == keyText)
                {
                    return node;
                }

                foreach (TreeNode chileNode in node.Nodes)
                {
                    returnValue = FindNode(chileNode, keyText);
                    if (returnValue != null)
                    {
                        return returnValue;
                    }
                }
                return returnValue;
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                return returnValue;
            }
        }

        public void GetAttribLangBeforeList(TreeNode node)
        {
            string clsName = string.Empty;
            string attribName = string.Empty;
            string pseudoName = string.Empty;
            string hasValue = string.Empty;
            foreach (TreeNode nodeItem in node.Nodes)
            {
                if (nodeItem.Text == "CLASS")
                {
                    clsName = nodeItem.FirstNode.Text;
                }
                else if (nodeItem.Text == "ATTRIB")
                {
                    if (nodeItem.FirstNode.NextNode.Text == "ATTRIBEQUAL")
                    {
                        attribName = nodeItem.Nodes[2].Text;
                    }
                    if (nodeItem.FirstNode.NextNode.Text == "HASVALUE")
                    {
                        hasValue = nodeItem.Nodes[2].Text;
                        hasValue = hasValue.Replace("-", string.Empty);
                    }
                }
                else if (nodeItem.Text == "PSEUDO")
                {
                    pseudoName = nodeItem.FirstNode.Text;
                }
            }
            if (attribName != string.Empty)
            {
                clsName = clsName + "_." + attribName;
            }

            string contentValue = hasValue;

            //if (pseudoName == "after")
            //{
            //    _styleName.PseudoAttrib.Add(clsName, concatValue);
            //}
            //else 
            if (pseudoName == "before")
            {
                if (!_styleName.AttribLangBeforeList.ContainsKey(clsName))
                {
                    _styleName.AttribLangBeforeList[clsName] = hasValue;
                }
                else
                {
                    _styleName.AttribLangBeforeList.Clear();
                    _styleName.AttribLangBeforeList[clsName] = hasValue;
                }
            }
        }

        /// -------------------------------------------------------------------------------------------
        /// <summary>
        /// To add the "current" attributed node in ArrayList
        /// 
        /// <list> 
        /// </list>
        /// </summary>
        /// <param name="node">TreeNode</param>
        /// <returns>Nothing</returns>
        /// -------------------------------------------------------------------------------------------

        public void GetAttribHasValueDetails(TreeNode node)
        {
            try
            {

                string clsName = string.Empty;
                string attribHasValue = string.Empty;
                string returnValue = string.Empty;
                foreach (TreeNode nodeItem in node.Nodes)
                {
                    if (nodeItem.Text == "CLASS")
                    {
                        clsName = nodeItem.FirstNode.Text;
                        if (nodeItem.LastNode.Text == "ATTRIB")
                        {
                            if (nodeItem.LastNode.FirstNode.NextNode.Text == "HASVALUE")
                            {
                                attribHasValue = nodeItem.LastNode.Nodes[2].Text.Replace("'", "");
                                attribHasValue = attribHasValue.Replace("-", "");
                            }
                        }
                    }
                    //else if (nodeItem.Text == "ATTRIB")
                    //{
                    //    if (nodeItem.FirstNode.NextNode.Text == "HASVALUE")
                    //    {
                    //        attribHasValue = nodeItem.Nodes[2].Text.Replace("'", "");
                    //    }
                    //}
                }
                string concatValue = string.Empty;
                ArrayList concate = new ArrayList();
                concate.Add(attribHasValue);
                if (_styleName.AttribAncestor.ContainsKey(clsName))
                {
                    ArrayList value = new ArrayList();
                    value = _styleName.AttribAncestor[clsName];
                    value.Add(attribHasValue);
                    _styleName.AttribAncestor[clsName] = value;
                }
                else
                {
                    _styleName.AttribAncestor[clsName] = concate;
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
        }
        /// <summary>
        /// To check Pseudo Content
        /// </summary>
        /// <param name="node">Tree Node</param>
        /// <returns></returns>
        public bool CheckPseudoContent(TreeNode node)
        {
            string clsName = string.Empty;
            string insertValue = string.Empty;
            string pseudoName = string.Empty;

            try
            {
                foreach (TreeNode nodeItem in node.Nodes)
                {
 
                    if (nodeItem.Text == "PSEUDO")
                    {
                        pseudoName = nodeItem.FirstNode.Text;
                    }
                    else if (nodeItem.Text == "PROPERTY")
                    {
                        if (nodeItem.FirstNode.Text == "content")
                        {
                            insertValue = nodeItem.FirstNode.NextNode.Text;
                        }
                    }
                }


                if (pseudoName == "" && insertValue == "")
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                return false;
            }
        }
 /// <summary>
 /// To get Attribute Pseudo
 /// </summary>
 /// <param name="node">Tree Node</param>
 /// <returns></returns>
        public string GetAttribPseudo(TreeNode node)
        {
            string clsName = string.Empty;
            string attribName = string.Empty;
            string precedeName = string.Empty;
            string precedeAttribName = string.Empty;
            string insertValue = string.Empty;
            string pseudoName = string.Empty;
            bool parentOfValue = false;

            try
            {
                foreach (TreeNode nodeItem in node.Nodes)
                {
                    if (nodeItem.Text == "CLASS" || nodeItem.Text == "TAG")
                    {
                        clsName = nodeItem.Nodes[0].Text;
                        if (nodeItem.NextNode.Text == "PRECEDES" && nodeItem.LastNode.Text == "ATTRIB")
                        {

                                if (nodeItem.LastNode.FirstNode.NextNode.Text == "ATTRIBEQUAL")
                                {
                                    precedeAttribName = "_." + nodeItem.LastNode.Nodes[2].Text;
                                    if (precedeAttribName.IndexOf("\'") >= 0)
                                        precedeAttribName = precedeAttribName.Replace("'", "");
                                    if (precedeAttribName.IndexOf("\"") >= 0)
                                        precedeAttribName = precedeAttribName.Replace("\"", "");
                                }
                            
                        }
                        else if(nodeItem.LastNode.Text == "ATTRIB")
                        {
                            if (nodeItem.LastNode.FirstNode.NextNode.Text == "ATTRIBEQUAL")
                            {
                                attribName = "_." + nodeItem.LastNode.Nodes[2].Text;
                                attribName = attribName.Replace("'", "");
                                attribName = attribName.Replace("\"", "");
                            }
                        }
                    }
                    else if (nodeItem.Text == "PARENTOF")
                    {
                        parentOfValue = true;
                    }
                    if (nodeItem.Text == "PSEUDO")
                    {
                        pseudoName = nodeItem.FirstNode.Text;
                    }
                    else if (nodeItem.Text == "PRECEDES")
                    {
                        if (nodeItem.NextNode.Text == "CLASS")
                        {
                            precedeName = nodeItem.NextNode.Nodes[0].Text;
                        }
                        if (nodeItem.NextNode.LastNode.Text == "ATTRIB")
                        {
                            if (nodeItem.NextNode.LastNode.FirstNode.NextNode.Text == "ATTRIBEQUAL")
                            {
                                attribName = "_." + nodeItem.NextNode.LastNode.Nodes[2].Text;
                                attribName = attribName.Replace("'", "");
                                attribName = attribName.Replace("\"", "");
                            }
                        }
                    }
                    else if (nodeItem.Text == "PROPERTY")
                    {
                        if (nodeItem.FirstNode.Text == "content")
                        {
                            insertValue = nodeItem.FirstNode.NextNode.Text;
                            insertValue = insertValue.Replace("'", "");
                            if (insertValue == "normal" || insertValue == "none")
                            {
                                insertValue = string.Empty;
                            }
                        }
                    }
                }
                _styleName.PseudoClass.Add(clsName + "+" + precedeName);

                if (precedeName.Trim().Length > 0)
                    precedeName = "_" + precedeName;
                if (clsName.Trim().Length > 0)
                    clsName = clsName + attribName + precedeName + precedeAttribName;

                if (parentOfValue == false)
                {
                    if (pseudoName == "after")
                    {
                        //_styleName.PseudoAttrib.Add(clsName, concatValue);
                    }
                    else if (pseudoName == "before") 
                    {
                        _styleName.PseudoAttrib[clsName] = insertValue;
                        if (_styleName.PseudoPosition.Contains(clsName))
                        {
                            _styleName.PseudoPosition.Remove(clsName);
                        }
                        _styleName.PseudoPosition.Add(clsName);
                    }
                    //else if (pseudoName == "" && insertValue != string.Empty)
                    //{
                    //    _styleName.ClassContent.Add(clsName, insertValue.Replace("\"", ""));
                    //}
                    else if (pseudoName == "")
                    {
                        if (insertValue != string.Empty)
                        {
                            _styleName.ClassContent.Add(clsName, insertValue.Replace("\"", ""));
                        }
                        else
                        {
                            _styleName.PrecedeClass[clsName] =  "";
                        }
                        
                    }
                }
                if (pseudoName != "")
                {
                    clsName = clsName + "-" + pseudoName;
                }
                return clsName;
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                return clsName;
            }
        }

        #region MergeTag Methods
        /// -------------------------------------------------------------------------------------------
        /// <summary>
        /// Merge Standard tag property(h1) to css tag property(h1.entry)
        /// <list> 
        /// </list>
        /// </summary>
        /// <param name="tagName">Tag Name</param>
        /// <returns>Dictionary with TAG Properties.</returns>
        /// -------------------------------------------------------------------------------------------
        public void MergeTag()
        {
            Dictionary<string, string> tagProperty = new Dictionary<string, string>();
            Utility util = new Utility();
            XmlDocument doc = new XmlDocument();
            doc.Load(_styleFilePath);
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("st", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
            nsmgr.AddNamespace("fo", "urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0");
            XmlElement root = doc.DocumentElement;
            try
            {
                bool isPropertyExist;
                foreach (string tag in _tagName) // for each tag like h1, h2 etc.
                {
                    string style = "//st:style[@st:name='" + tag + "']"; // Find class
                    XmlNode node = root.SelectSingleNode(style, nsmgr);

                    XmlNode paragraphNode = null;
                    XmlNode textNode = null;
                    for (int i = 0; i < node.ChildNodes.Count; i++) // find paragraph & Text node
                    {
                        if (node.ChildNodes[i].Name == "style:paragraph-properties")
                        {
                            paragraphNode = node.ChildNodes[i];
                        }
                        else if (node.ChildNodes[i].Name == "style:text-properties")
                        {
                            textNode = node.ChildNodes[i];
                        }
                    }

                    string[] tagClass = tag.Split('.'); // tagClass[0] - className & tagClass[1] - TagName
                    string tagClassN = string.Empty;
                    if (tagClass.Length == 1)
                    {
                        tagClassN = tagClass[0];
                    }
                    else if (tagClass.Length == 2)
                    {
                        tagClassN = tagClass[1];
                    }
                    tagProperty = _tagProperty[tagClassN];

                    foreach (KeyValuePair<string, string> prop in tagProperty) // font-size: etc.
                    {
                        isPropertyExist = false;

                        for (int i = 0; i < node.ChildNodes.Count; i++) // open paragraph & Text node
                        {
                            XmlNode child = node.ChildNodes[i];

                            foreach (XmlAttribute attribute in child.Attributes) // open it'line attributes
                            {
                                if (prop.Key == attribute.Name)
                                {
                                    isPropertyExist = true;
                                    i = 10; // exit two loops
                                    break;
                                }
                            }
                        }
                        if (isPropertyExist == false)
                        {
                            string propertyName = prop.Key.Substring(prop.Key.IndexOf(':') + 1);
                            if (_allParagraphProperty.ContainsKey(propertyName)) // add property in Paragraph node
                            {
                                if (paragraphNode == null)
                                {
                                    paragraphNode = node.InsertBefore(doc.CreateElement("style:paragraph-properties", nsmgr.LookupNamespace("st").ToString()), node.FirstChild);
                                }
                                paragraphNode.Attributes.Append(doc.CreateAttribute(prop.Key, nsmgr.LookupNamespace("st").ToString())).InnerText = prop.Value;
                            }
                            else if (_allTextProperty.ContainsKey(propertyName)) // add fullString in Paragraph node
                            {
                                if (textNode == null)
                                {
                                    textNode = node.AppendChild(doc.CreateElement("style:text-properties", nsmgr.LookupNamespace("st").ToString()));
                                }
                                textNode.Attributes.Append(doc.CreateAttribute(prop.Key, nsmgr.LookupNamespace("fo").ToString())).InnerText = prop.Value;
                            }
                        }
                    }

                    if (tagClassN == "div")
                    {
                        XmlDocumentFragment styleNode = doc.CreateDocumentFragment();
                        styleNode.InnerXml = node.OuterXml;
                        node.ParentNode.InsertAfter(styleNode, node);

                        XmlElement nameElement = (XmlElement)node;
                        nameElement.SetAttribute("style:name", tagClass[0]);
                    }
                }
                doc.Save(_styleFilePath);
            }
            catch
            {
            }
        }

        #endregion
        #endregion

        #region Private Methods
        /// -------------------------------------------------------------------------------------------
        /// <summary>
        /// create attributes collection
        /// 
        /// <list> 
        /// </list>
        /// </summary>
        /// <returns></returns>
        /// -------------------------------------------------------------------------------------------
        private void LoadAllProperty()
        {
            // Inialize style properties
            _allParagraphProperty = new Dictionary<string, string>();
            _allColumnProperty = new Dictionary<string, string>();
            _allTextProperty = new Dictionary<string, string>();
            _allPageLayoutProperty = new Dictionary<string, string>();
            _pageLayoutProperty = new Dictionary<string, string>();
            _firstPageLayoutProperty = new Dictionary<string, string>();
            try
            {
                _allParagraphProperty.Add("text-align", "fo:");
                _allParagraphProperty.Add("text-indent", "fo:");
                _allParagraphProperty.Add("margin-left", "fo:");
                _allParagraphProperty.Add("margin-right", "fo:");
                _allParagraphProperty.Add("margin-top", "fo:");
                _allParagraphProperty.Add("margin-bottom", "fo:");
                _allParagraphProperty.Add("auto-text-indent", "style:");
                _allParagraphProperty.Add("padding", "fo:");
                //_allParagraphProperty.Add("display", "fo:"); // Commented for TD-172 (Same key name)
                _allParagraphProperty.Add("background-color", "fo:");
                _allParagraphProperty.Add("break-before", "fo:");
                _allParagraphProperty.Add("line-spacing", "style:");
                _allParagraphProperty.Add("line-height", "fo:");
                _allParagraphProperty.Add("line-height-at-least", "style:");
                _allParagraphProperty.Add("border-bottom", "fo:");
                _allParagraphProperty.Add("border-top", "fo:");
                _allParagraphProperty.Add("border-left", "fo:");
                _allParagraphProperty.Add("border-right", "fo:");
                _allParagraphProperty.Add("padding-left", "fo:");
                _allParagraphProperty.Add("padding-right", "fo:");
                _allParagraphProperty.Add("padding-bottom", "fo:");
                _allParagraphProperty.Add("padding-top", "fo:");
                _allParagraphProperty.Add("vertical-align", "style:");
                _allParagraphProperty.Add("writing-mode", "style:");
                _allParagraphProperty.Add("widows", "fo:");
                _allParagraphProperty.Add("orphans", "fo:");
                _allParagraphProperty.Add("break-after", "fo:");
                _allParagraphProperty.Add("hyphenation-ladder-count", "fo:"); //TD-345
                _allParagraphProperty.Add("keep-with-next", "fo:");
                _allParagraphProperty.Add("keep-together", "fo:");
                _allParagraphProperty.Add("float", "fo:"); //TD-416

                //_allTextProperty = new Dictionary<string, string>();
                _allTextProperty.Add("font-weight", "fo:");
                _allTextProperty.Add("font-size", "fo:");
                _allTextProperty.Add("font-family", "fo:");
                _allTextProperty.Add("font-style", "fo:");
                _allTextProperty.Add("font-variant", "fo:");
                _allTextProperty.Add("font", "fo:");
                _allTextProperty.Add("text-indent", "fo:");
                _allTextProperty.Add("text-transform", "fo:");
                _allTextProperty.Add("letter-spacing", "fo:");
                _allTextProperty.Add("word-spacing", "fo:");
                _allTextProperty.Add("color", "fo:");
                _allTextProperty.Add("text-line-through-style", "style:");
                _allTextProperty.Add("text-decoration", "style:");
                _allTextProperty.Add("text-underline-style", "style:");
                _allTextProperty.Add("background-color", "fo:");
                //_allTextProperty.Add("content", ""); // for @page header and footer
                _allTextProperty.Add("text-position", "style:");
                //TD-172
                _allTextProperty.Add("display", "text:");
                _allTextProperty.Add("country", "fo:");
                _allTextProperty.Add("language", "fo:");
                //TD-345
                _allTextProperty.Add("hyphenate", "fo:");
                _allTextProperty.Add("hyphenation-remain-char-count", "fo:");
                _allTextProperty.Add("hyphenation-push-char-count", "fo:");

                _allTextProperty.Add("pathway", "fo:");


                //_allColumnProperty = new Dictionary<string, string>();
                _allColumnProperty.Add("column-count", "fo:");
                _allColumnProperty.Add("column-gap", "fo:");
                _allColumnProperty.Add("column-fill", "fo:");
                _allColumnProperty.Add("column-rule", "style:");
                _allColumnProperty.Add("dont-balance-text-columns", "text:");



                //_allPageLayoutProperty = new Dictionary<string, string>();
                _allPageLayoutProperty.Add("page-width", "fo:");
                _allPageLayoutProperty.Add("page-height", "fo:");
                _allPageLayoutProperty.Add("num-format", "style:");
                _allPageLayoutProperty.Add("print-orientation", "style:");
                _allPageLayoutProperty.Add("margin-top", "fo:");
                _allPageLayoutProperty.Add("margin-right", "fo:");
                _allPageLayoutProperty.Add("margin-bottom", "fo:");
                _allPageLayoutProperty.Add("margin-left", "fo:");
                _allPageLayoutProperty.Add("writing-mode", "style:");
                _allPageLayoutProperty.Add("footnote-max-height", "style:");
                _allPageLayoutProperty.Add("background-color", "fo:");
                _allPageLayoutProperty.Add("border-bottom", "fo:");
                _allPageLayoutProperty.Add("border-top", "fo:");
                _allPageLayoutProperty.Add("border-left", "fo:");
                _allPageLayoutProperty.Add("border-right", "fo:");
                _allPageLayoutProperty.Add("padding-top", "fo:");
                _allPageLayoutProperty.Add("padding-bottom", "fo:");
                _allPageLayoutProperty.Add("padding-left", "fo:");
                _allPageLayoutProperty.Add("padding-right", "fo:");

                //_pageLayoutProperty = new Dictionary<string, string>();
                _pageLayoutProperty.Add("fo:page-width", "8.5in");
                _pageLayoutProperty.Add("fo:page-height", "11in");
                _pageLayoutProperty.Add("style:num-format", "1");
                _pageLayoutProperty.Add("style:print-orientation", "portrait");
                _pageLayoutProperty.Add("fo:margin-top", "0.7874in");
                _pageLayoutProperty.Add("fo:margin-right", "0.7874in");
                _pageLayoutProperty.Add("fo:margin-bottom", "0.7874in");
                _pageLayoutProperty.Add("fo:margin-left", "0.7874in");
                _pageLayoutProperty.Add("style:writing-mode", "lr-tb");
                _pageLayoutProperty.Add("style:footnote-max-height", "0in");

                //_firstPageLayoutProperty = new Dictionary<string, string>();

                // Add all tag property
                _baseTagName.Add("h1");
                _baseTagName.Add("h2");
                _baseTagName.Add("h3");
                _baseTagName.Add("h4");
                _baseTagName.Add("h5");
                _baseTagName.Add("h6");
                _baseTagName.Add("ol");
                _baseTagName.Add("ul");
                _baseTagName.Add("li");
                _baseTagName.Add("p");

                _baseTagName.Add("a");  // Anchor Tag

                _allTagName.AddRange(_baseTagName);

                Dictionary<string, string> tagProp = new Dictionary<string, string>();
                tagProp.Add("style:line-spacing", "12pt");
                tagProp.Add("fo:font-weight", "700");
                tagProp.Add("fo:font-size", "24pt");
                _tagProperty["h1"] = tagProp;

                tagProp = new Dictionary<string, string>();
                tagProp.Add("style:line-spacing", "8pt");
                tagProp.Add("fo:font-weight", "700");
                tagProp.Add("fo:font-size", "18pt");
                _tagProperty["h2"] = tagProp;

                tagProp = new Dictionary<string, string>();
                tagProp.Add("style:line-spacing", "7pt");
                tagProp.Add("fo:font-weight", "700");
                tagProp.Add("fo:font-size", "14pt");
                _tagProperty["h3"] = tagProp;

                tagProp = new Dictionary<string, string>();
                tagProp.Add("style:line-spacing", "6pt");
                tagProp.Add("fo:font-weight", "700");
                tagProp.Add("fo:font-size", "12pt");
                _tagProperty["h4"] = tagProp;

                tagProp = new Dictionary<string, string>();
                tagProp.Add("style:line-spacing", "5.5pt");
                tagProp.Add("fo:font-weight", "700");
                tagProp.Add("fo:font-size", "10pt");
                _tagProperty["h5"] = tagProp;

                tagProp = new Dictionary<string, string>();
                tagProp.Add("style:line-spacing", "5.5pt");
                tagProp.Add("fo:font-weight", "700");
                tagProp.Add("fo:font-size", "8pt");
                _tagProperty["h6"] = tagProp;

                tagProp = new Dictionary<string, string>();
                //tagProp.Add("fo:margin-left", "0pt");
                _tagProperty["ol"] = tagProp;
                _tagProperty["ul"] = tagProp;
                _tagProperty["li"] = tagProp;

                //tagProp = new Dictionary<string, string>();
                //tagProp.Add("fo:margin-top", "0.1598in");
                //tagProp.Add("fo:margin-bottom", "0.1598in");
                _tagProperty["p"] = tagProp;

                tagProp = new Dictionary<string, string>();
                _tagProperty["a"] = tagProp; // Anchor Tag

            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }

        }

        private void InitializeObject(string outputFile)
        {
            try
            {
                _verboseWriter.ErrorFileName = Path.GetFileNameWithoutExtension(outputFile) + "_err.html";
                if (File.Exists(_verboseWriter.ErrorFileName))
                {
                    File.Delete(_verboseWriter.ErrorFileName);
                }

                _verboseWriter.ShowError = false;
                DirectoryInfo folder = new DirectoryInfo(Path.GetDirectoryName(outputFile));
                FileInfo[] deFiles = folder.GetFiles("*.de");
                string deFile;
                if (deFiles.Length > 0)
                {
                    deFile = deFiles[0].FullName;
                    XmlNode deNode = Common.GetXmlNode(deFile, "//Project");
                    XmlAttribute errAttrib = deNode.Attributes["ShowError"];
                    if (errAttrib != null)
                    {
                        _verboseWriter.ShowError = bool.Parse(errAttrib.Value);
                    }
                }
            }
            catch(Exception ex)
            {
                _verboseWriter.ShowError = false;
            }

            // Creating new Objects
            _columnProperty = new Dictionary<string, string>();
            _sectionProperty = new Dictionary<string, string>();
            _columnSep = new Dictionary<string, string>();
            _pageHeaderFooter = new Dictionary<string, string>[24];
            _paragraphProperty = new Dictionary<string, string>();
            _textProperty = new Dictionary<string, string>();
            isMirrored = false;

            _styleName.SpellCheck = new Dictionary<string, ArrayList>();
            _styleName.AttribAncestor = new Dictionary<string, ArrayList>();
            _styleName.AttribLangBeforeList = new Dictionary<string, string>(); //PseudoLang
            _styleName.BackgroundColor = new ArrayList();
            _styleName.BorderProperty = new Dictionary<string, string>();
            _styleName.ClassContent = new Dictionary<string, string>();
            _styleName.ClearProperty = new Dictionary<string, string>();
            _styleName.ColumnGapEm = new Dictionary<string, Dictionary<string, string>>();
            _styleName.ContentCounter = new Dictionary<string, int>();
            _styleName.ContentCounterReset = new Dictionary<string, string>();
            _styleName.CounterParent = new Dictionary<string, Dictionary<string, string>>();
            _styleName.CssClassName = new Dictionary<string, IDictionary<string, string>>();
            _styleName.DisplayBlock = new ArrayList();
            _styleName.DisplayFootNote = new ArrayList();
            _styleName.DisplayInline = new ArrayList();
            _styleName.DisplayNone = new ArrayList();
            _styleName.FloatAlign = new Dictionary<string, string>();
            _styleName.FootNoteCall = new Dictionary<string, string>();
            _styleName.FootNoteMarker = new Dictionary<string, string>();
            _styleName.FootNoteSeperator = new Dictionary<string, string>();
            _styleName.ImageSize = new Dictionary<string, ArrayList>();
            _styleName.IsMacroEnable = false;
            _styleName.MasterDocument = new ArrayList();
            _styleName.PseudoAncestorBefore = new Dictionary<string, string>();
            _styleName.PseudoAttrib = new Dictionary<string, string>();
            _styleName.PseudoClass = new ArrayList();
            _styleName.PseudoClassAfter = new Dictionary<string, string>();
            _styleName.PseudoClassBefore = new Dictionary<string, string>();
            _styleName.PseudoPosition = new ArrayList();
            _styleName.SectionName = new ArrayList();
            _styleName.TagAttrib = new Dictionary<string, string>();
            _styleName.ListType = new Dictionary<string, string>();
            _styleName.WhiteSpace = new ArrayList();
            _styleName.ClassContainsSelector = new Dictionary<string, string>(); //TD-351[Implement :contains("Lamutua")]
            _styleName.ImageSource = new Dictionary<string, Dictionary<string, string>>();
            _styleName.AllCSSName = new ArrayList();
            _styleName.DropCap = new ArrayList();
            _styleName.ReplaceSymbolToText = new Dictionary<string, string>();
            _styleName.ReferenceFormat = "";
			_styleName.PrecedeClass = new Dictionary<string, string>();
            _styleName.IsAutoWidthforCaption = false;
            _styleName.VisibilityClassName = new Dictionary<string, string>();

            _styleName.PseudoWithoutStyles = new List<string>();

            for (int i = 0; i <= 23; i++)
            {
                _pageHeaderFooter[i] = new Dictionary<string, string>();
            }
        }


        /// -------------------------------------------------------------------------------------------
        /// <summary>
        /// Generate Styles.xml body from Antlr tree
        /// 
        /// <list> 
        /// </list>
        /// </summary>
        /// <param name="node">Antlr XMLNode</param>
        /// <param name="outputFile">Target File</param>
        /// <returns></returns>
        /// -------------------------------------------------------------------------------------------
        private void ProcessCSSTree(TreeNode node, string outputFile)
        {
            try
            {
                foreach (TreeNode child in node.Nodes)
                {
                    if (child.Text == "RULE") // Handle Class and Property
                    {
                        ClassAndProperty(child);
                    }
                    else if (child.Text == "PAGE") // Handle @page class
                    {
                        if (Common.OdType != Common.OdtType.OdtChild || Common.Testing) // Allow only when odm
                        {
                            PageProperty(child, outputFile);
                            ChangePageDimension(_pageFirst);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
        }

        /// <summary>
        /// To replace the symbol to text
        /// </summary>
        /// <param name="value">input symbol</param>
        /// <returns>Replaced text</returns>
        private static string ReplaceSymbolToText(string value)
        {
            if (value.IndexOf("&") >= 0)
            {
                value = value.Replace("&", "&amp;");
            }
            else if (value.IndexOf("<") >= 0)
            {
                value = value.Replace("<", "&lt;");
            }
            else if (value.IndexOf(">") >= 0)
            {
                value = value.Replace(">", "&gt;");
            }
            else if (value.IndexOf("\"") >= 0)
            {
                value = value.Replace("\"", "&quot;");
            }
            else if (value.IndexOf("'") >= 0)
            {
                value = value.Replace("'", "&apos;");
            }
            return value;
        }

        /// -------------------------------------------------------------------------------------------
        /// <summary>
        /// Generate class nodes
        /// 
        /// <list> 
        /// </list>
        /// </summary>
        /// <param name="t">Antlr Tree</param>
        /// <param name="ctp">Antlr tree collection</param>
        /// <param name="Parent">Antlr parent tree</param>
        /// <param name="_className">style name</param>
        /// <returns></returns>
        /// -------------------------------------------------------------------------------------------
        private void ClassAndProperty(TreeNode tree)
        {
            _pseudoClassName = false;
            _paragraphProperty.Clear();
            _textProperty.Clear();
            _columnProperty.Clear();
            _attributeInfo = new StyleAttribute();
            Dictionary<string, string> attribInfo = new Dictionary<string, string>();
            Dictionary<string, string> attribute = new Dictionary<string, string>();
            bool ancestorClassName = false;
            string pseudoClassName = string.Empty;
            string className = string.Empty;
            string baseClassName = string.Empty;
            string lang = string.Empty;
            try
            {
                foreach (TreeNode node in tree.Nodes)
                {
                    switch (node.Text)
                    {
                        case "CLASS":
                            lang = ClassNode(ref attribInfo, ref attribute, ref ancestorClassName, ref pseudoClassName, ref className, node);
                            baseClassName = className;
                            break;
                        case "TAG":
                            lang = ClassNode(ref attribInfo, ref attribute, ref ancestorClassName, ref pseudoClassName, ref className, node);
                            //isTagClass = true; 
                            break;
                        case "PROPERTY":
                            _attributeInfo = Properties(node);
                            if (_attributeInfo.Name.ToLower() == "prince-text-replace")
                            {
                                string[] values = _attributeInfo.StringValue.Split(',');
                                for (int i = 0; i < values.Length; i++)
                                {
                                    string key = values[i].Replace("\"", "");
                                    key = ReplaceSymbolToText(key);
                                    string value = values[i+1].Replace("\"", "");
                                    value = ReplaceSymbolToText(value);
                                    _styleName.ReplaceSymbolToText[key] = UnicodeConversion(value);
                                    i++;
                                }
                            }
                            else if (_attributeInfo.Name.ToLower() != "content")
                            {
                                _attributeInfo = MapProperty(className, node, _attributeInfo);
                                if (_attributeInfo.Name != "font")
                                {
                                    AddProperties(_attributeInfo, className);
                                }
                            }
                            else if (!_styleName.ClassContent.ContainsKey(className.Replace("\"", "")) && _pseudoClassName == false)
                            {
                                _attributeInfo.StringValue = _attributeInfo.StringValue.Replace("'", "");
                                _styleName.ClassContent.Add(className, _attributeInfo.StringValue.Replace("\"", ""));
                            }
                            break;
                        case "PARENTOF":
                            if (_pseudoClassName == false)
                            {
                                pseudoClassName = GetPseudoParent(tree);
                                _attributeInfo.ClassName = pseudoClassName;
                                _pseudoClassName = true;
                            }
                            break;
                        case "PRECEDES":

                            if (CheckPseudoContent(tree))
                            {
                                className = GetAttribPseudo(tree);
                            }
                            break;
                        case "PSEUDO":
                            if (_pseudoClassName == false)
                            {
                                pseudoClassName = GetPseudoDetails(tree);
                                _attributeInfo.ClassName = pseudoClassName;
                                _pseudoClassName = true;
                            }
                            break;
                        default:
                            break;
                    }
                }

                _writer.WriteStartElement("style:style");
                string familyType = "paragraph";

                if (pseudoClassName == string.Empty)
                {
                    _writer.WriteAttributeString("style:name", className);
                }
                else
                {
                    className = pseudoClassName;
                    _writer.WriteAttributeString("style:name", className);
                    familyType = "text";
                }
                _styleName.AllCSSName.Add(className);


                if (_baseTagName.Contains(className)) // className = h1,h2
                {
                    _tagName.Add(className);
                    _baseTagName.Remove(className);
                }
                else // className = h1.a ,h2.b
                {
                    string[] tagClass = className.Split('.'); // for mergeTag function.
                    if (tagClass.Length > 1)
                    {
                        if (_allTagName.Contains(tagClass[1]))
                        {
                            _tagName.Add(className);
                        }
                    }
                }

                //if (_baseTagName.Contains(className))
                //{
                //    _tagName.Add(className);
                //    _baseTagName.Remove(className);
                //}

                if (_tagProperty.ContainsKey(className)) // Is the Tag
                {
                    foreach (KeyValuePair<string, string> prop in _tagProperty[className]) // Merge Text and Paragraph Property.
                    {
                        string[] property = prop.Key.Split(':');
                        string propName = property[1];
                        if (_allParagraphProperty.ContainsKey(propName))
                        {
                            if (!_paragraphProperty.ContainsKey(prop.Key))
                                _paragraphProperty[prop.Key] = prop.Value;
                        }
                        else if (_allTextProperty.ContainsKey(propName))
                        {
                            if (!_textProperty.ContainsKey(prop.Key))
                                _textProperty[prop.Key] = prop.Value;
                        }
                    }

                    foreach (KeyValuePair<string, string> para in _paragraphProperty)
                    {
                        _tagProperty[className][para.Key] = para.Value;
                    }
                    foreach (KeyValuePair<string, string> text in _textProperty)
                    {
                        _tagProperty[className][text.Key] = text.Value;
                    }
                }

                _writer.WriteAttributeString("style:family", familyType); // "paragraph" will override by ContentXML.cs
                _writer.WriteAttributeString("style:parent-style-name", "none");
                if (familyType == "paragraph")
                {
                    if (_styleName.BorderProperty.Count > 0) // To fill the border properties for TD-307
                    {
                        GetBorderValues(_styleName.BorderProperty);
                        //string top = string.Empty;
                        //string right = string.Empty;
                        //string bottom = string.Empty;
                        //string left = string.Empty;
                        //foreach (string item in _styleName.BorderProperty.Keys)
                        //{
                        //    string[] splitComma = _styleName.BorderProperty[item].Split(',');
                        //    if (item == "border-style")
                        //    {
                        //        top = top + splitComma[0].ToString() + " ";
                        //        right = right + splitComma[0].ToString() + " ";
                        //        bottom = bottom + splitComma[0].ToString() + " ";
                        //        left = left + splitComma[0].ToString() + " ";
                        //    }
                        //    else if (item == "border-width")
                        //    {
                        //        FillBorderValues(ref top, ref right, ref bottom, ref left, splitComma);
                        //    }
                        //    else if (item == "border-color")
                        //    {
                        //        FillBorderValues(ref top, ref right, ref bottom, ref left, splitComma);
                        //    }
                        //}
                        //_paragraphProperty[_allParagraphProperty["border-left"].ToString() + "border-left"] = left;
                        //_paragraphProperty[_allParagraphProperty["border-right"].ToString() + "border-right"] = right;
                        //_paragraphProperty[_allParagraphProperty["border-top"].ToString() + "border-top"] = top;
                        //_paragraphProperty[_allParagraphProperty["border-bottom"].ToString() + "border-bottom"] = bottom;
                        //_styleName.BorderProperty.Clear();
                    }
                    if (_paragraphProperty.Count > 0)
                    {
                        _writer.WriteStartElement("style:paragraph-properties");
                        DropCap(className);
                        //_writer.WriteStartElement("style:paragraph-properties");
                        foreach (KeyValuePair<string, string> para in _paragraphProperty)
                        {
                            _writer.WriteAttributeString(para.Key, para.Value);
                        }
                        _writer.WriteEndElement();
                    }
                    else
                    {
                        if (pseudoClassName.IndexOf("-after") > 0 || pseudoClassName.IndexOf("-before") > 0)
                        {
                            if (!_styleName.PseudoWithoutStyles.Contains(pseudoClassName))
                                _styleName.PseudoWithoutStyles.Add(pseudoClassName);
 
                        }
                        
                    }
                }

                if (_columnProperty.Count > 0) // create a value XML file for content.xml with column property.
                {
                    CreateColumnXMLFile(className);
                }

                if (lang.Length > 0)
                {
                        //Library lib = new Library();
                        string language, country;
                        Common.GetCountryCode(out language, out country, lang, _styleName.SpellCheck);
                        //if (language != null)
                        //{
                        _textProperty["fo:language"] = language;
                        _textProperty["fo:country"] = country;
                        //}
                }

                if (_textProperty.Count > 0)
                {
                    _writer.WriteStartElement("style:text-properties");
                    
                    SuperscriptSubscriptIncreaseFontSize(baseClassName);
                    foreach (KeyValuePair<string, string> text in _textProperty)
                    {
                        //For TD-1501
                        if (text.Key == "fo:pathway" && text.Value == "emptyPsuedo")
                        {
                            if (!_styleName.PseudoWithoutStyles.Contains(pseudoClassName))
                                _styleName.PseudoWithoutStyles.Add(pseudoClassName);
                            continue;
                        }
                        _writer.WriteAttributeString(text.Key, text.Value);
                        if (text.Key == "fo:font-weight" || text.Key == "fo:font-size" || text.Key == "fo:font-family")
                        {
                            string propertyName = text.Key;
                            propertyName = propertyName == "fo:font-family" ? "style:font-name" : propertyName.Replace("fo:", "style:");
                            _writer.WriteAttributeString(propertyName + "-complex", text.Value);
                        }
                    }
                    if (_paragraphProperty.ContainsKey("fo:background-color"))
                    {
                        _writer.WriteAttributeString("fo:background-color", _paragraphProperty["fo:background-color"]);
                    }
                    _writer.WriteEndElement();
                }
                else
                {
                    if (pseudoClassName.IndexOf("-after") > 0 || pseudoClassName.IndexOf("-before") > 0)
                    {
                        if (!_styleName.PseudoWithoutStyles.Contains(pseudoClassName))
                            _styleName.PseudoWithoutStyles.Add(pseudoClassName);

                    }
                }
                _writer.WriteEndElement();
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
        }

        private void GetBorderValues(Dictionary<string, string> dict)
        {
            string right;
            string bottom;
            string left;
            string bStyle = dict.ContainsKey("border-style") ? dict["border-style"] : "solid";
            string top = right = bottom = left = bStyle;

            if (dict.ContainsKey("border-width"))
            {
                string[] splitComma = _styleName.BorderProperty["border-width"].Split(',');
                FillBorderValues(ref top, ref right, ref bottom, ref left, splitComma);
            }
            else
            {
                var sides = new[] {"top", "right", "left", "bottom"};
                foreach (string side in sides)
                {
                    string propertyName = "border-" + side + "-width";
                    string bWidth = dict.ContainsKey(propertyName) ? dict[propertyName].Replace(",", "") : ".5pt";
                    if(side == "top") {top = top + " " + bWidth;}
                    if (side == "left") {left = left + " " + bWidth;}
                    if (side == "bottom"){bottom = bottom + " " + bWidth;}
                    if (side == "right"){right = right + " " + bWidth;}
                }
            }

            string defaultColor = "#000000";
            if (_paragraphProperty.ContainsKey("fo:padding-top")
                || _paragraphProperty.ContainsKey("fo:padding-left")
                || _paragraphProperty.ContainsKey("fo:padding-bottom")
                || _paragraphProperty.ContainsKey("fo:padding-right"))
            {
                defaultColor = "#ffffff";
            }

            string bColor = dict.ContainsKey("border-color") ? dict["border-color"] : defaultColor;
            
            top = top + " " + bColor;
            right = right + " " + bColor;
            bottom = bottom + " " + bColor;
            left = left + " " + bColor;

            _paragraphProperty[_allParagraphProperty["border-left"] + "border-left"] = left;
            _paragraphProperty[_allParagraphProperty["border-right"] + "border-right"] = right;
            _paragraphProperty[_allParagraphProperty["border-top"] + "border-top"] = top;
            _paragraphProperty[_allParagraphProperty["border-bottom"] + "border-bottom"] = bottom;
            _styleName.BorderProperty.Clear();
        }



        /// <summary>
        /// Increase font-size 200% for Subscript and Superscript
        /// </summary>
        /// <param name="baseClassName">CSS classname</param>
        private void SuperscriptSubscriptIncreaseFontSize(string baseClassName)
        {
            if (_textProperty.ContainsKey("style:text-position")) // increase font-size for superscipt & subscript
            {
                string newValue = "100%";
                bool isRelativeValue = true;
                if (_textProperty.ContainsKey("fo:font-size"))
                {
                    string fontValue = _textProperty["fo:font-size"];
                    int counter;
                    string retValue = Common.GetNumericChar(fontValue, out counter);
                    if (retValue.Length > 0)
                    {
                        float value = float.Parse(retValue) * 1.4F;
                        string unit = fontValue.Substring(counter);
                        newValue = value + unit;
                        if (!(unit == "%" || unit == "em"))
                        {
                            isRelativeValue = false;
                        }
                    }
                    else
                    {
                        if (fontValue == "larger")
                        {
                            newValue = "largerx2";
                        }
                        else if (fontValue == "smaller")
                        {
                            newValue = "smallerx2";
                        }
                    }
                }
                if (isRelativeValue)
                {
                    if (!_styleName.CssClassName.ContainsKey(baseClassName))
                    {
                        _attribute = new Dictionary<string, string>();
                        _styleName.CssClassName[baseClassName] = _attribute;
                    }
                    _styleName.CssClassName[baseClassName]["fo:font-size"] = newValue;
                }
                _textProperty["fo:font-size"] = newValue;
            }
        }

        private void DropCap(string className)
        {
            if (_paragraphProperty.ContainsKey("fo:float") && _paragraphProperty.ContainsKey("style:vertical-align"))
            {
                _styleName.DropCap.Add(className);
                _paragraphProperty.Clear();  // Remove all paragraph property
                _writer.WriteStartElement("style:drop-cap");
                if (_textProperty.ContainsKey("fo:font-size"))
                {
                    string lines = "2";
                    if (_textProperty["fo:font-size"].IndexOf('%') > 0)
                    {
                        lines = (int.Parse(_textProperty["fo:font-size"].Replace("%", "")) / 100).ToString();
                    }
                    _writer.WriteAttributeString("style:lines", lines);
                    _textProperty.Remove("fo:font-size");
                    // _textProperty.Remove("fo:font-weight");
                }
                _writer.WriteAttributeString("style:distance", "0.20cm");
                _writer.WriteAttributeString("style:length", "1"); // No of Character
                _writer.WriteEndElement();
            }
        }

        private string ClassNode(ref Dictionary<string, string> attribInfo, ref Dictionary<string, string> attribute, ref bool ancestorClassName, ref string pseudoClassName, ref string className, TreeNode node)
        {
            string lang = string.Empty;
            _borderAdded = false;

            if (_pseudoClassName == false)
            {
                className = GetClassName(node);
                _attributeInfo.ClassName = className;
            }

            if (node.LastNode.Text == "ATTRIB")
            {
                attribute.Clear();
                attribute = GetAttribValue(node);
                if (attribute.ContainsKey("lang"))
                {
                    lang = attribute["lang"];
                    className = className + "_." + attribute["lang"];
                    _attributeInfo.ClassName = _attributeInfo.ClassName + "_." + attribute["lang"];

                    if (_isDictionary)
                    {
                        _attributeInfo.Name = "language";
                        _attributeInfo.StringValue = attribute["lang"];


                        attribInfo = _mapProperty.MapMultipleValue(_attributeInfo);
                        foreach (KeyValuePair<string, string> para in attribInfo)
                        {
                            _textProperty[_allTextProperty[para.Key].ToString() + para.Key] = para.Value;
                        }
                    }
                }
                else if (node.Text == "TAG")
                {
                    string classAttrib = string.Empty;
                    string key = "";
                    string value = string.Empty;
                    foreach (KeyValuePair<string, string> attrib in attribute)
                    {
                        classAttrib += attrib.Key + attrib.Value;
                        key = attrib.Key;
                        value = attrib.Value;

                    }
                    if (node.FirstNode.Text == "img")
                    {
                        Dictionary<string, string> prop = new Dictionary<string, string>();
                        prop[key] = value;
                        _styleName.ImageSource[value.ToLower()] = prop;
                        className = value.ToLower();
                        _pseudoClassName = true;
                    }
                    else
                    {

                        _styleName.TagAttrib[className] = key;
                        className = classAttrib + "_." + className;
                        _pseudoClassName = true;
                    }
                }
                if (_pseudoClassName == false)
                {
                    foreach (TreeNode childNodeItem in node.LastNode.Nodes)
                    {
                        if (childNodeItem.Text == "HASVALUE")
                        {
                            GetAttribHasValueDetails(node.Parent);
                            GetAttribLangBeforeList(node.Parent);
                            pseudoClassName = GetPseudoDetails(node.Parent);
                            _attributeInfo.ClassName = pseudoClassName;
                            className = GetClassName(node.Parent.FirstNode) + " " + childNodeItem.NextNode.Text.Replace("'", "");
                            _attributeInfo.ClassName = className;
                            _pseudoClassName = true;
                        }
                    }
                }
            }

            if (_pseudoClassName == false)
            {
                if (node.Parent != null)
                {
                    if (node.Parent.Nodes.Count > 2)
                    {
                        if (node.Text != "TAG" && node.NextNode.Text == "CLASS" && node.NextNode.NextNode.Text == "PROPERTY")
                        {
                            if (_pseudoClassName == false)
                            {
                                string ancestorClass = className;
                                string clsName = GetClassName(node.NextNode);
                                className = GetClassName(node.NextNode) + "." + className;
                                _attributeInfo.ClassName = className;
                                string concatValue = string.Empty;
                                ArrayList concat = new ArrayList();
                                concat.Add(ancestorClass);
                                if (_styleName.AttribAncestor.ContainsKey(clsName))
                                {
                                    ArrayList value = new ArrayList();
                                    value = _styleName.AttribAncestor[clsName];
                                    value.Add(ancestorClass);
                                    _styleName.AttribAncestor[clsName] = value;
                                }
                                else
                                {
                                    _styleName.AttribAncestor[clsName] = concat;
                                }
                                _pseudoClassName = true;
                            }
                            //ancestorClassName = true;
                        }
                        else if (node.Text == "TAG" && node.NextNode.Text == "CLASS" && node.NextNode.NextNode.Text == "PROPERTY")
                        {
                            if (_pseudoClassName == false)
                            {
                                string clsName = GetClassName(node.NextNode);
                                className = GetClassName(node.NextNode) + "." + className;
                                _attributeInfo.ClassName = className;
                                _pseudoClassName = true;
                            }
                        }
                        else if (node.PrevNode == null && node.NextNode.Text == "PARENTOF" && node.NextNode.NextNode.Text == "CLASS" && node.NextNode.NextNode.NextNode.Text == "PROPERTY")
                        {
                            if (_pseudoClassName == false)
                            {
                                string ancestorClass = className;
                                string clsName = GetClassName(node.NextNode.NextNode);
                                className = GetClassName(node.NextNode.NextNode) + "." + className;
                                _attributeInfo.ClassName = className;
                                string concatValue = string.Empty;
                                ArrayList Concat = new ArrayList();
                                Concat.Add(ancestorClass);
                                if (_styleName.AttribAncestor.ContainsKey(clsName))
                                {
                                    ArrayList value = new ArrayList();
                                    value = _styleName.AttribAncestor[clsName];
                                    value.Add(ancestorClass);
                                    _styleName.AttribAncestor[clsName] = value;
                                }
                                else
                                {
                                    _styleName.AttribAncestor[clsName] = Concat;
                                }
                                _pseudoClassName = true;
                            }
                            //ancestorClassName = true;
                        }
                    }
                    if (node.Parent.Nodes.Count > 1)
                    {
                        if (_pseudoClassName == false)
                        {
                            if (node.Text == "CLASS" && node.NextNode.Text == "PRECEDES")
                            {
                                if (node.Parent.Nodes[3].Text != "PARENTOF")
                                {
                                    pseudoClassName = GetAttribPseudo(node.Parent);
                                    _attributeInfo.ClassName = pseudoClassName;
                                    _pseudoClassName = true;
                                }
                            }
                            else if (node.Text == "TAG" && node.LastNode.Text == "ATTRIB")
                            {
                                pseudoClassName = GetAttribPseudo(node.Parent);
                                _attributeInfo.ClassName = pseudoClassName;
                                _pseudoClassName = true;
                            }
                            else if (node.Text == "CLASS" && node.LastNode.Text == "ATTRIB")
                            {
                                pseudoClassName = GetPseudoDetails(node.Parent);
                                if (!pseudoClassName.Contains("-"))
                                {
                                    pseudoClassName = "";
                                }
                                _attributeInfo.ClassName = pseudoClassName;
                            }
                        }
                    }
                }
            }
            return lang;
        }

        private void ClassPropertyOLD(TreeNode tree)
        {
            _pseudoClassName = false;
            _paragraphProperty.Clear();
            _textProperty.Clear();
            _columnProperty.Clear();
            _attributeInfo = new StyleAttribute();
            Dictionary<string, string> attribInfo = new Dictionary<string, string>();
            Dictionary<string, string> attribute = new Dictionary<string, string>();
            bool ancestorClassName = false;
            string pseudoClassName = string.Empty;
            string className = string.Empty;

            try
            {
                foreach (TreeNode node in tree.Nodes)
                {
                    if (node.Text == "CLASS" || node.Text == "TAG")
                    {
                        _borderAdded = false;
                        if (ancestorClassName == false)
                        {
                            if (_pseudoClassName == false)
                            {
                                className = GetClassName(node);
                                if (className == "footnote")
                                    className = "Footnote";
                                _attributeInfo.ClassName = className;
                            }
                        }

                        if (node.LastNode.Text == "ATTRIB")
                        {
                            attribute.Clear();
                            attribute = GetAttribValue(node);
                            if (attribute.ContainsKey("lang"))
                            {
                                className = className + "_." + attribute["lang"];
                                _attributeInfo.ClassName = _attributeInfo.ClassName + "_." + attribute["lang"];

                                if (_isDictionary)
                                {
                                    _attributeInfo.Name = "language";
                                    _attributeInfo.StringValue = attribute["lang"];


                                    attribInfo = _mapProperty.MapMultipleValue(_attributeInfo);
                                    foreach (KeyValuePair<string, string> para in attribInfo)
                                    {
                                        _textProperty[_allTextProperty[para.Key].ToString() + para.Key] = para.Value;
                                    }
                                }
                            }
                            else if (node.Text == "TAG")
                            {
                                string classAttrib = string.Empty;
                                string key = "";
                                foreach (KeyValuePair<string, string> attrib in attribute)
                                {
                                    classAttrib += attrib.Key + attrib.Value;
                                    key = attrib.Key;
                                }

                                _styleName.TagAttrib[className] = key;
                                className = classAttrib + "_." + className;
                                _pseudoClassName = true;
                            }
                            if (_pseudoClassName == false)
                            {
                                foreach (TreeNode childNodeItem in node.LastNode.Nodes)
                                {
                                    if (childNodeItem.Text == "HASVALUE")
                                    {
                                        GetAttribHasValueDetails(node.Parent);
                                        GetAttribLangBeforeList(node.Parent);
                                        pseudoClassName = GetPseudoDetails(node.Parent);
                                        _attributeInfo.ClassName = pseudoClassName;
                                        className = GetClassName(node.Parent.FirstNode) + " " + childNodeItem.NextNode.Text.Replace("'", "");
                                        _attributeInfo.ClassName = className;
                                        _pseudoClassName = true;
                                    }
                                }
                            }
                        }

                        if (_pseudoClassName == false)
                        {
                            if (node.Parent != null)
                            {
                                if (node.Parent.Nodes.Count > 2)
                                {
                                    if (node.NextNode.Text == "CLASS" && node.NextNode.NextNode.Text == "PROPERTY")
                                    {
                                        if (_pseudoClassName == false)
                                        {
                                            string ancestorClass = className;
                                            string clsName = GetClassName(node.NextNode);
                                            className = GetClassName(node.NextNode) + "." + className;
                                            _attributeInfo.ClassName = className;
                                            string concatValue = string.Empty;
                                            ArrayList concat = new ArrayList();
                                            concat.Add(ancestorClass);
                                            if (_styleName.AttribAncestor.ContainsKey(clsName))
                                            {
                                                ArrayList value = new ArrayList();
                                                value = _styleName.AttribAncestor[clsName];
                                                value.Add(ancestorClass);
                                                _styleName.AttribAncestor[clsName] = value;
                                            }
                                            else
                                            {
                                                _styleName.AttribAncestor[clsName] = concat;
                                            }
                                            _pseudoClassName = true;
                                        }
                                        ancestorClassName = true;
                                    }
                                    else if (node.PrevNode == null && node.NextNode.Text == "PARENTOF" && node.NextNode.NextNode.Text == "CLASS" && node.NextNode.NextNode.NextNode.Text == "PROPERTY")
                                    {
                                        if (_pseudoClassName == false)
                                        {
                                            string ancestorClass = className;
                                            string clsName = GetClassName(node.NextNode.NextNode);
                                            className = GetClassName(node.NextNode.NextNode) + "." + className;
                                            _attributeInfo.ClassName = className;
                                            string concatValue = string.Empty;
                                            ArrayList Concat = new ArrayList();
                                            Concat.Add(ancestorClass);
                                            if (_styleName.AttribAncestor.ContainsKey(clsName))
                                            {
                                                ArrayList value = new ArrayList();
                                                value = _styleName.AttribAncestor[clsName];
                                                value.Add(ancestorClass);
                                                _styleName.AttribAncestor[clsName] = value;
                                            }
                                            else
                                            {
                                                _styleName.AttribAncestor[clsName] = Concat;
                                            }
                                            _pseudoClassName = true;
                                        }
                                        ancestorClassName = true;
                                    }
                                }
                                if (node.Parent.Nodes.Count > 1)
                                {
                                    if (_pseudoClassName == false)
                                    {
                                        if (node.Text == "CLASS" && node.NextNode.Text == "PRECEDES")
                                        {
                                            if (node.Parent.Nodes[3].Text != "PARENTOF")
                                            {
                                                pseudoClassName = GetAttribPseudo(node.Parent);
                                                _attributeInfo.ClassName = pseudoClassName;
                                                _pseudoClassName = true;
                                            }
                                        }
                                        else if (node.Text == "TAG" && node.LastNode.Text == "ATTRIB")
                                        {
                                            pseudoClassName = GetAttribPseudo(node.Parent);
                                            _attributeInfo.ClassName = pseudoClassName;
                                            _pseudoClassName = true;
                                        }
                                        else if (node.Text == "CLASS" && node.LastNode.Text == "ATTRIB")
                                        {
                                            pseudoClassName = GetPseudoDetails(node.Parent);
                                            if (!pseudoClassName.Contains("-"))
                                            {
                                                pseudoClassName = "";
                                            }
                                            _attributeInfo.ClassName = pseudoClassName;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (node.Text == "PARENTOF")
                    {
                        if (_pseudoClassName == false)
                        {
                            pseudoClassName = GetPseudoParent(tree);
                            _attributeInfo.ClassName = pseudoClassName;
                            _pseudoClassName = true;
                        }
                    }
                    else if (node.Text == "PSEUDO")
                    {
                        if (_pseudoClassName == false)
                        {
                            pseudoClassName = GetPseudoDetails(tree);
                            _attributeInfo.ClassName = pseudoClassName;
                            _pseudoClassName = true;
                        }
                    }
                    else if (node.Text == "PROPERTY")
                    {
                        _attributeInfo = Properties(node);
                        if (_attributeInfo.Name.ToLower() != "content")
                        {
                            _attributeInfo = MapProperty(className, node, _attributeInfo);
                            if (_attributeInfo.Name != "font")
                            {
                                AddProperties(_attributeInfo, className);
                            }
                        }
                        else if (!_styleName.ClassContent.ContainsKey(className.Replace("\"", "")) && _pseudoClassName == false)
                        {
                            _attributeInfo.StringValue = _attributeInfo.StringValue.Replace("'", "");
                            _styleName.ClassContent.Add(className, _attributeInfo.StringValue.Replace("\"", ""));
                        }
                    }
                }
                _writer.WriteStartElement("style:style");
                string familyType = "paragraph";
                if (pseudoClassName == string.Empty)
                {
                    _writer.WriteAttributeString("style:name", className);
                }
                else
                {
                    className = pseudoClassName;
                    _writer.WriteAttributeString("style:name", className);
                    familyType = "text";
                }

                string[] tagClass = className.Split('.'); // for mergeTag function.
                if (tagClass.Length > 1)
                {
                    _tagName.Add(className);
                }
                _baseTagName.Remove(className);

                if (_tagProperty.ContainsKey(className)) // Is the Tag
                {
                    foreach (KeyValuePair<string, string> prop in _tagProperty[className]) // Merge Text and Paragraph Property.
                    {
                        string[] property = prop.Key.Split(':');
                        string propName = property[1];
                        if (_allParagraphProperty.ContainsKey(propName))
                        {
                            if (!_paragraphProperty.ContainsKey(prop.Key))
                                _paragraphProperty[prop.Key] = prop.Value;
                        }
                        else if (_allTextProperty.ContainsKey(propName))
                        {
                            if (!_textProperty.ContainsKey(prop.Key))
                                _textProperty[prop.Key] = prop.Value;
                        }
                    }

                    foreach (KeyValuePair<string, string> para in _paragraphProperty)
                    {
                        _tagProperty[className][para.Key] = para.Value;
                    }
                    foreach (KeyValuePair<string, string> text in _textProperty)
                    {
                        _tagProperty[className][text.Key] = text.Value;
                    }
                }

                _writer.WriteAttributeString("style:family", familyType); // "paragraph" will override by ContentXML.cs
                _writer.WriteAttributeString("style:parent-style-name", "none");
                if (familyType == "paragraph")
                {
                    if (_styleName.BorderProperty.Count > 0) // To fill the border properties for TD-307
                    {
                        string top = string.Empty;
                        string right = string.Empty;
                        string bottom = string.Empty;
                        string left = string.Empty;
                        foreach (string item in _styleName.BorderProperty.Keys)
                        {
                            string[] splitComma = _styleName.BorderProperty[item].Split(',');
                            if (item == "border-style")
                            {
                                top = top + splitComma[0].ToString() + " ";
                                right = right + splitComma[0].ToString() + " ";
                                bottom = bottom + splitComma[0].ToString() + " ";
                                left = left + splitComma[0].ToString() + " ";
                            }
                            else if (item == "border-width")
                            {
                                FillBorderValues(ref top, ref right, ref bottom, ref left, splitComma);
                            }
                            else if (item == "border-color")
                            {
                                FillBorderValues(ref top, ref right, ref bottom, ref left, splitComma);
                            }
                        }
                        _paragraphProperty[_allParagraphProperty["border-left"].ToString() + "border-left"] = left;
                        _paragraphProperty[_allParagraphProperty["border-right"].ToString() + "border-right"] = right;
                        _paragraphProperty[_allParagraphProperty["border-top"].ToString() + "border-top"] = top;
                        _paragraphProperty[_allParagraphProperty["border-bottom"].ToString() + "border-bottom"] = bottom;
                        _styleName.BorderProperty.Clear();
                    }
                    if (_paragraphProperty.Count > 0)
                    {
                        _writer.WriteStartElement("style:paragraph-properties");
                        foreach (KeyValuePair<string, string> para in _paragraphProperty)
                        {
                            _writer.WriteAttributeString(para.Key, para.Value);
                        }
                        _writer.WriteEndElement();
                    }
                }

                if (_columnProperty.Count > 0) // create a value XML file for content.xml with column property.
                {
                    CreateColumnXMLFile(className);
                }
                if (_textProperty.Count > 0)
                {
                    _writer.WriteStartElement("style:text-properties");
                    foreach (KeyValuePair<string, string> text in _textProperty)
                    {
                        _writer.WriteAttributeString(text.Key, text.Value);
                    }
                    if (_paragraphProperty.ContainsKey("fo:background-color"))
                    {
                        _writer.WriteAttributeString("fo:background-color", _paragraphProperty["fo:background-color"]);
                    }
                    _writer.WriteEndElement();
                }
                _writer.WriteEndElement();
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
        }

        #region FillBorderValues
        /// <summary>
        /// To fill the border properties based on the position.
        /// </summary>
        /// <param name="top"></param>
        /// <param name="right"></param>
        /// <param name="bottom"></param>
        /// <param name="left"></param>
        /// <param name="splitComma"></param>
        private static void FillBorderValues(ref string top, ref string right, ref string bottom, ref string left, string[] splitComma)
        {
            if (splitComma.Length == 1)
            {
                top = top + " " + splitComma[0];
                right = right + " " + splitComma[0];
                bottom = bottom + " " + splitComma[0];
                left = left + " " + splitComma[0];
            }
            else if (splitComma.Length == 2)
            {
                top = top + " " + splitComma[0];
                right = right + " " + splitComma[1] ;
                bottom = bottom + " " + splitComma[0] ;
                left = left + " " + splitComma[1];
            }
            else if (splitComma.Length == 3)
            {
                top = top + " " + splitComma[0];
                right = right + " " + splitComma[1];
                bottom = bottom + " " + splitComma[2];
                left = left + " " + splitComma[1];
            }
            else if (splitComma.Length == 4)
            {
                top = top + " " + splitComma[0];
                right = right + " " + splitComma[1];
                bottom = bottom + " " + splitComma[2];
                left = left + " " + splitComma[3];
            }
        }

        #endregion

        /// -------------------------------------------------------------------------------------------
        /// <summary>
        /// Map _attributeInfo to MapPropertys.cs
        /// </summary>
        /// <param name="className"></param>
        /// <param name="node"></param>
        /// <param name="styleAttributeInfo">StyleAttribute _attributeInfo</param>
        /// <returns>StyleAttribute _attributeInfo</returns>
        /// -------------------------------------------------------------------------------------------        
        /// 
        private StyleAttribute MapProperty(string className, TreeNode node, StyleAttribute styleAttributeInfo)
        {
            Dictionary<string, string> attributeInfo = new Dictionary<string, string>();
            styleAttributeInfo.ClassName = className;
            try
            {
                if (styleAttributeInfo.Name == "padding" || styleAttributeInfo.Name == "margin" || styleAttributeInfo.Name == "border")
                {
                    attributeInfo = _mapProperty.MapMultipleValue(styleAttributeInfo);
                    foreach (KeyValuePair<string, string> para in attributeInfo)
                    {
                        _paragraphProperty[_allParagraphProperty[para.Key] + para.Key] = para.Value;
                    }
                }
                else if (styleAttributeInfo.Name == "list-style-position")
                {
                    attributeInfo = _mapProperty.MapMultipleValue(styleAttributeInfo);
                    foreach (KeyValuePair<string, string> para in attributeInfo)
                    {
                        _paragraphProperty[_allParagraphProperty[para.Key].ToString() + para.Key] = para.Value;
                    }
                }
                else if (styleAttributeInfo.Name == "border-width"
                    || styleAttributeInfo.Name == "border-color"
                    || styleAttributeInfo.Name == "border-style"
                    || styleAttributeInfo.Name == "border-top-width"
                    || styleAttributeInfo.Name == "border-left-width"
                    || styleAttributeInfo.Name == "border-right-width"
                    || styleAttributeInfo.Name == "border-bottom-width"
                    )
                {
                    if (styleAttributeInfo.Name == "border-style" 
                    || styleAttributeInfo.Name == "border-top-width"
                    || styleAttributeInfo.Name == "border-left-width"
                    || styleAttributeInfo.Name == "border-right-width"
                    || styleAttributeInfo.Name == "border-bottom-width")
                    {
                        _styleName.BorderProperty[styleAttributeInfo.Name] = styleAttributeInfo.StringValue;
                    }
                    else
                    {
                        SplitValue(styleAttributeInfo.StringValue, styleAttributeInfo.Name);
                    }
                }
                else if (styleAttributeInfo.Name == "column-rule")
                {
                    attributeInfo = _mapProperty.MapMultipleValue(styleAttributeInfo);
                    foreach (KeyValuePair<string, string> para in attributeInfo)
                    {
                        _columnSep[para.Key] = para.Value;
                    }
                }
                else if (styleAttributeInfo.Name == "font")
                {
                    attributeInfo = _mapProperty.MapMultipleValue(styleAttributeInfo);
                    StyleAttribute fontInfo = new StyleAttribute();
                    foreach (KeyValuePair<string, string> para in attributeInfo)
                    {
                        fontInfo.Name = para.Key;
                        fontInfo.StringValue = para.Value;
                        fontInfo = _mapProperty.MapSingleValue(fontInfo);
                        AddProperties(fontInfo, className);
                    }
                }
                else if (styleAttributeInfo.Name == "float")
                {
                    string clsName = GetClassName(node.Parent.FirstNode);
                    string position = styleAttributeInfo.StringValue;
                    //_styleName.FloatAlign.Add(clsName, position);
                    _styleName.FloatAlign[clsName] = position;

                    if (_styleName.ImageSource.ContainsKey(className))
                    {
                        var tempDict = new Dictionary<string, string>();
                        tempDict = _styleName.ImageSource[className];
                        tempDict[styleAttributeInfo.Name] = position;
                        _styleName.ImageSource[className] = tempDict;
                    }
                }
                else if (styleAttributeInfo.Name == "position")
                {
                    if (styleAttributeInfo.StringValue == "relative")
                        IsPosition = true;
                }
                else if (styleAttributeInfo.Name == "list-style-type")
                {
                    CreateListType(className, styleAttributeInfo.StringValue);
                }
                else if ((styleAttributeInfo.Name == "left" || styleAttributeInfo.Name == "right")
                    && IsPosition == true)
                {
                    styleAttributeInfo.StringValue = styleAttributeInfo.StringValue.Replace(",", "");
                    if (styleAttributeInfo.Name == "left")
                    {
                        _paragraphProperty[_allParagraphProperty["margin-left"] + "margin-left"] = styleAttributeInfo.StringValue;
                    }
                    else if (styleAttributeInfo.Name == "right")
                    {
                        if (styleAttributeInfo.StringValue.IndexOf("-") >= 0)
                        {
                            _paragraphProperty[_allParagraphProperty["margin-left"] + "margin-left"] = styleAttributeInfo.StringValue.Replace("-", "");
                        }
                        else
                        {
                            _paragraphProperty[_allParagraphProperty["margin-left"] + "margin-left"] = "-" + styleAttributeInfo.StringValue;
                        }
                    }

                }
                else if (styleAttributeInfo.Name == "height" || styleAttributeInfo.Name == "width")
                {
                    if (styleAttributeInfo.StringValue == "auto")
                    {
                        _styleName.IsAutoWidthforCaption = true;
                    }
                    string clsName = GetClassName(node.Parent.FirstNode);
                    string concatValue = styleAttributeInfo.Name + ", " + styleAttributeInfo.StringValue;
                    ArrayList size = new ArrayList();
                    size.Add(concatValue);
                    if (_styleName.ImageSize.ContainsKey(clsName))
                    {
                        ArrayList value = new ArrayList();
                        value = _styleName.ImageSize[clsName];
                        value.Add(concatValue);
                        _styleName.ImageSize[clsName] = value;
                    }
                    else
                    {
                        _styleName.ImageSize[clsName] = size;
                    }

                    if (_styleName.ImageSource.ContainsKey(className))
                    {
                        Dictionary<string, string> tempDict = new Dictionary<string, string>();
                        tempDict = _styleName.ImageSource[className];

                        if (concatValue.IndexOf(",") >= 0)
                        {
                            string[] splitValue = concatValue.Split(',');
                            if (splitValue.Length == 3)
                            {
                                if (splitValue[2].Trim().ToLower() != "in")
                                {
                                    float convertedValue = Common.UnitConverterOO(splitValue[1] + splitValue[2], "in");
                                    if (convertedValue != 0)
                                    {
                                        concatValue = splitValue[0] + "," + convertedValue + ", in";
                                    }
                                }
                            }
                        }

                        tempDict[styleAttributeInfo.Name] = concatValue;
                        _styleName.ImageSource[className] = tempDict;
                    }


                }
                else if (styleAttributeInfo.Name == "background-color")
                {
                    styleAttributeInfo = _mapProperty.MapSingleValue(styleAttributeInfo);
                    _styleName.BackgroundColor.Add(className);
                }
                else if (styleAttributeInfo.Name == "white-space" && styleAttributeInfo.StringValue == "pre")
                {
                    _styleName.WhiteSpace.Add(className);
                }
                else if (styleAttributeInfo.Name == "display")
                {
                    if (className != "img")
                    {
                        if (styleAttributeInfo.StringValue == "block")
                        {
                            _styleName.DisplayBlock.Add(className);
                        }
                        else if (styleAttributeInfo.StringValue == "inline")
                        {
                            _styleName.DisplayInline.Add(className);
                        }
                        else if (styleAttributeInfo.StringValue == "footnote" || styleAttributeInfo.StringValue == "prince-footnote")
                        {
                            _styleName.DisplayFootNote.Add(className);
                        }
                        else if (styleAttributeInfo.StringValue == "none")
                        {
                            styleAttributeInfo = _mapProperty.MapSingleValue(styleAttributeInfo);
                            _styleName.DisplayNone.Add(className);
                        }
                        //else if (styleAttributeInfo.StringValue == "inherit" || styleAttributeInfo.StringValue == "run-in")
                        //{
                        //}
                        else
                        {
                            throw new Exception("Not a valid command");
                        }
                    }
                }
                else if (styleAttributeInfo.Name == "counter-increment")
                {
                    Dictionary<string, string> key = new Dictionary<string, string>();
                    if (styleAttributeInfo.StringValue.IndexOf(',') > 0)
                    {
                        string[] splitcomma = styleAttributeInfo.StringValue.Split(',');
                        if (splitcomma.Length > 1)
                        {
                            key[splitcomma[0]] = splitcomma[1];
                            _styleName.CounterParent[className] = key;
                            _styleName.ContentCounter[splitcomma[0]] = 0;
                        }
                    }
                    else
                    {
                        key[styleAttributeInfo.StringValue] = "1";
                        _styleName.CounterParent[className] = key;
                        _styleName.ContentCounter[styleAttributeInfo.StringValue] = 0;
                    }
                }
                else if (styleAttributeInfo.Name == "counter-reset")
                {
                    _styleName.ContentCounterReset[className] = styleAttributeInfo.StringValue;
                }
                else if (styleAttributeInfo.Name == "clear")
                {
                    string clsName = GetClassName(node.Parent.FirstNode);
                    _styleName.ClearProperty[clsName] = styleAttributeInfo.StringValue;
                }
                else if (styleAttributeInfo.Name == "direction")
                {
                    attributeInfo = _mapProperty.MapMultipleValue(styleAttributeInfo);
                    foreach (KeyValuePair<string, string> para in attributeInfo)
                    {
                        _paragraphProperty[_allParagraphProperty[para.Key] + para.Key] = para.Value;
                    }
                }
                else if (styleAttributeInfo.Name == "hyphens" || styleAttributeInfo.Name == "prince-hyphenate")
                {
                    string enableHyphen = "false";
                    if (styleAttributeInfo.StringValue == "auto")
                    {
                        enableHyphen = "true";
                    }
                    _textProperty[_allTextProperty["hyphenate"].ToString() + "hyphenate"] = enableHyphen;
                }
                else if (styleAttributeInfo.Name == "hyphenate-before")
                {
                    _textProperty[_allTextProperty["hyphenation-push-char-count"].ToString() + "hyphenation-push-char-count"] = styleAttributeInfo.StringValue;
                }
                else if (styleAttributeInfo.Name == "hyphenate-after")
                {
                    _textProperty[_allTextProperty["hyphenation-remain-char-count"].ToString() + "hyphenation-remain-char-count"] = styleAttributeInfo.StringValue;
                }
                else if (styleAttributeInfo.Name == "hyphenate-lines")
                {
                    _paragraphProperty[_allParagraphProperty["hyphenation-ladder-count"].ToString() + "hyphenation-ladder-count"] = styleAttributeInfo.StringValue;
                }
                else
                {
                    styleAttributeInfo = _mapProperty.MapSingleValue(styleAttributeInfo); // convert attributes to Open Office format
                    if (styleAttributeInfo.Name == "visibility" && styleAttributeInfo.StringValue == "hidden")
                    {
                        _styleName.VisibilityClassName[styleAttributeInfo.ClassName] = styleAttributeInfo.StringValue;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                _verboseWriter.WriteError(styleAttributeInfo.ClassName, styleAttributeInfo.Name, ex.Message, styleAttributeInfo.StringValue);
            }
            return styleAttributeInfo;
        }

        /// -------------------------------------------------------------------------------------------
        /// <summary>
        /// Add Property Values
        /// 
        /// <list> 
        /// </list>
        /// </summary>
        /// <param name="attributeInfo">Attributes</param>
        /// <param name="className">class Name</param>
        /// <returns>null</returns>
        /// -------------------------------------------------------------------------------------------        
        /// 
        private void AddProperties(StyleAttribute attributeInfo, string className)
        {
            try
            {
                string attributeName = attributeInfo.Name;
                string prefix = string.Empty;
                if (_allParagraphProperty.ContainsKey(attributeName)) // paragraph property
                {
                    prefix = _allParagraphProperty[attributeName].ToString();
                    if (attributeName.IndexOf("border") >= 0 && _paragraphProperty.ContainsKey("fo:" + attributeName))
                    {
                        _paragraphProperty.Remove("fo:" + attributeName);
                    }

                    if (attributeName.IndexOf("padding") >= 0 && _borderAdded == false)
                    {
                        _borderAdded = true;
                        string propName = "fo:" + attributeName;
                        if (!_paragraphProperty.ContainsKey(propName) && attributeInfo.StringValue.IndexOf(",") == -1
                            && attributeInfo.StringValue != "0")
                        {
                            _paragraphProperty.Add(propName, attributeInfo.StringValue);
                        }
                        if (!_paragraphProperty.ContainsKey("fo:border-bottom"))
                        {
                            _paragraphProperty.Add("fo:border-bottom", "0.5pt solid #ffffff");
                        }
                        if (!_paragraphProperty.ContainsKey("fo:border-top"))
                        {
                            _paragraphProperty.Add("fo:border-top", "0.5pt solid #ffffff");
                        }
                        if (!_paragraphProperty.ContainsKey("fo:border-left"))
                        {
                            _paragraphProperty.Add("fo:border-left", "0.5pt solid #ffffff");
                        }
                        if (!_paragraphProperty.ContainsKey("fo:border-right"))
                        {
                            _paragraphProperty.Add("fo:border-right", "0.5pt solid #ffffff");
                        }
                    }
                    else
                    {
                        _paragraphProperty[prefix + attributeName] = attributeInfo.StringValue;
                    }
                }
                else if (_allTextProperty.ContainsKey(attributeName)) // fullString property
                {
                    prefix = _allTextProperty[attributeName].ToString();
                    _textProperty[prefix + attributeName] = attributeInfo.StringValue;
                }
                else if (_allColumnProperty.ContainsKey(attributeName)) // fullString property
                {
                    prefix = _allColumnProperty[attributeName].ToString();
                    _columnProperty[prefix + attributeName] = attributeInfo.StringValue;
                }
                else if (attributeName == "direction") // fullString property
                {
                    Dictionary<string, string> p = _mapProperty.MapMultipleValue(attributeInfo);
                    prefix = _allParagraphProperty["writing-mode"];
                    _sectionProperty[prefix + "writing-mode"] = p["writing-mode"];
                }
                else
                {
                    switch (attributeName)
                    {
                        case "content":
                            break;

                        default:
                            {
                                break;
                            }
                    }
                }

                //Handle Relative Values
                if (attributeInfo.StringValue.IndexOf('%') >= 0 || attributeInfo.StringValue.IndexOf("em") >= 0 ||
                attributeInfo.StringValue.ToLower() == "bolder" || attributeInfo.StringValue.ToLower() == "lighter" ||
                attributeInfo.StringValue.ToLower() == "larger" || attributeInfo.StringValue.ToLower() == "smaller")
                {
                    if (_styleName.CssClassName.ContainsKey(className))
                    {
                        _attribute[prefix + attributeName] = attributeInfo.StringValue;
                    }
                    else
                    {
                        _attribute = new Dictionary<string, string>();
                        _attribute[prefix + attributeName] = attributeInfo.StringValue;
                        _styleName.CssClassName[className] = _attribute;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
        }

        /// <summary>
        /// Create custom List
        /// list-style-type: none;
        /// </summary>
        /// <param name="listType">Type Name</param>
        private void CreateListType(string className, string listType)
        {
            string listName = "Listdecimal";
            string numFormat = "1";
            string numSuffix = ".";
            if (listType == "none")
            {
                listName = "Listnone";
                numFormat = string.Empty;
                numSuffix = string.Empty;
            }
            else if (listType == "disc")
            {
                listName = "Listdisc";
                numFormat = "•";
            }
            else if (listType == "circle")
            {
                listName = "Listcircle";
                numFormat = "◦";
            }
            else if (listType == "square")
            {
                listName = "Listsquare";
                numFormat = "▪";
            }
            else if (listType == "decimal")
            {
                listName = "Listdecimal";
                numFormat = "1";
            }
            else if (listType == "lower-roman")
            {
                listName = "Listlowerroman";
                numFormat = "i";
            }
            else if (listType == "upper-roman")
            {
                listName = "Listupperroman";
                numFormat = "I";
            }
            else if (listType == "lower-alpha")
            {
                listName = "Listloweralpha";
                numFormat = "a";
            }
            else if (listType == "upper-alpha")
            {
                listName = "Listupperalpha";
                numFormat = "A";
            }

            switch (listType)
            {

                case "disc":
                case "circle":
                case "square":
                    {
                        _writer.WriteStartElement("text:list-style");
                        _writer.WriteAttributeString("style:name", listName);
                        _writer.WriteStartElement("text:list-level-style-bullet");
                        _writer.WriteAttributeString("text:level", "1");
                        _writer.WriteAttributeString("text:style-name", "Bullet_20_Symbols");
                        _writer.WriteAttributeString("style:num-suffix", numSuffix);
                        _writer.WriteAttributeString("text:bullet-char", numFormat);
                        break;
                    }
                case "none":
                case "decimal":
                case "lower-roman":
                case "upper-roman":
                case "lower-alpha":
                case "upper-alpha":
                    {
                        _writer.WriteStartElement("text:list-style");
                        _writer.WriteAttributeString("style:name", listName);
                        _writer.WriteStartElement("text:list-level-style-number");
                        _writer.WriteAttributeString("text:level", "1");
                        _writer.WriteAttributeString("text:style-name", "Numbering_20_Symbols");
                        _writer.WriteAttributeString("style:num-suffix", numSuffix);
                        _writer.WriteAttributeString("style:num-format", numFormat);
                        break;
                    }
            }

            _writer.WriteStartElement("style:list-level-properties");
            _writer.WriteAttributeString("text:list-level-position-and-space-mode", "label-alignment");
            _writer.WriteStartElement("style:list-level-label-alignment");
            _writer.WriteAttributeString("text:label-followed-by", "listtab");
            _writer.WriteAttributeString("text:list-tab-stop-position", "0.5in");
            _writer.WriteAttributeString("fo:text-indent", "-0.25in");
            _writer.WriteAttributeString("fo:margin-left", "0.5in");
            _writer.WriteEndElement();
            _writer.WriteEndElement();
            _writer.WriteEndElement();
            _writer.WriteEndElement();
            _styleName.ListType[className] = listName;
        }

        /// -------------------------------------------------------------------------------------------
        /// <summary>
        /// Create value XML file for storing section and column info
        /// 
        /// <list> 
        /// </list>
        /// </summary>
        /// <param name="tree">Antlr Tree</param>
        /// <param name="ctp">Antlr tree collection</param>
        /// <returns>class Name</returns>
        /// -------------------------------------------------------------------------------------------        
        /// 
        private void CreateColumnXMLFile(string className)
        {
            try
            {

                _styleName.SectionName.Add(className.Trim());
                string path = Common.PathCombine(Path.GetTempPath(), "_" + className.Trim() + ".xml");

                XmlTextWriter writerCol = new XmlTextWriter(path, null);
                writerCol.Formatting = Formatting.Indented;
                writerCol.WriteStartDocument();
                writerCol.WriteStartElement("office:document-content");
                writerCol.WriteAttributeString("xmlns:office", "urn:oasis:names:tc:opendocument:xmlns:office:1.0");
                writerCol.WriteAttributeString("xmlns:style", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
                writerCol.WriteAttributeString("xmlns:fo", "urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0");
                writerCol.WriteAttributeString("xmlns:text", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");
                writerCol.WriteAttributeString("style:parent-style-name", "none");

                writerCol.WriteStartElement("style:style");
                writerCol.WriteAttributeString("style:name", "Sect_" + className.Trim());
                writerCol.WriteAttributeString("style:family", "section");

                writerCol.WriteStartElement("style:section-properties");
                if (_columnProperty.ContainsKey("text:dont-balance-text-columns"))
                {
                    writerCol.WriteAttributeString("text:dont-balance-text-columns", _columnProperty["text:dont-balance-text-columns"]);
                    _columnProperty.Remove("text:dont-balance-text-columns");
                }
                else
                {
                    writerCol.WriteAttributeString("text:dont-balance-text-columns", "false");
                }

                if (_sectionProperty != null && _sectionProperty.ContainsKey("style:writing-mode"))
                {
                    writerCol.WriteAttributeString("style:writing-mode", _sectionProperty["style:writing-mode"]);
                }

                writerCol.WriteAttributeString("style:editable", "false");

                writerCol.WriteStartElement("style:columns");


                string columnGap = "0pt";
                byte columnCount = 0;
                foreach (KeyValuePair<string, string> text in _columnProperty)
                {
                    if (text.Key == "fo:column-gap")
                    {
                        columnGap = text.Value;
                    }
                    else if (text.Key == "fo:column-count")
                    {
                        columnCount = (byte)Common.ConvertToInch(text.Value);
                    }
                }

                writerCol.WriteAttributeString("fo:column-count", columnCount.ToString());
                float pageWidth = 0;
                float relWidth = 0;
                float spacing = 0;
                float colWidth = 0;
                if (columnCount > 1)
                {
                    pageWidth = Common.ConvertToInch(_pageLayoutProperty["fo:page-width"]);
                    spacing = Common.ConvertToInch(columnGap) / 2;
                    relWidth = (pageWidth - (spacing * (columnCount * 2))) / columnCount;

                    if (columnGap.IndexOf("em") > 0 || columnGap.IndexOf("%") > 0) // Column Gap will be calculte in content.xml
                    {
                        colWidth = (pageWidth - Common.ConvertToInch(_pageLayoutProperty["fo:margin-left"])
                                        - Common.ConvertToInch(_pageLayoutProperty["fo:margin-left"])) / 2.0F;
                    }
                    else
                    {
                        colWidth = (pageWidth - Common.ConvertToInch(columnGap) - Common.ConvertToInch(_pageLayoutProperty["fo:margin-left"])
                                     - Common.ConvertToInch(_pageLayoutProperty["fo:margin-left"])) / 2.0F;
                    }
                    _styleName.ColumnWidth = colWidth; // for picture size calculation

                    string relWidthStr = relWidth.ToString() + "*";
                    for (int i = 1; i <= columnCount; i++)
                    {
                        writerCol.WriteStartElement("style:column");
                        if (i == 1)
                        {
                            writerCol.WriteAttributeString("style:rel-width", relWidthStr);
                            writerCol.WriteAttributeString("fo:start-indent", 0 + "in");
                            writerCol.WriteAttributeString("fo:end-indent", spacing + "in");
                        }
                        else if (i == columnCount)
                        {
                            writerCol.WriteAttributeString("style:rel-width", relWidthStr);
                            writerCol.WriteAttributeString("fo:start-indent", spacing + "in");
                            writerCol.WriteAttributeString("fo:end-indent", 0 + "in");
                        }
                        else
                        {
                            writerCol.WriteAttributeString("style:rel-width", relWidthStr);
                            writerCol.WriteAttributeString("fo:start-indent", spacing + "in");
                            writerCol.WriteAttributeString("fo:end-indent", spacing + "in");
                        }
                        writerCol.WriteEndElement();
                    }

                    if (columnGap.IndexOf("em") > 0)
                    {
                        Dictionary<string, string> pageProperties = new Dictionary<string, string>();
                        pageProperties["pageWidth"] = pageWidth.ToString();
                        pageProperties["columnCount"] = columnCount.ToString();
                        pageProperties["columnGap"] = columnGap.ToString();

                        _styleName.ColumnGapEm["Sect_" + className.Trim()] = pageProperties;
                    }
                }

                if (_columnSep.Count > 0)
                {
                    writerCol.WriteStartElement("style:column-sep");
                    foreach (KeyValuePair<string, string> text in _columnSep)
                    {
                        writerCol.WriteAttributeString(text.Key, text.Value);
                    }
                    writerCol.WriteEndElement();
                }

                writerCol.WriteEndElement();
                writerCol.WriteEndElement();
                writerCol.WriteEndElement();

                writerCol.WriteEndElement();
                writerCol.WriteEndDocument();
                writerCol.Flush();
                writerCol.Close();
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
        }

        /// -------------------------------------------------------------------------------------------
        /// <summary>
        /// Get className from the node
        /// 
        /// <list> 
        /// </list>
        /// </summary>
        /// <param name="tree">Antlr Tree</param>
        /// <param name="ctp">Antlr tree collection</param>
        /// <returns>class Name</returns>
        /// -------------------------------------------------------------------------------------------        
        /// 
        private string GetClassName(TreeNode tree)
        {
            string className = string.Empty;
            try
            {
                //TODO - Remove NEST classNames
                className = tree.FirstNode.Text;
                return (className);
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                return (className);
            }
        }
        /// -------------------------------------------------------------------------------------------
        /// <summary>
        /// Get GetAttributes Values from the node
        /// 
        /// <list> 
        /// </list>
        /// </summary>
        /// <param name="tree">Antlr Tree</param>
        /// <returns>ATTRIB</returns>
        /// -------------------------------------------------------------------------------------------        
        /// 
        private Dictionary<string, string> GetAttribValue(TreeNode tree)
        {
            Dictionary<string, string> attribute = new Dictionary<string, string>();
            try
            {
                foreach (TreeNode node in tree.Nodes)
                {
                    if (node.Text == "ATTRIB")
                    {
                        if (node.Nodes.Count > 1)
                        {
                            attribute[node.FirstNode.Text] = node.LastNode.Text.Replace("'", "");
                        }
                        else
                        {
                            attribute[node.FirstNode.Text] = "";
                        }
                    }
                }
                return (attribute);
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                return (attribute);
            }
        }

        /// -------------------------------------------------------------------------------------------
        /// <summary>
        /// Generate property nodes
        /// 
        /// <list> 
        /// </list>
        /// </summary>
        /// <param name="tree">Css tree</param>
        /// <returns></returns>
        /// -------------------------------------------------------------------------------------------        
        /// 
        private StyleAttribute Properties(TreeNode tree)
        {
            ArrayList unit = new ArrayList();
            unit.Add("ex");
            unit.Add("px");
            unit.Add("pc");

            StyleAttribute attribute = new StyleAttribute();
            attribute.Name = string.Empty;
            try
            {
                StringBuilder attributeVal = new StringBuilder("");
                string numericProperty = string.Empty;
                int numericPropertyPosition = 0;
                foreach (TreeNode node in tree.Nodes)
                {
                    if (attribute.Name == string.Empty)
                    {
                        attribute.Name = node.Text;
                    }
                    else
                    {
                        if (unit.Contains(node.Text))
                        {
                            if (Common.ValidateNumber(numericProperty))
                            {
                                string targetUnit = "pt";
                                if (node.Text == "ex")
                                {
                                    targetUnit = "em";
                                }
                                string propertyWithUnit = numericProperty + node.Text;
                                float convertedValue = Common.UnitConverterOO(propertyWithUnit, targetUnit);

                                string convertedValueWithUnit = convertedValue.ToString() + "," + targetUnit + ",";

                                //attributeVal = attributeVal.Remove( (0, numericPropertyPosition);
                                attributeVal = attributeVal.Remove(numericPropertyPosition, attributeVal.Length - numericPropertyPosition);

                                attributeVal = attributeVal.Append(convertedValueWithUnit);
                                numericPropertyPosition = 0;
                            }
                        }
                        else
                        {
                            numericProperty = node.Text;
                            numericPropertyPosition = attributeVal.Length;
                            attributeVal = attributeVal.Append(node.Text + ",");
                        }
                    }
                }

                int len = attributeVal.Length;
                if (len > 0)
                {
                    attributeVal.Remove(len - 1, 1);
                    attribute.StringValue = attributeVal.ToString();
                }
                else
                {
                    attribute.StringValue = "0";

                }

                return (attribute);
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                if (_verboseWriter.ShowError)
                {
                    _verboseWriter.WriteError("", "", ex.Message + "</BR>", "");
                }
                return (attribute);
            }
            //TODO Invalid Value
        }

        private string GetPageName(TreeNode node)
        {
            string pageName = "PAGE";
            if (node.FirstNode.Text == "PSEUDO")
            {
                pageName = pageName + node.FirstNode.FirstNode.Text;
            }
            return pageName;
        }


        /// -------------------------------------------------------------------------------------------
        /// <summary>
        /// Generate Non Class Nodes
        /// 
        /// <list> 
        /// </list>
        /// </summary>
        /// <param name="tree">Css tree</param>
        /// <returns></returns>
        /// -------------------------------------------------------------------------------------------
        private void PageProperty(TreeNode tree, string outputFile)
        {
            string className = string.Empty;
            string attributeName;
            StyleAttribute styleAttributeInfo = new StyleAttribute();
            Dictionary<string, string> attributeInfo = new Dictionary<string, string>();
            try
            {
                if (tree.Text == "PAGE")
                {
                    _attribClassName = "@page";
                    string pageName = GetPageName(tree);
                    if (pageName == "PAGE")
                    {
                        _pageFirst = false;
                    }
                    else if (pageName.IndexOf("first") > 0)
                    {
                        _pageFirst = true;  // used to add in dictionary
                    }
                    else if (pageName.IndexOf("left") > 0 || pageName.IndexOf("right") > 0)
                    {
                        foreach (TreeNode RegNodes in tree.Nodes)
                        {
                            if (RegNodes.Text == "REGION")
                            {
                                foreach (TreeNode propNode in RegNodes.Nodes)
                                {
                                    if(propNode.FirstNode != null && propNode.FirstNode.Text == "content"
                                        && propNode.FirstNode.NextNode.Text.IndexOf("string") >= 0)
                                    {
                                        isMirrored = true;
                                    }
                                }
                            }
                            if(isMirrored)
                                break;
                        }
                    }
                    if (pageName.IndexOf("first") == -1 && _styleName.ReferenceFormat.Length == 0)
                    {
                        string cValue = GetContentValue(tree);
                        if (cValue != string.Empty)
                        {
                            _styleName.ReferenceFormat = cValue;
                        }
                    }
                    PageHeaderFooter(tree, pageName);
                }

                foreach (TreeNode node in tree.Nodes)
                {
                    if (node.Text == "REGION")
                    {
                        if (node.Nodes.Count > 1 && node.FirstNode.Text == "footnotes")
                        {
                            foreach (TreeNode childNode in node.Nodes)
                            {
                                if (childNode.Text == "PROPERTY")
                                {
                                    styleAttributeInfo = Properties(childNode);
                                    attributeName = styleAttributeInfo.Name;
                                    if (styleAttributeInfo.Name == "border-top"
                                        || styleAttributeInfo.Name == "border-bottom"
                                        || styleAttributeInfo.Name == "border-left"
                                        || styleAttributeInfo.Name == "border-right")

                                    {
                                        styleAttributeInfo = _mapProperty.MapSingleValue(styleAttributeInfo);
                                        _styleName.FootNoteSeperator[styleAttributeInfo.Name] = styleAttributeInfo.StringValue;
                                    }
                                    else if (styleAttributeInfo.Name == "padding")
                                    {
                                        attributeInfo = _mapProperty.MapMultipleValue(styleAttributeInfo);
                                        foreach (KeyValuePair<string, string> keyValue in attributeInfo)
                                        {
                                            _styleName.FootNoteSeperator[keyValue.Key] = keyValue.Value;
                                        }
                                    }

                                }
                            }
                        }
                    }
                    else if (node.Text == "PROPERTY")
                    {
                        styleAttributeInfo = Properties(node);
                        attributeName = styleAttributeInfo.Name;
                        styleAttributeInfo.ClassName = _attribClassName;

                        if (styleAttributeInfo.Name == "margin" || styleAttributeInfo.Name == "border")
                        {
                            attributeInfo = _mapProperty.MapMultipleValue(styleAttributeInfo); // @page (margin: 3cm 4cm 5cm 6cm)
                            foreach (KeyValuePair<string, string> para in attributeInfo)
                            {
                                if (_pageFirst)
                                {
                                    _firstPageLayoutProperty[_allPageLayoutProperty[para.Key].ToString() + para.Key] = para.Value;
                                }
                                else
                                {
                                    _pageLayoutProperty[_allPageLayoutProperty[para.Key].ToString() + para.Key] = para.Value;
                                }
                            }
                        }
                        else if (styleAttributeInfo.Name.ToLower() == "-ps-referenceformat-string")
                        {
                            _styleName.ReferenceFormat = styleAttributeInfo.StringValue.Replace("\"", "");
                        }
                        else if (styleAttributeInfo.Name == "border-top"
                            || styleAttributeInfo.Name == "border-bottom"
                            || styleAttributeInfo.Name == "border-left"
                            || styleAttributeInfo.Name == "border-right"
                            || styleAttributeInfo.Name == "margin-top"
                            || styleAttributeInfo.Name == "margin-bottom"
                            || styleAttributeInfo.Name == "margin-left"
                            || styleAttributeInfo.Name == "margin-right"
                            || styleAttributeInfo.Name == "padding-top"
                            || styleAttributeInfo.Name == "padding-bottom"
                            || styleAttributeInfo.Name == "padding-left"
                            || styleAttributeInfo.Name == "padding-right"
                            || styleAttributeInfo.Name == "visibility")
                        {
                            styleAttributeInfo = _mapProperty.MapSingleValue(styleAttributeInfo);
                            if (_pageFirst)
                            {
                                _firstPageLayoutProperty[_allPageLayoutProperty[styleAttributeInfo.Name].ToString() + styleAttributeInfo.Name] = styleAttributeInfo.StringValue;
                            }
                            else
                            {
                                _pageLayoutProperty[_allPageLayoutProperty[styleAttributeInfo.Name].ToString() + styleAttributeInfo.Name] = styleAttributeInfo.StringValue;
                            }
                        }
                        else if (styleAttributeInfo.Name == "size")
                        {
                            if (styleAttributeInfo.StringValue.ToLower() == "landscape" || styleAttributeInfo.StringValue.ToLower() == "portrait" || styleAttributeInfo.StringValue.ToLower() == "auto")
                            {
                                if (_pageFirst)
                                {
                                    _firstPageLayoutProperty["style:print-orientation"] = styleAttributeInfo.StringValue.ToLower();
                                }
                                else
                                {
                                    _pageLayoutProperty["style:print-orientation"] = styleAttributeInfo.StringValue.ToLower();
                                }
                            }
                            else
                            {
                                attributeInfo = _mapProperty.MapMultipleValue(styleAttributeInfo);
                                foreach (KeyValuePair<string, string> para in attributeInfo)
                                {
                                    if (_pageFirst)
                                    {
                                        _firstPageLayoutProperty[_allPageLayoutProperty[para.Key].ToString() + para.Key] = para.Value;
                                    }
                                    else
                                    {
                                        _pageLayoutProperty[_allPageLayoutProperty[para.Key].ToString() + para.Key] = para.Value;
                                    }
                                }
                            }
                        }
                        else if (styleAttributeInfo.Name == "color" || styleAttributeInfo.Name == "background-color")
                        {
                            styleAttributeInfo = _mapProperty.MapSingleValue(styleAttributeInfo);
                            if (!_pageFirst)
                            {
                                _pageLayoutProperty[_allPageLayoutProperty[styleAttributeInfo.Name].ToString() + styleAttributeInfo.Name] = styleAttributeInfo.StringValue;
                            }
                            else
                            {
                                _firstPageLayoutProperty[_allPageLayoutProperty[styleAttributeInfo.Name].ToString() + styleAttributeInfo.Name] = styleAttributeInfo.StringValue;
                            }
                        }
                        else if (styleAttributeInfo.Name == "marks" && styleAttributeInfo.StringValue == "crop")
                        {
                            StylesXML.IsCropMarkChecked = true;
                        }
                        else if (styleAttributeInfo.Name.ToLower() == "dictionary")
                        {
                            if (styleAttributeInfo.StringValue == "true")
                            {
                                _isDictionary = true;
                            }
                        }
                        else
                        {
                            styleAttributeInfo = _mapProperty.MapSingleValue(styleAttributeInfo);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
        }

        private void ChangePageDimension(bool isPageFirst)
        {
            if (StylesXML.IsCropMarkChecked)
            {
                ChangePageProperty("fo:page-width", isPageFirst);
                ChangePageProperty("fo:page-height", isPageFirst);
                ChangePageProperty("fo:margin-left", isPageFirst);
                ChangePageProperty("fo:margin-top", isPageFirst);
                ChangePageProperty("fo:margin-bottom", isPageFirst);
                ChangePageProperty("fo:margin-right", isPageFirst);
            }
        }

        private string GetContentValue(TreeNode node)
        {
            string value = string.Empty;
            foreach (TreeNode regNode in node.Nodes)
            {
                if(regNode.Text == "REGION")
                {
                    foreach (TreeNode propNode in regNode.Nodes)
                    {
                        if (propNode.Text == "PROPERTY" && propNode.FirstNode.Text == "content" && propNode.Nodes.Count > 2)
                        {
                            for (int i = 1; i < propNode.Nodes.Count; i++)
                            {
                                value = value + propNode.Nodes[i].Text;
                            }
                            if (value != string.Empty && value.IndexOf("string(bookname") >= 0)
                            {
                                if(value.IndexOf("string(verse") >= 0)
                                {
                                    return "Genesis 1:1 Genesis 1:15";
                                }
                                else
                                {
                                    return "Genesis 1";
                                    
                                }
                            }
                            
                        }
                    }
                }
            }
            return value;
        }


        /// -------------------------------------------------------------------------------------------
        /// <summary>
        /// Generate first block of Styles.xml
        /// 
        /// <list> 
        /// </list>
        /// </summary>
        /// <param name="targetFile">content.xml</param>
        /// <returns> </returns>
        /// -------------------------------------------------------------------------------------------
        private void CreateODTStyles(string targetFile)
        {
            try
            {
                _styleFilePath = targetFile;
                _writer = new XmlTextWriter(targetFile, null);
                _writer.Formatting = Formatting.Indented;
                _writer.WriteStartDocument();

                //office:document-content Attributes.
                _writer.WriteStartElement("office:document-styles");
                _writer.WriteAttributeString("xmlns:office", "urn:oasis:names:tc:opendocument:xmlns:office:1.0");
                _writer.WriteAttributeString("xmlns:style", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
                _writer.WriteAttributeString("xmlns:text", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");
                _writer.WriteAttributeString("xmlns:table", "urn:oasis:names:tc:opendocument:xmlns:table:1.0");
                _writer.WriteAttributeString("xmlns:draw", "urn:oasis:names:tc:opendocument:xmlns:drawing:1.0");
                _writer.WriteAttributeString("xmlns:fo", "urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0");
                _writer.WriteAttributeString("xmlns:xlink", "http://www.w3.org/1999/xlink");
                _writer.WriteAttributeString("xmlns:dc", "http://purl.org/dc/elements/1.1/");
                _writer.WriteAttributeString("xmlns:meta", "urn:oasis:names:tc:opendocument:xmlns:meta:1.0");
                _writer.WriteAttributeString("xmlns:number", "urn:oasis:names:tc:opendocument:xmlns:datastyle:1.0");
                _writer.WriteAttributeString("xmlns:svg", "urn:oasis:names:tc:opendocument:xmlns:svg-compatible:1.0");
                _writer.WriteAttributeString("xmlns:chart", "urn:oasis:names:tc:opendocument:xmlns:chart:1.0");
                _writer.WriteAttributeString("xmlns:dr3d", "urn:oasis:names:tc:opendocument:xmlns:dr3d:1.0");
                _writer.WriteAttributeString("xmlns:math", "http://www.w3.org/1998/Math/MathML");
                _writer.WriteAttributeString("xmlns:form", "urn:oasis:names:tc:opendocument:xmlns:form:1.0");
                _writer.WriteAttributeString("xmlns:script", "urn:oasis:names:tc:opendocument:xmlns:script:1.0");
                _writer.WriteAttributeString("xmlns:ooo", "http://openoffice.org/2004/office");
                _writer.WriteAttributeString("xmlns:ooow", "http://openoffice.org/2004/writer");
                _writer.WriteAttributeString("xmlns:oooc", "http://openoffice.org/2004/calc");
                _writer.WriteAttributeString("xmlns:dom", "http://www.w3.org/2001/xml-events");
                _writer.WriteAttributeString("xmlns:rpt", "http://openoffice.org/2005/report");
                _writer.WriteAttributeString("xmlns:of", "urn:oasis:names:tc:opendocument:xmlns:of:1.2");
                _writer.WriteAttributeString("xmlns:xhtml", "http://www.w3.org/1999/xhtml");
                _writer.WriteAttributeString("xmlns:grddl", "http://www.w3.org/2003/g/data-view#");
                _writer.WriteAttributeString("office:version", "1.2");
                _writer.WriteAttributeString("grddl:transformation", "http://docs.oasis-open.org/office/1.2/xslt/odf2rdf.xsl");

                //office:font-face-decls Attributes.
                _writer.WriteStartElement("office:font-face-decls");
                _writer.WriteStartElement("style:font-face");
                _writer.WriteAttributeString("style:name", "Times New Roman");
                _writer.WriteAttributeString("svg:font-family", "'Times New Roman'");
                _writer.WriteAttributeString("style:font-family-generic", "roman");
                _writer.WriteAttributeString("style:font-pitch", "variable");
                _writer.WriteEndElement();
                _writer.WriteStartElement("style:font-face");
                 _writer.WriteAttributeString("style:name", "Yi plus Phonetics");
                _writer.WriteAttributeString("svg:font-family", "'Yi plus Phonetics'");
                _writer.WriteEndElement();
                _writer.WriteStartElement("style:font-face");
                _writer.WriteAttributeString("style:name", "Arial");
                _writer.WriteAttributeString("svg:font-family", "'Arial'");
                _writer.WriteAttributeString("style:font-family-generic", "swiss");
                _writer.WriteAttributeString("style:font-pitch", "variable");
                _writer.WriteEndElement();
                _writer.WriteStartElement("style:font-face");
                _writer.WriteAttributeString("style:name", "Lucida Sans Unicode");
                _writer.WriteAttributeString("svg:font-family", "'Lucida Sans Unicode'");
                _writer.WriteAttributeString("style:font-family-generic", "system");
                _writer.WriteAttributeString("style:font-pitch", "variable");
                _writer.WriteEndElement();
                _writer.WriteStartElement("style:font-face");
                _writer.WriteAttributeString("style:name", "MS Mincho");
                _writer.WriteAttributeString("svg:font-family", "'MS Mincho'");
                _writer.WriteAttributeString("style:font-family-generic", "system");
                _writer.WriteAttributeString("style:font-pitch", "variable");
                _writer.WriteEndElement();
                _writer.WriteStartElement("style:font-face");
                _writer.WriteAttributeString("style:name", "Tahoma");
                _writer.WriteAttributeString("svg:font-family", "'Tahoma'");
                _writer.WriteAttributeString("style:font-family-generic", "system");
                _writer.WriteAttributeString("style:font-pitch", "variable");
                _writer.WriteEndElement();
                _writer.WriteStartElement("style:font-face");
                _writer.WriteAttributeString("style:name", "Scheherazade Graphite Alpha");
                _writer.WriteAttributeString("svg:font-family", "'Scheherazade Graphite Alpha'");
                _writer.WriteEndElement();
                _writer.WriteEndElement();

                //office:styles Attributes.
                _writer.WriteStartElement("office:styles");

                // for Empty Class
                _writer.WriteStartElement("style:style");
                _writer.WriteAttributeString("style:name", "Empty");
                _writer.WriteAttributeString("style:family", "text");
                _writer.WriteAttributeString("style:parent-style-name", "none");
                _writer.WriteEndElement();

                _writer.WriteStartElement("style:style");
                _writer.WriteAttributeString("style:name", "hide");
                _writer.WriteAttributeString("style:family", "paragraph");
                _writer.WriteAttributeString("style:parent-style-name", "Heading_20_9");
                _writer.WriteAttributeString("style:class", "text");
                _writer.WriteStartElement("style:text-properties");
                _writer.WriteAttributeString("text:display", "true");
                _writer.WriteEndElement();
                _writer.WriteEndElement();

                _writer.WriteStartElement("style:style");
                _writer.WriteAttributeString("style:name", "GuideL");
                _writer.WriteAttributeString("style:family", "paragraph");
                _writer.WriteAttributeString("style:parent-style-name", "Heading_20_9");
                _writer.WriteAttributeString("style:default-outline-level", "9");
                _writer.WriteStartElement("style:text-properties");
                _writer.WriteAttributeString("text:display", "true");
                _writer.WriteEndElement();
                _writer.WriteEndElement();

                _writer.WriteStartElement("style:style");
                _writer.WriteAttributeString("style:name", "GuideR");
                _writer.WriteAttributeString("style:family", "paragraph");
                _writer.WriteAttributeString("style:parent-style-name", "Heading_20_10");
                _writer.WriteAttributeString("style:default-outline-level", "10");
                _writer.WriteStartElement("style:text-properties");
                _writer.WriteAttributeString("text:display", "true");
                _writer.WriteEndElement();
                _writer.WriteEndElement();

                _writer.WriteStartElement("style:style");
                _writer.WriteAttributeString("style:name", "Header");
                _writer.WriteAttributeString("style:family", "paragraph");
                _writer.WriteAttributeString("style:parent-style-name", "Standard");
                _writer.WriteAttributeString("style:class", "extra");
                _writer.WriteEndElement();
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
        }

        /// -------------------------------------------------------------------------------------------
        /// <summary>
        /// To fill the haeder and footer values based on CSS input.
        /// 
        /// <list> 
        /// </list>
        /// </summary>
        /// <param name="contentValue">Value of the content from dictionary</param>
        /// <param name="index">Dictionary Index</param>
        /// <returns> </returns>
        /// -------------------------------------------------------------------------------------------
        private void FillHeaderFooter(string contentValue, byte index)
        {
            try
            {
                if (_pageHeaderFooter[index].Count > 0)
                {
                    if (contentValue.IndexOf("first,)") > 0 && contentValue.IndexOf("last,)") > 0)
                    {
                        _styleName.IsMacroEnable = true;
                        _writer.WriteStartElement("text:chapter");
                        _writer.WriteAttributeString("text:display", "name");
                        _writer.WriteAttributeString("text:outline-level", "9");
                        _writer.WriteEndElement();
                        _writer.WriteStartElement("text:chapter");
                        _writer.WriteAttributeString("text:display", "name");
                        _writer.WriteAttributeString("text:outline-level", "10");
                        _writer.WriteEndElement();
                    }
                    else if (contentValue.IndexOf("first,)") > 0)
                    {
                        _styleName.IsMacroEnable = true;
                        _writer.WriteStartElement("text:chapter");
                        _writer.WriteAttributeString("text:display", "name");
                        _writer.WriteAttributeString("text:outline-level", "9");
                        _writer.WriteEndElement();
                    }
                    else if (contentValue.IndexOf("last,)") > 0)
                    {
                        _styleName.IsMacroEnable = true;
                        _writer.WriteStartElement("text:chapter");
                        _writer.WriteAttributeString("text:display", "name");
                        _writer.WriteAttributeString("text:outline-level", "10");
                        _writer.WriteEndElement();
                    }
                    else if (contentValue.IndexOf("page,)") > 0)
                    {
                        _writer.WriteStartElement("text:page-number");
                        _writer.WriteAttributeString("text:select-page", "current");
                        _writer.WriteString("4");
                        _writer.WriteEndElement();
                    }
                    else if (contentValue.IndexOf("start,)") > 0)
                    {
                        _styleName.IsMacroEnable = true;
                        _writer.WriteStartElement("text:chapter");
                        _writer.WriteAttributeString("text:display", "name");
                        _writer.WriteAttributeString("text:outline-level", "9");
                        _writer.WriteEndElement();
                    }
                    else
                    {
                        _writer.WriteString(_pageHeaderFooter[index]["content"]);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        private bool IsContentAvailable(byte startIndex)
        {
            try
            {
                for (int i = startIndex; i < startIndex + 3; i++)
                {
                    if (_pageHeaderFooter[i].ContainsKey("content"))
                    {
                        //if (_pageHeaderFooter[childCount]["content"].IndexOf("page,)") > 0 || _pageHeaderFooter[childCount]["content"].IndexOf("first,)") > 0 || _pageHeaderFooter[childCount]["content"].IndexOf("last,)") > 0)
                        //{
                        return true;
                        //}
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                return false;
            }
        }

        /// -------------------------------------------------------------------------------------------
        /// <summary>
        /// Add Tag in styles.xml, if missing in CSS
        /// 
        /// </summary>
        /// <param name="tagName">TagName to add</param>
        /// <returns> null </returns>
        /// -------------------------------------------------------------------------------------------
        private void AddTagStyle()
        {
            foreach (string tagName in _baseTagName)
            {
                switch (tagName)
                {
                    case "h1":
                        _writer.WriteStartElement("style:style");
                        _writer.WriteAttributeString("style:name", "h1");
                        _writer.WriteAttributeString("style:family", "paragraph");
                        _writer.WriteAttributeString("style:parent-style-name", "none");
                        _writer.WriteStartElement("style:paragraph-properties");
                        //_writer.WriteAttributeString("style:line-spacing", "12pt");
                        _writer.WriteAttributeString("fo:margin-top", "0.1in");
                        _writer.WriteAttributeString("fo:margin-bottom", "0.1in");
                        _writer.WriteAttributeString("style:line-spacing", "100%");
                        _writer.WriteEndElement();
                        _writer.WriteStartElement("style:text-properties");
                        _writer.WriteAttributeString("fo:font-weight", "700");
                        _writer.WriteAttributeString("fo:font-size", "24pt");
                        _writer.WriteEndElement();
                        _writer.WriteEndElement();
                        break;
                    case "h2":
                        _writer.WriteStartElement("style:style");
                        _writer.WriteAttributeString("style:name", "h2");
                        _writer.WriteAttributeString("style:family", "paragraph");
                        _writer.WriteAttributeString("style:parent-style-name", "none");
                        _writer.WriteStartElement("style:paragraph-properties");
                        _writer.WriteAttributeString("fo:margin-top", "0.1in");
                        _writer.WriteAttributeString("fo:margin-bottom", "0.1in");
                        _writer.WriteAttributeString("style:line-spacing", "100%");
                        _writer.WriteEndElement();
                        _writer.WriteStartElement("style:text-properties");
                        _writer.WriteAttributeString("fo:font-weight", "700");
                        _writer.WriteAttributeString("fo:font-size", "18pt");
                        _writer.WriteEndElement();
                        _writer.WriteEndElement();
                        break;
                    case "h3":
                        _writer.WriteStartElement("style:style");
                        _writer.WriteAttributeString("style:name", "h3");
                        _writer.WriteAttributeString("style:family", "paragraph");
                        _writer.WriteAttributeString("style:parent-style-name", "none");
                        _writer.WriteStartElement("style:paragraph-properties");
                        _writer.WriteAttributeString("fo:margin-top", "0.1in");
                        _writer.WriteAttributeString("fo:margin-bottom", "0.1in");
                        _writer.WriteAttributeString("style:line-spacing", "100%");
                        _writer.WriteEndElement();
                        _writer.WriteStartElement("style:text-properties");
                        _writer.WriteAttributeString("fo:font-weight", "700");
                        _writer.WriteAttributeString("fo:font-size", "14pt");
                        _writer.WriteEndElement();
                        _writer.WriteEndElement();
                        break;
                    case "h4":
                        _writer.WriteStartElement("style:style");
                        _writer.WriteAttributeString("style:name", "h4");
                        _writer.WriteAttributeString("style:family", "paragraph");
                        _writer.WriteAttributeString("style:parent-style-name", "none");
                        _writer.WriteStartElement("style:paragraph-properties");
                        _writer.WriteAttributeString("fo:margin-top", "0.1in");
                        _writer.WriteAttributeString("fo:margin-bottom", "0.1in");
                        _writer.WriteAttributeString("style:line-spacing", "100%");
                        _writer.WriteEndElement();
                        _writer.WriteStartElement("style:text-properties");
                        _writer.WriteAttributeString("fo:font-weight", "700");
                        _writer.WriteAttributeString("fo:font-size", "12pt");
                        _writer.WriteEndElement();
                        _writer.WriteEndElement();
                        break;
                    case "h5":
                        _writer.WriteStartElement("style:style");
                        _writer.WriteAttributeString("style:name", "h5");
                        _writer.WriteAttributeString("style:family", "paragraph");
                        _writer.WriteAttributeString("style:parent-style-name", "none");
                        _writer.WriteStartElement("style:paragraph-properties");
                        _writer.WriteAttributeString("fo:margin-top", "0.1in");
                        _writer.WriteAttributeString("fo:margin-bottom", "0.1in");
                        _writer.WriteAttributeString("style:line-spacing", "100%");
                        _writer.WriteEndElement();
                        _writer.WriteStartElement("style:text-properties");
                        _writer.WriteAttributeString("fo:font-weight", "700");
                        _writer.WriteAttributeString("fo:font-size", "10pt");
                        _writer.WriteEndElement();
                        _writer.WriteEndElement();
                        break;
                    case "h6":
                        _writer.WriteStartElement("style:style");
                        _writer.WriteAttributeString("style:name", "h6");
                        _writer.WriteAttributeString("style:family", "paragraph");
                        _writer.WriteAttributeString("style:parent-style-name", "none");
                        _writer.WriteStartElement("style:paragraph-properties");
                        _writer.WriteAttributeString("fo:margin-top", "0.1in");
                        _writer.WriteAttributeString("fo:margin-bottom", "0.1in");
                        _writer.WriteAttributeString("style:line-spacing", "100%");
                        _writer.WriteEndElement();
                        _writer.WriteStartElement("style:text-properties");
                        _writer.WriteAttributeString("fo:font-weight", "700");
                        _writer.WriteAttributeString("fo:font-size", "8pt");
                        _writer.WriteEndElement();
                        _writer.WriteEndElement();
                        break;
                    case "ol":
                        _writer.WriteStartElement("text:list-style");
                        _writer.WriteAttributeString("style:name", "ol");
                        _writer.WriteStartElement("text:list-level-style-number");
                        _writer.WriteAttributeString("text:level", "1");
                        _writer.WriteAttributeString("text:style-name", "Numbering_20_Symbols");
                        _writer.WriteAttributeString("style:num-suffix", ".");
                        _writer.WriteAttributeString("style:num-format", "1");
                        _writer.WriteStartElement("style:list-level-properties");
                        _writer.WriteAttributeString("text:list-level-position-and-space-mode", "label-alignment");
                        _writer.WriteStartElement("style:list-level-label-alignment");
                        _writer.WriteAttributeString("text:label-followed-by", "listtab");
                        _writer.WriteAttributeString("text:list-tab-stop-position", "0.5in");
                        _writer.WriteAttributeString("fo:text-indent", "-0.25in");
                        _writer.WriteAttributeString("fo:margin-left", "0.5in");
                        _writer.WriteEndElement();
                        _writer.WriteEndElement();
                        _writer.WriteEndElement();
                        _writer.WriteEndElement();
                        break;
                    case "ul":
                        _writer.WriteStartElement("text:list-style");
                        _writer.WriteAttributeString("style:name", "ul");
                        _writer.WriteStartElement("text:list-level-style-bullet");
                        _writer.WriteAttributeString("text:level", "1");
                        _writer.WriteAttributeString("text:style-name", "Bullet_20_Symbols");
                        _writer.WriteAttributeString("style:num-suffix", ".");
                        _writer.WriteAttributeString("text:bullet-char", "•");
                        _writer.WriteStartElement("style:list-level-properties");
                        _writer.WriteAttributeString("text:list-level-position-and-space-mode", "label-alignment");
                        _writer.WriteStartElement("style:list-level-label-alignment");
                        _writer.WriteAttributeString("text:label-followed-by", "listtab");
                        _writer.WriteAttributeString("text:list-tab-stop-position", "0.5in");
                        _writer.WriteAttributeString("fo:text-indent", "-0.25in");
                        _writer.WriteAttributeString("fo:margin-left", "0.5in");
                        _writer.WriteEndElement();
                        _writer.WriteEndElement();
                        _writer.WriteEndElement();
                        _writer.WriteEndElement();

                        _writer.WriteStartElement("text:list-style");
                        _writer.WriteAttributeString("style:name", "ul");
                        _writer.WriteStartElement("text:list-level-style-bullet");
                        _writer.WriteAttributeString("text:level", "2");
                        _writer.WriteAttributeString("text:style-name", "Bullet_20_Symbols");
                        _writer.WriteAttributeString("style:num-suffix", ".");
                        _writer.WriteAttributeString("text:bullet-char", "•");
                        _writer.WriteStartElement("style:list-level-properties");
                        _writer.WriteAttributeString("text:list-level-position-and-space-mode", "label-alignment");
                        _writer.WriteStartElement("style:list-level-label-alignment");
                        _writer.WriteAttributeString("text:label-followed-by", "listtab");
                        _writer.WriteAttributeString("text:list-tab-stop-position", "0.5in");
                        _writer.WriteAttributeString("fo:text-indent", "-0.25in");
                        _writer.WriteAttributeString("fo:margin-left", "1.905cm");
                        _writer.WriteEndElement();
                        _writer.WriteEndElement();
                        _writer.WriteEndElement();
                        _writer.WriteEndElement();
                        break;
                    case "li":
                        _writer.WriteStartElement("style:style");
                        _writer.WriteAttributeString("style:name", tagName);
                        _writer.WriteAttributeString("style:family", "paragraph");
                        _writer.WriteAttributeString("style:parent-style-name", "none");
                        _writer.WriteAttributeString("style:list-style-name", "ol");
                        _writer.WriteStartElement("style:paragraph-properties");
                        //_writer.WriteAttributeString("fo:margin-left", "2pt");
                        _writer.WriteEndElement();
                        _writer.WriteEndElement();
                        break;
                    case "p":
                        _writer.WriteStartElement("style:style");
                        _writer.WriteAttributeString("style:name", "p");
                        _writer.WriteAttributeString("style:family", "paragraph");
                        _writer.WriteAttributeString("style:parent-style-name", "none");
                        _writer.WriteStartElement("style:paragraph-properties");
                        //_writer.WriteAttributeString("fo:margin-top", "0.1598in");
                        //_writer.WriteAttributeString("fo:margin-bottom", "0.1598in");
                        _writer.WriteAttributeString("fo:margin-top", "0.1in");
                        _writer.WriteAttributeString("fo:margin-bottom", "0.1in");
                        _writer.WriteAttributeString("style:line-spacing", "100%");
                        _writer.WriteEndElement();
                        _writer.WriteEndElement();
                        break;
                    case "a":  // Anchor Tag
                        _writer.WriteStartElement("style:style");
                        _writer.WriteAttributeString("style:name", "a");
                        _writer.WriteAttributeString("style:family", "text");
                        _writer.WriteAttributeString("style:parent-style-name", "none");
                        _writer.WriteStartElement("style:text-properties");
                        _writer.WriteAttributeString("style:text-underline-style", "solid"); // underline
                        _writer.WriteAttributeString("fo:color", "#0000ff"); // color blue
                        _writer.WriteEndElement();
                        _writer.WriteEndElement();
                        break;
                }
            }
            _baseTagName.Clear();
        }

        /// -------------------------------------------------------------------------------------------
        /// <summary>
        /// Generate last block of Styles.xml
        /// 
        /// <list> 
        /// </list>
        /// </summary>
        /// <returns> </returns>
        /// -------------------------------------------------------------------------------------------
        private void CloseODTStyles()
        {
            try
            {
                //All PageProperty information is assinged to First PageProperty, when First PageProperty is not found
                for (int i = 0; i <= 5; i++)
                {
                    if (_firstPageContentNone.Contains(i))
                    {
                        continue; // no need of copy. for content : normal or none;
                    }

                    if (_pageHeaderFooter[i].Count == 0) // Only copy @page values when equivalent element is empty in @PageProperty:first
                    {
                        _pageHeaderFooter[i] = _pageHeaderFooter[i + 6];
                    }
                }

                // "graphic"
                _writer.WriteStartElement("style:default-style");
                _writer.WriteAttributeString("style:family", "graphic");
                _writer.WriteStartElement("style:graphic-properties");
                _writer.WriteAttributeString("draw:shadow-offset-x", "0.1181in");
                _writer.WriteAttributeString("draw:shadow-offset-y", "0.1181in");
                _writer.WriteAttributeString("draw:start-line-spacing-horizontal", "0.1114in");
                _writer.WriteAttributeString("draw:start-line-spacing-vertical", "0.1114in");
                _writer.WriteAttributeString("draw:end-line-spacing-horizontal", "0.1114in");
                _writer.WriteAttributeString("draw:end-line-spacing-vertical", "0.1114in");
                //_writer.WriteAttributeString("style:flow-with-text", "false");
                _writer.WriteAttributeString("style:flow-with-text", "true");
                _writer.WriteEndElement();

                _writer.WriteStartElement("style:paragraph-properties");
                _writer.WriteAttributeString("style:text-autospace", "ideograph-alpha");
                _writer.WriteAttributeString("style:line-break", "strict");
                _writer.WriteAttributeString("style:writing-mode", "lr-tb");
                _writer.WriteAttributeString("style:font-independent-line-spacing", "false");

                _writer.WriteStartElement("style:tab-stops");
                _writer.WriteEndElement();
                _writer.WriteEndElement();

                _writer.WriteStartElement("style:text-properties");
                _writer.WriteAttributeString("style:use-window-font-color", "true");
                _writer.WriteAttributeString("fo:font-size", "12pt");
                //_writer.WriteAttributeString("fo:language", "en");
                //_writer.WriteAttributeString("fo:country", "US");
                _writer.WriteAttributeString("fo:language", "none");
                _writer.WriteAttributeString("fo:country", "none");
                _writer.WriteAttributeString("style:letter-kerning", "true");
                _writer.WriteAttributeString("style:font-size-asian", "12pt");
                _writer.WriteAttributeString("style:language-asian", "zxx");
                _writer.WriteAttributeString("style:country-asian", "none");
                _writer.WriteAttributeString("style:font-size-complex", "12pt");
                _writer.WriteAttributeString("style:language-complex", "zxx");
                _writer.WriteAttributeString("style:country-complex", "none");
                _writer.WriteEndElement();
                _writer.WriteEndElement();

                //office:styles Attributes.
                //// "paragraph"
                _writer.WriteStartElement("style:default-style");
                _writer.WriteAttributeString("style:family", "paragraph");
                _writer.WriteStartElement("style:paragraph-properties");
                _writer.WriteAttributeString("fo:hyphenation-ladder-count", "no-limit");
                _writer.WriteAttributeString("style:text-autospace", "ideograph-alpha");
                _writer.WriteAttributeString("style:punctuation-wrap", "hanging");
                _writer.WriteAttributeString("style:line-break", "strict");
                _writer.WriteAttributeString("style:tab-stop-distance", "0.4925in");
                _writer.WriteAttributeString("style:writing-mode", "page");
                _writer.WriteEndElement();

                _writer.WriteStartElement("style:text-properties");
                _writer.WriteAttributeString("style:use-window-font-color", "true");
                _writer.WriteAttributeString("style:font-name", "Times New Roman");
                _writer.WriteAttributeString("fo:font-size", "12pt");
                //_writer.WriteAttributeString("fo:language", "en");
                //_writer.WriteAttributeString("fo:country", "US");
                _writer.WriteAttributeString("fo:language", "none");
                _writer.WriteAttributeString("fo:country", "none");
                _writer.WriteAttributeString("style:letter-kerning", "true");
                _writer.WriteAttributeString("style:font-name-asian", "Yi plus Phonetics");
                _writer.WriteAttributeString("style:font-size-asian", "12pt");
                _writer.WriteAttributeString("style:language-asian", "none");
                _writer.WriteAttributeString("style:country-asian", "none");
                _writer.WriteAttributeString("style:font-name-complex", "Tahoma");
                _writer.WriteAttributeString("style:font-size-complex", "12pt");
                _writer.WriteAttributeString("style:language-complex", "zxx");
                _writer.WriteAttributeString("style:country-complex", "none");
                _writer.WriteAttributeString("fo:hyphenate", "false");
                _writer.WriteAttributeString("fo:hyphenation-remain-char-count", "2");
                _writer.WriteAttributeString("fo:hyphenation-push-char-count", "2");
                _writer.WriteEndElement();
                _writer.WriteEndElement();

                //office:styles Attributes.
                //// "table"
                _writer.WriteStartElement("style:default-style");
                _writer.WriteAttributeString("style:family", "table");
                _writer.WriteStartElement("style:table-properties");
                _writer.WriteAttributeString("table:border-model", "collapsing");
                _writer.WriteEndElement();
                _writer.WriteEndElement();

                //office:styles Attributes.
                //// "table-row"
                _writer.WriteStartElement("style:default-style");
                _writer.WriteAttributeString("style:family", "table-row");
                _writer.WriteStartElement("style:table-row-properties");
                _writer.WriteAttributeString("fo:keep-together", "auto");
                _writer.WriteEndElement();
                _writer.WriteEndElement();

                //office:styles Attributes.
                //// "Standard"
                _writer.WriteStartElement("style:style");
                _writer.WriteAttributeString("style:name", "Standard");
                _writer.WriteAttributeString("style:family", "paragraph");
                _writer.WriteAttributeString("style:class", "text");
                _writer.WriteEndElement();

                //office:styles Attributes.
                //// "Header"
                _writer.WriteStartElement("style:style");
                _writer.WriteAttributeString("style:name", "Header");
                _writer.WriteAttributeString("style:family", "paragraph");
                _writer.WriteAttributeString("style:parent-style-name", "Standard");
                _writer.WriteAttributeString("style:next-style-name", "Text_20_body");
                _writer.WriteAttributeString("style:class", "text");

                _writer.WriteStartElement("style:paragraph-properties");//style:paragraph-properties
                _writer.WriteAttributeString("fo:margin-top", "0.0005in");
                _writer.WriteAttributeString("fo:margin-bottom", "0.0835in");
                _writer.WriteAttributeString("fo:keep-with-next", "always");

                InsertHeaderRule();

                _writer.WriteAttributeString("text:number-lines", "false");
                _writer.WriteAttributeString("text:line-number", "0");

                _writer.WriteStartElement("style:tab-stops");//style:tab-stops
                _writer.WriteStartElement("style:tab-stop");//style:tab-stop

                // Finds the PageProperty Header's center.(GuideWord or PageNo)
                float borderLeft;
                if (_pageLayoutProperty.ContainsKey("fo:border-left"))
                {
                    string[] parameters = _pageLayoutProperty["fo:border-left"].Split(' ');
                    string pageBorderLeft = "0pt";
                    foreach (string param in parameters)
                    {
                        if (Common.ValidateNumber(param[0].ToString() + "1"))
                        {
                            pageBorderLeft = param;
                            break;
                        }
                    }
                    borderLeft = Common.ConvertToInch(pageBorderLeft);
                }
                else
                {
                    borderLeft = 0F;
                }

                float width = Common.ConvertToInch(_pageLayoutProperty["fo:page-width"]);
                float left = Common.ConvertToInch(_pageLayoutProperty["fo:margin-left"]);
                float right = Common.ConvertToInch(_pageLayoutProperty["fo:margin-left"]);
                float mid = width / 2F - left - borderLeft;
                float rightGuide = (width - left - right);

                _writer.WriteAttributeString("style:position", mid.ToString() + "in");

                _writer.WriteAttributeString("style:type", "center");
                _writer.WriteEndElement();

                _writer.WriteStartElement("style:tab-stop");//style:tab-stop
                _writer.WriteAttributeString("style:position", rightGuide.ToString() + "in");
                _writer.WriteAttributeString("style:type", "right");
                _writer.WriteEndElement();

                _writer.WriteEndElement();//style:tab-stops
                _writer.WriteEndElement();//style:paragraph-properties
                _writer.WriteEndElement();


                //// "Footer"
                _writer.WriteStartElement("style:style");
                _writer.WriteAttributeString("style:name", "Footer");
                _writer.WriteAttributeString("style:family", "paragraph");
                _writer.WriteAttributeString("style:parent-style-name", "Standard");
                _writer.WriteAttributeString("style:next-style-name", "Text_20_body");
                _writer.WriteAttributeString("style:class", "text");

                _writer.WriteStartElement("style:paragraph-properties");//style:paragraph-properties
                _writer.WriteAttributeString("fo:margin-top", "0.0005in");
                _writer.WriteAttributeString("fo:margin-bottom", "0.0835in");
                _writer.WriteAttributeString("fo:keep-with-next", "always");

                _writer.WriteAttributeString("text:number-lines", "false");
                _writer.WriteAttributeString("text:line-number", "0");

                _writer.WriteStartElement("style:tab-stops");//style:tab-stops

                _writer.WriteStartElement("style:tab-stop");//style:tab-stop
                _writer.WriteAttributeString("style:position", mid.ToString() + "in");
                _writer.WriteAttributeString("style:type", "center");
                _writer.WriteEndElement();

                _writer.WriteStartElement("style:tab-stop");//style:tab-stop
                _writer.WriteAttributeString("style:position", rightGuide.ToString() + "in");
                _writer.WriteAttributeString("style:type", "right");
                _writer.WriteEndElement();

                _writer.WriteEndElement();//style:tab-stops
                _writer.WriteEndElement();//style:paragraph-properties
                _writer.WriteEndElement();//Footer style

                //office:styles Attributes.
                //// "Text_20_body"
                _writer.WriteStartElement("style:style");
                _writer.WriteAttributeString("style:name", "Text_20_body");
                _writer.WriteAttributeString("style:display-name", "Text body");
                _writer.WriteAttributeString("style:family", "paragraph");
                _writer.WriteAttributeString("style:parent-style-name", "Standard");
                _writer.WriteAttributeString("style:class", "text");
                _writer.WriteStartElement("style:paragraph-properties");
                _writer.WriteAttributeString("fo:margin-top", "0in");
                _writer.WriteAttributeString("fo:margin-bottom", "0.0835in");
                _writer.WriteEndElement();
                _writer.WriteEndElement();

                //office:styles Attributes.
                //// "List"
                _writer.WriteStartElement("style:style");
                _writer.WriteAttributeString("style:name", "List");
                _writer.WriteAttributeString("style:family", "paragraph");
                _writer.WriteAttributeString("style:parent-style-name", "Text_20_body");
                _writer.WriteAttributeString("style:class", "list");
                _writer.WriteStartElement("style:text-properties");
                _writer.WriteAttributeString("style:font-name-asian", "Yi plus Phonetics");
                _writer.WriteAttributeString("style:font-name-complex", "Tahoma1");
                _writer.WriteEndElement();
                _writer.WriteEndElement();

                //office:styles Attributes.
                //// "Caption"
                _writer.WriteStartElement("style:style");
                _writer.WriteAttributeString("style:name", "Caption");
                _writer.WriteAttributeString("style:family", "paragraph");
                _writer.WriteAttributeString("style:parent-style-name", "Standard");
                _writer.WriteAttributeString("style:class", "extra");
                _writer.WriteStartElement("style:paragraph-properties");
                _writer.WriteAttributeString("fo:margin-top", "0.0835in");
                _writer.WriteAttributeString("fo:margin-bottom", "0.0835in");
                _writer.WriteAttributeString("text:number-lines", "false");
                _writer.WriteAttributeString("text:line-number", "0");
                _writer.WriteEndElement();
                _writer.WriteStartElement("style:text-properties");
                _writer.WriteAttributeString("fo:font-size", "12pt");
                _writer.WriteAttributeString("fo:font-style", "italic");
                _writer.WriteAttributeString("style:font-name-asian", "Yi plus Phonetics");
                _writer.WriteAttributeString("style:font-size-asian", "12pt");
                _writer.WriteAttributeString("style:font-style-asian", "italic");
                _writer.WriteAttributeString("style:font-name-complex", "Tahoma1");
                _writer.WriteAttributeString("style:font-size-complex", "12pt");
                _writer.WriteAttributeString("style:font-style-complex", "italic");
                _writer.WriteEndElement();
                _writer.WriteEndElement();

                //office:styles Attributes.
                //// "index"
                _writer.WriteStartElement("style:style");
                _writer.WriteAttributeString("style:name", "Index");
                _writer.WriteAttributeString("style:family", "paragraph");
                _writer.WriteAttributeString("style:parent-style-name", "Standard");
                _writer.WriteAttributeString("style:class", "index");
                _writer.WriteStartElement("style:paragraph-properties");
                _writer.WriteAttributeString("text:number-lines", "false");
                _writer.WriteAttributeString("text:line-number", "0");
                _writer.WriteEndElement();
                _writer.WriteStartElement("style:text-properties");
                _writer.WriteAttributeString("style:font-name-asian", "Yi plus Phonetics");
                _writer.WriteAttributeString("style:font-name-complex", "Tahoma1");
                _writer.WriteEndElement();
                _writer.WriteEndElement();

                _writer.WriteStartElement("style:style");
                _writer.WriteAttributeString("style:name", "Graphics");
                _writer.WriteAttributeString("style:family", "graphic");
                _writer.WriteStartElement("style:graphic-properties");
                _writer.WriteAttributeString("text:anchor-type", "paragraph");
                _writer.WriteAttributeString("svg:x", "0in");
                _writer.WriteAttributeString("svg:y", "0in");
                if (!isMirrored)
                {
                    _writer.WriteAttributeString("style:mirror", "none");
                }
                _writer.WriteAttributeString("fo:clip", "rect(0in 0in 0in 0in)");
                _writer.WriteAttributeString("draw:luminance", "0%");
                _writer.WriteAttributeString("draw:contrast", "0%");
                _writer.WriteAttributeString("draw:red", "0%");
                _writer.WriteAttributeString("draw:green", "0%");
                _writer.WriteAttributeString("draw:blue", "0%");
                _writer.WriteAttributeString("draw:gamma", "100%");
                _writer.WriteAttributeString("draw:color-inversion", "false");
                _writer.WriteAttributeString("draw:image-opacity", "100%");
                _writer.WriteAttributeString("draw:color-mode", "standard");
                _writer.WriteAttributeString("style:wrap", "none");
                _writer.WriteAttributeString("style:vertical-pos", "top");
                _writer.WriteAttributeString("style:vertical-rel", "paragraph");
                _writer.WriteAttributeString("style:horizontal-pos", "center");
                _writer.WriteAttributeString("style:horizontal-rel", "paragraph");
                _writer.WriteEndElement();
                _writer.WriteEndElement();

                //office:styles Attributes.
                //// fullString:outline-style
                _writer.WriteStartElement("text:outline-style");
                _writer.WriteStartElement("text:outline-level-style");
                _writer.WriteAttributeString("text:level", "1");
                _writer.WriteAttributeString("style:num-format", "");
                _writer.WriteStartElement("style:list-level-properties");
                _writer.WriteAttributeString("text:min-label-distance", "0.15in");
                _writer.WriteEndElement();
                _writer.WriteEndElement();
                _writer.WriteStartElement("text:outline-level-style");
                _writer.WriteAttributeString("text:level", "2");
                _writer.WriteAttributeString("style:num-format", "");
                _writer.WriteStartElement("style:list-level-properties");
                _writer.WriteAttributeString("text:min-label-distance", "0.15in");
                _writer.WriteEndElement();
                _writer.WriteEndElement();
                _writer.WriteStartElement("text:outline-level-style");
                _writer.WriteAttributeString("text:level", "3");
                _writer.WriteAttributeString("style:num-format", "");
                _writer.WriteStartElement("style:list-level-properties");
                _writer.WriteAttributeString("text:min-label-distance", "0.15in");
                _writer.WriteEndElement();
                _writer.WriteEndElement();
                _writer.WriteStartElement("text:outline-level-style");
                _writer.WriteAttributeString("text:level", "4");
                _writer.WriteAttributeString("style:num-format", "");
                _writer.WriteStartElement("style:list-level-properties");
                _writer.WriteAttributeString("text:min-label-distance", "0.15in");
                _writer.WriteEndElement();
                _writer.WriteEndElement();
                _writer.WriteStartElement("text:outline-level-style");
                _writer.WriteAttributeString("text:level", "5");
                _writer.WriteAttributeString("style:num-format", "");
                _writer.WriteStartElement("style:list-level-properties");
                _writer.WriteAttributeString("text:min-label-distance", "0.15in");
                _writer.WriteEndElement();
                _writer.WriteEndElement();
                _writer.WriteStartElement("text:outline-level-style");
                _writer.WriteAttributeString("text:level", "6");
                _writer.WriteAttributeString("style:num-format", "");
                _writer.WriteStartElement("style:list-level-properties");
                _writer.WriteAttributeString("text:min-label-distance", "0.15in");
                _writer.WriteEndElement();
                _writer.WriteEndElement();
                _writer.WriteStartElement("text:outline-level-style");
                _writer.WriteAttributeString("text:level", "7");
                _writer.WriteAttributeString("style:num-format", "");
                _writer.WriteStartElement("style:list-level-properties");
                _writer.WriteAttributeString("text:min-label-distance", "0.15in");
                _writer.WriteEndElement();
                _writer.WriteEndElement();
                _writer.WriteStartElement("text:outline-level-style");
                _writer.WriteAttributeString("text:level", "8");
                _writer.WriteAttributeString("style:num-format", "");
                _writer.WriteStartElement("style:list-level-properties");
                _writer.WriteAttributeString("text:min-label-distance", "0.15in");
                _writer.WriteEndElement();
                _writer.WriteEndElement();
                _writer.WriteStartElement("text:outline-level-style");
                _writer.WriteAttributeString("text:level", "9");
                _writer.WriteAttributeString("style:num-format", "");
                _writer.WriteStartElement("style:list-level-properties");
                _writer.WriteAttributeString("text:min-label-distance", "0.15in");
                _writer.WriteEndElement();
                _writer.WriteEndElement();
                _writer.WriteStartElement("text:outline-level-style");
                _writer.WriteAttributeString("text:level", "10");
                _writer.WriteAttributeString("style:num-format", "");
                _writer.WriteStartElement("style:list-level-properties");
                _writer.WriteAttributeString("text:min-label-distance", "0.15in");
                _writer.WriteEndElement();
                _writer.WriteEndElement();
                _writer.WriteEndElement();

                //office:styles Attributes.
                //// fullString:notes-configuration footnote
                _writer.WriteStartElement("text:notes-configuration");
                _writer.WriteAttributeString("text:note-class", "footnote");
                _writer.WriteAttributeString("text:citation-style-name", "Footnote Symbol");
                _writer.WriteAttributeString("text:citation-body-style-name", "Footnote anchor");
                _writer.WriteAttributeString("style:num-format", "1");
                _writer.WriteAttributeString("text:start-value", "0");
                _writer.WriteAttributeString("text:footnotes-position", "page");
                _writer.WriteAttributeString("text:start-numbering-at", "document");
                _writer.WriteEndElement();

                //office:styles Attributes.
                //// fullString:notes-configuration endnote
                _writer.WriteStartElement("text:notes-configuration");
                _writer.WriteAttributeString("text:note-class", "endnote");
                _writer.WriteAttributeString("style:num-format", "i");
                _writer.WriteAttributeString("text:start-value", "0");
                _writer.WriteEndElement();

                //office:styles Attributes.
                //// fullString:linenumbering-configuration
                _writer.WriteStartElement("text:linenumbering-configuration");
                _writer.WriteAttributeString("text:number-lines", "false");
                _writer.WriteAttributeString("text:offset", "0.1965in");
                _writer.WriteAttributeString("style:num-format", "1");
                _writer.WriteAttributeString("text:number-position", "left");
                _writer.WriteAttributeString("text:increment", "5");
                _writer.WriteEndElement();
                _writer.WriteEndElement();

                ODTPageFooter(); // Creating Footer Information for OpenOffice Document.

                _writer.WriteStartElement("office:master-styles"); // pm1
                
                ///STANDARD CODE PART
                _writer.WriteStartElement("style:master-page");
                _writer.WriteAttributeString("style:name", "Standard");
                _writer.WriteAttributeString("style:page-layout-name", "pm1");
                _writer.WriteStartElement("style:header");
                _writer.WriteStartElement("text:p");
                _writer.WriteAttributeString("text:style-name", "Header");
                //Right page Contents
                if (_pageHeaderFooter[18].Count > 0 || _pageHeaderFooter[19].Count > 0 || _pageHeaderFooter[20].Count > 0)
                {
                    if (_pageHeaderFooter[18].ContainsKey("content"))
                    {
                        _writer.WriteStartElement("text:span");
                        _writer.WriteAttributeString("text:style-name", "AllHeaderPageLeft");
                        FillHeaderFooter(_pageHeaderFooter[18]["content"], 18);
                        _writer.WriteEndElement();
                    }
                    _writer.WriteStartElement("text:tab");
                    _writer.WriteEndElement();
                    if (_pageHeaderFooter[19].ContainsKey("content"))
                    {
                        _writer.WriteStartElement("text:span");
                        _writer.WriteAttributeString("text:style-name", "AllHeaderPageNumber");
                        FillHeaderFooter(_pageHeaderFooter[19]["content"], 19);
                        _writer.WriteEndElement();
                    }
                    _writer.WriteStartElement("text:tab");
                    _writer.WriteEndElement();
                    if (_pageHeaderFooter[20].ContainsKey("content"))
                    {
                        _writer.WriteStartElement("text:span");
                        _writer.WriteAttributeString("text:style-name", "AllHeaderPageRight");
                        FillHeaderFooter(_pageHeaderFooter[20]["content"], 20);
                        _writer.WriteEndElement();
                    }
                }
                else
                {
                    if (isMirrored)
                    {
                        //If no content in right, loads from allpage contents
                        SetAllPageHeader();
                    }
                }
                _writer.WriteEndElement(); // Close if p
                _writer.WriteEndElement(); // Close of header

                if (isMirrored)
                {
                    //header-left only created when page is set to mirrored
                    _writer.WriteStartElement("style:header-left");
                    _writer.WriteStartElement("text:p");
                    _writer.WriteAttributeString("text:style-name", "Header");
                    if (_pageHeaderFooter[12].Count > 0 || _pageHeaderFooter[13].Count > 0 || _pageHeaderFooter[14].Count > 0)
                    {
                        if (_pageHeaderFooter[12].ContainsKey("content"))
                        {
                            _writer.WriteStartElement("text:span");
                            _writer.WriteAttributeString("text:style-name", "AllHeaderPageLeft");
                            FillHeaderFooter(_pageHeaderFooter[12]["content"], 12);
                            _writer.WriteEndElement();
                        }
                        _writer.WriteStartElement("text:tab");
                        _writer.WriteEndElement();
                        if (_pageHeaderFooter[13].ContainsKey("content"))
                        {
                            _writer.WriteStartElement("text:span");
                            _writer.WriteAttributeString("text:style-name", "AllHeaderPageNumber");
                            FillHeaderFooter(_pageHeaderFooter[13]["content"], 13);
                            _writer.WriteEndElement();
                        }
                        _writer.WriteStartElement("text:tab");
                        _writer.WriteEndElement();
                        if (_pageHeaderFooter[14].ContainsKey("content"))
                        {
                            _writer.WriteStartElement("text:span");
                            _writer.WriteAttributeString("text:style-name", "AllHeaderPageRight");
                            FillHeaderFooter(_pageHeaderFooter[14]["content"], 14);
                            _writer.WriteEndElement();
                        }
                    }
                    else
                    {
                        SetAllPageHeader();
                    }
                    _writer.WriteEndElement(); // Close of p
                    _writer.WriteEndElement(); // Close of Header-left
                    }

                _writer.WriteStartElement("style:footer");
                _writer.WriteStartElement("text:p");
                _writer.WriteAttributeString("text:style-name", "Footer");
                if (_pageHeaderFooter[21].Count > 0 || _pageHeaderFooter[22].Count > 0 || _pageHeaderFooter[23].Count > 0)
                {
                    if (_pageHeaderFooter[21].ContainsKey("content"))
                    {
                        _writer.WriteStartElement("text:span");
                        _writer.WriteAttributeString("text:style-name", "AllFooterPageLeft");
                        FillHeaderFooter(_pageHeaderFooter[21]["content"], 21);
                        _writer.WriteEndElement();
                    }
                    _writer.WriteStartElement("text:tab");
                    _writer.WriteEndElement();
                    if (_pageHeaderFooter[22].ContainsKey("content"))
                    {
                        _writer.WriteStartElement("text:span");
                        _writer.WriteAttributeString("text:style-name", "AllFooterPageNumber");
                        FillHeaderFooter(_pageHeaderFooter[22]["content"], 22);
                        _writer.WriteEndElement();
                    }
                    _writer.WriteStartElement("text:tab");
                    _writer.WriteEndElement();
                    if (_pageHeaderFooter[23].ContainsKey("content"))
                    {
                        _writer.WriteStartElement("text:span");
                        _writer.WriteAttributeString("text:style-name", "AllFooterPageRight");
                        FillHeaderFooter(_pageHeaderFooter[23]["content"], 23);
                        _writer.WriteEndElement();
                    }
                }
                else
                {
                    if (isMirrored)
                {
                        //If no content in right, loads from allpage contents
                        SetAllPageFooter();
                    }
                }
                _writer.WriteEndElement(); // Close of p
                _writer.WriteEndElement(); // Close if footer

                    _writer.WriteStartElement("style:footer-left");
                    _writer.WriteStartElement("text:p");
                    _writer.WriteAttributeString("text:style-name", "Footer");
                if (_pageHeaderFooter[15].Count > 0 || _pageHeaderFooter[15].Count > 0 || _pageHeaderFooter[17].Count > 0)
                {
                    if (_pageHeaderFooter[15].ContainsKey("content"))
                    {
                        _writer.WriteStartElement("text:span");
                        _writer.WriteAttributeString("text:style-name", "AllFooterPageLeft");
                        FillHeaderFooter(_pageHeaderFooter[15]["content"], 15);
                        _writer.WriteEndElement();
                    }
                    _writer.WriteStartElement("text:tab");
                    _writer.WriteEndElement();
                    if (_pageHeaderFooter[16].ContainsKey("content"))
                    {
                        _writer.WriteStartElement("text:span");
                        _writer.WriteAttributeString("text:style-name", "AllFooterPageNumber");
                        FillHeaderFooter(_pageHeaderFooter[16]["content"], 16);
                        _writer.WriteEndElement();
                    }
                    _writer.WriteStartElement("text:tab");
                    _writer.WriteEndElement();
                    if (_pageHeaderFooter[17].ContainsKey("content"))
                    {
                        _writer.WriteStartElement("text:span");
                        _writer.WriteAttributeString("text:style-name", "AllFooterPageRight");
                        FillHeaderFooter(_pageHeaderFooter[17]["content"], 17);
                        _writer.WriteEndElement();
                    }
                }
                else
                {
                    if (isMirrored)
                    {
                        SetAllPageFooter();
                    }
                }
                _writer.WriteEndElement(); // Close of p
                _writer.WriteEndElement(); // Close of Footer-Left

                _writer.WriteEndElement(); // Close of Master page Standard

                ///STANDARD CODE PART ENDS

                ////XHTML CODE PART START

                _writer.WriteStartElement("style:master-page");
                _writer.WriteAttributeString("style:name", "XHTML"); // All PageProperty
                _writer.WriteAttributeString("style:page-layout-name", "pm2");
                if (!isMirrored)
                {
                    /* Begin AllPage Header */
                    if (_pageHeaderFooter[6].Count > 0 || _pageHeaderFooter[7].Count > 0 || _pageHeaderFooter[8].Count > 0)
                    {
                        _writer.WriteStartElement("style:header");
                        _writer.WriteStartElement("text:p");
                        _writer.WriteAttributeString("text:style-name", "Header");
                        SetAllPageHeader();
                        _writer.WriteEndElement();
                        _writer.WriteEndElement();
                    }
                    /* Begin AllPage Footer */
                    if (_pageHeaderFooter[9].Count > 0 || _pageHeaderFooter[10].Count > 0 || _pageHeaderFooter[11].Count > 0)
                    {
                        _writer.WriteStartElement("style:footer");
                        _writer.WriteStartElement("text:p");
                        _writer.WriteAttributeString("text:style-name", "Footer");
                        SetAllPageFooter();
                        _writer.WriteEndElement(); // close of p
                        _writer.WriteEndElement(); // Close of Footer
                        }
                        }
                _writer.WriteEndElement(); // Close of Master Page

                ////XHTML CODE PART ENDS

                //// First CODE PART START

                _writer.WriteStartElement("style:master-page");
                _writer.WriteAttributeString("style:name", "First_20_Page");
                _writer.WriteAttributeString("style:display-name", "First Page"); // First PageProperty
                if (isMirrored)
                {
                    _writer.WriteAttributeString("style:page-layout-name", "pm2");
                    _writer.WriteAttributeString("style:next-style-name", "Standard");
                }
                else
                {
                    _writer.WriteAttributeString("style:page-layout-name", "pm3");
                    _writer.WriteAttributeString("style:next-style-name", "XHTML");
                }
                /*Begin Firstpage Header */
                if (_pageHeaderFooter[0].Count > 0 || _pageHeaderFooter[1].Count > 0 || _pageHeaderFooter[2].Count > 0)
                {
                    if (IsContentAvailable(0) || IsCropMarkChecked)
                    {
                        _writer.WriteStartElement("style:header");
                        _writer.WriteStartElement("text:p");
                        _writer.WriteAttributeString("text:style-name", "Header");
                        if (IsCropMarkChecked)
                        {
                            foreach (KeyValuePair<string, string> para in _firstPageLayoutProperty)
                            {
                                _pageLayoutProperty[para.Key] = para.Value;
                            }

                            foreach (KeyValuePair<string, string> para in _pageLayoutProperty)
                            {
                                _writer.WriteAttributeString(para.Key, para.Value);
                            }
                            AddHeaderCropMarks(_pageLayoutProperty);
                        }
                        if (_pageHeaderFooter[0].ContainsKey("content"))
                        {
                            _writer.WriteStartElement("text:span");
                            _writer.WriteAttributeString("text:style-name", "HeaderPageLeft");
                            FillHeaderFooter(_pageHeaderFooter[0]["content"], 0);
                            _writer.WriteEndElement();
                        }
                        _writer.WriteStartElement("text:tab");
                        _writer.WriteEndElement();
                        if (_pageHeaderFooter[1].ContainsKey("content"))
                        {
                            _writer.WriteStartElement("text:span");
                            _writer.WriteAttributeString("text:style-name", "HeaderPageNumber");
                            FillHeaderFooter(_pageHeaderFooter[1]["content"], 1);
                            _writer.WriteEndElement();
                        }
                        _writer.WriteStartElement("text:tab");
                        _writer.WriteEndElement();
                        if (_pageHeaderFooter[2].ContainsKey("content"))
                        {
                            _writer.WriteStartElement("text:span");
                            _writer.WriteAttributeString("text:style-name", "HeaderPageRight");
                            FillHeaderFooter(_pageHeaderFooter[2]["content"], 2);
                            _writer.WriteEndElement();
                        }
                        _writer.WriteEndElement(); // Close of p
                        _writer.WriteEndElement(); // Close of Header
                    }
                }

                    /*Begin Firstpage Footer */
                    if (_pageHeaderFooter[3].Count > 0 || _pageHeaderFooter[4].Count > 0 || _pageHeaderFooter[5].Count > 0)
                    {
                        if (IsContentAvailable(3))
                        {
                            _writer.WriteStartElement("style:footer");
                            _writer.WriteStartElement("text:p");
                            _writer.WriteAttributeString("text:style-name", "Footer");

                            if (_pageHeaderFooter[3].ContainsKey("content"))
                            {
                                _writer.WriteStartElement("text:span");
                                _writer.WriteAttributeString("text:style-name", "FooterPageLeft");
                                FillHeaderFooter(_pageHeaderFooter[3]["content"], 3);
                                _writer.WriteEndElement();
                            }
                            _writer.WriteStartElement("text:tab");
                            _writer.WriteEndElement();

                            if (_pageHeaderFooter[4].ContainsKey("content"))
                            {
                                _writer.WriteStartElement("text:span");
                                _writer.WriteAttributeString("text:style-name", "FooterPageNumber");
                                FillHeaderFooter(_pageHeaderFooter[4]["content"], 4);
                                _writer.WriteEndElement();
                            }

                            _writer.WriteStartElement("text:tab");
                            _writer.WriteEndElement();

                            if (_pageHeaderFooter[5].ContainsKey("content"))
                            {
                                _writer.WriteStartElement("text:span");
                                _writer.WriteAttributeString("text:style-name", "FooterPageRight");
                                FillHeaderFooter(_pageHeaderFooter[5]["content"], 5);
                                _writer.WriteEndElement();
                            }

                        _writer.WriteEndElement(); // Close of p
                        _writer.WriteEndElement(); // Close of Footer
                        }
                        /*End Firstpage Footer */
                    }
                _writer.WriteEndElement(); // Close of Master Page
                //// First CODE PART ENDS
                _writer.WriteEndDocument();
                _writer.Flush();
                _writer.Close();

                try
                {
                    if (_verboseWriter.ShowError && _verboseWriter.ErrorWritten)  // error file closing
                    {
                        _verboseWriter.WriteError("</table>");
                        _verboseWriter.WriteError("</body>");
                        _verboseWriter.WriteError("</html>");
                        _verboseWriter.Close();
                    }
                }
                catch (Exception ex)
                {
                    Console.Write(ex.Message);
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                _writer.Flush();
                _writer.Close();
            }
        }

        private void InsertHeaderRule()
        {
            if (!string.IsNullOrEmpty(HeaderRule))
            {
                _writer.WriteAttributeString("fo:border-bottom", HeaderRule);
            }
            else
            {
                _writer.WriteAttributeString("fo:border-bottom", "1pt solid #000000");
            }
            HeaderRule = string.Empty;
        }

        /// <summary>
        /// Function to load content of Allpage.
        /// </summary>
        private void SetAllPageHeader()
        {
            if (_pageHeaderFooter[6].Count > 0 || _pageHeaderFooter[7].Count > 0 || _pageHeaderFooter[8].Count > 0 || IsCropMarkChecked == true)
            {
                if (_pageHeaderFooter[6].ContainsKey("content"))
                {
                    _writer.WriteStartElement("text:span");
                    _writer.WriteAttributeString("text:style-name", "AllHeaderPageLeft");
                    FillHeaderFooter(_pageHeaderFooter[6]["content"], 6);
                    _writer.WriteEndElement();
                }
                if (IsCropMarkChecked)
                {
                    AddHeaderCropMarks(_pageLayoutProperty);
                }
                _writer.WriteStartElement("text:tab");
                _writer.WriteEndElement();

                if (_pageHeaderFooter[7].ContainsKey("content"))
                {
                    _writer.WriteStartElement("text:span");
                    _writer.WriteAttributeString("text:style-name", "AllHeaderPageNumber");
                    FillHeaderFooter(_pageHeaderFooter[7]["content"], 7);
                    _writer.WriteEndElement();
                }

                _writer.WriteStartElement("text:tab");
                _writer.WriteEndElement();

                if (_pageHeaderFooter[8].ContainsKey("content"))
                {
                    _writer.WriteStartElement("text:span");
                    _writer.WriteAttributeString("text:style-name", "AllHeaderPageRight");
                    FillHeaderFooter(_pageHeaderFooter[8]["content"], 8);
                    _writer.WriteEndElement();
                }
            }
        }

        /// <summary>
        /// Function to load content of Allpage.
        /// </summary>
        private void SetAllPageFooter()
        {
            if (_pageHeaderFooter[9].Count > 0 || _pageHeaderFooter[10].Count > 0 || _pageHeaderFooter[11].Count > 0 || IsCropMarkChecked == true)
            {
                if (_pageHeaderFooter[9].ContainsKey("content"))
                {
                    _writer.WriteStartElement("text:span");
                    _writer.WriteAttributeString("text:style-name", "AllFooterPageLeft");
                    FillHeaderFooter(_pageHeaderFooter[9]["content"], 9);
                    _writer.WriteEndElement();
                }
                if (IsCropMarkChecked)
                {
                    AddHeaderCropMarks(_pageLayoutProperty);
                }
                _writer.WriteStartElement("text:tab");
                _writer.WriteEndElement();

                if (_pageHeaderFooter[10].ContainsKey("content"))
                {
                    _writer.WriteStartElement("text:span");
                    _writer.WriteAttributeString("text:style-name", "AllFooterPageNumber");
                    FillHeaderFooter(_pageHeaderFooter[10]["content"], 10);
                    _writer.WriteEndElement();
                }

                _writer.WriteStartElement("text:tab");
                _writer.WriteEndElement();

                if (_pageHeaderFooter[11].ContainsKey("content"))
                {
                    _writer.WriteStartElement("text:span");
                    _writer.WriteAttributeString("text:style-name", "AllFooterPageRight");
                    FillHeaderFooter(_pageHeaderFooter[11]["content"], 11);
                    _writer.WriteEndElement();
                }
            }
        }

        private void ODTPageFooter()
        {
            //office:styles Attributes.
            //// fullString:notes-configuration footnote
            _writer.WriteStartElement("office:automatic-styles");

            // Styles applies to First PageProperty
            _writer.WriteStartElement("style:style");
            _writer.WriteAttributeString("style:name", "HeaderPageLeft");
            _writer.WriteAttributeString("style:family", "text");
            _writer.WriteStartElement("style:text-properties");
            foreach (KeyValuePair<string, string> attProperty in _pageHeaderFooter[0])
            {
                _writer.WriteAttributeString(attProperty.Key, attProperty.Value);
            }
            _writer.WriteEndElement();
            _writer.WriteEndElement();

            _writer.WriteStartElement("style:style");
            _writer.WriteAttributeString("style:name", "HeaderPageNumber");
            _writer.WriteAttributeString("style:family", "text");
            _writer.WriteStartElement("style:text-properties");
            foreach (KeyValuePair<string, string> attProperty in _pageHeaderFooter[1])
            {
                _writer.WriteAttributeString(attProperty.Key, attProperty.Value);
            }
            _writer.WriteEndElement();
            _writer.WriteEndElement();

            _writer.WriteStartElement("style:style");
            _writer.WriteAttributeString("style:name", "HeaderPageRight");
            _writer.WriteAttributeString("style:family", "text");
            _writer.WriteStartElement("style:text-properties");
            foreach (KeyValuePair<string, string> attProperty in _pageHeaderFooter[2])
            {
                _writer.WriteAttributeString(attProperty.Key, attProperty.Value);
            }
            _writer.WriteEndElement();
            _writer.WriteEndElement();

            //Begin Footer FirstPage Styles
            _writer.WriteStartElement("style:style");
            _writer.WriteAttributeString("style:name", "FooterPageLeft");
            _writer.WriteAttributeString("style:family", "text");
            _writer.WriteStartElement("style:text-properties");
            foreach (KeyValuePair<string, string> attProperty in _pageHeaderFooter[3])
            {
                _writer.WriteAttributeString(attProperty.Key, attProperty.Value);
            }
            _writer.WriteEndElement();
            _writer.WriteEndElement();

            _writer.WriteStartElement("style:style");
            _writer.WriteAttributeString("style:name", "FooterPageNumber");
            _writer.WriteAttributeString("style:family", "text");
            _writer.WriteStartElement("style:text-properties");
            foreach (KeyValuePair<string, string> attProperty in _pageHeaderFooter[4])
            {
                _writer.WriteAttributeString(attProperty.Key, attProperty.Value);
            }
            _writer.WriteEndElement();
            _writer.WriteEndElement();

            _writer.WriteStartElement("style:style");
            _writer.WriteAttributeString("style:name", "FooterPageRight");
            _writer.WriteAttributeString("style:family", "text");
            _writer.WriteStartElement("style:text-properties");
            foreach (KeyValuePair<string, string> attProperty in _pageHeaderFooter[5])
            {
                _writer.WriteAttributeString(attProperty.Key, attProperty.Value);
            }
            _writer.WriteEndElement();
            _writer.WriteEndElement();
            //End Footer FirstPage Styles

            //Begin Header Styles applies to All Pages except First PageProperty
            _writer.WriteStartElement("style:style");
            _writer.WriteAttributeString("style:name", "AllHeaderPageLeft");
            _writer.WriteAttributeString("style:family", "text");
            _writer.WriteStartElement("style:text-properties");
            foreach (KeyValuePair<string, string> attProperty in _pageHeaderFooter[6])
            {
                if (attProperty.Key.Substring(attProperty.Key.Length - 1) != ":")
                    _writer.WriteAttributeString(attProperty.Key, attProperty.Value);
            }
            _writer.WriteEndElement();
            _writer.WriteEndElement();

            _writer.WriteStartElement("style:style");
            _writer.WriteAttributeString("style:name", "AllHeaderPageNumber");
            _writer.WriteAttributeString("style:family", "text");
            _writer.WriteStartElement("style:text-properties");
            foreach (KeyValuePair<string, string> attProperty in _pageHeaderFooter[7])
            {
                _writer.WriteAttributeString(attProperty.Key, attProperty.Value);
            }
            _writer.WriteEndElement();
            _writer.WriteEndElement();

            _writer.WriteStartElement("style:style");
            _writer.WriteAttributeString("style:name", "AllHeaderPageRight");
            _writer.WriteAttributeString("style:family", "text");
            _writer.WriteStartElement("style:text-properties");
            foreach (KeyValuePair<string, string> attProperty in _pageHeaderFooter[8])
            {
                if (attProperty.Key.Substring(attProperty.Key.Length - 1) != ":")
                    _writer.WriteAttributeString(attProperty.Key, attProperty.Value);
            }
            _writer.WriteEndElement();
            _writer.WriteEndElement();

            //Begin Footer Allpage  Styles applies to All Pages except First PageProperty
            _writer.WriteStartElement("style:style");
            _writer.WriteAttributeString("style:name", "AllFooterPageLeft");
            _writer.WriteAttributeString("style:family", "text");
            _writer.WriteStartElement("style:text-properties");
            foreach (KeyValuePair<string, string> attProperty in _pageHeaderFooter[9])
            {
                _writer.WriteAttributeString(attProperty.Key, attProperty.Value);
            }
            _writer.WriteEndElement();
            _writer.WriteEndElement();

            _writer.WriteStartElement("style:style");
            _writer.WriteAttributeString("style:name", "AllFooterPageNumber");
            _writer.WriteAttributeString("style:family", "text");
            _writer.WriteStartElement("style:text-properties");
            foreach (KeyValuePair<string, string> attProperty in _pageHeaderFooter[10])
            {
                _writer.WriteAttributeString(attProperty.Key, attProperty.Value);
            }
            _writer.WriteEndElement();
            _writer.WriteEndElement();

            _writer.WriteStartElement("style:style");
            _writer.WriteAttributeString("style:name", "AllFooterPageRight");
            _writer.WriteAttributeString("style:family", "text");
            _writer.WriteStartElement("style:text-properties");
            foreach (KeyValuePair<string, string> attProperty in _pageHeaderFooter[11])
            {
                _writer.WriteAttributeString(attProperty.Key, attProperty.Value);
            }
            _writer.WriteEndElement();
            _writer.WriteEndElement();
            //End Footer AllPage Style

            _writer.WriteStartElement("style:page-layout");
            _writer.WriteAttributeString("style:name", "pm1");
            if (isMirrored)
            {
                _writer.WriteAttributeString("style:page-usage", "mirrored"); // If mirrored Page TD-410
                _writer.WriteStartElement("style:page-layout-properties");
                foreach (KeyValuePair<string, string> para in _pageLayoutProperty)
                {
                    _writer.WriteAttributeString(para.Key, para.Value);
                }
            }
            else
            {
                _writer.WriteStartElement("style:page-layout-properties");
            }

            // START FootNote Seperator
            FootnoteSeperator();
            // END FootNote Seperator
            _writer.WriteEndElement();
            _writer.WriteStartElement("style:header-style");
            _writer.WriteEndElement();
            _writer.WriteStartElement("style:footer-style");
            _writer.WriteEndElement();
            _writer.WriteEndElement();


            /* pm2 starts */
            _writer.WriteStartElement("style:page-layout");  // pm2
            _writer.WriteAttributeString("style:name", "pm2");  // All Page
            if (isMirrored)
            {
                _writer.WriteAttributeString("style:page-usage", "mirrored"); // If mirrored Page TD 410
            }
            _writer.WriteStartElement("style:page-layout-properties");
            foreach (KeyValuePair<string, string> para in _pageLayoutProperty)
            {
                _writer.WriteAttributeString(para.Key, para.Value);
            }
            _writer.WriteStartElement("style:background-image");

            _writer.WriteEndElement();
            _writer.WriteEndElement(); // end of style:page-layout-properties
            //Header & Footer styles for pm2
            _writer.WriteStartElement("style:header-style");
            LoadHeaderFooterSettings();
            _writer.WriteEndElement();
            _writer.WriteStartElement("style:footer-style");
            LoadHeaderFooterSettings();
            _writer.WriteEndElement();
            //End Header & Footer styles for pm2
            _writer.WriteEndElement();
            /* pm2 Ends*/

            /* pm3 starts */
            _writer.WriteStartElement("style:page-layout"); // pm3
            _writer.WriteAttributeString("style:name", "pm3");  // First Page
            _writer.WriteStartElement("style:page-layout-properties");
            foreach (KeyValuePair<string, string> para in _firstPageLayoutProperty)
            {
                _pageLayoutProperty[para.Key] = para.Value;
            }
            foreach (KeyValuePair<string, string> para in _pageLayoutProperty)
            {
                _writer.WriteAttributeString(para.Key, para.Value);
            }
            _writer.WriteStartElement("style:background-image");
            _writer.WriteEndElement();
            // START FootNote Seperator
            FootnoteSeperator();
            // END FootNote Seperator
            _writer.WriteEndElement(); // end of style:page-layout-properties
            //Header & Footer styles for pm3
            _writer.WriteStartElement("style:header-style");
            LoadHeaderFooterSettings();
            _writer.WriteEndElement();
            _writer.WriteStartElement("style:footer-style");
            LoadHeaderFooterSettings();
            _writer.WriteEndElement();
            //End Header & Footer styles for pm3
            _writer.WriteEndElement();
            /* pm3 ends*/
            // office:automatic-styles - Ends
            _writer.WriteEndElement();
        }
        private void LoadHeaderFooterSettings()
        {
            _writer.WriteStartElement("style:header-footer-properties");
            //height = 1/2 top-margin + 1/2 font point size + padding-top
            string height = "28.42pt";
            string space = "14.21pt";
            const string defaultUnit = "pt";
            if (_pageLayoutProperty.ContainsKey("fo:padding-top"))
            {
                float marginTop = float.Parse(Common.UnitConverterOO(_pageLayoutProperty["fo:margin-top"], defaultUnit).ToString().Replace(defaultUnit, ""));
                float paddingTop = float.Parse(Common.UnitConverterOO(_pageLayoutProperty["fo:padding-top"], defaultUnit).ToString().Replace(defaultUnit, ""));
                const float defaultfontSize = 12F;
                float calcSpace = (1 * marginTop / 2) + (1 * defaultfontSize / 2) + paddingTop;
                space = calcSpace + defaultUnit;
                height = "14.21" + defaultUnit;
            }
            _writer.WriteAttributeString("fo:margin-bottom", space); //Spacing
            _writer.WriteAttributeString("fo:min-height", height); //Height
            _writer.WriteAttributeString("fo:margin-left", "0pt");
            _writer.WriteAttributeString("fo:margin-right", "0pt");
            _writer.WriteAttributeString("style:dynamic-spacing", "false");
            _writer.WriteEndElement();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private string CheckParent(TreeNode node)
        {
            string hasParent = string.Empty;
            hasParent = "no";
            foreach (TreeNode childNode in node.Nodes)
            {
                string parent = childNode.Text.ToLower();
                if (parent == "PARENTOF")
                {
                    hasParent = "yes";
                }
            }
            return hasParent;
        }
        #endregion

        #region SplitValue

        private void SplitValue(string attributeStringValue, string keyName)
        {
            string attribVal = string.Empty;
            try
            {
                int attribCount = 0;
                string[] parameter = attributeStringValue.Split(',');
                foreach (string param in parameter)
                {
                    attribVal = attribVal + param;
                    if (param == "0" || Common.ValidateAlphabets(param) == true || param.IndexOf('#') > -1)
                    {
                        attribCount++;
                        if (attribCount != parameter.Length)
                        {
                            attribVal = attribVal + "+";
                        }
                    }
                }
                if (attribCount == 1)
                {
                    string[] splitPlus = attribVal.Split('+');
                    _styleName.BorderProperty[keyName] = splitPlus[0];
                }
                else if (attribCount == 2)
                {
                    string[] splitPlus = attribVal.Split('+');
                    _styleName.BorderProperty[keyName] = splitPlus[0] + "," + splitPlus[1] + ","
                        + splitPlus[0] + "," + splitPlus[1];
                }
                else if (attribCount == 3)
                {
                    string[] splitPlus = attribVal.Split('+');
                    _styleName.BorderProperty[keyName] = splitPlus[0] + "," + splitPlus[1] + ","
                        + splitPlus[2] + "," + splitPlus[1];
                }
                else if (attribCount == 4)
                {
                    string[] splitPlus = attribVal.Split('+');
                    _styleName.BorderProperty[keyName] = splitPlus[0] + "," + splitPlus[1] + ","
                        + splitPlus[2] + "," + splitPlus[3];
                }
            }
            catch
            {
                _styleName.BorderProperty[keyName] = "0pt";
            }
        }
        #endregion

        #region FootnoteSeperator

        private void FootnoteSeperator()
        {
            if (_styleName.FootNoteSeperator.Count > 1)
            {
                string[] border;
                _writer.WriteStartElement("style:footnote-sep");
                string text = _styleName.FootNoteSeperator["border-top"].ToString();
                border = text.Split(' ');
                for (int i = 0; i < border.Length; i++)
                {
                    if (border[i].ToString() == "thin")
                    {
                        _writer.WriteAttributeString("style:width", "0.5pt");
                    }
                    else if (border[i].ToString() == "medium")
                    {
                        _writer.WriteAttributeString("style:width", "1.0pt");
                    }
                    else if (border[i].ToString() == "thick")
                    {
                        _writer.WriteAttributeString("style:width", "1.5pt");
                    }
                    else if (border[i].Contains("in") || border[i].Contains("pt"))
                    {
                        _writer.WriteAttributeString("style:width", "1.5pt");
                    }
                    else if (border[i] == "solid")
                    {
                        _writer.WriteAttributeString("style:line-style", border[i].ToString());
                    }
                    else if (border[i].IndexOf('#') > -1)
                    {
                        _writer.WriteAttributeString("style:color", border[i].ToString());
                    }
                }
                _writer.WriteAttributeString("style:distance-before-sep", _styleName.FootNoteSeperator["padding-top"]);
                _writer.WriteAttributeString("style:distance-after-sep", _styleName.FootNoteSeperator["padding-bottom"]);
                _writer.WriteAttributeString("style:adjustment", "centre");
                _writer.WriteAttributeString("style:rel-width", "100%");
                _writer.WriteEndElement();
            }
            else
            {
                _writer.WriteStartElement("style:footnote-sep");
                _writer.WriteAttributeString("style:width", "0.0071in");
                _writer.WriteAttributeString("style:distance-before-sep", "0.0398in");
                _writer.WriteAttributeString("style:distance-after-sep", "0.0398in");
                _writer.WriteAttributeString("style:adjustment", "left");
                _writer.WriteAttributeString("style:rel-width", "25%");
                _writer.WriteAttributeString("style:color", "#000000");
                _writer.WriteEndElement();
            }
        }
        #endregion

        /// -------------------------------------------------------------------------------------------
        /// <summary>
        /// Get className from the node
        /// 
        /// <list> 
        /// </list>
        /// </summary>
        /// <param name="tree">Antlr Tree</param>
        /// <param name="ctp">Antlr tree collection</param>
        /// <returns>class Name</returns>
        /// -------------------------------------------------------------------------------------------        
        /// 
        private string GetFirstChild(TreeNode tree)
        {
            string className = string.Empty;
            try
            {
                className = tree.FirstNode.Text;
                return (className);
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                return (className);
            }
        }

        /// -------------------------------------------------------------------------------------------
        /// <summary>
        /// Get GetAttributes Values from the node
        /// 
        /// <list> 
        /// </list>
        /// </summary>
        /// <param name="tree">Antlr Tree</param>
        /// <returns>ATTRIB</returns>
        /// -------------------------------------------------------------------------------------------        
        /// 
        private ClassAttribute GetAttribValue_NEW(TreeNode tree)
        {
            try
            {
                ClassAttribute classAttribute = new ClassAttribute();
                if (tree.Nodes.Count == 1)
                {
                    classAttribute.SetAttribute(tree.FirstNode.Text);
                }
                else if (tree.Nodes.Count == 3)
                {
                    classAttribute.SetAttribute(tree.Nodes[0].Text, tree.Nodes[1].Text, tree.Nodes[2].Text);
                }
                return (classAttribute);
            }
            catch
            {
                return (null);
            }
        }

        /// -------------------------------------------------------------------------------------------
        /// <summary>
        /// Unicode Conversion 
        /// </summary>
        /// <param name="parameter">input String</param>
        /// <returns>Unicode Character</returns>
        /// -------------------------------------------------------------------------------------------
        public string UnicodeConversion(string parameter)
        {
            int count = 0;
            string result = string.Empty;

            if (!(parameter[0] == '\"' || parameter[0] == '\''))
            {
                parameter = "'" + parameter + "'";
            }
            int strlen = parameter.Length;
            char quoteOpen = ' ';
            while (count < strlen)
            {
                // Handling Single / Double Quotes
                char c1 = parameter[count];
                Console.WriteLine(c1);
                if (parameter[count] == '\"' || parameter[count] == '\'')
                {
                    if (parameter[count] == quoteOpen)
                    {
                        quoteOpen = ' ';
                        count++;
                        continue;
                    }
                    if (quoteOpen == ' ')
                    {
                        quoteOpen = parameter[count];
                        count++;
                        continue;
                    }
                }

                if (parameter[count] == '\\')
                {
                    string unicode = string.Empty;
                    count++;
                    if (parameter[count] == 'u')
                    {
                        count++;
                    }
                    while (count < strlen)
                    {
                        int value = parameter[count];
                        if ((value > 47 && value < 58) || (value > 64 && value < 71) || (value > 96 && value < 103))
                        {
                            unicode += parameter[count];
                        }
                        else
                        {
                            break;
                        }
                        count++;
                    }
                    // unicode convertion
                    int decimalvalue = Convert.ToInt32(unicode, 16);
                    var c = (char)decimalvalue;
                    result += c.ToString();
                }
                else
                {
                    result += parameter[count];
                    count++;
                }
            }
            if (quoteOpen != ' ')
            {
                result = "";
            }
            else
            {
                // Replace <, > and & character to &lt; &gt; &amp;
                result = result.Replace("&", "&amp;");
                result = result.Replace("<", "&lt;");
                result = result.Replace(">", "&gt;");
            }

            return result;
        }

        /// <summary>
        /// Dimension calculation for Crop_Marks by Placing horizontal and Vertical line at each corners.
        /// </summary>
        /// 
        private void AddHeaderCropMarks(Dictionary<string, string> PageLayoutProperty)
        {
            PageLayoutProperty = _pageLayoutProperty;
            if (_firstPageLayoutProperty.Count > 0)
            {
                double pageMargin = double.Parse(Common.UnitConverterOO(_pageLayoutProperty["fo:margin-left"], "cm").ToString().Replace("cm", ""));
                double pageFirstMargin = double.Parse(Common.UnitConverterOO(_firstPageLayoutProperty["fo:margin-left"], "cm").ToString().Replace("cm", ""));
                if (pageMargin < pageFirstMargin)
                {
                    PageLayoutProperty = _firstPageLayoutProperty;
                }
            }

            // Marks-Crop Calculation 
            _PageWidth = float.Parse(PageLayoutProperty["fo:page-width"].Replace("cm", ""));
            _leftMargin = float.Parse(Common.UnitConverterOO(PageLayoutProperty["fo:margin-left"], "cm").ToString().Replace("cm", ""));
            _rightMargin = float.Parse(Common.UnitConverterOO(PageLayoutProperty["fo:margin-right"], "cm").ToString().Replace("cm", ""));
            _rightPosition = _PageWidth - (_leftMargin + _rightMargin); // position

            _PageHeight = float.Parse(PageLayoutProperty["fo:page-height"].Replace("cm", ""));
            _topMargin = float.Parse(Common.UnitConverterOO(PageLayoutProperty["fo:margin-top"], "cm").ToString().Replace("cm", ""));
            _bottomMargin = float.Parse(Common.UnitConverterOO(PageLayoutProperty["fo:margin-bottom"], "cm").ToString().Replace("cm", ""));
            _bottomPosition = _PageHeight - (_topMargin + _bottomMargin); // position

            //top - left - horizontal
            _writer.WriteStartElement("draw:line");
            _writer.WriteAttributeString("text:anchor-type", "paragraph");
            _writer.WriteAttributeString("draw:style-name", "Mgr1");
            _writer.WriteAttributeString("draw:z-index", "0");
            _writer.WriteAttributeString("svg:x1", (-_gapToMargin - _leftMargin + 1.27) + "cm");
            _writer.WriteAttributeString("svg:y1", -_topMargin + 1.27 + "cm");
            _writer.WriteAttributeString("svg:x2", (-_gapToMargin - _leftMargin - _lineLength + 1.27) + "cm");
            _writer.WriteAttributeString("svg:y2", -_topMargin + 1.27 + "cm");
            _writer.WriteEndElement();

            // top - left - vertical
            _writer.WriteStartElement("draw:line");
            _writer.WriteAttributeString("text:anchor-type", "paragraph");
            _writer.WriteAttributeString("draw:style-name", "Mgr1");
            _writer.WriteAttributeString("draw:z-index", "1");
            _writer.WriteAttributeString("svg:x1", -_leftMargin + 1.27 + "cm");
            _writer.WriteAttributeString("svg:y1", (-_gapToMargin - _topMargin + 1.27) + "cm");
            _writer.WriteAttributeString("svg:x2", -_leftMargin + 1.27 + "cm");
            _writer.WriteAttributeString("svg:y2", (-_gapToMargin - _topMargin - _lineLength + 1.27) + "cm");
            _writer.WriteEndElement();

            //top - right - horizontal
            _writer.WriteStartElement("draw:line");
            _writer.WriteAttributeString("text:anchor-type", "paragraph");
            _writer.WriteAttributeString("draw:style-name", "Mgr1");
            _writer.WriteAttributeString("draw:z-index", "2");
            _writer.WriteAttributeString("svg:x1", (_rightPosition + _rightMargin + _gapToMargin - 1.27) + "cm");
            _writer.WriteAttributeString("svg:y1", -_topMargin + 1.27 + "cm");
            _writer.WriteAttributeString("svg:x2", (_rightPosition + _rightMargin + _gapToMargin + _lineLength - 1.27) + "cm");
            _writer.WriteAttributeString("svg:y2", -_topMargin + 1.27 + "cm");
            _writer.WriteEndElement();

            // top -right - vertical
            _writer.WriteStartElement("draw:line");
            _writer.WriteAttributeString("text:anchor-type", "paragraph");
            _writer.WriteAttributeString("draw:style-name", "Mgr1");
            _writer.WriteAttributeString("draw:z-index", "3");
            _writer.WriteAttributeString("svg:x1", (_rightPosition + _rightMargin - 1.27) + "cm");
            _writer.WriteAttributeString("svg:y1", (-_gapToMargin - _topMargin - _lineLength + 1.27) + "cm");
            _writer.WriteAttributeString("svg:x2", (_rightPosition + _rightMargin - 1.27) + "cm");
            _writer.WriteAttributeString("svg:y2", (-_gapToMargin - _topMargin + 1.27) + "cm");
            _writer.WriteEndElement();

            //bot - left - horizontal
            _writer.WriteStartElement("draw:line");
            _writer.WriteAttributeString("text:anchor-type", "paragraph");
            _writer.WriteAttributeString("draw:style-name", "Mgr1");
            _writer.WriteAttributeString("draw:z-index", "4");
            _writer.WriteAttributeString("svg:x1", (-_gapToMargin - _leftMargin - _lineLength + 1.27) + "cm");
            _writer.WriteAttributeString("svg:y1", (_bottomPosition + _bottomMargin - 1.27) + "cm");
            _writer.WriteAttributeString("svg:x2", (-_gapToMargin - _leftMargin + 1.27) + "cm");
            _writer.WriteAttributeString("svg:y2", (_bottomPosition + _bottomMargin - 1.27) + "cm");
            _writer.WriteEndElement();

            // bot - left - vertical
            _writer.WriteStartElement("draw:line");
            _writer.WriteAttributeString("text:anchor-type", "paragraph");
            _writer.WriteAttributeString("draw:style-name", "Mgr1");
            _writer.WriteAttributeString("draw:z-index", "5");
            _writer.WriteAttributeString("svg:x1", -_leftMargin + 1.27 + "cm");
            _writer.WriteAttributeString("svg:y1", (_bottomPosition + _gapToMargin + _bottomMargin + _lineLength - 1.27) + "cm");
            _writer.WriteAttributeString("svg:x2", -_leftMargin + 1.27 + "cm");
            _writer.WriteAttributeString("svg:y2", (_bottomPosition + _gapToMargin + _bottomMargin - 1.27) + "cm");
            _writer.WriteEndElement();

            //bot - right - horizontal
            _writer.WriteStartElement("draw:line");
            _writer.WriteAttributeString("text:anchor-type", "paragraph");
            _writer.WriteAttributeString("draw:style-name", "Mgr1");
            _writer.WriteAttributeString("draw:z-index", "6");
            _writer.WriteAttributeString("svg:x1", (_rightPosition + _bottomMargin + _gapToMargin - 1.27) + "cm");
            _writer.WriteAttributeString("svg:y1", (_bottomPosition + _rightMargin - 1.27) + "cm");
            _writer.WriteAttributeString("svg:x2", (_rightPosition + _bottomMargin + _gapToMargin + _lineLength - 1.27) + "cm");
            _writer.WriteAttributeString("svg:y2", (_bottomPosition + _rightMargin - 1.27) + "cm");
            _writer.WriteEndElement();

            // bot -right - vertical
            _writer.WriteStartElement("draw:line");
            _writer.WriteAttributeString("text:anchor-type", "paragraph");
            _writer.WriteAttributeString("draw:style-name", "Mgr1");
            _writer.WriteAttributeString("draw:z-index", "7");
            _writer.WriteAttributeString("svg:x1", (_rightPosition + _rightMargin - 1.27) + "cm");
            _writer.WriteAttributeString("svg:y1", (_bottomPosition + _gapToMargin + _rightMargin + _lineLength - 1.27) + "cm");
            _writer.WriteAttributeString("svg:x2", _rightPosition + _rightMargin - 1.27 + "cm");
            _writer.WriteAttributeString("svg:y2", (_bottomPosition + _gapToMargin + _rightMargin - 1.27) + "cm");
            _writer.WriteEndElement();
        }

        /// <summary>
        /// To verify default value for page and increase the page values when the mark: crop is ON
        /// </summary>
        /// <param name="keyName">Page property</param>
        /// <param name="isPageFirst">bool - is first page?</param>
        private void ChangePageProperty(string keyName, bool isPageFirst)
        {
            if (isPageFirst)
            {
                if (_firstPageLayoutProperty.Count > 0)
                {
                    if (keyName == "fo:page-width" || keyName == "fo:page-height")
                    {
                        _firstPageLayoutProperty[keyName] = (float.Parse(Common.UnitConverterOO(_firstPageLayoutProperty[keyName], "cm").ToString().Replace("cm", "")) + 2.54) + "cm";
                    }
                    else
                    {
                        _firstPageLayoutProperty[keyName] = (float.Parse(Common.UnitConverterOO(_firstPageLayoutProperty[keyName], "cm").ToString().Replace("cm", "")) + 1.27) + "cm";
                        IsMarginChanged = true;
                    }
                    _isFirstpageDimensionChanged = true;
                }
            }
            else
            {
                if ((keyName == "fo:page-width") || (keyName == "fo:page-height"))
                {
                    _pageLayoutProperty[keyName] = (float.Parse(Common.UnitConverterOO(_pageLayoutProperty[keyName], "cm").ToString().Replace("cm", "")) + 2.54) + "cm";
                }
                else
                {
                    _pageLayoutProperty[keyName] = (float.Parse(Common.UnitConverterOO(_pageLayoutProperty[keyName], "cm").ToString().Replace("cm", "")) + 1.27) + "cm";
                    IsMarginChanged = true;
                }
            }
        }

    }
    #endregion
}

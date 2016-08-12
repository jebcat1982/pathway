﻿// ---------------------------------------------------------------------------------------------
#region // Copyright (c) 2016, SIL International. All Rights Reserved.
// <copyright from='2016' to='2016' company='SIL International'>
//		Copyright (c) 2016, SIL International. All Rights Reserved.
//
//		Distributable under the terms of either the Common Public License or the
//		GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright>
#endregion
//
// File: FlattenStyles.cs
// Responsibility: Greg Trihus
// ---------------------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace CssSimpler
{
    public class FlattenStyles : XmlCopy
    {
        private readonly Dictionary<string, List<XmlElement>> _ruleIndex = new Dictionary<string, List<XmlElement>>();
        private const int StackSize = 30;
        private readonly ArrayList _classes = new ArrayList(StackSize);
        private readonly ArrayList _langs = new ArrayList(StackSize); 
        private readonly ArrayList _levelRules = new ArrayList(StackSize);
        private readonly ArrayList _savedSibling = new ArrayList(StackSize);
        private string _lastClass = String.Empty;
        private string _precedingClass = String.Empty;
        private readonly SortedSet<string> _needHigher;

        public FlattenStyles(string input, string output, XmlDocument xmlCss, SortedSet<string> needHigher)
            : base(input, output)
        {
            _needHigher = needHigher;
            MakeRuleIndex(xmlCss);
            _xmlCss = xmlCss;
            Suffix = string.Empty;
            DeclareBefore(XmlNodeType.Attribute, SaveClassLang);
            DeclareBefore(XmlNodeType.Element, SaveSibling);
            DeclareBefore(XmlNodeType.Element, InsertBefore);
            DeclareBefore(XmlNodeType.EndElement, SetForEnd);
            DeclareBefore(XmlNodeType.Text, TextNode);
            DeclareBefore(XmlNodeType.EntityReference, OtherNode);
            DeclareBefore(XmlNodeType.Whitespace, OtherNode);
            DeclareBefore(XmlNodeType.SignificantWhitespace, OtherNode);
            DeclareBefore(XmlNodeType.CDATA, OtherNode);
            DeclareBeforeEnd(XmlNodeType.EndElement, DivEnds);
            DeclareBeforeEnd(XmlNodeType.EndElement, UnsaveClass);
            Parse();
        }

        private void InsertBefore(XmlReader r)
        {
            var nextClass = r.GetAttribute("class");
            SkipNode = r.Name == "span";
            //if (nextClass == "letter")
            //{
            //    Debug.Print("break;");
            //}
            CollectRules(r, GetRuleKey(r.Name, nextClass));
            CollectRules(r, GetRuleKey(r.Name, _lastClass));
            CollectRules(r, nextClass);
            CollectRules(r, GetRuleKey(r.Name, ""));
            if (!SkipNode)
            {
                GetStyle(r);
            }
        }

        private void SetForEnd(XmlReader r)
        {
            SkipNode = r.Name == "span";
        }

        private void DivEnds(int depth, string name)
        {
            SkipNode = name == "span";
            if (_levelRules.Count > depth)
            {
                _levelRules[depth] = null;
            }
        }

        private void UnsaveClass(int depth, string name)
        {
            var index = depth + 1;
            if (index >= _classes.Count) return;
            _precedingClass = _classes[index] as string;
            _classes[index] = null;
        }

        private void CollectRules(XmlReader r, string target)
        {
            if (target == null) return;
            var dirty = false;
            var index = r.Depth;
            var found = _levelRules.Count > index && _levelRules[index] != null ? (List<XmlElement>)_levelRules[index] : new List<XmlElement>();
            foreach (var t in target.Split(' '))
            {
                var targets = _ruleIndex;
                if (!targets.ContainsKey(t)) continue;
                foreach (var node in targets[t])
                {
                    if (!Applies(node, r)) continue;
                    found.Add(node);
                    dirty = true;
                }
            }
            if (dirty)
            {
                AddInHierarchy(_levelRules, index, found);
            }
        }

        private bool Applies(XmlNode node, XmlReader r)
        {
            var index = r.Depth;
            while (node != null && node.Name == "PROPERTY")
            {
                node = node.PreviousSibling;
            }
            var requireParent = false;
            // We should be at the tag / class for the rule being applied so look before it.
            if (node != null && node.ChildNodes.Count == 1)
            {
                node = node.PreviousSibling;
            }
            while (node != null)
            {
                switch (node.Name)
                {
                    case "PARENTOF":
                        requireParent = true;
                        break;
                    case "CLASS":
                        string name = node.FirstChild.InnerText;
                        while (!requireParent && index > 0 && !MatchClass(index, name))
                        {
                            index -= 1;
                        }
                        requireParent = false;
                        if (!MatchClass(index, name)) return false;
                        index -= 1;
                        break;
                    case "PRECEDES":
                        if (_firstSibling) return false;
                        node = node.PreviousSibling;
                        Debug.Assert(node != null, "Nothing preceding PRECEDES");
                        string precedingName = node.FirstChild.InnerText;
                        if (_precedingClass != precedingName && precedingName != "span") return false;
                        break;
                    case "SIBLING":
                        node = node.PreviousSibling;
                        Debug.Assert(node != null, "Nothing preceding SIBLING");
                        string siblingName = node.FirstChild.InnerText;
                        int position = _savedSibling.IndexOf(siblingName);
                        if (position == -1 || position == _savedSibling.Count - 1) return false;
                        break;
                    case "TAG":
                        if (!CheckAttrib(node, r)) return false;
                        break;
                    default:
                        throw new NotImplementedException();
                }
                node = node.PreviousSibling;
            }
            return true;

        }

        private static bool CheckAttrib(XmlNode node, XmlReader r)
        {
            if (node.ChildNodes.Count > 1)
            {
                var attrNode = node.ChildNodes[1];
                if (attrNode.Name == "ATTRIB")
                {
                    if (!AttribEval(r, attrNode)) return false;
                }
                else
                {
                    throw new NotImplementedException("non-ATTRIB modifier");
                }
            }
            return true;
        }

        private static readonly char[] Quotes = {'\'', '"'};
        private static bool AttribEval(XmlReader r, XmlNode attrNode)
        {
            var attrName = attrNode.FirstChild.InnerText;
            var actualVal = r.GetAttribute(attrName);
            var attrOp = attrNode.ChildNodes[1].Name;
            switch (attrOp)
            {
                case "BEGINSWITH":
                    var expVal = attrNode.ChildNodes[2].InnerText.Trim(Quotes);
                    if (actualVal != expVal) return false;
                    break;
                default:
                    throw new NotImplementedException();
            }
            return true;
        }

        private bool MatchClass(int index, string name)
        {
            if (index >= _classes.Count) return false;
            var classNames = _classes[index] as string;
            if (classNames == null) return false;
            if (!classNames.Split(' ').Contains(name)) return false;
            return true;
        }

        private void TextNode(XmlReader r)
        {
            WriteContent(r.Value, GetStyle(r, 1), GetLang(r));
            SkipNode = true;
        }

        public readonly Dictionary<string, string> RuleStyleMap = new Dictionary<string, string>();
        private readonly Dictionary<string, int> _usedStyles = new Dictionary<string, int>();

        // ReSharper disable once UnusedMethodReturnValue.Local
        private string GetStyle(XmlReader r)
        {
            return GetStyle(r, 0);
        }

        private string GetStyle(XmlReader r, int adjustLevel)
        {
            var myClass = GetClass(r);
            if (myClass == null) return null;
            //if (myClass == "letter")
            //{
            //    Debug.Print("break;");
            //}
            var ruleNums = GetRuleNumbers(r, adjustLevel);
            var key = myClass + ":" + ruleNums;
            if (_usedStyles.ContainsKey(myClass))
            {
                if (RuleStyleMap.ContainsKey(key))
                {
                    myClass = RuleStyleMap[key];
                }
                else
                {
                    _usedStyles[myClass] += 1;
                    myClass += _usedStyles[myClass];
                    RuleStyleMap[key] = myClass;
                }
            }
            else
            {
                _usedStyles[myClass] = 1;
                RuleStyleMap[key] = myClass;
            }
            return myClass;
        }

        private readonly List<int> _ruleNums = new List<int>();
        private string GetRuleNumbers(XmlReader r, int adjustLevel)
        {
            _ruleNums.Clear();
            var inherited = false;
            for (var i = r.Depth - adjustLevel; i >= 0; i -= 1)
            {
                if (_levelRules.Count <= i) continue;
                var levelList = _levelRules[i] as List<XmlElement>;
                if (levelList == null) continue;
                foreach (XmlElement node in levelList)
                {
                    var numNode = node.SelectSingleNode("parent::*/@pos");
                    if (numNode == null) continue;
                    var num = int.Parse(numNode.InnerText);
                    if (_ruleNums.Contains(num)) continue;
                    _ruleNums.Add(num);
                }
                if (inherited) continue;
                inherited = true;
                _ruleNums.Add(-1);
            }
            return string.Join(",", _ruleNums);
        }

        private readonly SortedDictionary<string, string> _reverseMap = new SortedDictionary<string, string>();
        private readonly XmlDocument _flatCss = new XmlDocument();
        private readonly XmlDocument _xmlCss;
        private readonly string[] _notInherted = {"column-count", "clear", "width", "margin-left", "padding-bottom", "padding-top", "display"};
        public XmlDocument MakeFlatCss()
        {
            _flatCss.RemoveAll();
            _flatCss.LoadXml("<ROOT/>");
            CopyTagNodes();
            foreach (var key in RuleStyleMap.Keys)
            {
                _reverseMap[RuleStyleMap[key]] = key;
            }
            var pos = 1;
            foreach (var style in _reverseMap.Keys)
            {
                //if (style == "letter")
                //{
                //    Debug.Print("break;");
                //}
                var ruleNode = _flatCss.CreateElement("RULE");
                var classNode = _flatCss.CreateElement("CLASS");
                var nameNode = _flatCss.CreateElement("name");
                nameNode.InnerText = style;
                classNode.AppendChild(nameNode);
                ruleNode.AppendChild(classNode);
                Debug.Assert(_flatCss.DocumentElement != null, "_flatCss.DocumentElement != null");
                _flatCss.DocumentElement.AppendChild(ruleNode);
                ruleNode.SetAttribute("pos", pos.ToString());
                var incProps = new SortedSet<string>();
                var activeRules = _reverseMap[style];
                var inherited = false;
                foreach (Match m in Regex.Matches(activeRules.Substring(activeRules.IndexOf(":", StringComparison.Ordinal)), @"[\d-]+"))
                {
                    if (m.Value == "-1")
                    {
                        inherited = true;
                        continue;
                    }
                    var pattern = string.Format("//*[@pos='{0}']/PROPERTY", m.Value);
                    var propNodes = _xmlCss.SelectNodes(pattern);
                    if (propNodes == null) continue;
                    foreach (XmlElement node in propNodes)
                    {
                        Debug.Assert(node != null, "node != null");
                        var propNameNode = node.SelectSingleNode(".//name");
                        if (propNameNode == null) continue;
                        var name = propNameNode.InnerText;
                        if (incProps.Contains(name)) continue;
                        if (inherited && (_notInherted.Contains(name) || name.StartsWith("-"))) continue;
                        incProps.Add(name);
                        ruleNode.AppendChild(_flatCss.ImportNode(node, true));
                    }
                }
            }
            return _flatCss;
        }

        private void CopyTagNodes()
        {
            var tagNodes = _xmlCss.SelectNodes("//*[@term='1' and count(TAG) = 1]");
            Debug.Assert(tagNodes != null, "tagNodes != null");
            foreach (XmlElement node in tagNodes)
            {
                Debug.Assert(_flatCss.DocumentElement != null, "_flatCss.DocumentElement != null");
                _flatCss.DocumentElement.AppendChild(_flatCss.ImportNode(node, true));
            }
        }

        private void OtherNode(XmlReader r)
        {
            SkipNode = false;
        }

        private void SaveClassLang(XmlReader r)
        {
            if (r.Name == "class")
            {
                if (!_needHigher.Contains(r.Value))
                {
                    _lastClass = r.Value;
                }
                AddInHierarchy(r, _classes);
            }
            else if (r.Name == "lang")
            {
                AddInHierarchy(r, _langs);
            }
        }

        private static void AddInHierarchy(XmlReader r, IList arrayList)
        {
            AddInHierarchy(arrayList, r.Depth, r.Value);
        }

        private static void AddInHierarchy(IList arrayList, int index, object value)
        {
            if (index >= arrayList.Count)
            {
                while (arrayList.Count < index)
                {
                    // ReSharper disable once AssignNullToNotNullAttribute
                    arrayList.Add(null);
                }
                arrayList.Add(value);
            }
            else
            {
                arrayList[index] = value;
            }
        }

        private string GetClass(XmlReader r)
        {
            var myClass = r.GetAttribute("class");
            var depth = r.Depth;
            while (string.IsNullOrEmpty(myClass) && depth > 0)
            {
                myClass = _classes.Count > depth? (string)_classes[depth]: null;
                depth -= 1;
            }
            return myClass;
        }

        private string GetLang(XmlReader r)
        {
            var myLang = r.GetAttribute("lang");
            var depth = r.Depth;
            while (string.IsNullOrEmpty(myLang) && depth > 0)
            {
                myLang = _langs.Count > depth ? (string)_langs[depth] : null;
                depth -= 1;
            }
            return myLang;
        }

        private int _nextFirst = -1;
        private bool _firstSibling;

        private void SaveSibling(XmlReader r)
        {
            _firstSibling = r.Depth == _nextFirst;
            var myClass = r.GetAttribute("class");
            if (!string.IsNullOrEmpty(myClass))
            {
                if (_firstSibling)
                {
                    _savedSibling.Clear();
                }
                _savedSibling.Add(myClass);
            }
            _nextFirst = r.Depth + 1;
        }

        private void MakeRuleIndex(XmlDocument xmlCss)
        {
            Debug.Assert(xmlCss != null, "xmlCss != null");
            var targets = _ruleIndex;
            var styleRules = xmlCss.SelectNodes("//RULE/PROPERTY[1]");
            Debug.Assert(styleRules != null, "styleRules != null");
            foreach (XmlElement styleRule in styleRules)
            {
                var rule = styleRule.ParentNode as XmlElement;
                Debug.Assert(rule != null, "rule is null");
                var target = GetRuleKey(rule.GetAttribute("target"), rule.GetAttribute("lastClass"));
                if (!targets.ContainsKey(target))
                {
                    targets[target] = new List<XmlElement> {styleRule};
                }
                else
                {
                    // insert pseduo node so rules are process in order of priority
                    var index = 0;
                    var added = false;
                    foreach (var term in targets[target])
                    {
                        var targetRule = term.ParentNode as XmlElement;
                        Debug.Assert(targetRule != null, "targetRule != null");
                        var ruleTerms = int.Parse(targetRule.GetAttribute("term"));
                        // ReSharper disable once TryCastAlwaysSucceeds
                        var curRule = styleRule.ParentNode as XmlElement;
                        var curTerms = int.Parse(curRule.GetAttribute("term"));
                        if (curTerms >= ruleTerms)
                        {
                            targets[target].Insert(index, styleRule);
                            added = true;
                            break;
                        }
                        index++;
                    }
                    if (!added)
                    {
                        targets[target].Add(styleRule);
                    }
                }
            }
        }

        private static string GetRuleKey(string target, string lastClass)
        {
            if (target == "span" || target == "xitem")
            {
                if (lastClass == null) return null;
                if (!lastClass.Contains(" "))
                {
                    target = string.Format("{0}:{1}", target, lastClass);
                }
                else
                {
                    var sb = new StringBuilder();
                    var first = true;
                    foreach (var s in lastClass.Split(' '))
                    {
                        if (!first)
                        {
                            sb.Append(" ");
                        }
                        else
                        {
                            first = false;
                        }
                        sb.Append(string.Format("{0}:{1}", target, s));
                    }
                    target = sb.ToString();
                }
            }
            return target;
        }
    }
}

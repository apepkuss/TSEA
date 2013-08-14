using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Sam.XmlDiffPath
{
    public enum XmlDiffAlgorithm
    {
        Auto,
        Fast,
        Precise,
    }

    internal enum TriStateBool
    {
        Yes,
        No,
        DontKnown,
    }

    /// <summary>
    /// Options for comparing XML documents. 
    /// </summary>
    public enum XmlDiffOptions
    {
        None = 0x0,
        IgnoreChildOrder = 0x1,
        IgnoreComments = 0x2,
        IgnorePI = 0x4,
        IgnoreWhitespace = 0x8,
        IgnoreNamespaces = 0x10,
        IgnorePrefixes = 0x20,
        IgnoreXmlDecl = 0x40,
        IgnoreDtd = 0x80,
    }

    internal enum XmlDiffNodeType
    {
        XmlDeclaration = -2,
        DocumentType = -1,
        None = 0,
        Element = XmlNodeType.Element,
        Attribute = XmlNodeType.Attribute,
        Text = XmlNodeType.Text,
        CDATA = XmlNodeType.CDATA,
        Comment = XmlNodeType.Comment,
        Document = XmlNodeType.Document,
        EntityReference = XmlNodeType.EntityReference,
        ProcessingInstruction = XmlNodeType.ProcessingInstruction,
        SignificantWhitespace = XmlNodeType.SignificantWhitespace,

        Namespace = 100,
        ShrankNode = 101,
    }

    internal enum XmlDiffOperation
    {
        Match = 0,
        Add = 1,
        Remove = 2,

        ChangeElementName = 3,
        ChangeElementAttr1 = 4,
        ChangeElementAttr2 = 5,
        ChangeElementAttr3 = 6,
        ChangeElementNameAndAttr1 = 7,
        ChangeElementNameAndAttr2 = 8,
        ChangeElementNameAndAttr3 = 9,

        ChangePI = 10,
        ChangeER = 11,
        ChangeCharacterData = 12,
        ChangeXmlDeclaration = 13,
        ChangeDTD = 14,

        Undefined = 15,

        ChangeAttr = 16,
    }

    internal enum EditScriptOperation
    {
        None = 0,
        Match = 1,
        Add = 2,
        Remove = 3,
        ChangeNode = 4,
        EditScriptReference = 5,
        EditScriptPostponed = 6,

        OpenedAdd = 7,
        OpenedRemove = 8,
        OpenedMatch = 9,
    }

    internal enum XmlDiffDescriptorType
    {
        Move,
        PrefixChange,
        NamespaceChange,
    }
}

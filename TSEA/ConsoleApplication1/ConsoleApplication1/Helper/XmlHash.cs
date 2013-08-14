using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Sam.XmlDiffPath
{
    internal class XmlHash
    {
        #region Fields
        
        bool isIgnoreChildOrder = false;
        bool isIgnoreComments = false;
        bool isIgnorePI = false;
        bool isIgnoreWhitespace = false;
        bool isIgnoreNamespaces = false;
        bool isIgnorePrefixes = false;
        bool isIgnoreXmlDecl = false;
        bool isIgnoreDtd = false;

        const string Delimiter = "\0x01";

        #endregion

        // Constructor

        internal XmlHash(XmlDiff xmlDiff)
        {
            // set flags
            isIgnoreChildOrder = xmlDiff.IgnoreChildOrder;
            isIgnoreComments = xmlDiff.IgnoreComments;
            isIgnorePI = xmlDiff.IgnorePI;
            isIgnoreWhitespace = xmlDiff.IgnoreWhitespace;
            isIgnoreNamespaces = xmlDiff.IgnoreNamespaces;
            isIgnorePrefixes = xmlDiff.IgnorePrefixes;
            isIgnoreXmlDecl = xmlDiff.IgnoreXmlDecl;
            isIgnoreDtd = xmlDiff.IgnoreDtd;
        }

        internal XmlHash()
        {
        }

        // Methods

        private void ClearFlags()
        {
            isIgnoreChildOrder = false;
            isIgnoreComments = false;
            isIgnorePI = false;
            isIgnoreWhitespace = false;
            isIgnoreNamespaces = false;
            isIgnorePrefixes = false;
            isIgnoreXmlDecl = false;
            isIgnoreDtd = false;
        }

        internal ulong ComputeHash(XmlNode node, XmlDiffOptions options)
        {
            isIgnoreChildOrder = (((int)options & (int)(XmlDiffOptions.IgnoreChildOrder)) > 0);
            isIgnoreComments = (((int)options & (int)(XmlDiffOptions.IgnoreComments)) > 0);
            isIgnorePI = (((int)options & (int)(XmlDiffOptions.IgnorePI)) > 0);
            isIgnoreWhitespace = (((int)options & (int)(XmlDiffOptions.IgnoreWhitespace)) > 0);
            isIgnoreNamespaces = (((int)options & (int)(XmlDiffOptions.IgnoreNamespaces)) > 0);
            isIgnorePrefixes = (((int)options & (int)(XmlDiffOptions.IgnorePrefixes)) > 0);
            isIgnoreXmlDecl = (((int)options & (int)(XmlDiffOptions.IgnoreXmlDecl)) > 0);
            isIgnoreDtd = (((int)options & (int)(XmlDiffOptions.IgnoreDtd)) > 0);

            return ComputeHash(node);
        }

        internal ulong ComputeHash(XmlNode node)
        {
            switch (node.NodeType)
            {
                case XmlNodeType.Document:
                    return ComputeHashXmlDocument((XmlDocument)node);
                case XmlNodeType.DocumentFragment:
                    return ComputeHashXmlFragment((XmlDocumentFragment)node);
                default:
                    return ComputeHashXmlNode(node);
            }
        }

        private ulong ComputeHashXmlDocument(XmlDocument doc)
        {
            HashAlgorithm ha = new HashAlgorithm();
            HashDocument(ha);
            ComputeHashXmlChildren(ha, doc);
            return ha.Hash;
        }

        private ulong ComputeHashXmlFragment(XmlDocumentFragment frag)
        {
            HashAlgorithm ha = new HashAlgorithm();
            ComputeHashXmlChildren(ha, frag);
            return ha.Hash;
        }

        internal ulong ComputeHashXmlDiffDocument(XmlDiffDocument doc)
        {
            HashAlgorithm ha = new HashAlgorithm();
            HashDocument(ha);
            ComputeHashXmlDiffChildren(ha, doc);
            return ha.Hash;
        }

        internal ulong ComputeHashXmlDiffElement(XmlDiffElement el)
        {
            HashAlgorithm ha = new HashAlgorithm();
            HashElement(ha, el.LocalName, el.Prefix, el.NamespaceURI);
            ComputeHashXmlDiffAttributes(ha, el);
            ComputeHashXmlDiffChildren(ha, el);
            return ha.Hash;
        }

        private void ComputeHashXmlDiffAttributes(HashAlgorithm ha, XmlDiffElement el)
        {
            int attrCount = 0;
            ulong attrHashAll = 0;
            XmlDiffAttributeOrNamespace curAttrOrNs = el.attributes;
            while (curAttrOrNs != null)
            {
                attrHashAll += curAttrOrNs.HashValue;
                attrCount++;
                curAttrOrNs = (XmlDiffAttributeOrNamespace)curAttrOrNs.nextSibling;
            }

            if (attrCount > 0)
            {
                ha.AddULong(attrHashAll);
                ha.AddInt(attrCount);
            }
        }

        private void ComputeHashXmlDiffChildren(HashAlgorithm ha, XmlDiffParentNode parent)
        {
            int childrenCount = 0;
            if (isIgnoreChildOrder)
            {
                ulong totalHash = 0;
                XmlDiffNode curChild = parent.FirstChildNode;
                while (curChild != null)
                {
                    Debug.Assert(!(curChild is XmlDiffAttributeOrNamespace));
                    Debug.Assert(curChild.HashValue != 0);

                    totalHash += curChild.HashValue;
                    childrenCount++;
                    curChild = curChild.nextSibling;
                }
                ha.AddULong(totalHash);
            }
            else
            {
                XmlDiffNode curChild = parent.FirstChildNode;
                while (curChild != null)
                {
                    Debug.Assert(!(curChild is XmlDiffAttributeOrNamespace));
                    Debug.Assert(curChild.HashValue != 0);

                    ha.AddULong(curChild.HashValue);
                    childrenCount++;
                    curChild = curChild.nextSibling;
                }
            }

            if (childrenCount != 0)
                ha.AddInt(childrenCount);
        }

        private void ComputeHashXmlChildren(HashAlgorithm ha, XmlNode parent)
        {
            XmlElement el = parent as XmlElement;
            if (el != null)
            {
                ulong attrHashSum = 0;
                int attrsCount = 0;
                XmlAttributeCollection attrs = ((XmlElement)parent).Attributes;
                for (int i = 0; i < attrs.Count; i++)
                {
                    XmlAttribute attr = (XmlAttribute)attrs.Item(i);

                    ulong hashValue = 0;

                    // default namespace def
                    if (attr.LocalName == "xmlns" && attr.Prefix == string.Empty)
                    {
                        if (isIgnoreNamespaces)
                        {
                            continue;
                        }
                        hashValue = HashNamespace(string.Empty, attr.Value);
                    }
                    // namespace def
                    else if (attr.Prefix == "xmlns")
                    {
                        if (isIgnoreNamespaces)
                        {
                            continue;
                        }
                        hashValue = HashNamespace(attr.LocalName, attr.Value);
                    }
                    // attribute
                    else
                    {
                        if (isIgnoreWhitespace)
                            hashValue = HashAttribute(attr.LocalName, attr.Prefix, attr.NamespaceURI, XmlDiff.NormalizeText(attr.Value));
                        else
                            hashValue = HashAttribute(attr.LocalName, attr.Prefix, attr.NamespaceURI, attr.Value);
                    }

                    Debug.Assert(hashValue != 0);

                    attrsCount++;
                    attrHashSum += hashValue;
                }

                if (attrsCount != 0)
                {
                    ha.AddULong(attrHashSum);
                    ha.AddInt(attrsCount);
                }
            }

            int childrenCount = 0;
            if (isIgnoreChildOrder)
            {
                ulong totalHashSum = 0;
                XmlNode curChild = parent.FirstChild;
                while (curChild != null)
                {
                    ulong hashValue = ComputeHashXmlNode(curChild);
                    if (hashValue != 0)
                    {
                        totalHashSum += hashValue;
                        childrenCount++;
                    }
                    curChild = curChild.NextSibling;
                }
                ha.AddULong(totalHashSum);
            }
            else
            {
                XmlNode curChild = parent.FirstChild;
                while (curChild != null)
                {
                    ulong hashValue = ComputeHashXmlNode(curChild);
                    if (hashValue != 0)
                    {
                        ha.AddULong(hashValue);
                        childrenCount++;
                    }
                    curChild = curChild.NextSibling;
                }
            }
            if (childrenCount != 0)
                ha.AddInt(childrenCount);
        }

        private ulong ComputeHashXmlNode(XmlNode node)
        {
            switch (node.NodeType)
            {
                case XmlNodeType.Element:
                    {
                        XmlElement el = (XmlElement)node;
                        HashAlgorithm ha = new HashAlgorithm();

                        HashElement(ha, el.LocalName, el.Prefix, el.NamespaceURI);
                        ComputeHashXmlChildren(ha, el);

                        return ha.Hash;
                    }
                case XmlNodeType.Attribute:
                    // attributes are hashed in ComputeHashXmlChildren;
                    Debug.Assert(false);
                    return 0;

                case XmlNodeType.Whitespace:
                    return 0;

                case XmlNodeType.SignificantWhitespace:
                    if (!isIgnoreWhitespace)
                        goto case XmlNodeType.Text;
                    return 0;
                case XmlNodeType.Comment:
                    if (!isIgnoreComments)
                        return HashCharacterNode(XmlNodeType.Comment, ((XmlCharacterData)node).Value);
                    return 0;
                case XmlNodeType.Text:
                    {
                        XmlCharacterData cd = (XmlCharacterData)node;
                        if (isIgnoreWhitespace)
                            return HashCharacterNode(cd.NodeType, XmlDiff.NormalizeText(cd.Value));
                        else
                            return HashCharacterNode(cd.NodeType, cd.Value);
                    }
                case XmlNodeType.CDATA:
                    {
                        XmlCharacterData cd = (XmlCharacterData)node;
                        return HashCharacterNode(cd.NodeType, cd.Value);
                    }
                case XmlNodeType.ProcessingInstruction:
                    {
                        if (isIgnorePI)
                            return 0;

                        XmlProcessingInstruction pi = (XmlProcessingInstruction)node;
                        return HashPI(pi.Target, pi.Value);
                    }
                case XmlNodeType.EntityReference:
                    {
                        XmlEntityReference er = (XmlEntityReference)node;
                        return HashER(er.Name);
                    }
                case XmlNodeType.XmlDeclaration:
                    {
                        if (isIgnoreXmlDecl)
                            return 0;
                        XmlDeclaration decl = (XmlDeclaration)node;
                        return HashXmlDeclaration(XmlDiff.NormalizeXmlDeclaration(decl.Value));
                    }
                case XmlNodeType.DocumentType:
                    {
                        if (isIgnoreDtd)
                            return 0;
                        XmlDocumentType docType = (XmlDocumentType)node;
                        return HashDocumentType(docType.Name, docType.PublicId, docType.SystemId, docType.InternalSubset);
                    }
                case XmlNodeType.DocumentFragment:
                    return 0;
                default:
                    Debug.Assert(false);
                    return 0;
            }
        }

        private void HashDocument(HashAlgorithm ha)
        {
            // Intentionally empty
        }

        internal void HashElement(HashAlgorithm ha, string localName, string prefix, string ns)
        {
            ha.AddString((int)(XmlNodeType.Element) +
                          Delimiter +
                          ((isIgnoreNamespaces || isIgnorePrefixes) ? string.Empty : prefix) +
                          Delimiter +
                          (isIgnoreNamespaces ? string.Empty : ns) +
                          Delimiter +
                          localName);
        }

        internal ulong HashAttribute(string localName, string prefix, string ns, string value)
        {
            return HashAlgorithm.GetHash((int)XmlNodeType.Attribute +
                                          Delimiter +
                                          ((isIgnoreNamespaces || isIgnorePrefixes) ? string.Empty : prefix) +
                                          Delimiter +
                                          (isIgnoreNamespaces ? string.Empty : ns) +
                                          Delimiter +
                                          localName +
                                          Delimiter +
                                          value);
        }

        internal ulong HashNamespace(string prefix, string ns)
        {
            Debug.Assert(!isIgnoreNamespaces);

            return HashAlgorithm.GetHash((int)XmlDiffNodeType.Namespace +
                                          Delimiter +
                                          (isIgnorePrefixes ? string.Empty : prefix) +
                                          Delimiter +
                                          ns);
        }

        internal ulong HashCharacterNode(XmlNodeType nodeType, string value)
        {
            return HashAlgorithm.GetHash(((int)nodeType).ToString() +
                                          Delimiter +
                                          value);
        }

        internal ulong HashPI(string target, string value)
        {
            return HashAlgorithm.GetHash(((int)XmlNodeType.ProcessingInstruction).ToString() +
                                          Delimiter +
                                          target +
                                          Delimiter +
                                          value);
        }

        internal ulong HashER(string name)
        {
            return HashAlgorithm.GetHash(((int)XmlNodeType.EntityReference).ToString() +
                                          Delimiter +
                                           name);
        }

        internal ulong HashXmlDeclaration(string value)
        {
            return HashAlgorithm.GetHash(((int)XmlNodeType.XmlDeclaration).ToString() +
                                          Delimiter +
                                           value);
        }

        internal ulong HashDocumentType(string name, string publicId, string systemId, string subset)
        {
            return HashAlgorithm.GetHash(((int)XmlNodeType.DocumentType).ToString() +
                                          Delimiter +
                                          name +
                                          Delimiter +
                                          publicId +
                                          Delimiter +
                                          systemId +
                                          Delimiter +
                                          subset);
        }
    }

    internal class HashAlgorithm
    {
        // Fields
        ulong _hash;

        // Constructor
        internal HashAlgorithm()
        {
        }

        // Properties
        internal ulong Hash { get { return _hash; } }

        // Methods
        static internal ulong GetHash(string data)
        {
            return GetHash(data, 0);
        }

        internal void AddString(string data)
        {
            _hash = GetHash(data, _hash);
        }

        internal void AddInt(int i)
        {
            _hash += (_hash << 11) + (ulong)i;
        }

        internal void AddULong(ulong u)
        {
            _hash += (_hash << 11) + u;
        }

        static private ulong GetHash(string data, ulong hash)
        {
            hash += (hash << 13) + (ulong)data.Length;
            for (int i = 0; i < data.Length; i++)
                hash += (hash << 17) + data[i];
            return hash;
        }

    }
}

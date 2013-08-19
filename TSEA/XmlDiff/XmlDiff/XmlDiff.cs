using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sam.XmlDiff
{
    using System.Collections;
    using System.IO;
    using System.Xml;

    public class XmlDiff
    {
        #region Fields

        private string originalXsd;

        private string changedXsd;

        private XmlDocument originalXmlDoc;

        private XmlDocument changedXmlDoc;

        private List<MismatchedElementPair> mismatchedNodePairs = new List<MismatchedElementPair>();

        private List<MismatchedAttributePair> mismatchedAttrPairs = new List<MismatchedAttributePair>();
        #endregion

        #region Constructors

        public XmlDiff(string sourceFile)
        {
            this.originalXsd = sourceFile;
        }

        public XmlDiff(string sourceFile, string changedFile)
        {
            this.originalXsd = sourceFile;
            this.changedXsd = changedFile;
        }

        #endregion

        #region Properties

        public List<string> ElementsWithRefAttribute { get; private set; }

        public List<XmlNode> InteranlRefNodes { get; private set; }

        public List<XmlNode> ExternalRefNodes { get; private set; }

        public List<XmlNode> InternalConNodes { get; private set; }

        public int ElementCount { get; private set; }

        #endregion

        #region Methods

        public void Parse()
        {
            if (!string.IsNullOrEmpty(this.originalXsd))
            {
                this.originalXmlDoc = new XmlDocument();
                this.originalXmlDoc.Load(this.originalXsd);

                // Parse, expand, and save the new doc
                this.Preprocess(ref this.originalXmlDoc, this.originalXsd);
            }

            if (!string.IsNullOrEmpty(this.changedXsd))
            {
                this.changedXmlDoc = new XmlDocument();
                this.changedXmlDoc.Load(this.changedXsd);

                this.Preprocess(ref this.changedXmlDoc, this.changedXsd);
            }

            this.Display("Parsing is done!");
        }

        public void Diff()
        {
            #region TODO: TO BE REMOVED

            //XmlDocument originalXmlDoc = new XmlDocument();
            //XmlDocument changedXmlDoc = new XmlDocument();

            //try
            //{
            //    originalXmlDoc.Load(this.originalXsd);

            //    // parse internal and external ref-nodes
            //    this.Preprocess(ref originalXmlDoc, this.originalXsd);

            //    // save the expanded XML doc
            //    this.Save(ref originalXmlDoc, this.originalXsd);

            //    changedXmlDoc.Load(this.changedXsd);
            //    this.Preprocess(ref changedXmlDoc, this.changedXsd);
            //    this.Save(ref changedXmlDoc, this.changedXsd);

            //    this.Display("All done!");
            //}
            //catch (System.Exception ex)
            //{
            //    this.Display(ex.Message);
            //}

            #endregion

            if (this.originalXmlDoc.DocumentElement != null && this.changedXmlDoc.DocumentElement != null)
            {
                this.Compare(this.originalXmlDoc.DocumentElement, this.changedXmlDoc.DocumentElement);
            }

            this.GenerateDelta();
        }

        public void GenerateDelta()
        {
            if (this.mismatchedAttrPairs.Count > 0)
            {
                // TODO
            }

            if (this.mismatchedNodePairs.Count > 0)
            {
                // TODO
            }
        }

        #endregion

        #region Private Methods

        #region Private Methods for Parse

        private void Preprocess(ref XmlDocument xmlDoc, string xsdFile)
        {
            this.ElementsWithRefAttribute = null;
            this.InteranlRefNodes = null;
            this.ExternalRefNodes = null;
            this.InternalConNodes = null;

            try
            {
                XmlNodeList nodes;

                #region Parse internal ref-nodes
                nodes = xmlDoc.GetElementsByTagName("xs:element");
                this.ElementCount = nodes.Count;

                foreach (XmlNode node in nodes)
                {
                    XmlElement element = node as XmlElement;
                    if (element != null && element.HasAttribute("ref"))
                    {
                        XmlElement parent = node.ParentNode as XmlElement;

                        // filter and store ref-elements
                        if (this.ElementsWithRefAttribute == null)
                        {
                            this.ElementsWithRefAttribute = new List<string>();
                        }

                        if (this.InteranlRefNodes == null)
                        {
                            this.InteranlRefNodes = new List<XmlNode>();
                        }

                        if (!element.GetAttribute("ref").Contains(":"))
                        {
                            // handle internal ref node
                            this.InteranlRefNodes.Add(node);
                        }

                        this.ElementsWithRefAttribute.Add(element.GetAttribute("ref"));
                    }
                    else
                    {
                        // filter and store non-ref nodes
                        if (this.InternalConNodes == null)
                        {
                            this.InternalConNodes = new List<XmlNode>();
                        }

                        this.InternalConNodes.Add(node);
                    }
                }

                #endregion

                this.ExpandInternalRefNodes();

                #region Parse external ref-nodes
                nodes = xmlDoc.GetElementsByTagName("xs:element");
                this.ElementCount = nodes.Count;
                string attributeValue;

                foreach (XmlNode node in nodes)
                {
                    XmlElement element = node as XmlElement;

                    // filter ref-nodes
                    if (element != null && (element.HasAttribute("ref") || element.HasAttribute("type")))
                    {
                        // filter and store ref-elements
                        if (this.ElementsWithRefAttribute == null)
                        {
                            this.ElementsWithRefAttribute = new List<string>();
                        }

                        if (this.ExternalRefNodes == null)
                        {
                            this.ExternalRefNodes = new List<XmlNode>();
                        }

                        if (element.HasAttribute("ref"))
                        {
                            attributeValue = element.GetAttribute("ref");
                        }
                        else
                        {
                            attributeValue = element.GetAttribute("type");
                        }

                        if (attributeValue.Contains(":") && !attributeValue.Contains("xs:"))
                        {
                            // Handle external ref node
                            this.ExternalRefNodes.Add(node);
                        }
                    }
                }
                #endregion

                this.ExpandExternalRefNodes(ref xmlDoc, xsdFile);

                // save the expanded XML doc
                this.Save(ref xmlDoc, xsdFile);
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine("Exception: {0}", ex.Message);
            }

        }

        private void Expand(ref XmlDocument xmlDoc)
        {
            try
            {
                // expand internal ref nodes
                this.ExpandInternalRefNodes();

                // expand external ref nodes
                //this.ExpandExternalRefNodes(ref xmlDoc);
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine("Exception: {0}", ex.Message);
            }
        }

        private void ExpandInternalRefNodes()
        {
            int counter = 0;
            foreach (XmlNode refNode in this.InteranlRefNodes)
            {
                XmlElement refElement = refNode as XmlElement;
                if (refElement != null && refElement.HasAttribute("ref"))
                {
                    string refAttrValue = refElement.GetAttribute("ref");

                    // replace the elements holding an internal 'ref' attribute with the concrete node.
                    foreach (XmlNode conNode in this.InternalConNodes)
                    {
                        XmlElement conElement = conNode as XmlElement;
                        if (conElement != null && conElement.HasAttribute("name") && string.Equals(refAttrValue, conElement.GetAttribute("name")))
                        {
                            // append non-ref attributes of refNode to conNode
                            foreach (XmlAttribute attribute in refNode.Attributes)
                            {
                                if (attribute.Name != "ref")
                                {
                                    conNode.Attributes.Append(attribute);
                                }
                            }

                            XmlNode newNode = conNode.Clone();

                            // replace an internal ref node with its corresponding concrete node
                            XmlElement parent = refNode.ParentNode as XmlElement;
                            parent.ReplaceChild(newNode, refNode);

                            break;
                        }
                    }
                }

                counter++;
            }
        }

        private void ExpandExternalRefNodes(ref XmlDocument xmlDoc, string xsdFile)
        {
            string externalFileName;
            XmlDocument externalDoc;
            Dictionary<string, string> externalFiles = null;

            if (string.IsNullOrEmpty(xsdFile))
            {
                throw new ArgumentException("The second argument 'xsdFile' should not be empty or null.");
            }

            // get external file names
            XmlNodeList importNodes = xmlDoc.GetElementsByTagName("xs:import");
            foreach (XmlNode node in importNodes)
            {
                XmlElement importElement = node as XmlElement;
                if (importElement.HasAttribute("namespace") && importElement.HasAttribute("schemaLocation"))
                {
                    if (externalFiles == null)
                    {
                        externalFiles = new Dictionary<string, string>();
                    }

                    externalFiles.Add(importElement.GetAttribute("namespace").ToLower(), importElement.GetAttribute("schemaLocation"));
                }

            }

            foreach (XmlNode refNode in this.ExternalRefNodes)
            {
                XmlElement refElement = refNode as XmlElement;
                string[] refValue = null;
                bool hasTypeAttribute = false;

                if (refElement.HasAttribute("ref"))
                {
                    refValue = refElement.GetAttribute("ref").Split(':');
                }
                else if (refElement.HasAttribute("type"))
                {
                    refValue = refElement.GetAttribute("type").Split(':');
                    hasTypeAttribute = true;
                }

                if (refValue == null)
                {
                    this.Display("The attribute does not exist.");
                }

                externalFileName = Path.Combine(Path.GetDirectoryName(xsdFile), externalFiles[refValue[0].ToLower()]);
                externalDoc = new XmlDocument();
                externalDoc.Load(externalFileName);

                XmlNodeList conNodes = null;

                if (hasTypeAttribute)
                {
                    conNodes = externalDoc.GetElementsByTagName("xs:simpleType");
                }
                else
                {
                    conNodes = externalDoc.GetElementsByTagName("xs:element");
                }

                foreach (XmlNode conNode in conNodes)
                {
                    XmlElement conElement = conNode as XmlElement;
                    if (conElement != null && conElement.HasAttribute("name") && string.Equals(refValue[1], conElement.GetAttribute("name")))
                    {
                        if (hasTypeAttribute)
                        {
                            conElement.RemoveAttribute("name");
                        }

                        XmlNode newNode = xmlDoc.ImportNode(conNode, true);

                        if (hasTypeAttribute)
                        {
                            refElement.RemoveAttribute("type");

                            // replace an internal ref node with its corresponding concrete node
                            refNode.AppendChild(newNode);
                        }
                        else
                        {
                            // append non-ref attributes of refNode to conNode
                            foreach (XmlAttribute attribute in refNode.Attributes)
                            {
                                if (attribute.Name != "ref")
                                {
                                    XmlElement newElement = newNode as XmlElement;
                                    newElement.SetAttribute(attribute.Name, attribute.Value);

                                    break;
                                }
                            }

                            // replace an internal ref node with its corresponding concrete node
                            XmlElement parent = refNode.ParentNode as XmlElement;
                            parent.ReplaceChild(newNode, refNode);
                        }

                        break;
                    }
                }
            }
        }

        #endregion

        #region Private Methods for Diff
        
        private void Compare(XmlNode sourceNode, XmlNode changedNode)
        {
            if (!string.Equals(sourceNode.Name, changedNode.Name))
            {
                mismatchedNodePairs.Add(new MismatchedElementPair(sourceNode, changedNode));

                if (sourceNode.HasChildNodes && changedNode.HasChildNodes)
                {
                    // TODO: handling the child elements of the mismatched non-leaf elements
                    this.Compare(sourceNode.FirstChild, changedNode.FirstChild);
                }
            }

            // compare the value of "name" attribute
            XmlElement sourceElement = sourceNode as XmlElement;
            XmlElement changedElement = changedNode as XmlElement;

            // compare attributes and get those mismatched ones.
            List<MismatchedAttributePair> mismatchedAttributes = this.CompareAttributes(sourceElement.Attributes, changedElement.Attributes);
            
            if (mismatchedAttributes.Count > 0)
            {
                foreach (MismatchedAttributePair misAttr in mismatchedAttributes)
                {
                    misAttr.SourceOwnerNode = sourceNode;
                    misAttr.ChangedOwnerNode = changedNode;
                }

                this.mismatchedAttrPairs.AddRange(mismatchedAttributes);
            }

            if (sourceNode.HasChildNodes && changedNode.HasChildNodes)
            {
                this.Compare(sourceNode.FirstChild, changedNode.FirstChild);
            }

            if (sourceNode.NextSibling != null && changedNode.NextSibling != null)
            {
                this.Compare(sourceNode.NextSibling, changedNode.NextSibling);
            }
        }

        private List<MismatchedAttributePair> CompareAttributes(XmlAttributeCollection sourceAttrCollection, XmlAttributeCollection changedAttrCollection)
        {
            List<MismatchedAttributePair> mismatchedAttributes = new List<MismatchedAttributePair>();

            if (sourceAttrCollection.Count == changedAttrCollection.Count)
            {
                if (sourceAttrCollection.Count != 0)
                {
                    mismatchedAttributes = this.CompareSimpleAttributes(sourceAttrCollection, changedAttrCollection);
                }
            }
            else
            {
                mismatchedAttributes = this.CompareComplexAttributes(sourceAttrCollection, changedAttrCollection);
            }

            return mismatchedAttributes;
        }

        private List<MismatchedAttributePair> CompareSimpleAttributes(XmlAttributeCollection sourceAttrCollection, XmlAttributeCollection changedAttrCollection)
        {
            List<MismatchedAttributePair> mismatchedAttributes = new List<MismatchedAttributePair>();

            foreach (XmlAttribute attrSource in sourceAttrCollection)
            {
                foreach (XmlAttribute attrChanged in changedAttrCollection)
                {
                    if (string.Equals(attrSource.Name, attrChanged.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!string.Equals(attrSource.Value, attrChanged.Value))
                        {
                            mismatchedAttributes.Add(new MismatchedAttributePair(attrSource, attrChanged));
                        }

                        break;
                    }
                }
            }

            return mismatchedAttributes;
        }

        private List<MismatchedAttributePair> CompareComplexAttributes(XmlAttributeCollection sourceAttrCollection, XmlAttributeCollection changedAttrCollection)
        {
            bool isMatched = false;
            List<MismatchedAttributePair> mismatchedAttributes = new List<MismatchedAttributePair>();
            XmlAttributeCollection bigAttrCollection;
            XmlAttributeCollection smallAttrCollection;

            bool flag = sourceAttrCollection.Count > changedAttrCollection.Count;
            if (flag)
            {
                bigAttrCollection = sourceAttrCollection;
                smallAttrCollection = changedAttrCollection;
            }
            else
            {
                bigAttrCollection = changedAttrCollection;
                smallAttrCollection = sourceAttrCollection;
            }

            foreach (XmlAttribute bAttr in bigAttrCollection)
            {
                foreach (XmlAttribute sAttr in smallAttrCollection)
                {
                    if (string.Equals(bAttr.Name, sAttr.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!string.Equals(bAttr.Value, sAttr.Value))
                        {
                            this.Display(string.Format(
                                "Attribute Diff: source: {0}=\"{1}\"; changed: {2}=\"{3}\".",
                                bAttr.Name, bAttr.Value,
                                sAttr.Name, sAttr.Value));

                            if (flag)
                            {
                                mismatchedAttributes.Add(new MismatchedAttributePair(bAttr, sAttr));
                            }
                            else
                            {
                                mismatchedAttributes.Add(new MismatchedAttributePair(sAttr, bAttr));
                            }
                        }

                        isMatched = true;
                        break;
                    }
                }

                if (!isMatched)
                {
                    if (flag)
                    {
                        mismatchedAttributes.Add(new MismatchedAttributePair(bAttr, null));
                    }
                    else
                    {
                        mismatchedAttributes.Add(new MismatchedAttributePair(null, bAttr));
                    }
                }

                isMatched = false;
            }

            return mismatchedAttributes;
        }

        #endregion

        private void Display(string message)
        {
            Console.WriteLine("Message: " + message);
        }

        private void Save(ref XmlDocument doc, string xsdFile)
        {
            if (string.IsNullOrEmpty(xsdFile))
            {
                throw new ArgumentException("The second argument 'xsdFile' should not be empty or null.");
            }

            string filename = Path.Combine(Path.GetDirectoryName(xsdFile), "merge" + DateTime.Now.GetHashCode() + ".xsd");
            doc.Save(filename);

            this.Display("Save Done!");
            this.Display("You can locate the file in " + filename);
        }

        #endregion

    }

    public enum MismatchedType
    {
        TypeToSimpleType,
        SimpleTypeToType,

        ChangeTypeValue,
        RemoveType,
        AddType,

        ChangeQuantifierValue,
        RemoveQuantifier,
        AddQuantifier
    }

    public class MismatchedElementPair
    {
        public MismatchedElementPair(XmlNode sourceNode, XmlNode changedNode)
        {
            this.SourceNode = sourceNode;
            this.ChangedNode = changedNode;
        }

        public XmlNode SourceNode { get; set; }
        public XmlNode ChangedNode { get; set; }
    }

    public class MismatchedAttributePair
    {
        public MismatchedAttributePair(XmlAttribute sourceAttribute, XmlAttribute changedAttribute)
        {
            this.SourceAttribute = sourceAttribute;
            this.ChangedAttribute = changedAttribute;
        }

        public XmlAttribute SourceAttribute { get; set; }
        public XmlAttribute ChangedAttribute { get; set; }

        public XmlNode SourceOwnerNode { get; set; }
        public XmlNode ChangedOwnerNode { get; set; }
    }
}

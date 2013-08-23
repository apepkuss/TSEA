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

        public List<XmlNode> InternalRefNodes { get; private set; }

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

            this.GenerateDeltaFile();
        }

        public void GenerateDeltaFile()
        {
            if (this.mismatchedNodePairs.Count > 0)
            {
                // TODO
                this.Display("Output mismatched node pairs, done!");
            }
        }

        #endregion

        #region Private Methods

        #region Private Methods for Parse

        private void Preprocess(ref XmlDocument xmlDoc, string xsdFile)
        {
            bool isExpanded = false;
            this.ElementsWithRefAttribute = null;
            this.InternalRefNodes = null;
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

                        if (this.InternalRefNodes == null)
                        {
                            this.InternalRefNodes = new List<XmlNode>();
                        }

                        if (!element.GetAttribute("ref").Contains(":"))
                        {
                            // handle internal ref node
                            this.InternalRefNodes.Add(node);
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

                if (this.InternalRefNodes != null)
                {
                    this.ExpandInternalRefNodes();
                    isExpanded = true;
                }
                
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

                if (this.ExternalRefNodes != null)
                {
                    this.ExpandExternalRefNodes(ref xmlDoc, xsdFile);
                    isExpanded = true;
                }
                
                if (isExpanded)
                {
                    // save the expanded XML doc
                    this.SaveExpandedXDoc(ref xmlDoc, xsdFile);
                }
            }
            catch (FileNotFoundException ex)
            {
                this.Display(string.Format("Exception: {0}", ex.Message));
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
            foreach (XmlNode refNode in this.InternalRefNodes)
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
                                    XmlAttribute additionalAttr = attribute.Clone() as XmlAttribute;
                                    conNode.Attributes.Append(additionalAttr);
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

                try
                {
                    externalDoc.Load(externalFileName);
                }
                catch (System.Exception ex)
                {
                    this.Display(string.Format("Exception: {0}", ex.Message));
                    return;
                }
                

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
            // compare current node pair
            if (!string.Equals(sourceNode.Name, changedNode.Name, StringComparison.OrdinalIgnoreCase))
            {
                MismatchedElementPair pair = new MismatchedElementPair(sourceNode, changedNode);

                #region Set mismatched type
                
                if (string.Equals(sourceNode.LocalName, "complexType", StringComparison.OrdinalIgnoreCase) && 
                    string.Equals(changedNode.LocalName, "simpleType", StringComparison.OrdinalIgnoreCase))
                {
                    pair.MismatchedType = EvolutionTypes.ComplexTypeToSimpleType;
                }

                if (string.Equals(sourceNode.LocalName, "simpleType", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(changedNode.LocalName, "complexType", StringComparison.OrdinalIgnoreCase))
                {
                    pair.MismatchedType = EvolutionTypes.SimpleTypeToComplexType;
                }

                #endregion

                mismatchedNodePairs.Add(pair);

                if (pair.MismatchedType == EvolutionTypes.ComplexTypeToSimpleType || 
                    pair.MismatchedType == EvolutionTypes.SimpleTypeToComplexType)
                {
                    return;
                }
            }
            else
            {
                XmlElement sourceElement = sourceNode as XmlElement;
                XmlElement changedElement = changedNode as XmlElement;

                if (string.Equals(sourceNode.LocalName, "group", StringComparison.OrdinalIgnoreCase) &&
                    sourceElement.GetAttribute("name") != "GhostingProps")
                {
                    // TODO: this IF statement will stop the compare operation on "xs:group" element.

                    // traverse their sibling nodes
                    if (sourceNode.NextSibling != null && changedNode.NextSibling != null)
                    {
                        this.Compare(sourceNode.NextSibling, changedNode.NextSibling);
                    }

                    return;
                }

                // compare attributes and get those mismatched ones.
                List<MismatchedAttributePair> mismatchedAttributes = this.CompareAttributes(sourceElement.Attributes, changedElement.Attributes);

                if (mismatchedAttributes.Count > 0)
                {
                    MismatchedElementPair pair = new MismatchedElementPair(sourceNode, changedNode);

                    foreach (MismatchedAttributePair misAttr in mismatchedAttributes)
                    {
                        misAttr.SourceOwnerNode = sourceNode;
                        misAttr.ChangedOwnerNode = changedNode;

                        if (misAttr.MismatchedType == EvolutionTypes.TypeChange_Remove)
                        {
                            misAttr.MismatchedType = this.CheckTypeChange(sourceNode, changedNode, misAttr);
                        }
                        else if (misAttr.MismatchedType == EvolutionTypes.TypeChange_Add)
                        {
                            if (string.Equals(sourceNode.FirstChild.LocalName, "simpleType"))
                            {
                                misAttr.MismatchedType = EvolutionTypes.SimpleTypeToType;
                            }
                            else if (string.Equals(sourceNode.FirstChild.LocalName, "complexType"))
                            {
                                misAttr.MismatchedType = EvolutionTypes.ComplexTypeToType;
                            }
                        }
                        else if (string.Equals(sourceNode.LocalName, "maxLength", StringComparison.OrdinalIgnoreCase))
                        {
                            if (Convert.ToInt32(misAttr.SourceAttribute.Value) > Convert.ToInt32(misAttr.ChangedAttribute.Value))
                            {
                                misAttr.MismatchedType = EvolutionTypes.ChangeRestriction_MaxLength_Decreased;
                            } 
                            else
                            {
                                misAttr.MismatchedType = EvolutionTypes.ChangeRestriction_MaxLength_Increased;
                            }
                            
                            pair.MismatchedType = EvolutionTypes.ChangeRestriction_MaxLength;
                        }
                        else if (string.Equals(sourceNode.LocalName, "import", StringComparison.OrdinalIgnoreCase))
                        {
                            pair.MismatchedType = EvolutionTypes.ImportElementChange;
                        }
                    }

                    pair.AddMismatchedAttributes(mismatchedAttributes.ToArray());
                    this.mismatchedNodePairs.Add(pair);
                }
            }
            

            // traverse their child nodes
            if (sourceNode.HasChildNodes && changedNode.HasChildNodes)
            {
                this.Compare(sourceNode.FirstChild, changedNode.FirstChild);
            }

            // traverse their sibling nodes
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
                            // Check the type of the pair of mismatched attributes.
                            MismatchedAttributePair pair = this.CheckAttrMismatchedType(attrSource, attrChanged);
                            mismatchedAttributes.Add(pair);

                            break;
                        }
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
                            // record the mis-matched attribute pair
                            if (flag)
                            {
                                MismatchedAttributePair pair = this.CheckAttrMismatchedType(bAttr, sAttr);
                                mismatchedAttributes.Add(pair);
                            }
                            else
                            {
                                MismatchedAttributePair pair = this.CheckAttrMismatchedType(sAttr, bAttr);
                                mismatchedAttributes.Add(pair);
                            }
                        }

                        isMatched = true;
                        break;
                    }
                }

                // handling the redundant attribute
                if (!isMatched)
                {
                    if (flag)
                    {
                        MismatchedAttributePair pair = new MismatchedAttributePair(bAttr, null);
                        this.CheckRedAttrMismachedType(bAttr.Name, ref pair);

                        mismatchedAttributes.Add(pair);
                    }
                    else
                    {
                        MismatchedAttributePair pair = new MismatchedAttributePair(null, bAttr);
                        this.CheckRedAttrMismachedType(bAttr.Name, ref pair);

                        mismatchedAttributes.Add(pair);
                    }
                }

                isMatched = false;
            }

            return mismatchedAttributes;
        }

        private EvolutionTypes CheckTypeChange(XmlNode sourceNode, XmlNode changedNode, MismatchedAttributePair attribute)
        {
            if (attribute.ChangedAttribute == null)
            {
                if (string.Equals(changedNode.FirstChild.LocalName, "simpleType", StringComparison.OrdinalIgnoreCase))
                {
                    return this.CheckTypeChangeSimpleType(attribute.SourceAttribute.Value, changedNode.FirstChild);
                }
                else if (string.Equals(changedNode.FirstChild.LocalName, "complexType", StringComparison.OrdinalIgnoreCase))
                {
                    return this.CheckTypeChangeComplexType(attribute.SourceAttribute.Value, changedNode.FirstChild);
                }
                else
                {
                    throw new Exception("An exception thrown from CheckTypeChange method.");
                }
            }
            else if (attribute.SourceAttribute == null)
            {
                if (string.Equals(sourceNode.FirstChild.LocalName, "simpleType", StringComparison.OrdinalIgnoreCase))
                {
                    return this.CheckTypeChangeSimpleType(attribute.ChangedAttribute.Value, sourceNode.FirstChild);
                }
                else if (string.Equals(sourceNode.FirstChild.LocalName, "complexType", StringComparison.OrdinalIgnoreCase))
                {
                    return this.CheckTypeChangeComplexType(attribute.ChangedAttribute.Value, sourceNode.FirstChild);
                }
                else
                {
                    throw new Exception("An exception thrown from CheckTypeChange method.");
                }
                
            }
            else
            {
                throw new Exception("An exception thrown from CheckTypeChange method.");
            }
        }

        private EvolutionTypes CheckTypeChangeSimpleType(string value, XmlNode xmlNode)
        {
            XmlNode node = xmlNode;

            while (!string.Equals(node.LocalName, "restriction", StringComparison.OrdinalIgnoreCase))
            {
                node = node.FirstChild;
            }

            XmlElement element = node as XmlElement;
            if (!string.Equals(value, element.GetAttribute("base"), StringComparison.OrdinalIgnoreCase))
            {
                return EvolutionTypes.TypeChange_Update;
            }
            else
            {
                return EvolutionTypes.TypeToSimpleType;
            }

            //if (string.Equals(changedNode.FirstChild.LocalName, "simpleType"))
            //{
            //    attribute.MismatchedType = EvolutionTypes.TypeToSimpleType;
            //}
            //else if (string.Equals(changedNode.FirstChild.LocalName, "complexType"))
            //{
            //    attribute.MismatchedType = EvolutionTypes.TypeToComplexType;
            //}
        }

        private EvolutionTypes CheckTypeChangeComplexType(string p, XmlNode xmlNode)
        {
            throw new NotImplementedException();

            // TODO: need more logic to deal with this case.
        }

        private MismatchedAttributePair CheckAttrMismatchedType(XmlAttribute attrSource, XmlAttribute attrChanged)
        {
            MismatchedAttributePair pair = new MismatchedAttributePair(attrSource, attrChanged);

            // set EvolutionType
            if (string.Equals(attrSource.Value, attrChanged.Value, StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(attrSource.Name, "name", StringComparison.OrdinalIgnoreCase))
                {
                    pair.MismatchedType = EvolutionTypes.Element_NameAttribute_Update;
                }
            }
            else if (string.Equals(attrSource.Name, "type", StringComparison.OrdinalIgnoreCase))
            {
                pair.MismatchedType = EvolutionTypes.TypeChange_Update;
            }
            else if (string.Equals(attrSource.Name, "minOccurs", StringComparison.OrdinalIgnoreCase))
            {
                if (Convert.ToInt32(attrSource.Value) > Convert.ToInt32(attrChanged.Value))
                {
                    pair.MismatchedType = EvolutionTypes.DecreasedMinOccurs;
                }
                else
                {
                    pair.MismatchedType = EvolutionTypes.IncreasedMinOccurs;
                }
            }
            else if (string.Equals(attrSource.Name, "maxOccurs", StringComparison.OrdinalIgnoreCase))
            {
                if (Convert.ToInt32(attrSource.Value) > Convert.ToInt32(attrChanged.Value))
                {
                    pair.MismatchedType = EvolutionTypes.DecreasedMaxOccurs;
                }
                else
                {
                    pair.MismatchedType = EvolutionTypes.IncreasedMaxOccurs;
                }
            }
            else if (string.Equals(attrSource.Name, "namespace", StringComparison.OrdinalIgnoreCase))
            {
                pair.MismatchedType = EvolutionTypes.ImportElementChange_Namespace_Update;
            }
            else if (string.Equals(attrSource.Name, "schemaLocation", StringComparison.OrdinalIgnoreCase))
            {
                pair.MismatchedType = EvolutionTypes.ImportElementChange_SchemaLocation_Update;
            }

            return pair;
        }

        private void CheckRedAttrMismachedType(string name, ref MismatchedAttributePair pair)
        {
            if (string.Equals(name, "type", StringComparison.OrdinalIgnoreCase))
            {
                pair.MismatchedType = EvolutionTypes.TypeChange_Remove;
            }
            else if (string.Equals(name, "minOccurs", StringComparison.OrdinalIgnoreCase))
            {
                pair.MismatchedType = EvolutionTypes.RemoveMinOccurs;
            }
            else if (string.Equals(name, "maxOccurs", StringComparison.OrdinalIgnoreCase))
            {
                pair.MismatchedType = EvolutionTypes.RemoveMaxOccurs;
            }
            else if (string.Equals(name, "namespace", StringComparison.OrdinalIgnoreCase))
            {
                pair.MismatchedType = EvolutionTypes.ImportElementChange_Namespace_Remove;
            }
            else if (string.Equals(name, "schemaLocation", StringComparison.OrdinalIgnoreCase))
            {
                pair.MismatchedType = EvolutionTypes.ImportElementChange_SchemaLocation_Remove;
            }
        }
        #endregion

        private void Display(string message)
        {
            Console.WriteLine("Message: " + message);
        }

        /// <summary>
        /// Save the expanded XSD file to the physical disk, where the raw XSD file is.
        /// </summary>
        /// <param name="doc">The expanded XSD document, which an instance of XmlDocument type</param>
        /// <param name="xsdFile">The raw XSD file with its path information</param>
        private void SaveExpandedXDoc(ref XmlDocument doc, string xsdFile)
        {
            if (string.IsNullOrEmpty(xsdFile))
            {
                throw new ArgumentException("The second argument 'xsdFile' should not be empty or null.");
            }

            string filename = Path.GetFileNameWithoutExtension(xsdFile);
            string path = Path.Combine(Path.GetDirectoryName(xsdFile), "expanded-" + filename + "-"  + DateTime.Now.GetHashCode() + ".xsd");
            doc.Save(path);

            this.Display("Save Done!");
            this.Display("You can locate the file in " + path);
        }

        #endregion

    }

    public enum EvolutionTypes
    {
        /// <summary>
        /// Default value: a hold-place value and no real meaning.
        /// </summary>
        None,


        TypeChange_Update, // higher severity than TypeToSimpleType and TypeToComplexType
        TypeChange_Remove,
        TypeChange_Add,

        TypeToSimpleType,
        SimpleTypeToType,

        TypeToComplexType,
        ComplexTypeToType,

        SimpleTypeToComplexType,
        ComplexTypeToSimpleType,

        /// <summary>
        /// The value of name attribute of an element has been changed, 
        /// which will affect the variable name in proxy class and test suite.
        /// </summary>
        Element_NameAttribute_Update,


        // minOccurs
        IncreasedMinOccurs,
        DecreasedMinOccurs,
        AddMinOccurs,
        RemoveMinOccurs,

        // maxOccurs
        IncreasedMaxOccurs,
        DecreasedMaxOccurs,
        AddMaxOccurs,
        RemoveMaxOccurs,
        

        ChangeRestriction_MaxLength, // for element
        ChangeRestriction_MaxLength_Increased,
        ChangeRestriction_MaxLength_Decreased,

        ImportElementChange, // for element
        ImportElementChange_Namespace_Update,
        ImportElementChange_Namespace_Add,
        ImportElementChange_Namespace_Remove,
        ImportElementChange_SchemaLocation_Update,
        ImportElementChange_SchemaLocation_Add,
        ImportElementChange_SchemaLocation_Remove

        
    }

    public class MismatchedElementPair
    {
        #region Fields
        
        private List<MismatchedAttributePair> mismatchedAttributes = new List<MismatchedAttributePair>();

        #endregion

        #region Constructors
        
        public MismatchedElementPair(XmlNode sourceNode, XmlNode changedNode)
        {
            this.SourceNode = sourceNode;
            this.ChangedNode = changedNode;
        }

        #endregion

        #region Properties

        public XmlNode SourceNode { get; set; }
        public XmlNode ChangedNode { get; set; }

        public bool HasMismatchedAttributes { get { return this.mismatchedAttributes.Count > 0 ? true : false; } }
        public EvolutionTypes MismatchedType { get; set; }
        #endregion

        #region Methods

        public void AddMismatchedAttributes(MismatchedAttributePair[] attributes)
        {
            this.mismatchedAttributes.AddRange(attributes);
        }

        #endregion
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

        public EvolutionTypes MismatchedType { get; set; }
    }
}

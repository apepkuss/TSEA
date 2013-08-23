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

        private bool isInternalExpanded = false;

        private bool isExternalExpanded = false;

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

        public List<XmlNode> ExternalImportNodes { get; private set; }

        public List<XmlNode> ExternalIncludeNodes { get; private set; }

        public List<XmlNode> InternalConNodes { get; private set; }

        public XmlNodeList IncludeNodes { get; private set; }

        public XmlNodeList ImportNodes { get; private set; }

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

            if (!this.isExternalExpanded || !this.isInternalExpanded)
            {
                return;
            }

            if (this.originalXmlDoc.DocumentElement != null && this.changedXmlDoc.DocumentElement != null)
            {
                this.Compare(this.originalXmlDoc.DocumentElement, this.changedXmlDoc.DocumentElement);
                this.GenerateDeltaFile();
            }
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
            this.ElementsWithRefAttribute = new List<string>();
            this.InternalRefNodes = new List<XmlNode>();
            this.ExternalImportNodes = new List<XmlNode>();
            this.ExternalIncludeNodes = new List<XmlNode>();
            this.InternalConNodes = new List<XmlNode>();
            this.ImportNodes = null;
            this.IncludeNodes = null;

            try
            {
                XmlNodeList nodes;

                #region Extract "Include" nodes

                this.IncludeNodes = xmlDoc.GetElementsByTagName("xs:include");
                
                #endregion

                #region Extract "Import" nodes

                this.ImportNodes = xmlDoc.GetElementsByTagName("xs:import");

                #endregion


                #region Extract internal and external ref-nodes

                nodes = xmlDoc.GetElementsByTagName("xs:element");
                this.ElementCount = nodes.Count;
                string attributeValue1;

                foreach (XmlNode node in nodes)
                {
                    XmlElement element = node as XmlElement;

                    // filter ref-nodes
                    if (element != null)
                    {
                        if (element.HasAttribute("ref"))
                        {
                            attributeValue1 = element.GetAttribute("ref");

                            //////////////////////////////////////////////////////////////////////////
                            //
                            // ref="elementname" -> internal ref (current doc or include)
                            // ref="xxxx:yyyy"   -> external ref (import)
                            //
                            //////////////////////////////////////////////////////////////////////////
                            if (!attributeValue1.Contains(":"))
                            {
                                // handle internal ref node
                                this.InternalRefNodes.Add(node);
                                continue;
                            }
                            else
                            {
                                // Handle external ref node
                                this.ExternalImportNodes.Add(node);
                                continue;
                            }
                        }
                        else if (element.HasAttribute("type"))
                        {
                            attributeValue1 = element.GetAttribute("type");

                            //////////////////////////////////////////////////////////////////////////
                            //
                            // type="xxxx"      -> internal ref node
                            // type="xxxx:yyyy" -> external ref node (import)
                            // type="xs:xxxx"   -> basic type (ignore)
                            //
                            //////////////////////////////////////////////////////////////////////////
                            if (!attributeValue1.Contains(":"))
                            {
                                // handle internal ref node
                                this.InternalRefNodes.Add(node);
                                continue;
                            }
                            else if (attributeValue1.Contains(":") && !attributeValue1.Contains("xs:"))
                            {
                                // Handle external ref node
                                this.ExternalImportNodes.Add(node);
                                continue;
                            }
                        }

                        // filter and store non-ref nodes
                        this.InternalConNodes.Add(node);
                    }
                }
                #endregion

                #region TODO: remove Parse internal ref-nodes
                //nodes = xmlDoc.GetElementsByTagName("xs:element");
                //this.ElementCount = nodes.Count;

                //foreach (XmlNode node in nodes)
                //{
                //    XmlElement element = node as XmlElement;
                //    if (element != null && element.HasAttribute("ref"))
                //    {
                //        XmlElement parent = node.ParentNode as XmlElement;

                //        // filter and store ref-elements
                //        if (this.ElementsWithRefAttribute == null)
                //        {
                //            this.ElementsWithRefAttribute = new List<string>();
                //        }

                //        if (!element.GetAttribute("ref").Contains(":"))
                //        {
                //            // handle internal ref node
                //            this.InternalRefNodes.Add(node);
                //        }

                //        this.ElementsWithRefAttribute.Add(element.GetAttribute("ref"));
                //    }
                //    else
                //    {
                //        // filter and store non-ref nodes
                //        this.InternalConNodes.Add(node);
                //    }
                //}

                #endregion

                if (this.InternalRefNodes != null && this.InternalRefNodes.Count > 0)
                {
                    // expand internal ref-nodes
                    isInternalExpanded = this.ExpandInternalRefNodes(ref xmlDoc, xsdFile);
                }
                
                #region TODO: remove Parse external ref-nodes
                //nodes = xmlDoc.GetElementsByTagName("xs:element");
                //this.ElementCount = nodes.Count;
                //string attributeValue;

                //foreach (XmlNode node in nodes)
                //{
                //    XmlElement element = node as XmlElement;

                //    // filter ref-nodes
                //    if (element != null && (element.HasAttribute("ref") || element.HasAttribute("type")))
                //    {
                //        // filter and store ref-elements
                //        if (this.ElementsWithRefAttribute == null)
                //        {
                //            this.ElementsWithRefAttribute = new List<string>();
                //        }

                //        if (element.HasAttribute("ref"))
                //        {
                //            attributeValue = element.GetAttribute("ref");
                //        }
                //        else
                //        {
                //            attributeValue = element.GetAttribute("type");
                //        }

                //        if (attributeValue.Contains(":") && !attributeValue.Contains("xs:"))
                //        {
                //            // Handle external ref node
                //            this.ExternalImportNodes.Add(node);
                //        }
                //    }
                //}
                #endregion

                if (this.ExternalImportNodes != null && this.ExternalImportNodes.Count > 0)
                {
                    // expand external ref-nodes
                    isExternalExpanded = this.ExpandExternalRefNodes(ref xmlDoc, xsdFile);
                }

                if (isInternalExpanded && isExternalExpanded)
                {
                    // save the expanded XML doc
                    this.SaveExpandedXDoc(ref xmlDoc, xsdFile);
                }
                else
                {
                    if (!isInternalExpanded)
                    {
                        this.Display("Fail to expand the internal reference node.");
                    }
                    else if (!isExternalExpanded)
                    {
                        this.Display("Fail to expand the external reference node.");
                    }
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
                //this.ExpandInternalRefNodes(ref xmlDoc);

                // expand external ref nodes
                //this.ExpandExternalRefNodes(ref xmlDoc);
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine("Exception: {0}", ex.Message);
            }
        }

        private bool ExpandInternalRefNodes(ref XmlDocument xmlDoc, string xsdFile)
        {
            Dictionary<string, string> externalFiles = null;

            // get external file names
            if (this.IncludeNodes != null && this.IncludeNodes.Count > 0)
            {
                externalFiles = this.GetExternalFiles(this.IncludeNodes);
            }
            
            int counter = 0;
            foreach (XmlNode refNode in this.InternalRefNodes)
            {
                bool isExpanded = false;
                XmlElement refElement = refNode as XmlElement;
                if (refElement != null && refElement.HasAttribute("ref"))
                {
                    string refAttrValue = refElement.GetAttribute("ref");

                    #region replace the elements holding an internal 'ref' attribute with the concrete node.

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

                            isExpanded = true;
                            break;
                        }
                    }

                    #endregion

                    #region handling ref-node by "include"
                    // if the internal ref-node fails to locate the concrete node in the current doc,
                    // continue to the search into the "include" docs.
                    if (!isExpanded && externalFiles != null)
                    {
                        foreach (string value in externalFiles.Values)
                        {
                            XmlDocument externalDoc = this.LoadExternalXmlFile(value, xsdFile);

                            if (externalDoc == null)
                            {
                                this.Display(string.Format("The file of {0}.xsd does not exist.", value));
                                return false;
                            }

                            // get concrete nodes
                            XmlNodeList conNodes = externalDoc.GetElementsByTagName("xs:element");

                            if (conNodes == null || conNodes.Count == 0)
                            {
                                break;
                            }

                            foreach (XmlNode conNode in conNodes)
                            {
                                XmlElement conElement = conNode as XmlElement;

                                if (conElement != null && conElement.HasAttribute("name") && string.Equals(refAttrValue, conElement.GetAttribute("name")))
                                {
                                    XmlNode newNode = xmlDoc.ImportNode(conNode, true);

                                    // append non-ref attributes of refNode to conNode
                                    foreach (XmlAttribute attribute in refNode.Attributes)
                                    {
                                        if (attribute.Name != "ref")
                                        {
                                            XmlElement newElement = newNode as XmlElement;
                                            newElement.SetAttribute(attribute.Name, attribute.Value);
                                        }
                                    }

                                    // replace an internal ref node with its corresponding concrete node
                                    XmlElement parent = refNode.ParentNode as XmlElement;
                                    parent.ReplaceChild(newNode, refNode);

                                    isExpanded = true;
                                    break;
                                }
                            }

                            if (isExpanded)
                            {
                                break;
                            }
                        }
                    }

                    #endregion

                    // if the internal ref-node fails to get the concrete node in both current and 
                    // "include" docs, report the failure.
                    if (!isExpanded)
                    {
                        this.Display(string.Format("Fail to expand: ref=\"{0}\"", refAttrValue));
                        return false;
                    }
                }

                counter++;
            }

            return true;
        }

        private bool ExpandExternalRefNodes(ref XmlDocument xmlDoc, string xsdFile)
        {
            Dictionary<string, string> externalFiles = null;

            // get external file names
            if (this.ImportNodes != null && this.ImportNodes.Count > 0)
            {
                externalFiles = this.GetExternalFiles(this.ImportNodes);
            }
            else
            {
                this.Display("There is no import element available.");
                return false;
            }
            
            foreach (XmlNode refNode in this.ExternalImportNodes)
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
                    return false;
                }

                string filename;
                try
                {
                    filename = externalFiles[refValue[0]];
                }
                catch (KeyNotFoundException ex)
                {
                    this.Display(string.Format("The key of {0} was not found.", refValue[0]));
                    return false;
                }

                // load external file
                XmlDocument externalDoc = this.LoadExternalXmlFile(filename,xsdFile);

                if (externalDoc == null)
                {
                    this.Display(string.Format("The file of {0}.xsd does not exist.", refValue[0]));
                    return false;
                }
                
                // get concrete nodes
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

            return true;
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

        private Dictionary<string, string> GetExternalFiles(XmlNodeList nodes)
        {
            Dictionary<string, string> externalFiles = new Dictionary<string, string>();

            foreach (XmlNode node in nodes)
            {
                XmlElement importElement = node as XmlElement;

                if (importElement.HasAttribute("schemaLocation"))
                {
                    if (importElement.HasAttribute("namespace"))
                    {
                        externalFiles.Add(importElement.GetAttribute("namespace").ToLower(), importElement.GetAttribute("schemaLocation"));
                    }
                    else
                    {
                        externalFiles.Add("include", importElement.GetAttribute("schemaLocation"));
                    }
                }
            }

            return externalFiles;
        }

        private XmlDocument LoadExternalXmlFile(string filename, string xsdFile)
        {
            XmlDocument externalDoc = new XmlDocument();

            try
            {
                string externalFile = Path.Combine(Path.GetDirectoryName(xsdFile), filename);
                externalDoc.Load(externalFile);
            }
            catch (System.Exception ex)
            {
                this.Display(string.Format("Exception: {0}", ex.Message));
                return null;
            }

            return externalDoc;
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

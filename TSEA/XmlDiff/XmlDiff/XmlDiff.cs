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

        #endregion

        #region Constructors

        public XmlDiff(string sourceFile)
        {
            this.originalXsd = sourceFile;
        }

        #endregion

        #region Properties

        public List<string> ElementsWithRefAttribute { get; private set; }

        public List<XmlNode> InteranlRefNodes { get; private set; }

        public List<XmlNode> ExternalRefNodes { get; private set; }

        public List<XmlNode> ConcreteNodes { get; private set; }

        public int ElementCount { get; private set; }

        #endregion

        #region Methods

        public void Diff()
        {
            XmlDocument originalXmlDoc = new XmlDocument();

            try
            {
                originalXmlDoc.Load(this.originalXsd);

                // parse internal and external ref-nodes
                this.Parse(ref originalXmlDoc);

                // expand ref-nodes
                //this.Expand(ref originalXmlDoc);

                // save the expanded XML doc
                this.Save(ref originalXmlDoc);

                this.Display("All done!");
            }
            catch (System.Exception ex)
            {
                this.Display(ex.Message);
            }
        }

        private void Parse(ref XmlDocument xmlDoc)
        {
            this.ElementsWithRefAttribute = null;
            this.InteranlRefNodes = null;
            this.ConcreteNodes = null;

            try
            {
                XmlNodeList nodes;
                string attributeValue = string.Empty;

                #region Parse internal ref-nodes
                nodes = xmlDoc.GetElementsByTagName("xs:element");
                this.ElementCount = nodes.Count;

                foreach (XmlNode node in nodes)
                {
                    XmlElement element = node as XmlElement;
                    if (element != null && element.HasAttribute("ref"))
                    {
                        // filter and store ref-elements
                        if (this.ElementsWithRefAttribute == null)
                        {
                            this.ElementsWithRefAttribute = new List<string>();
                        }

                        if (this.InteranlRefNodes == null)
                        {
                            this.InteranlRefNodes = new List<XmlNode>();
                        }

                        attributeValue = element.GetAttribute("ref");

                        if (!attributeValue.Contains(":"))
                        {
                            // handle internal ref node
                            this.InteranlRefNodes.Add(node);
                        }

                        this.ElementsWithRefAttribute.Add(element.GetAttribute("ref"));

                    }
                    else
                    {
                        // filter and store non-ref nodes
                        if (this.ConcreteNodes == null)
                        {
                            this.ConcreteNodes = new List<XmlNode>();
                        }

                        this.ConcreteNodes.Add(node);
                    }
                }

                #endregion

                this.ExpandInternalRefNodes();

                #region Parse external ref-nodes
                nodes = xmlDoc.GetElementsByTagName("xs:element");
                this.ElementCount = nodes.Count;

                foreach (XmlNode node in nodes)
                {
                    XmlElement element = node as XmlElement;
                    if (element != null && element.HasAttribute("ref"))
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

                        attributeValue = element.GetAttribute("ref");

                        if (attributeValue.Contains(":") && !attributeValue.Contains("xs:"))
                        {
                            // Handle external ref node
                            this.ExternalRefNodes.Add(node);
                        }

                        this.ElementsWithRefAttribute.Add(element.GetAttribute("ref"));

                    }
                }
                #endregion

                this.ExpandExternalRefNodes(ref xmlDoc);

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
                this.ExpandExternalRefNodes(ref xmlDoc);
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine("Exception: {0}", ex.Message);
            }
        }

        private void ExpandInternalRefNodes()
        {
            foreach (XmlNode refNode in this.InteranlRefNodes)
            {
                XmlElement refElement = refNode as XmlElement;
                if (refElement != null && refElement.HasAttribute("ref"))
                {
                    string refAttrValue = refElement.GetAttribute("ref");

                    // replace the elements holding an internal 'ref' attribute with the concrete node.
                    foreach (XmlNode conNode in this.ConcreteNodes)
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
                        }
                    }
                }
            }
        }

        private void ExpandExternalRefNodes(ref XmlDocument xmlDoc)
        {
            string externalFileName;
            XmlDocument externalDoc;
            
            foreach (XmlNode refNode in this.ExternalRefNodes)
            {
                XmlElement refElement = refNode as XmlElement;
                string[] refValue = refElement.GetAttribute("ref").Split(':');

                externalFileName = Path.Combine(Path.GetDirectoryName(this.originalXsd), refValue[0] + ".xsd");
                externalDoc = new XmlDocument();
                externalDoc.Load(externalFileName);

                foreach (XmlNode conNode in externalDoc.GetElementsByTagName("xs:element"))
                {
                    XmlElement conElement = conNode as XmlElement;
                    if (conElement != null && conElement.HasAttribute("name") && string.Equals(refValue[1], conElement.GetAttribute("name")))
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
                    }
                }
            }
        }

        private void Display(string message)
        {
            Console.WriteLine("Message: " + message);
        }

        private void Save(ref XmlDocument doc, string filename = null)
        {
            if (string.IsNullOrEmpty(filename))
            {
                filename = Path.Combine(Path.GetDirectoryName(this.originalXsd), "merge" + DateTime.Now.GetHashCode() + ".xsd");
            }
            

            doc.Save(filename);

            this.Display("Save Done!");
        }

        #endregion
    }
}

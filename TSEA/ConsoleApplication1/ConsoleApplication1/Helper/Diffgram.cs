

namespace Sam.XmlDiffPath
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml;



    internal class Diffgram : DiffgramParentOperation
    {
        #region Fields
        private XmlDiff xmlDiff;
        OperationDescriptor descriptors;
        #endregion

        #region Constructor
        internal Diffgram(XmlDiff xmlDiff) : base(0)
        {
            this.xmlDiff = xmlDiff;
        }
        #endregion

        #region Internal Methods

        internal override void WriteTo(XmlWriter xmlWriter, XmlDiff xmlDiff)
        {
            this.xmlDiff = xmlDiff;
            WriteTo(xmlWriter);
        }

        internal void WriteTo(XmlWriter xmlWriter)
        {
            Debug.Assert(this.xmlDiff.fragments != TriStateBool.DontKnown);

            xmlWriter.WriteStartDocument();

            xmlWriter.WriteStartElement(XmlDiff.Prefix, "xmldiff", XmlDiff.NamespaceUri);
            xmlWriter.WriteAttributeString("version", "1.0");
            xmlWriter.WriteAttributeString("srcDocHash", this.xmlDiff.sourceDiffDoc.HashValue.ToString());
            xmlWriter.WriteAttributeString("options", this.xmlDiff.GetXmlDiffOptionsString());
            xmlWriter.WriteAttributeString("fragments", (this.xmlDiff.fragments == TriStateBool.Yes) ? "yes" : "no");

            this.WriteChildrenTo(xmlWriter, this.xmlDiff);

            OperationDescriptor curOD = descriptors;
            while (curOD != null)
            {
                curOD.WriteTo(xmlWriter);
                curOD = curOD.nextDescriptor;
            }

            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
        }

        internal void AddDescriptor(OperationDescriptor desc)
        {
            desc.nextDescriptor = this.descriptors;
            this.descriptors = desc;
        }

        #endregion
    }
}

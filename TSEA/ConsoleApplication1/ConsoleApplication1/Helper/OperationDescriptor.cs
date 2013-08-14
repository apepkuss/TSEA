using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Sam.XmlDiffPath
{
    internal abstract class OperationDescriptor
    {
        // Fields
        protected ulong operationID;
        internal OperationDescriptor nextDescriptor;

        // Constructor
        internal OperationDescriptor(ulong opid)
        {
            Debug.Assert(opid > 0);
            operationID = opid;
        }

        // Properties
        internal abstract string Type { get; }

        // Methods
        internal virtual void WriteTo(XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartElement(XmlDiff.Prefix, "descriptor", XmlDiff.NamespaceUri);
            xmlWriter.WriteAttributeString("opid", operationID.ToString());
            xmlWriter.WriteAttributeString("type", Type);
            xmlWriter.WriteEndElement();
        }
    }

    internal class OperationDescrMove : OperationDescriptor
    {
        // Constructor
        internal OperationDescrMove(ulong opid) : base(opid)
        {
        }

        // Properties
        internal override string Type { get { return "move"; } }
    }

    internal class OperationDescrNamespaceChange : OperationDescriptor
    {
        // Fields
        DiffgramGenerator.NamespaceChange _nsChange;

        // Constructor
        internal OperationDescrNamespaceChange(DiffgramGenerator.NamespaceChange nsChange)
            : base(nsChange._opid)
        {
            _nsChange = nsChange;
        }

        // Properties
        internal override string Type { get { return "namespace change"; } }

        // Methods
        internal override void WriteTo(XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartElement(XmlDiff.Prefix, "descriptor", XmlDiff.NamespaceUri);
            xmlWriter.WriteAttributeString("opid", operationID.ToString());
            xmlWriter.WriteAttributeString("type", Type);

            xmlWriter.WriteAttributeString("prefix", _nsChange._prefix);
            xmlWriter.WriteAttributeString("oldNs", _nsChange._oldNS);
            xmlWriter.WriteAttributeString("newNs", _nsChange._newNS);

            xmlWriter.WriteEndElement();
        }
    }

    internal class OperationDescrPrefixChange : OperationDescriptor
    {
        // Fields
        DiffgramGenerator.PrefixChange _prefixChange;

        // Constructor
        internal OperationDescrPrefixChange(DiffgramGenerator.PrefixChange prefixChange)
            : base(prefixChange._opid)
        {
            _prefixChange = prefixChange;
        }

        // Properties
        internal override string Type { get { return "prefix change"; } }

        // Methods
        internal override void WriteTo(XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartElement(XmlDiff.Prefix, "descriptor", XmlDiff.NamespaceUri);
            xmlWriter.WriteAttributeString("opid", operationID.ToString());
            xmlWriter.WriteAttributeString("type", Type);

            xmlWriter.WriteAttributeString("ns", _prefixChange._NS);
            xmlWriter.WriteAttributeString("oldPrefix", _prefixChange._oldPrefix);
            xmlWriter.WriteAttributeString("newPrefix", _prefixChange._newPrefix);

            xmlWriter.WriteEndElement();
        }
    }
}

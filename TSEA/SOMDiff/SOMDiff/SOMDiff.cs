using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xin.SOMDiff
{
    using System.Collections;
    using System.IO;
    using System.Xml;
    using System.Xml.Schema;


    public class SOMDiff
    {
        public void SOMExpand()
        {
            string soureFile = @"D:\8-GitHub\TSEA\TSEA\ConsoleApplication1\ConsoleApplication1\Resources\OriginalXSD\Email.xsd";
            //string changedFile = @"D:\8-GitHub\TSEA\TSEA\ConsoleApplication1\ConsoleApplication1\Resources\ChangedXSD\email.xsd";

            XmlTextReader reader = new XmlTextReader(soureFile);
            XmlSchema xmlSchema = XmlSchema.Read(reader, this.ValidationCallBack);


            // Add the customer schema to a new XmlSchemaSet and compile it.
            // Any schema validation warnings and errors encountered reading or 
            // compiling the schema are handled by the ValidationEventHandler delegate.
            XmlSchemaSet schemaSet = new XmlSchemaSet();
            schemaSet.ValidationEventHandler += new ValidationEventHandler(this.ValidationCallBack);
            schemaSet.Add(xmlSchema);
            schemaSet.Compile();

            // Retrieve the compiled XmlSchema object from the XmlSchemaSet
            // by iterating over the Schemas property.
            //foreach (XmlSchema schema in schemaSet.Schemas())
            //{
            //    xmlSchema = schema;
            //}

            

            // Iterate over each XmlSchemaElement in the Values collection
            // of the Elements property.
            foreach (XmlSchemaElement element in xmlSchema.Elements.Values)
            {
                //Console.WriteLine("Element: {0}", element.Name);

                //// Get the complex type of the Customer element.
                //XmlSchemaComplexType complexType = element.ElementSchemaType as XmlSchemaComplexType;

                //// If the complex type has any attributes, get an enumerator 
                //// and write each attribute name to the console.
                //if (complexType.AttributeUses.Count > 0)
                //{
                //    IDictionaryEnumerator enumerator =
                //        complexType.AttributeUses.GetEnumerator();

                //    while (enumerator.MoveNext())
                //    {
                //        XmlSchemaAttribute attribute =
                //            (XmlSchemaAttribute)enumerator.Value;

                //        Console.WriteLine("Attribute: {0}", attribute.Name);
                //    }
                //}

                //// Get the sequence particle of the complex type.
                //XmlSchemaSequence sequence = complexType.ContentTypeParticle as XmlSchemaSequence;

                //// Iterate over each XmlSchemaElement in the Items collection.
                //foreach (XmlSchemaElement childElement in sequence.Items)
                //{
                //    Console.WriteLine("Element: {0}", childElement.Name);
                //}
            }

            foreach (XmlSchemaGroup group in xmlSchema.Groups.Values)
            {
                Console.WriteLine("Element: {0}", group.Name);

                if (group.Particle is XmlSchemaSequence)
                {
                    XmlSchemaSequence sequence = group.Particle as XmlSchemaSequence;

                    foreach (XmlSchemaObject item in sequence.Items)
                    {
                        if (item is XmlSchemaElement)
                        {
                            // TODO
                        }
                        else if (item is XmlSchemaChoice)
                        {
                            this.ParseChoiceNode(item);
                        }
                        else if (item is XmlSchemaSequence)
                        {
                        }
                        else if (item is XmlSchemaAny)
                        {
                        }
                        else if (item is XmlSchemaAttributeGroupRef)
                        {
                        }
                    }
                }
                else if (group.Particle is XmlSchemaChoice)
                {
                }
                else if (group.Particle is XmlSchemaAll)
                {
                }

            }
        }

        private void ParseChoiceNode(XmlSchemaObject node)
        {
            XmlSchemaChoice choice = node as XmlSchemaChoice;

            foreach (XmlSchemaObject item in choice.Items)
            {
                if (item is XmlSchemaElement)
                {
                    this.ParseElement(item);
                }
                else if (item is XmlSchemaChoice)
                {
                    this.ParseChoiceNode(item);
                }
                else if (item is XmlSchemaSequence)
                {
                }
                else if (item is XmlSchemaAny)
                {
                }
                else if (item is XmlSchemaGroupRef)
                {
                }
            }
        }

        private void ParseElement(XmlSchemaObject node)
        {
            XmlSchemaElement element = node as XmlSchemaElement;

            int i = 0;

            if (element.RefName != null)
            {
                
            }

        }

        private void ValidationCallBack(object sender, ValidationEventArgs args)
        {
            if (args.Severity == XmlSeverityType.Warning)
                Console.Write("WARNING: ");
            else if (args.Severity == XmlSeverityType.Error)
                Console.Write("ERROR: ");

            Console.WriteLine(args.Message);
        }
    }
}

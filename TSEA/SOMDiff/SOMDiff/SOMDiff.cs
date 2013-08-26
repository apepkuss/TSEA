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
        public void DiffSchemas(string sourefile, string changefile)
        {
            XmlTextReader sreader = new XmlTextReader(sourefile);
            XmlSchema sourceXmlSchema = XmlSchema.Read(sreader, this.ValidationCallBack);

            XmlTextReader creader = new XmlTextReader(changefile);
            XmlSchema changeXmlSchema = XmlSchema.Read(creader, this.ValidationCallBack);


            // Add the customer schema to a new XmlSchemaSet and compile it.
            // Any schema validation warnings and errors encountered reading or 
            // compiling the schema are handled by the ValidationEventHandler delegate.
            XmlSchemaSet schemaSet = new XmlSchemaSet();
            schemaSet.ValidationEventHandler += new ValidationEventHandler(this.ValidationCallBack);
            schemaSet.Add(sourceXmlSchema);
            schemaSet.Add(changeXmlSchema);
            schemaSet.Compile();

            // Compare elements
            this.CompareElements(sourceXmlSchema.Elements, changeXmlSchema.Elements, false);

            // Compare groups
            //this.CompareGroups(sourceXmlSchema.Groups, changeXmlSchema.Groups, false);

            Console.Read();

            #region Helper
            

            //// Iterate over each XmlSchemaElement in the Values collection
            //// of the Elements property.
            //foreach (XmlSchemaElement element in sourceXmlSchema.Elements.Values)
            //{
            //    if (element.RefName != null)
            //    {
            //        // TODO: HANDLING REF
            //    }




            //    //Console.WriteLine("Element: {0}", element.Name);

            //    //// Get the complex type of the Customer element.
            //    //XmlSchemaComplexType complexType = element.ElementSchemaType as XmlSchemaComplexType;

            //    //// If the complex type has any attributes, get an enumerator 
            //    //// and write each attribute name to the console.
            //    //if (complexType.AttributeUses.Count > 0)
            //    //{
            //    //    IDictionaryEnumerator enumerator =
            //    //        complexType.AttributeUses.GetEnumerator();

            //    //    while (enumerator.MoveNext())
            //    //    {
            //    //        XmlSchemaAttribute attribute =
            //    //            (XmlSchemaAttribute)enumerator.Value;

            //    //        Console.WriteLine("Attribute: {0}", attribute.Name);
            //    //    }
            //    //}

            //    //// Get the sequence particle of the complex type.
            //    //XmlSchemaSequence sequence = complexType.ContentTypeParticle as XmlSchemaSequence;

            //    //// Iterate over each XmlSchemaElement in the Items collection.
            //    //foreach (XmlSchemaElement childElement in sequence.Items)
            //    //{
            //    //    Console.WriteLine("Element: {0}", childElement.Name);
            //    //}
            //}

            //foreach (XmlSchemaGroup group in sourceXmlSchema.Groups.Values)
            //{
            //    Console.WriteLine("Element: {0}", group.Name);

            //    if (group.Particle is XmlSchemaSequence)
            //    {
            //        XmlSchemaSequence sequence = group.Particle as XmlSchemaSequence;

            //        foreach (XmlSchemaObject item in sequence.Items)
            //        {
            //            if (item is XmlSchemaElement)
            //            {
            //                // TODO
            //            }
            //            else if (item is XmlSchemaChoice)
            //            {
            //                this.ParseChoiceNode(item);
            //            }
            //            else if (item is XmlSchemaSequence)
            //            {
            //            }
            //            else if (item is XmlSchemaAny)
            //            {
            //            }
            //            else if (item is XmlSchemaAttributeGroupRef)
            //            {
            //            }
            //        }
            //    }
            //    else if (group.Particle is XmlSchemaChoice)
            //    {
            //    }
            //    else if (group.Particle is XmlSchemaAll)
            //    {
            //    }

            //}

            #endregion
        }

        #region Compare elements
        
        private void CompareElements(XmlSchemaObjectTable source, XmlSchemaObjectTable change, bool ordered)
        {
            if (ordered)
            {
                this.CompareOrderedElements(source.Values.Cast<XmlSchemaElement>().ToArray(), source.Values.Cast<XmlSchemaElement>().ToArray());
            }
            else
            {
                this.CompareUnorderedElements(source.Values.Cast<XmlSchemaElement>().ToList(), source.Values.Cast<XmlSchemaElement>().ToList());
            }

            #region Unused code
            

            //bool bigger = source.Count >= change.Count ? true : false;

            //XmlSchemaObjectTable large = null;
            //XmlSchemaObjectTable small = null;

            //if (bigger)
            //{
            //    large = source;
            //    small = change;
            //}
            //else
            //{
            //    large = change;
            //    small = source;
            //}

            

            //foreach (XmlSchemaElement element1 in large.Values)
            //{
            //    Console.WriteLine("Element: {0}", element1.Name);

            //    bool hasRival = false;
            //    foreach (XmlSchemaElement element2 in small.Values)
            //    {
            //        if (string.Equals(element1.Name, element2.Name, StringComparison.OrdinalIgnoreCase))
            //        {
            //            this.CompareSingleElement(element1, element2);
            //            hasRival = true;
            //            break;
            //        }
            //        else
            //        {

            //        }
                    
            //    }

            //    if (!hasRival)
            //    {
            //        // record all independent nodes
            //    }
            //}

            #endregion
        }

        private void CompareOrderedElements(XmlSchemaElement[] source, XmlSchemaElement[] change)
        {
            if (source.Length == change.Length)
            {
                for (int i = 0; i < source.Length; i++)
                {
                    if (source[i].Name == change[i].Name)
                    {
                        this.CompareSingleElement(source[i], change[i]);
                        continue;
                    }
                    else
                    {
                        // Element_NameAttribute_Update
                    }
                }
            }
            else
            {
                // TODO
            }
        }

        private void CompareUnorderedElements(List<XmlSchemaElement> sourcelist, List<XmlSchemaElement> changelist)
        {
            while (sourcelist.Count > 0 && changelist.Count > 0)
            {
                XmlSchemaElement element1 = sourcelist.FirstOrDefault();

                XmlSchemaElement element2 = null;
                foreach (XmlSchemaElement element in changelist)
                {
                    if (element1.Name == element.Name)
                    {
                        element2 = element;

                        this.CompareSingleElement(element1, element2);

                        break;
                    }
                }

                sourcelist.Remove(element1);
                changelist.Remove(element2);
            }

            if (sourcelist.Count > 0)
            {
                // TODO
            }

            if (changelist.Count > 0)
            {
                // TODO
            }
        }

        private void CompareSingleElement(XmlSchemaElement element1, XmlSchemaElement element2)
        {
            if (element1.Name == "MeetingRequest")
            {
            }

            if (element1.ElementSchemaType == element2.ElementSchemaType)
            {
                if (element1.ElementSchemaType is XmlSchemaComplexType)
                {
                    XmlSchemaComplexType complex1 = element1.ElementSchemaType as XmlSchemaComplexType;
                    XmlSchemaComplexType complex2 = element2.ElementSchemaType as XmlSchemaComplexType;

                    this.CompareParticleComplexType(complex1, complex2);
                }
                else if (element1.ElementSchemaType is XmlSchemaSimpleType)
                {
                    // TODO
                }
            }
            else
            {

            }

        }

        private void CompareParticleComplexType(XmlSchemaComplexType complex1, XmlSchemaComplexType complex2)
        {
            XmlSchemaSequence sequence1 = complex1.ContentTypeParticle as XmlSchemaSequence;
            XmlSchemaSequence sequence2 = complex2.ContentTypeParticle as XmlSchemaSequence;

            if (sequence1 != null)
            {
                this.CompareParticleSequence(sequence1.Items, sequence2.Items, true);
            }
        }

        

        #endregion

        #region Compare sequence

        private void CompareParticleSequence(XmlSchemaObjectCollection xmlSchemaObjectCollection1, XmlSchemaObjectCollection xmlSchemaObjectCollection2, bool p)
        {
            // TODO
        }

        #endregion

        #region Compare groups

        private void CompareGroups(XmlSchemaObjectTable source, XmlSchemaObjectTable change, bool ordered)
        {
            Dictionary<string, XmlSchemaGroup> sourcegroups = new Dictionary<string, XmlSchemaGroup>();
            Dictionary<string, XmlSchemaGroup> changegroups = new Dictionary<string, XmlSchemaGroup>();


            foreach (XmlSchemaGroup group in source.Values)
            {
                sourcegroups.Add(group.Name, group);
            }

            foreach (XmlSchemaGroup group in change.Values)
            {
                changegroups.Add(group.Name, group);
            }

            if (sourcegroups.Count == changegroups.Count)
            {
                foreach (string key in sourcegroups.Keys)
                {
                    if (changegroups.ContainsKey(key))
                    {
                        this.CompareSingleGroup(sourcegroups[key], changegroups[key], ordered);
                    }
                    else
                    {
                        // TODO
                    }
                }
            }
            else
            {
                // TODO
            }


            //bool bigger = source.Count >= change.Count ? true : false;

            //XmlSchemaObjectTable large = null;
            //XmlSchemaObjectTable small = null;

            //if (bigger)
            //{
            //    large = source;
            //    small = change;
            //}
            //else
            //{
            //    large = change;
            //    small = source;
            //}

            //foreach (XmlSchemaGroup group1 in large.Values)
            //{
            //    foreach (XmlSchemaGroup group2 in small.Values)
            //    {
            //        this.CompareSingleGroup(group1, group2);
            //    }
            //}
        }

        private void CompareSingleGroup(XmlSchemaGroup group1, XmlSchemaGroup group2, bool ordered)
        {
            // TODO
        }

        #endregion

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

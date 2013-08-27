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
            XmlSchemaSet sourceSchemaSet = new XmlSchemaSet();
            sourceSchemaSet.ValidationEventHandler += new ValidationEventHandler(this.ValidationCallBack);
            sourceSchemaSet.Add(sourceXmlSchema);
            sourceSchemaSet.Compile();

            Uri sourceUri = new Uri(sourefile);
            foreach (XmlSchema schema in sourceSchemaSet.Schemas())
            {
                if (schema.SourceUri == sourceUri.ToString())
                {
                    sourceXmlSchema = schema;
                    break;
                }
            }


            XmlSchemaSet changeSchemaSet = new XmlSchemaSet();
            changeSchemaSet.ValidationEventHandler += new ValidationEventHandler(this.ValidationCallBack);
            changeSchemaSet.Add(changeXmlSchema);
            changeSchemaSet.Compile();

            Uri changeUri = new Uri(changefile);
            foreach (XmlSchema schema in changeSchemaSet.Schemas())
            {
                if (schema.SourceUri == changeUri.ToString())
                {
                    changeXmlSchema = schema;
                    break;
                }
            }

            // Compare elements
            this.CompareElements(sourceXmlSchema.Elements, changeXmlSchema.Elements);

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
        
        private void CompareElements(XmlSchemaObjectTable source, XmlSchemaObjectTable change, bool ordered = false)
        {
            if (ordered)
            {
                this.CompareOrderedElements(source.Values.Cast<XmlSchemaElement>().ToArray(), change.Values.Cast<XmlSchemaElement>().ToArray());
            }
            else
            {
                this.CompareUnorderedElements(source.Values.Cast<XmlSchemaElement>().ToList(), change.Values.Cast<XmlSchemaElement>().ToList());
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
            //foreach (XmlSchemaElement element1 in sourcelist)
            //{
            //    foreach (XmlSchemaElement element2 in changelist)
            //    {
            //        if (element1.Name == element2.Name)
            //        {
            //            this.CompareSingleElement(element1, element2);
            //            break;
            //        }
            //    }
            //}



            while (sourcelist.Count > 0 && changelist.Count > 0)
            {
                XmlSchemaElement element1 = sourcelist.FirstOrDefault();

                XmlSchemaElement element2 = null;
                foreach (XmlSchemaElement element in changelist)
                {
                    element2 = element;

                    if (!string.IsNullOrEmpty(element1.RefName.Name) && !string.IsNullOrEmpty(element.RefName.Name))
                    {
                        this.CompareRefElement(element1, element2);
                        break;
                    }
                    else if (element1.Name == element.Name)
                    {
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
            if (element1.Name == "Flag")
            {
                
            }

            if (!string.IsNullOrEmpty(element1.Name) && !string.IsNullOrEmpty(element2.Name))
            {
                if (element1.Name != element2.Name)
                {
                    // Change: Element_NameAttribute_Update
                }
            }
            else if (!string.IsNullOrEmpty(element1.Name))
            {

            }
            else if (!string.IsNullOrEmpty(element2.Name))
            {

            }
            else if (!string.IsNullOrEmpty(element1.RefName.Name) && !string.IsNullOrEmpty(element2.RefName.Name))
            {
                this.CompareRefElement(element1, element2);
            }
            else
            {

            }

            

            

            

            if (!string.IsNullOrEmpty(element1.SchemaTypeName.Name) && !string.IsNullOrEmpty(element2.SchemaTypeName.Name))
            {
                if (element1.SchemaTypeName.Name != element2.SchemaTypeName.Name)
                {
                    EvolutionTypes changeType = EvolutionTypes.TypeChange_Update;
                }

                this.CompareFacetMaxOccurs(element1.MaxOccursString, element2.MaxOccursString);

                this.CompareFacetMinOccurs(element1.MinOccursString, element2.MinOccursString);
            }
            else
            {
                if ((element1.ElementSchemaType is XmlSchemaComplexType) && (element2.ElementSchemaType is XmlSchemaComplexType))
                {
                    XmlSchemaComplexType complex1 = element1.ElementSchemaType as XmlSchemaComplexType;
                    XmlSchemaComplexType complex2 = element2.ElementSchemaType as XmlSchemaComplexType;

                    this.CompareParticleComplexType(complex1, complex2);
                }
                else if ((element1.ElementSchemaType is XmlSchemaSimpleType) && (element2.ElementSchemaType is XmlSchemaSimpleType))
                {
                    XmlSchemaSimpleType simple1 = element1.ElementSchemaType as XmlSchemaSimpleType;
                    XmlSchemaSimpleType simple2 = element2.ElementSchemaType as XmlSchemaSimpleType;

                    this.CompareParticleSimpleType(simple1, simple2);
                }
                else
                {
                    // TODO
                }
            }

        }

        private void CompareFacetMaxOccurs(string maxOccursString1, string maxOccursString2)
        {
            if (!string.IsNullOrEmpty(maxOccursString1) && !string.IsNullOrEmpty(maxOccursString2))
            {
                if (maxOccursString1 != maxOccursString2)
                {
                    // Change: maxOccurs
                    EvolutionTypes changeType = this.CompareQuantifiers(Convert.ToInt32(maxOccursString1), Convert.ToInt32(maxOccursString2)) ? EvolutionTypes.DecreasedMaxOccurs : EvolutionTypes.IncreasedMaxOccurs;
                }
            }
            else if (!string.IsNullOrEmpty(maxOccursString1))
            {
                EvolutionTypes changeType = EvolutionTypes.AddMaxOccurs;
            }
            else if (!string.IsNullOrEmpty(maxOccursString2))
            {
                EvolutionTypes changeType = EvolutionTypes.RemoveMaxOccurs;
            }
        }

        private void CompareFacetMinOccurs(string minOccursString1, string minOccursString2)
        {
            if (!string.IsNullOrEmpty(minOccursString1) && !string.IsNullOrEmpty(minOccursString2))
            {
                if (minOccursString1 != minOccursString2)
                {
                    // Change: minOccurs
                    EvolutionTypes changeType = this.CompareQuantifiers(Convert.ToInt32(minOccursString1), Convert.ToInt32(minOccursString2)) ? EvolutionTypes.DecreasedMinOccurs : EvolutionTypes.IncreasedMinOccurs;
                }
            }
            else if (!string.IsNullOrEmpty(minOccursString1))
            {
                EvolutionTypes changeType = EvolutionTypes.AddMinOccurs;
            }
            else if (!string.IsNullOrEmpty(minOccursString2))
            {
                EvolutionTypes changeType = EvolutionTypes.RemoveMinOccurs;
            }
        }

        private void CompareRefElement(XmlSchemaElement element1, XmlSchemaElement element2)
        {
            this.CompareFacetMaxOccurs(element1.MaxOccursString, element2.MaxOccursString);
            this.CompareFacetMinOccurs(element1.MinOccursString, element2.MinOccursString);

            if (element1.RefName.Name != element2.RefName.Name)
            {
                EvolutionTypes changeType = EvolutionTypes.Element_ReferenceChange_Update;
            }

            if ((element1.ElementSchemaType is XmlSchemaSimpleType) && (element2.ElementSchemaType is XmlSchemaSimpleType))
            {
                XmlSchemaSimpleType simple1 = element1.ElementSchemaType as XmlSchemaSimpleType;
                XmlSchemaSimpleType simple2 = element2.ElementSchemaType as XmlSchemaSimpleType;

                this.CompareParticleSimpleType(simple1, simple2);
            }
            else if ((element1.ElementSchemaType is XmlSchemaComplexType) && (element2.ElementSchemaType is XmlSchemaComplexType))
            {
                XmlSchemaComplexType complex1 = element1.ElementSchemaType as XmlSchemaComplexType;
                XmlSchemaComplexType complex2 = element2.ElementSchemaType as XmlSchemaComplexType;

                this.CompareParticleComplexType(complex1, complex2);
            }
            else
            {

            }
        }

        private void CompareParticleSimpleType(XmlSchemaSimpleType simple1, XmlSchemaSimpleType simple2)
        {
            if (simple1.TypeCode != simple2.TypeCode)
            {
                EvolutionTypes changeType = EvolutionTypes.TypeChange_Update;
            }

            if ((simple1.Content is XmlSchemaSimpleTypeRestriction) && (simple2.Content is XmlSchemaSimpleTypeRestriction))
            {
                XmlSchemaSimpleTypeRestriction restriction1 = simple1.Content as XmlSchemaSimpleTypeRestriction;
                XmlSchemaSimpleTypeRestriction restriction2 = simple2.Content as XmlSchemaSimpleTypeRestriction;

                this.CompareSimpleTypeRestriction(restriction1, restriction2);
            }
        }

        private void CompareSimpleTypeRestriction(XmlSchemaSimpleTypeRestriction restriction1, XmlSchemaSimpleTypeRestriction restriction2)
        {
            if (restriction1.BaseTypeName.Name != restriction2.BaseTypeName.Name)
            {
                // Change: base type update
            }

            if (restriction1.Facets.Count > 0 && restriction2.Facets.Count > 0)
            {
                this.CompareFacets(restriction1.Facets, restriction2.Facets);
            }
        }

        private void CompareFacets(XmlSchemaObjectCollection facets1, XmlSchemaObjectCollection facets2, bool ordered = false)
        {
            if(ordered)
            {
                this.CompareOrderedFacets(facets1, facets2);
            }
            else
            {
                this.CompareUnorderedFacets(facets1, facets2);
            }
        }

        private void CompareOrderedFacets(XmlSchemaObjectCollection facets1, XmlSchemaObjectCollection facets2)
        {
            // TODO
        }

        private void CompareUnorderedFacets(XmlSchemaObjectCollection facets1, XmlSchemaObjectCollection facets2)
        {
            List<XmlSchemaObject> sourcelist = new List<XmlSchemaObject>();
            List<XmlSchemaObject> changelist = new List<XmlSchemaObject>();




            XmlSchemaObject facet1 = facets1[0];
            XmlSchemaObject facet2 = facets2[0];

            this.CompareSingleFacet(facet1, facet2);




            if (sourcelist.Count > 0)
            {
                // TODO
            }

            if (changelist.Count > 0)
            {
                // TODO
            }
        }

        private void CompareSingleFacet(XmlSchemaObject facet1, XmlSchemaObject facet2)
        {
            if ((facet1 is XmlSchemaMaxLengthFacet) && (facet2 is XmlSchemaMaxLengthFacet))
            {
                XmlSchemaMaxLengthFacet maxLengthFacet1 = facet1 as XmlSchemaMaxLengthFacet;
                XmlSchemaMaxLengthFacet maxLengthFacet2 = facet2 as XmlSchemaMaxLengthFacet;

                if (maxLengthFacet1.Value != maxLengthFacet2.Value)
                {
                    // Change: maxLength
                    EvolutionTypes changeType = this.CompareQuantifiers(Convert.ToInt32(maxLengthFacet1.Value), Convert.ToInt32(maxLengthFacet2.Value)) ? EvolutionTypes.DecreasedMaxLength : EvolutionTypes.IncreasedMaxLength;
                }
            }
            else
            {
                // TODO
            }
        }

        private bool CompareQuantifiers(int value1, int value2)
        {
            return (value1 > value2) ? true : false;
        }

        private void CompareParticleComplexType(XmlSchemaComplexType complex1, XmlSchemaComplexType complex2)
        {
            bool elementOnly = complex1.ContentType == XmlSchemaContentType.ElementOnly && complex1.ContentType == XmlSchemaContentType.ElementOnly;

            if (complex1.ContentTypeParticle is XmlSchemaSequence)
            {
                if (complex2.ContentTypeParticle is XmlSchemaSequence)
                {
                    XmlSchemaSequence sequence1 = complex1.ContentTypeParticle as XmlSchemaSequence;
                    XmlSchemaSequence sequence2 = complex2.ContentTypeParticle as XmlSchemaSequence;

                    if (sequence1 != null && sequence2 != null)
                    {
                        this.CompareParticleSequence(sequence1, sequence2);
                    }
                    else
                    {

                    }
                }
                else
                {

                }
            }
            else if (complex1.ContentTypeParticle is XmlSchemaAll)
            {
                if (complex2.ContentTypeParticle is XmlSchemaAll)
                {
                    XmlSchemaAll all1 = complex1.ContentTypeParticle as XmlSchemaAll;
                    XmlSchemaAll all2 = complex2.ContentTypeParticle as XmlSchemaAll;

                    if (all1 != null && all2 != null)
                    {
                        this.CompareParticleAll(all1, all2);
                    }
                    else
                    {
                        
                    }
                }
                else if (complex2.ContentTypeParticle is XmlSchemaSequence)
                {
                    // Change: AllToSequence
                    EvolutionTypes changeType = EvolutionTypes.AllToSequence;

                    if (elementOnly)
                    {
                        XmlSchemaSequence sequence = complex2.ContentTypeParticle as XmlSchemaSequence;

                        List<XmlSchemaElement> items1 = this.GetElementsFromParticle(complex1.ContentTypeParticle);
                        List<XmlSchemaElement> items2 = this.GetElementsFromParticle(complex2.ContentTypeParticle);

                        this.CompareUnorderedElements(items1, items2);
                    }
                    else
                    {

                    }
                    

                }
            }
        }

        private List<XmlSchemaElement> GetElementsFromParticle(XmlSchemaParticle particle)
        {
            List<XmlSchemaElement> elements = new List<XmlSchemaElement>();

            if (particle is XmlSchemaSequence)
            {
                XmlSchemaSequence sequence = particle as XmlSchemaSequence;
                elements = this.GetElementsFromObjectCollection(sequence.Items);
            }
            else if (particle is XmlSchemaAll)
            {
                XmlSchemaAll all = particle as XmlSchemaAll;
                elements = this.GetElementsFromObjectCollection(all.Items);
            }
            else
            {

            }

            return elements;
        }

        private List<XmlSchemaElement> GetElementsFromObjectCollection(XmlSchemaObjectCollection items)
        {
            List<XmlSchemaElement> elements = new List<XmlSchemaElement>();

            foreach (XmlSchemaElement item in items)
            {
                elements.Add(item);
            }

            return elements;
        }

        private void CompareParticleAll(XmlSchemaAll all1, XmlSchemaAll all2)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Compare sequence

        private void CompareParticleSequence(XmlSchemaSequence sequence1, XmlSchemaSequence sequence2, bool ordered = false)
        {
            // TODO
            if (sequence1.Items.Count == sequence2.Items.Count)
            {
                for (int i = 0; i < sequence1.Items.Count; i++)
                {
                    this.CompareXmlSchemaObject(sequence1.Items[i], sequence2.Items[i], true);
                }
            } 
            else
            {
            }
            
        }

        private void CompareXmlSchemaObject(XmlSchemaObject xmlSchemaObject1, XmlSchemaObject xmlSchemaObject2, bool ordered = false)
        {
            if ((xmlSchemaObject1 is XmlSchemaElement) && (xmlSchemaObject2 is XmlSchemaElement))
            {
                XmlSchemaElement element1 = xmlSchemaObject1 as XmlSchemaElement;
                XmlSchemaElement element2 = xmlSchemaObject2 as XmlSchemaElement;

                if (!string.IsNullOrEmpty(element1.Name) && !string.IsNullOrEmpty(element2.Name))
                {
                    this.CompareSingleElement(element1, element2);
                }

                if (!string.IsNullOrEmpty(element1.RefName.Name) && !string.IsNullOrEmpty(element2.RefName.Name))
                {
                    this.CompareRefElement(element1, element2);
                }
            }
            else
            {

            }
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

        // maxLength
        IncreasedMaxLength,
        DecreasedMaxLength,
        AddMaxLength,
        RemoveMaxLength,


        ChangeRestriction_MaxLength, // for element
        ChangeRestriction_MaxLength_Increased,
        ChangeRestriction_MaxLength_Decreased,

        ImportElementChange, // for element
        ImportElementChange_Namespace_Update,
        ImportElementChange_Namespace_Add,
        ImportElementChange_Namespace_Remove,
        ImportElementChange_SchemaLocation_Update,
        ImportElementChange_SchemaLocation_Add,
        ImportElementChange_SchemaLocation_Remove,

        Element_ReferenceChange_Update,

        AllToSequence
    }
}

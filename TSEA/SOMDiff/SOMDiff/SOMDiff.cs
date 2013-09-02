﻿using System;
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
        #region Fields
        
        private static XmlSchema sourceXmlSchema = new XmlSchema();
        private static XmlSchema changeXmlSchema = new XmlSchema();

        private static XmlSchemaSet sourceSchemaSet = new XmlSchemaSet();
        private static XmlSchemaSet changeSchemaSet = new XmlSchemaSet();

        private static List<XmlSchemaFacet> removedFacets = new List<XmlSchemaFacet>();
        private static List<XmlSchemaFacet> addedFacets = new List<XmlSchemaFacet>();

        private static Stack<string> sourcePath = new Stack<string>();
        private static Stack<string> changePath = new Stack<string>();
        private static List<MismatchedPair> result = new List<MismatchedPair>();

        #endregion

        #region Constructors
        
        public SOMDiff()
        {
            sourcePath.Clear();
            changePath.Clear();
        }

        public SOMDiff(string sourefile, string changefile) : base()
        {
            XmlTextReader sreader = new XmlTextReader(sourefile);
            //XmlSchema sourceXmlSchema = XmlSchema.Read(sreader, this.ValidationCallBack);
            sourceXmlSchema = XmlSchema.Read(sreader, this.ValidationCallBack);

            XmlTextReader creader = new XmlTextReader(changefile);
            //XmlSchema changeXmlSchema = XmlSchema.Read(creader, this.ValidationCallBack);
            changeXmlSchema = XmlSchema.Read(creader, this.ValidationCallBack);


            // Add the customer schema to a new XmlSchemaSet and compile it.
            // Any schema validation warnings and errors encountered reading or 
            // compiling the schema are handled by the ValidationEventHandler delegate.
            //XmlSchemaSet sourceSchemaSet = new XmlSchemaSet();
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


            //XmlSchemaSet changeSchemaSet = new XmlSchemaSet();
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
        }

        #endregion

        #region Public Methods
        
        public void DiffSchemas(string sourefile, string changefile)
        {
            #region The following code has been moved to constructor
            
            //XmlTextReader sreader = new XmlTextReader(sourefile);
            //XmlSchema sourceXmlSchema = XmlSchema.Read(sreader, this.ValidationCallBack);

            //XmlTextReader creader = new XmlTextReader(changefile);
            //XmlSchema changeXmlSchema = XmlSchema.Read(creader, this.ValidationCallBack);


            //// Add the customer schema to a new XmlSchemaSet and compile it.
            //// Any schema validation warnings and errors encountered reading or 
            //// compiling the schema are handled by the ValidationEventHandler delegate.
            //XmlSchemaSet sourceSchemaSet = new XmlSchemaSet();
            //sourceSchemaSet.ValidationEventHandler += new ValidationEventHandler(this.ValidationCallBack);
            //sourceSchemaSet.Add(sourceXmlSchema);
            //sourceSchemaSet.Compile();

            //Uri sourceUri = new Uri(sourefile);
            //foreach (XmlSchema schema in sourceSchemaSet.Schemas())
            //{
            //    if (schema.SourceUri == sourceUri.ToString())
            //    {
            //        sourceXmlSchema = schema;
            //        break;
            //    }
            //}


            //XmlSchemaSet changeSchemaSet = new XmlSchemaSet();
            //changeSchemaSet.ValidationEventHandler += new ValidationEventHandler(this.ValidationCallBack);
            //changeSchemaSet.Add(changeXmlSchema);
            //changeSchemaSet.Compile();

            //Uri changeUri = new Uri(changefile);
            //foreach (XmlSchema schema in changeSchemaSet.Schemas())
            //{
            //    if (schema.SourceUri == changeUri.ToString())
            //    {
            //        changeXmlSchema = schema;
            //        break;
            //    }
            //}

            #endregion

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

        #endregion

        #region Private Methods

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
            while (sourcelist.Count > 0 && changelist.Count > 0)
            {
                XmlSchemaElement element1 = sourcelist.FirstOrDefault();

                XmlSchemaElement element2 = null;
                foreach (XmlSchemaElement element in changelist)
                {
                    element2 = element;

                    if (!string.IsNullOrEmpty(element1.RefName.Name) || !string.IsNullOrEmpty(element.RefName.Name))
                    {
                        this.CompareRefElement(element1, element2);
                        break;
                    }
                    else if (element1.Name == element.Name)
                    {
                        this.CompareSingleElement(element1, element2);
                        break;
                    }
                    else
                    {

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
            ChangeTypes changeType = ChangeTypes.None;

            if (element1.Name == "Reminder")
            {
                
            }

            if (element1.RefName.Name == "Reminder")
            {

            }

            // if element else ref-element
            if (element1.RefName.IsEmpty && element2.RefName.IsEmpty)
            {
                sourcePath.Push(element1.Name);
                changePath.Push(element2.Name);

                if (element1.Name != element2.Name)
                {
                    // Change: Element_NameAttribute_Update
                    changeType = ChangeTypes.Element_NameAttribute_Update;

                    this.AddMismatchedPair(sourcePath.ToArray(), element1, changePath.ToArray(), element2, ChangeTypes.Element_NameAttribute_Update);
                }


                // if simpleType/complexType else common element
                if (element1.SchemaTypeName.IsEmpty && element2.SchemaTypeName.IsEmpty)
                {
                    // simpleType or complexType
                    //this.CompareSchemaType(element1.ElementSchemaType, element2.ElementSchemaType);

                    this.CompareSchemaType(element1.SchemaType, element2.SchemaType);
                }
                else
                {
                    if (element1.SchemaTypeName.Namespace == element2.SchemaTypeName.Namespace)
                    {
                        if (element1.SchemaTypeName.Name != element2.SchemaTypeName.Name)
                        {
                            changeType = ChangeTypes.TypeChange_Update;

                            this.AddMismatchedPair(sourcePath.ToArray(), element1, changePath.ToArray(), element2, ChangeTypes.TypeChange_Update);
                        }
                    }
                    else
                    {

                    }

                    #region Commented Code
                    
                    //else
                    //{
                    //    XmlSchemaType schemaType1 = null;
                    //    if (element1.SchemaTypeName.Namespace != "http://www.w3.org/2001/XMLSchema")
                    //    {
                    //        schemaType1 = this.GetTypeByRefType(element1.SchemaTypeName, true);
                    //    }
                    //    else
                    //    {
                    //        schemaType1 = element1.ElementSchemaType;
                    //    }

                    //    XmlSchemaType schemaType2 = null;
                    //    if (element2.SchemaTypeName.Namespace != "http://www.w3.org/2001/XMLSchema")
                    //    {
                    //        schemaType2 = this.GetTypeByRefType(element2.SchemaTypeName, false);
                    //    }
                    //    else
                    //    {
                    //        schemaType2 = element2.ElementSchemaType;
                    //    }

                    //    this.CompareSchemaType(schemaType1, schemaType2);
                    //}

                    #endregion

                    if (element1.SchemaTypeName.Namespace != "http://www.w3.org/2001/XMLSchema" || element2.SchemaTypeName.Namespace != "http://www.w3.org/2001/XMLSchema")
                    {
                        XmlSchemaType schemaType1 = null;
                        if (element1.SchemaTypeName.Namespace != "http://www.w3.org/2001/XMLSchema")
                        {
                            schemaType1 = this.GetTypeByRefType(element1.SchemaTypeName, true);
                        }
                        else
                        {
                            schemaType1 = element1.ElementSchemaType;
                        }

                        XmlSchemaType schemaType2 = null;
                        if (element2.SchemaTypeName.Namespace != "http://www.w3.org/2001/XMLSchema")
                        {
                            schemaType2 = this.GetTypeByRefType(element2.SchemaTypeName, false);
                        }
                        else
                        {
                            schemaType2 = element2.ElementSchemaType;
                        }

                        this.CompareSchemaType(schemaType1, schemaType2);
                    }

                    changeType = this.CompareFacetMaxOccurs(element1.MaxOccursString, element2.MaxOccursString);
                    this.AddMismatchedPair(sourcePath.ToArray(), element1, changePath.ToArray(), element2, changeType);

                    changeType = this.CompareFacetMinOccurs(element1.MinOccursString, element2.MinOccursString);
                    this.AddMismatchedPair(sourcePath.ToArray(), element1, changePath.ToArray(), element2, changeType);
                }


                #region Commented Code
                
                //if (!string.IsNullOrEmpty(element1.SchemaTypeName.Name) && !string.IsNullOrEmpty(element2.SchemaTypeName.Name))
                //{
                //    if (element1.SchemaTypeName.Namespace == element2.SchemaTypeName.Namespace)
                //    {
                //        if (element1.SchemaTypeName.Name != element2.SchemaTypeName.Name)
                //        {
                //            EvolutionTypes changeType = EvolutionTypes.TypeChange_Update;
                //        }
                //    }
                //    else
                //    {
                //        XmlSchemaType schemaType1 = null;
                //        if (element1.SchemaTypeName.Namespace != "http://www.w3.org/2001/XMLSchema")
                //        {
                //            schemaType1 = this.GetTypeByRefType(element1.SchemaTypeName, true);
                //        }
                //        else
                //        {
                //            schemaType1 = element1.ElementSchemaType;
                //        }

                //        XmlSchemaType schemaType2 = null;
                //        if (element2.SchemaTypeName.Namespace != "http://www.w3.org/2001/XMLSchema")
                //        {
                //            schemaType2 = this.GetTypeByRefType(element2.SchemaTypeName, false);
                //        }
                //        else
                //        {
                //            schemaType2 = element2.ElementSchemaType;
                //        }

                //        this.CompareSchemaType(schemaType1, schemaType2);
                //    }


                //    this.CompareFacetMaxOccurs(element1.MaxOccursString, element2.MaxOccursString);

                //    this.CompareFacetMinOccurs(element1.MinOccursString, element2.MinOccursString);
                //}
                //else
                //{
                //    // Compare the type of the elements
                //    this.CompareSchemaType(element1.ElementSchemaType, element2.ElementSchemaType);
                //}

                #endregion

                sourcePath.Pop();
                changePath.Pop();
            }
            else
            {
                if (!string.IsNullOrEmpty(element1.RefName.Name) && !string.IsNullOrEmpty(element2.RefName.Name))
                {
                    this.CompareRefElement(element1, element2);
                }
                else if (!string.IsNullOrEmpty(element1.RefName.Name) || !string.IsNullOrEmpty(element2.RefName.Name))
                {
                    this.CompareRefElement(element1, element2);
                }
            }

            #region Commented Useful Code
            
            //if (!string.IsNullOrEmpty(element1.Name) && !string.IsNullOrEmpty(element2.Name))
            //{
            //    if (element1.Name != element2.Name)
            //    {
            //        // Change: Element_NameAttribute_Update
            //        EvolutionTypes changeType = EvolutionTypes.Element_NameAttribute_Update;
            //    }
            //}
            
            //if (!string.IsNullOrEmpty(element1.RefName.Name) && !string.IsNullOrEmpty(element2.RefName.Name))
            //{
            //    this.CompareRefElement(element1, element2);
            //}
            //else if (!string.IsNullOrEmpty(element1.RefName.Name))
            //{
            //    this.CompareRefElement(element1, element2);
            //}
            //else if (!string.IsNullOrEmpty(element2.RefName.Name))
            //{

            //}

            //if (!string.IsNullOrEmpty(element1.SchemaTypeName.Name) && !string.IsNullOrEmpty(element2.SchemaTypeName.Name))
            //{
            //    if (element1.SchemaTypeName.Namespace == element2.SchemaTypeName.Namespace)
            //    {
            //        if (element1.SchemaTypeName.Name != element2.SchemaTypeName.Name)
            //        {
            //            EvolutionTypes changeType = EvolutionTypes.TypeChange_Update;
            //        }
            //    }
            //    else
            //    {
            //        XmlSchemaType schemaType1 = null;
            //        if (element1.SchemaTypeName.Namespace != "http://www.w3.org/2001/XMLSchema")
            //        {
            //            schemaType1 = this.GetTypeByRefType(element1.SchemaTypeName, true);
            //        }
            //        else
            //        {
            //            schemaType1 = element1.ElementSchemaType;
            //        }

            //        XmlSchemaType schemaType2 = null;
            //        if (element2.SchemaTypeName.Namespace != "http://www.w3.org/2001/XMLSchema")
            //        {
            //            schemaType2 = this.GetTypeByRefType(element2.SchemaTypeName, false);
            //        }
            //        else
            //        {
            //            schemaType2 = element2.ElementSchemaType;
            //        }

            //        this.CompareSchemaType(schemaType1, schemaType2);
            //    }
                

            //    this.CompareFacetMaxOccurs(element1.MaxOccursString, element2.MaxOccursString);

            //    this.CompareFacetMinOccurs(element1.MinOccursString, element2.MinOccursString);
            //}
            //else
            //{
            //    // Compare the type of the elements
            //    this.CompareSchemaType(element1.ElementSchemaType, element2.ElementSchemaType);


            //    //if ((element1.ElementSchemaType is XmlSchemaComplexType) && (element2.ElementSchemaType is XmlSchemaComplexType))
            //    //{
            //    //    XmlSchemaComplexType complex1 = element1.ElementSchemaType as XmlSchemaComplexType;
            //    //    XmlSchemaComplexType complex2 = element2.ElementSchemaType as XmlSchemaComplexType;

            //    //    this.CompareParticleComplexType(complex1, complex2);
            //    //}
            //    //else if ((element1.ElementSchemaType is XmlSchemaSimpleType) && (element2.ElementSchemaType is XmlSchemaSimpleType))
            //    //{
            //    //    XmlSchemaSimpleType simple1 = element1.ElementSchemaType as XmlSchemaSimpleType;
            //    //    XmlSchemaSimpleType simple2 = element2.ElementSchemaType as XmlSchemaSimpleType;

            //    //    this.CompareParticleSimpleType(simple1, simple2);
            //    //}
            //    //else
            //    //{
            //    //    // TODO
            //    //}
            //}

            #endregion
        }

        private void CompareRefElement(XmlSchemaElement element1, XmlSchemaElement element2)
        {
            ChangeTypes changeType = ChangeTypes.None;

            changeType = this.CompareFacetMaxOccurs(element1.MaxOccursString, element2.MaxOccursString);
            this.AddMismatchedPair(sourcePath.ToArray(), element1, changePath.ToArray(), element2, changeType);

            changeType = this.CompareFacetMinOccurs(element1.MinOccursString, element2.MinOccursString);
            this.AddMismatchedPair(sourcePath.ToArray(), element1, changePath.ToArray(), element2, changeType);

            if (string.IsNullOrEmpty(element1.RefName.Name))
            {
                sourcePath.Push(element1.Name);
                changePath.Push(string.Format("{0}:{1}", element2.RefName.Namespace, element2.RefName.Name));

                if (element1.Name != element2.RefName.Name)
                {
                    // Change: Element_ReferenceChange_Update
                    changeType = ChangeTypes.Element_ReferenceChange_Update;

                    this.AddMismatchedPair(sourcePath.ToArray(), element1, changePath.ToArray(), element2, ChangeTypes.Element_ReferenceChange_Update);
                }

                XmlSchemaElement element = this.GetElementByRefName(element2.RefName, false);
                this.CompareSingleElement(element1, element);

                sourcePath.Pop();
                changePath.Pop();
            }
            else if (string.IsNullOrEmpty(element2.RefName.Name))
            {
                sourcePath.Push(string.Format("{0}:{1}", element1.RefName.Namespace, element1.RefName.Name));
                changePath.Push(element2.Name);

                if (element1.RefName.Name != element2.Name)
                {
                    // Change: Element_ReferenceChange_Update
                    changeType = ChangeTypes.Element_ReferenceChange_Update;

                    this.AddMismatchedPair(sourcePath.ToArray(), element1, changePath.ToArray(), element2, ChangeTypes.Element_ReferenceChange_Update);
                }

                XmlSchemaElement element = this.GetElementByRefName(element1.RefName, true);
                this.CompareSingleElement(element, element2);

                sourcePath.Pop();
                changePath.Pop();
            }
            else if (element1.RefName.Name != element2.RefName.Name)
            {
                sourcePath.Push(string.Format("{0}:{1}", element1.RefName.Namespace, element1.RefName.Name));
                changePath.Push(string.Format("{0}:{1}", element2.RefName.Namespace, element2.RefName.Name));

                // Change: Element_ReferenceChange_Update
                changeType = ChangeTypes.Element_ReferenceChange_Update;

                this.AddMismatchedPair(sourcePath.ToArray(), element1, changePath.ToArray(), element2, ChangeTypes.Element_ReferenceChange_Update);

                XmlSchemaElement elem1 = this.GetElementByRefName(element1.RefName, true);
                XmlSchemaElement elem2 = this.GetElementByRefName(element2.RefName, false);

                this.CompareSingleElement(elem1, elem2);

                sourcePath.Pop();
                changePath.Pop();
            }

            #region Commented Code
            
            //if ((element1.ElementSchemaType is XmlSchemaSimpleType) && (element2.ElementSchemaType is XmlSchemaSimpleType))
            //{
            //    XmlSchemaSimpleType simple1 = element1.ElementSchemaType as XmlSchemaSimpleType;
            //    XmlSchemaSimpleType simple2 = element2.ElementSchemaType as XmlSchemaSimpleType;

            //    this.CompareParticleSimpleType(simple1, simple2);
            //}
            //else if ((element1.ElementSchemaType is XmlSchemaComplexType) && (element2.ElementSchemaType is XmlSchemaComplexType))
            //{
            //    XmlSchemaComplexType complex1 = element1.ElementSchemaType as XmlSchemaComplexType;
            //    XmlSchemaComplexType complex2 = element2.ElementSchemaType as XmlSchemaComplexType;

            //    this.CompareParticleComplexType(complex1, complex2);
            //}
            //else
            //{

            //}

            #endregion
        }

        #endregion

        #region Compare groups

        private void CompareGroups(XmlSchemaObjectTable source, XmlSchemaObjectTable change, bool ordered = false)
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
                        this.CompareSingleGroup(sourcegroups[key], changegroups[key]);
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

        private void CompareSingleGroup(XmlSchemaGroup group1, XmlSchemaGroup group2, bool ordered = false)
        {
            sourcePath.Push(group1.Name);
            changePath.Push(group2.Name);

            if (group1.Particle is XmlSchemaSequence)
            {
                XmlSchemaSequence particle1 = group1.Particle as XmlSchemaSequence;

                if (group2.Particle is XmlSchemaSequence)
                {
                    XmlSchemaSequence particle2 = group2.Particle as XmlSchemaSequence;

                    this.CompareParticleSequence(particle1, particle2);
                }
                else
                {
                }
            }
            else
            {

            }

            sourcePath.Pop();
            changePath.Pop();
        }

        #endregion

        #region Compare Particles: sequence, choice, all, simpletype, complextype

        private void CompareParticleSequence(XmlSchemaSequence sequence1, XmlSchemaSequence sequence2, bool ordered = false)
        {
            sourcePath.Push("sequence");
            changePath.Push("sequence");

            ChangeTypes changeType = ChangeTypes.None;

            changeType = this.CompareFacetMaxOccurs(sequence1.MaxOccursString, sequence2.MaxOccursString);
            this.AddMismatchedPair(sourcePath.ToArray(), sequence1, changePath.ToArray(), sequence2, changeType);

            changeType = this.CompareFacetMinOccurs(sequence1.MinOccursString, sequence2.MinOccursString);
            this.AddMismatchedPair(sourcePath.ToArray(), sequence1, changePath.ToArray(), sequence2, changeType);

            if (sequence1.Items.Count == sequence2.Items.Count)
            {
                for (int i = 0; i < sequence1.Items.Count; i++)
                {
                    this.CompareXmlSchemaObject(sequence1.Items[i], sequence2.Items[i]);
                }
            }
            else
            {
            }


            sourcePath.Pop();
            changePath.Pop();
        }

        private void CompareParticleChoice(XmlSchemaChoice choice1, XmlSchemaChoice choice2)
        {
            sourcePath.Push("choice");
            changePath.Push("choice");

            ChangeTypes changeType = ChangeTypes.None;

            changeType = this.CompareFacetMaxOccurs(choice1.MaxOccursString, choice2.MaxOccursString);
            this.AddMismatchedPair(sourcePath.ToArray(), choice1, changePath.ToArray(), choice2, changeType);

            changeType = this.CompareFacetMinOccurs(choice1.MinOccursString, choice2.MinOccursString);
            this.AddMismatchedPair(sourcePath.ToArray(), choice1, changePath.ToArray(), choice2, changeType);

            if (choice1.Items.Count == choice2.Items.Count)
            {
                for (int i = 0; i < choice1.Items.Count; i++)
                {
                    this.CompareXmlSchemaObject(choice1.Items[i], choice2.Items[i]);
                }
            }
            else
            {
            }

            sourcePath.Pop();
            changePath.Pop();
        }

        private void CompareParticleAll(XmlSchemaAll all1, XmlSchemaAll all2)
        {
            sourcePath.Push("all");
            changePath.Push("all");

            ChangeTypes changeType = ChangeTypes.None;

            changeType = this.CompareFacetMaxOccurs(all1.MaxOccursString, all2.MaxOccursString);
            this.AddMismatchedPair(sourcePath.ToArray(), all1, changePath.ToArray(), all2, changeType);

            changeType = this.CompareFacetMinOccurs(all1.MinOccursString, all2.MinOccursString);
            this.AddMismatchedPair(sourcePath.ToArray(), all1, changePath.ToArray(), all2, changeType);

            if (all1.Items.Count == all2.Items.Count)
            {
                //for (int i = 0; i < all1.Items.Count; i++)
                //{
                //    this.CompareXmlSchemaObject(all1.Items[i], all2.Items[i]);
                //}

                this.CompareXmlSchemaObjectCollection(all1.Items, all2.Items);
            } 
            else
            {
            }

            sourcePath.Pop();
            changePath.Pop();
        }

        private void CompareSchemaType(XmlSchemaType schemaType1, XmlSchemaType schemaType2)
        {
            if (schemaType1 == null && schemaType2 == null)
            {
                return;
            }

            if (schemaType1 is XmlSchemaSimpleType)
            {
                XmlSchemaSimpleType simple1 = schemaType1 as XmlSchemaSimpleType;

                if (schemaType2 is XmlSchemaSimpleType)
                {
                    XmlSchemaSimpleType simple2 = schemaType2 as XmlSchemaSimpleType;

                    this.CompareParticleSimpleType(simple1, simple2);
                }
                else
                {

                }
            }
            else if (schemaType1 is XmlSchemaComplexType)
            {
                XmlSchemaComplexType complex1 = schemaType1 as XmlSchemaComplexType;

                if (schemaType2 is XmlSchemaComplexType)
                {
                    XmlSchemaComplexType complex2 = schemaType2 as XmlSchemaComplexType;

                    this.CompareParticleComplexType(complex1, complex2);
                }
                else
                {

                }
            }
            else
            {

            }
        }

        private void CompareParticleSimpleType(XmlSchemaSimpleType simple1, XmlSchemaSimpleType simple2)
        {
            sourcePath.Push("SimpleType");
            changePath.Push("SimpleType");

            if (simple1.TypeCode != simple2.TypeCode)
            {
                ChangeTypes changeType = ChangeTypes.TypeChange_Update;

                this.AddMismatchedPair(sourcePath.ToArray(), simple1, changePath.ToArray(), simple2, ChangeTypes.TypeChange_Update);
            }

            if (simple1.Content is XmlSchemaSimpleTypeRestriction)
            {
                XmlSchemaSimpleTypeRestriction restriction1 = simple1.Content as XmlSchemaSimpleTypeRestriction;

                if (simple2.Content is XmlSchemaSimpleTypeRestriction)
                {
                    XmlSchemaSimpleTypeRestriction restriction2 = simple2.Content as XmlSchemaSimpleTypeRestriction;

                    this.CompareSimpleTypeRestriction(restriction1, restriction2);
                } 
                else
                {

                }
            }
            else if ((simple1.Content is XmlSchemaSimpleTypeUnion) && (simple2.Content is XmlSchemaSimpleTypeUnion))
            {
                XmlSchemaSimpleTypeUnion union1 = simple1.Content as XmlSchemaSimpleTypeUnion;
                XmlSchemaSimpleTypeUnion union2 = simple2.Content as XmlSchemaSimpleTypeUnion;

                this.CompareSimpleTypeUnion(union1, union2);
            }
            else
            {

            }

            #region Commented Code
            
            //if (!string.IsNullOrEmpty(simple1.QualifiedName.Namespace) &&
            //    !string.IsNullOrEmpty(simple2.QualifiedName.Namespace) &&
            //    ((simple1.QualifiedName.Namespace != "http://www.w3.org/2001/XMLSchema") || 
            //    (simple2.QualifiedName.Namespace != "http://www.w3.org/2001/XMLSchema")))
            //{
            //    XmlSchemaSimpleType schemaType1 = null;
            //    if (simple1.QualifiedName.Namespace != "http://www.w3.org/2001/XMLSchema")
            //    {
            //        schemaType1 = this.GetTypeByRefType(simple1.QualifiedName, true) as XmlSchemaSimpleType;
            //    }
            //    else
            //    {
            //        schemaType1 = simple1;
            //    }

            //    XmlSchemaSimpleType schemaType2 = null;
            //    if (simple2.QualifiedName.Namespace != "http://www.w3.org/2001/XMLSchema")
            //    {
            //        schemaType2 = this.GetTypeByRefType(simple2.QualifiedName, false) as XmlSchemaSimpleType;
            //    }
            //    else
            //    {
            //        schemaType2 = simple2;
            //    }

            //    this.CompareParticleSimpleType(schemaType1, schemaType2);
            //}
            //else
            //{

            //}

            #endregion

            sourcePath.Pop();
            changePath.Pop();
        }

        private void CompareSimpleTypeUnion(XmlSchemaSimpleTypeUnion union1, XmlSchemaSimpleTypeUnion union2)
        {
            sourcePath.Push("Union");
            changePath.Push("Union");

            if (union1.MemberTypes.Length == union2.MemberTypes.Length)
            {
                //this.CompareQualifiedNames(union1.MemberTypes, union2.MemberTypes);
                foreach (XmlQualifiedName name1 in union1.MemberTypes)
                {
                    foreach (XmlQualifiedName name2 in union2.MemberTypes)
                    {
                        ChangeTypes changeType = this.CompareQualifiedName(name1, name2);
                        this.AddMismatchedPair(sourcePath.ToArray(), union1, changePath.ToArray(), union2, changeType);
                    }
                }
            }
            else
            {

            }

            sourcePath.Pop();
            changePath.Pop();
        }

        private void CompareQualifiedNames(XmlQualifiedName[] xmlQualifiedName1, XmlQualifiedName[] xmlQualifiedName2)
        {
            sourcePath.Push("QualifiedName");
            changePath.Push("QualifiedName");

            foreach (XmlQualifiedName name1 in xmlQualifiedName1)
            {
                foreach (XmlQualifiedName name2 in xmlQualifiedName2)
                {
                    ChangeTypes changeType = this.CompareQualifiedName(name1, name2);
                }
            }

            sourcePath.Pop();
            changePath.Pop();
        }

        private ChangeTypes CompareQualifiedName(XmlQualifiedName name1, XmlQualifiedName name2)
        {
            if (name1.Namespace != name2.Namespace || name1.Name != name2.Name)
            {
                return ChangeTypes.TypeChange_Update;
            }
            else
            {
                return ChangeTypes.None;
            }
        }

        private void CompareParticleComplexType(XmlSchemaComplexType complex1, XmlSchemaComplexType complex2)
        {
            sourcePath.Push("ComplexType");
            changePath.Push("ComplexType");

            bool elementOnly = complex1.ContentType == XmlSchemaContentType.ElementOnly && complex1.ContentType == XmlSchemaContentType.ElementOnly;

            if (complex1.Particle is XmlSchemaSequence)
            {
                if (complex2.Particle is XmlSchemaSequence)
                {
                    XmlSchemaSequence sequence1 = complex1.Particle as XmlSchemaSequence;
                    XmlSchemaSequence sequence2 = complex2.Particle as XmlSchemaSequence;

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
            else if (complex1.Particle is XmlSchemaAll)
            {
                if (complex2.Particle is XmlSchemaAll)
                {
                    XmlSchemaAll all1 = complex1.Particle as XmlSchemaAll;
                    XmlSchemaAll all2 = complex2.Particle as XmlSchemaAll;

                    if (all1 != null && all2 != null)
                    {
                        this.CompareParticleAll(all1, all2);
                    }
                    else
                    {

                    }
                }
                else if (complex2.Particle is XmlSchemaSequence)
                {
                    // Change: AllToSequence
                    ChangeTypes changeType = ChangeTypes.AllToSequence;

                    if (elementOnly)
                    {
                        XmlSchemaSequence sequence = complex2.Particle as XmlSchemaSequence;

                        List<XmlSchemaElement> items1 = this.GetElementsFromParticle(complex1.Particle);
                        List<XmlSchemaElement> items2 = this.GetElementsFromParticle(complex2.Particle);

                        this.CompareUnorderedElements(items1, items2);
                    }
                    else
                    {

                    }


                }
                else
                {

                }
            }

            sourcePath.Pop();
            changePath.Pop();
        }

        private void CompareSimpleTypeRestriction(XmlSchemaSimpleTypeRestriction restriction1, XmlSchemaSimpleTypeRestriction restriction2)
        {
            sourcePath.Push("Restriction");
            changePath.Push("Restriction");

            ChangeTypes changeType = ChangeTypes.None;

            if (!restriction1.BaseTypeName.IsEmpty && !restriction2.BaseTypeName.IsEmpty)
            {
                changeType = this.CompareQualifiedName(restriction1.BaseTypeName, restriction2.BaseTypeName);
                this.AddMismatchedPair(sourcePath.ToArray(), restriction1, changePath.ToArray(), restriction2, changeType);
            }
            else if (!restriction1.BaseTypeName.IsEmpty || !restriction2.BaseTypeName.IsEmpty)
            {
                this.AddMismatchedPair(sourcePath.ToArray(), restriction1, changePath.ToArray(), restriction2, ChangeTypes.TypeChange_Update);
            }

            if (restriction1.Facets.Count > 0 && restriction2.Facets.Count > 0)
            {
                this.CompareFacets(restriction1.Facets, restriction2.Facets);
            }
            else if (restriction1.Facets.Count > 0)
            {
                this.OutputFacetUpdate(restriction1.Facets, true);
            }
            else if (restriction2.Facets.Count > 0)
            {
                this.OutputFacetUpdate(restriction2.Facets, false);
                
            }

            sourcePath.Pop();
            changePath.Pop();
        }

        private void OutputFacetUpdate(XmlSchemaObjectCollection facets, bool flag)
        {
            //List<XmlSchemaFacet> addedFacets = new List<XmlSchemaFacet>();
            //List<XmlSchemaFacet> removedFacets = new List<XmlSchemaFacet>();

            //foreach (XmlSchemaFacet facet in facets)
            //{
            //    if (facet is XmlSchemaLengthFacet)
            //    {
            //        // TODO
            //    }
            //    else if (facet is XmlSchemaMinLengthFacet)
            //    {

            //    }
            //    else if (facet is XmlSchemaMaxLengthFacet)
            //    {

            //    }
            //    else if (facet is XmlSchemaPatternFacet)
            //    {

            //    }
            //    else if (facet is XmlSchemaEnumerationFacet)
            //    {

            //    }
            //    else if (facet is XmlSchemaMaxInclusiveFacet)
            //    {

            //    }
            //    else if (facet is XmlSchemaMaxExclusiveFacet)
            //    {

            //    }
            //    else if (facet is XmlSchemaMinInclusiveFacet)
            //    {

            //    }
            //    else if (facet is XmlSchemaMinExclusiveFacet)
            //    {

            //    }
            //    else if (facet is XmlSchemaFractionDigitsFacet)
            //    {

            //    }
            //    else if (facet is XmlSchemaTotalDigitsFacet)
            //    {

            //    }
            //    else if (facet is XmlSchemaWhiteSpaceFacet)
            //    {

            //    }
            //}
        }

        #endregion

        #region Compare Facets: MaxOccurs, MinOccurs

        private ChangeTypes CompareFacetMaxOccurs(string maxOccursString1, string maxOccursString2)
        {
            ChangeTypes changeType = ChangeTypes.None;

            if (!string.IsNullOrEmpty(maxOccursString1) && !string.IsNullOrEmpty(maxOccursString2))
            {
                if (maxOccursString1 != maxOccursString2)
                {
                    // Change: maxOccurs
                    changeType = this.CompareQuantifiers(Convert.ToInt32(maxOccursString1), Convert.ToInt32(maxOccursString2)) ? ChangeTypes.DecreasedMaxOccurs : ChangeTypes.IncreasedMaxOccurs;
                }
            }
            else if (!string.IsNullOrEmpty(maxOccursString1))
            {
                changeType = ChangeTypes.AddMaxOccurs;
            }
            else if (!string.IsNullOrEmpty(maxOccursString2))
            {
                changeType = ChangeTypes.RemoveMaxOccurs;
            }

            return changeType;
        }

        private ChangeTypes CompareFacetMinOccurs(string minOccursString1, string minOccursString2)
        {
            ChangeTypes changeType = ChangeTypes.None;

            if (!string.IsNullOrEmpty(minOccursString1) && !string.IsNullOrEmpty(minOccursString2))
            {
                if (minOccursString1 != minOccursString2)
                {
                    // Change: minOccurs
                    changeType = this.CompareQuantifiers(Convert.ToInt32(minOccursString1), Convert.ToInt32(minOccursString2)) ? ChangeTypes.DecreasedMinOccurs : ChangeTypes.IncreasedMinOccurs;
                }
            }
            else if (!string.IsNullOrEmpty(minOccursString1))
            {
                changeType = ChangeTypes.AddMinOccurs;
            }
            else if (!string.IsNullOrEmpty(minOccursString2))
            {
                changeType = ChangeTypes.RemoveMinOccurs;
            }

            return changeType;
        }

        private void CompareFacets(XmlSchemaObjectCollection facets1, XmlSchemaObjectCollection facets2, bool ordered = false)
        {
            if (ordered)
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
            Dictionary<FacetTypes, XmlSchemaFacet> sourcelist = new Dictionary<FacetTypes, XmlSchemaFacet>();
            Dictionary<FacetTypes, XmlSchemaFacet> changelist = new Dictionary<FacetTypes, XmlSchemaFacet>();

            Dictionary<string, XmlSchemaFacet> sourcelistForEnum = new Dictionary<string, XmlSchemaFacet>();
            Dictionary<string, XmlSchemaFacet> changelistForEnum = new Dictionary<string, XmlSchemaFacet>();

            List<XmlSchemaFacet> removedFacets = new List<XmlSchemaFacet>();

            foreach (XmlSchemaFacet facet in facets1)
            {
                if (facet is XmlSchemaEnumerationFacet)
                {
                    sourcelistForEnum.Add(facet.Value, facet);
                }
                else
                {
                    sourcelist.Add(this.GetFacetType(facet), facet);
                }
                
            }

            foreach (XmlSchemaFacet facet in facets2)
            {
                if (facet is XmlSchemaEnumerationFacet)
                {
                    changelistForEnum.Add(facet.Value, facet);
                }
                else
                {
                    changelist.Add(this.GetFacetType(facet), facet);
                }
            }

            this.CompareEnumerationFact(sourcelistForEnum, changelistForEnum);

            while (sourcelist.Count > 0 && changelist.Count > 0)
            {
                FacetTypes key = sourcelist.First().Key;

                if (!changelist.ContainsKey(key))
                {
                    this.CheckFacetChangeType(key, sourcelist[key], null);

                    //removedFacets.Add(sourcelist[key]);
                    sourcelist.Remove(key);
                }
                else
                {
                    if (sourcelist[key].Value != changelist[key].Value)
                    {
                        // Change:
                        this.CheckFacetChangeType(key, sourcelist[key], changelist[key]);
                    }

                    sourcelist.Remove(key);
                    changelist.Remove(key);
                }
            }

            if (sourcelist.Count > 0)
            {
                foreach (FacetTypes key in sourcelist.Keys)
                {
                    // Change: 
                    this.CheckFacetChangeType(key, sourcelist[key], null);
                }
                
            }

            if (changelist.Count > 0)
            {
                foreach (FacetTypes key in changelist.Keys)
                {
                    // Change: 
                    this.CheckFacetChangeType(key, null, changelist[key]);
                }
            }
        }

        private void CompareEnumerationFact(Dictionary<string, XmlSchemaFacet> sourcelistForEnum, Dictionary<string, XmlSchemaFacet> changelistForEnum)
        {
            // TODO:
        }

        private ChangeTypes CheckFacetChangeType(FacetTypes facetType, XmlSchemaFacet sourceFacet, XmlSchemaFacet changeFacet)
        {
            ChangeTypes changeType = ChangeTypes.None;

            if (facetType == FacetTypes.EnumerationFacet)
            {
                if (sourceFacet != null && changeFacet != null)
                {
                    sourcePath.Push("enumeration");
                    changePath.Push("enumeration");





                    sourcePath.Pop();
                    changePath.Pop();
                }
                else if (sourceFacet != null)
                {
                    sourcePath.Push("enumeration");





                    sourcePath.Pop();
                }
                else if (changeFacet != null)
                {
                    changePath.Push("enumeration");





                    changePath.Pop();
                }



                
            }
            else if (facetType == FacetTypes.MaxExclusiveFacet)
            {
            }
            else if (facetType == FacetTypes.MaxInclusiveFacet)
            {

            }
            else if (facetType == FacetTypes.MinExclusiveFacet)
            {
            }
            else if (facetType == FacetTypes.MinInclusiveFacet)
            {
            }
            else if (facetType == FacetTypes.NumericFacet)
            {
            }
            else if (facetType == FacetTypes.PatternFacet)
            {
            }
            else if (facetType == FacetTypes.WhiteSpaceFacet)
            {
            }
            else
            {

            }

            return changeType;
        }

        private FacetTypes GetFacetType(XmlSchemaFacet facet)
        {
            FacetTypes facetType = FacetTypes.None;

            if (facet is XmlSchemaEnumerationFacet)
            {
                facetType = FacetTypes.EnumerationFacet;
            }
            else if (facet is XmlSchemaMaxExclusiveFacet)
            {
                facetType = FacetTypes.MaxExclusiveFacet;
            }
            else if (facet is XmlSchemaMaxInclusiveFacet)
            {
                facetType = FacetTypes.MaxInclusiveFacet;
            } 
            else if(facet is XmlSchemaMinExclusiveFacet)
            {
                facetType = FacetTypes.MinExclusiveFacet;
            }
            else if (facet is XmlSchemaMinInclusiveFacet)
            {
                facetType = FacetTypes.MinInclusiveFacet;
            } 
            else if (facet is XmlSchemaNumericFacet)
            {
                facetType = FacetTypes.NumericFacet;
            }
            else if (facet is XmlSchemaPatternFacet)
            {
                facetType = FacetTypes.PatternFacet;
            }
            else if (facet is XmlSchemaWhiteSpaceFacet)
            {
                facetType = FacetTypes.WhiteSpaceFacet;
            }

            return facetType;
        }

        private void CompareSingleFacet(XmlSchemaObject facet1, XmlSchemaObject facet2)
        {
            ChangeTypes changeType = ChangeTypes.None;

            if ((facet1 is XmlSchemaMaxLengthFacet) && (facet2 is XmlSchemaMaxLengthFacet))
            {
                sourcePath.Push("MaxLength");
                changePath.Push("MaxLength");

                XmlSchemaMaxLengthFacet maxLengthFacet1 = facet1 as XmlSchemaMaxLengthFacet;
                XmlSchemaMaxLengthFacet maxLengthFacet2 = facet2 as XmlSchemaMaxLengthFacet;

                if (maxLengthFacet1.Value != maxLengthFacet2.Value)
                {
                    // Change: maxLength
                    changeType = this.CompareQuantifiers(Convert.ToInt32(maxLengthFacet1.Value), Convert.ToInt32(maxLengthFacet2.Value)) ? ChangeTypes.DecreasedMaxLength : ChangeTypes.IncreasedMaxLength;

                    this.AddMismatchedPair(sourcePath.ToArray(), facet1, changePath.ToArray(), facet2, changeType);
                }

                sourcePath.Pop();
                changePath.Pop();
            }
            else
            {
                // TODO
            }
        }

        #endregion

        #region Helper Methods

        private void CompareXmlSchemaObjectCollection(XmlSchemaObjectCollection xmlSchemaObjectCollection1, XmlSchemaObjectCollection xmlSchemaObjectCollection2, bool flag = false)
        {
            if (flag)
            {
                // compare the elements of the collections orderly
                foreach (XmlSchemaObject object1 in xmlSchemaObjectCollection1)
                {
                    foreach (XmlSchemaObject object2 in xmlSchemaObjectCollection2)
                    {
                        this.CompareXmlSchemaObject(object1, object2);
                    }
                }
            }
            else
            {
                // compare the elements of the collections unorderly
                List<XmlSchemaElement> elements1 = new List<XmlSchemaElement>();
                List<XmlSchemaElement> elements2 = new List<XmlSchemaElement>();

                foreach (XmlSchemaObject item in xmlSchemaObjectCollection1)
                {
                    if (item is XmlSchemaElement)
                    {
                        elements1.Add(item as XmlSchemaElement);
                    }
                }

                foreach (XmlSchemaObject item in xmlSchemaObjectCollection2)
                {
                    if (item is XmlSchemaElement)
                    {
                        elements2.Add(item as XmlSchemaElement);
                    }
                }

                this.CompareUnorderedElements(elements1, elements2);
            }
        }

        private void CompareXmlSchemaObject(XmlSchemaObject xmlSchemaObject1, XmlSchemaObject xmlSchemaObject2)
        {
            if (xmlSchemaObject1 is XmlSchemaElement)
            {
                XmlSchemaElement element1 = xmlSchemaObject1 as XmlSchemaElement;

                if (xmlSchemaObject2 is XmlSchemaElement)
                {
                    XmlSchemaElement element2 = xmlSchemaObject2 as XmlSchemaElement;

                    if (!string.IsNullOrEmpty(element1.Name) && !string.IsNullOrEmpty(element2.Name))
                    {
                        this.CompareSingleElement(element1, element2);
                    }

                    if (!string.IsNullOrEmpty(element1.RefName.Name) && !string.IsNullOrEmpty(element2.RefName.Name))
                    {
                        this.CompareRefElement(element1, element2);
                    }
                    else if (!string.IsNullOrEmpty(element1.RefName.Name))
                    {
                        this.CompareSingleElement(element1, element2);
                    }
                    else if (!string.IsNullOrEmpty(element2.RefName.Name))
                    {

                    }

                }
                else
                {
                }

            }
            else if (xmlSchemaObject1 is XmlSchemaChoice)
            {
                XmlSchemaChoice object1 = xmlSchemaObject1 as XmlSchemaChoice;

                if (xmlSchemaObject2 is XmlSchemaChoice)
                {
                    XmlSchemaChoice object2 = xmlSchemaObject2 as XmlSchemaChoice;

                    this.CompareParticleChoice(object1, object2);
                }
                else
                {
                }
            }
            else
            {

            }
        }

        private bool CompareQuantifiers(int value1, int value2)
        {
            return (value1 > value2) ? true : false;
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

        private XmlSchemaElement GetElementByRefName(XmlQualifiedName refName, bool flag)
        {
            XmlSchema refSchema = null;

            if (flag)
            {
                if (refName.Namespace == sourceXmlSchema.TargetNamespace)
                {
                    refSchema = sourceXmlSchema;
                }
                else
                {
                    foreach (XmlSchema schema in sourceSchemaSet.Schemas())
                    {
                        if (refName.Namespace == schema.TargetNamespace)
                        {
                            refSchema = schema;
                            break;
                        }
                    }
                }

            }
            else
            {
                if (refName.Namespace == changeXmlSchema.TargetNamespace)
                {
                    refSchema = changeXmlSchema;
                }
                else
                {
                    foreach (XmlSchema schema in changeSchemaSet.Schemas())
                    {
                        if (refName.Namespace == schema.TargetNamespace)
                        {
                            refSchema = schema;
                            break;
                        }
                    }
                }
            }

            foreach (XmlSchemaElement element in refSchema.Elements.Values)
            {
                if (refName.Name == element.Name)
                {
                    if (flag)
                    {
                        sourcePath.Push(string.Format("{0}:{1}", refName.Namespace, refName.Name));
                    }
                    else
                    {
                        changePath.Push(string.Format("{0}:{1}", refName.Namespace, refName.Name));
                    }

                    return element;
                }
            }

            return null;
        }

        private XmlSchemaType GetTypeByRefType(XmlQualifiedName refType, bool flag)
        {
            XmlSchema refSchema = null;

            if (flag)
            {
                if (refType.Namespace == sourceXmlSchema.TargetNamespace)
                {
                    refSchema = sourceXmlSchema;
                }
                else
                {
                    foreach (XmlSchema schema in sourceSchemaSet.Schemas())
                    {
                        if (refType.Namespace == schema.TargetNamespace)
                        {
                            refSchema = schema;
                            break;
                        }
                    }
                }

            }
            else
            {
                if (refType.Namespace == changeXmlSchema.TargetNamespace)
                {
                    refSchema = changeXmlSchema;
                }
                else
                {
                    foreach (XmlSchema schema in changeSchemaSet.Schemas())
                    {
                        if (refType.Namespace == schema.TargetNamespace)
                        {
                            refSchema = schema;
                            break;
                        }
                    }
                }
            }

            foreach (XmlSchemaType type in refSchema.SchemaTypes.Values)
            {

                if (refType.Name == type.Name)
                {
                    return type;
                }
            }

            return null;
        }

        private void ReportFacetCollection(List<XmlSchemaFacet> facets, bool flag)
        {
            if (flag)
            {
                // removed
                removedFacets.AddRange(facets);
            }
            else
            {
                // added
                addedFacets.AddRange(facets);
            }
        }

        private void AddMismatchedPair(string[] rawPath1, XmlSchemaObject object1, string[] rawPath2, XmlSchemaObject object2, ChangeTypes changeType)
        {
            if (changeType != ChangeTypes.None)
            {
                StringBuilder path1 = new StringBuilder();
                StringBuilder path2 = new StringBuilder();

                foreach (string path in rawPath1)
                {
                    path1.Append(path);
                }

                foreach (string path in rawPath2)
                {
                    path2.Append(path);
                }

                MismatchedPair pair = new MismatchedPair();

                pair.SourcePath = path1.ToString();
                pair.SourceObject = object1;
                pair.ChangePath = path2.ToString();
                pair.ChangeObject = object2;

                pair.ChangeType = changeType;

                result.Add(pair);
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
                else
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

        #endregion
    }

    public class MismatchedPair
    {
        public string SourcePath { get; set; }
        public string ChangePath { get; set; }

        public XmlSchemaObject SourceObject { get; set; }
        public XmlSchemaObject ChangeObject { get; set; }

        public ChangeTypes ChangeType { get; set; }
    }
}

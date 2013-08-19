using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DiffXAlgorithem
{
    public class DiffX
    {
        public DTree SourceTree { get; set; }
        public DTree ChangedTree { get; set; }

        public List<FragmentMapping> Mapptings { get; set; }

        /// <summary>
        /// Traverse an XML document to create a corresponding DTree and index the tree nodes.
        /// </summary>
        public void Preprocess(string filename)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filename);

            try
            {
                XmlElement root = doc.DocumentElement;
                if (root.HasChildNodes)
                {
                    // TODO
                }
            }
            catch (System.Exception ex)
            {
            	
            }
        }

        /// <summary>
        /// Create mapping relations between two trees.
        /// </summary>
        /// <param name="sourceTree"></param>
        /// <param name="changedTree"></param>
        public void Mapping(DTree sourceTree, DTree changedTree)
        {

        }

        /// <summary>
        /// Mapping isolated tree fragment.
        /// </summary>
        /// <param name="soureElement"></param>
        /// <param name="changedElement"></param>
        /// <param name="mapping1"></param>
        /// <param name="mapping2"></param>
        public void MatchFragment(Element soureElement, Element changedElement, FragmentMapping mapping1, FragmentMapping mapping2)
        {

        }

        /// <summary>
        /// Generate script.
        /// </summary>
        /// <param name="sourceTree"></param>
        /// <param name="changedTree"></param>
        /// <param name="mapping1"></param>
        /// <returns></returns>
        public List<string> GenerateScript(DTree sourceTree, DTree changedTree, FragmentMapping mapping1)
        {
            List<string> editScript = null;


            return editScript;
        }
    }

    #region Data Structures

    public class DTree
    {
        public Element Root { get; set; }
    }

    public class Element
    {
        // Record the position of the element in the tree
        public int Position { get; set; }

        public int Name { get; set; }

        // A unordered set of attributes
        public Attribute[] Attributes { get; set; }

        public string InternalText { get; set; }


        public Element Parent { get; set; }

        // An ordered set of child elements
        public Element[] Children { get; set; }

        public Element[] Siblings { get; set; }
    }

    public class Attribute
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    #endregion

    public class FragmentMapping
    {
        public List<MappingPair> MappingPairs { get; set; }
    }

    public class MappingPair
    {
        public Element source { get; set; }
        public Element changed { get; set; }
    }
}

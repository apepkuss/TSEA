using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xin.SOMDiff
{
    public enum ChangeTypes
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
        Element_Name_Update,
        AllToSequence
    }
}

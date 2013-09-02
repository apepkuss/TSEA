using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.CodeDom;
using System.CodeDom.Compiler;

using Microsoft.CSharp;

namespace Xin.XsdToClass
{
    public class XsdToClass
    {
        // Test for XmlSchemaImporter
        public void XsdToClassTest(string[] xsdfiles)
        {
            // identify the path to the xsd
            string xsdFileName = "Account.xsd";
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string xsdPath = Path.Combine(path, xsdFileName);
            xsdPath = @"D:\8-GitHub\TSEA\TSEA\ConsoleApplication1\ConsoleApplication1\Resources\OriginalXSD\Calendar.xsd";

            string sourefile = xsdPath;
            XmlSchema sourceXmlSchema = new XmlSchema();
            XmlSchemaSet sourceSchemaSet = new XmlSchemaSet();

            XmlTextReader sreader = new XmlTextReader(sourefile);
            //XmlSchema sourceXmlSchema = XmlSchema.Read(sreader, this.ValidationCallBack);
            sourceXmlSchema = XmlSchema.Read(sreader, this.ValidationCallBack);

            // Add the customer schema to a new XmlSchemaSet and compile it.
            // Any schema validation warnings and errors encountered reading or 
            // compiling the schema are handled by the ValidationEventHandler delegate.
            //XmlSchemaSet sourceSchemaSet = new XmlSchemaSet();
            sourceSchemaSet.ValidationEventHandler += new ValidationEventHandler(this.ValidationCallBack);
            sourceSchemaSet.Add(sourceXmlSchema);
            sourceSchemaSet.Compile();

            Uri sourceUri = new Uri(sourefile);
            XmlSchemas xsds = new XmlSchemas();
            foreach (XmlSchema schema in sourceSchemaSet.Schemas())
            {
                if (schema.SourceUri == sourceUri.ToString())
                {
                    sourceXmlSchema = schema;
                    break;
                }

                xsds.Add(schema);
            }


            Console.WriteLine("xsd.IsCompiled {0}", sourceXmlSchema.IsCompiled);

            xsds.Compile(null, true);
            XmlSchemaImporter schemaImporter = new XmlSchemaImporter(xsds);



            // load the xsd
            XmlSchema xsd;
            using (FileStream stream = new FileStream(xsdPath, FileMode.Open, FileAccess.Read))
            {
                xsd = XmlSchema.Read(stream, null);
            }
            Console.WriteLine("xsd.IsCompiled {0}", xsd.IsCompiled);

            //XmlSchemas xsds = new XmlSchemas();
            //xsds.Add(sourceXmlSchema);
            //xsds.Compile(null, true);
            //XmlSchemaImporter schemaImporter = new XmlSchemaImporter(xsds);

            // create the codedom
            CodeNamespace codeNamespace = new CodeNamespace("Generated");
            XmlCodeExporter codeExporter = new XmlCodeExporter(codeNamespace);

            List<XmlTypeMapping> maps = new List<XmlTypeMapping>();
            foreach (XmlSchemaType schemaType in sourceXmlSchema.SchemaTypes.Values)
            {
                maps.Add(schemaImporter.ImportSchemaType(schemaType.QualifiedName));
            }
            foreach (XmlSchemaElement schemaElement in sourceXmlSchema.Elements.Values)
            {
                maps.Add(schemaImporter.ImportTypeMapping(schemaElement.QualifiedName));
            }
            foreach (XmlTypeMapping map in maps)
            {
                codeExporter.ExportTypeMapping(map);
            }
            
            RemoveAttributes(codeNamespace);

            // Check for invalid characters in identifiers
            CodeGenerator.ValidateIdentifiers(codeNamespace);

            // output the C# code
            CSharpCodeProvider codeProvider = new CSharpCodeProvider();

            using (StringWriter writer = new StringWriter())
            {
                codeProvider.GenerateCodeFromNamespace(codeNamespace, writer, new CodeGeneratorOptions());
                Console.WriteLine(writer.GetStringBuilder().ToString());
            }

            Console.ReadLine();
        }

        // Remove all the attributes from each type in the CodeNamespace, except
        // System.Xml.Serialization.XmlTypeAttribute
        private void RemoveAttributes(CodeNamespace codeNamespace)
        {
            foreach (CodeTypeDeclaration codeType in codeNamespace.Types)
            {
                CodeAttributeDeclaration xmlTypeAttribute = null;
                foreach (CodeAttributeDeclaration codeAttribute in codeType.CustomAttributes)
                {
                    Console.WriteLine(codeAttribute.Name);
                    if (codeAttribute.Name == "System.Xml.Serialization.XmlTypeAttribute")
                    {
                        xmlTypeAttribute = codeAttribute;
                    }
                }
                codeType.CustomAttributes.Clear();
                if (xmlTypeAttribute != null)
                {
                    codeType.CustomAttributes.Add(xmlTypeAttribute);
                }
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

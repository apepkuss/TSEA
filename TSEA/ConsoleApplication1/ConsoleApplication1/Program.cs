
namespace ConsoleApplication1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using System.IO;
    using System.Xml;
    using System.Xml.Schema;
    using Sam.XmlDiff;
    using System.Collections;
    using System.Diagnostics;

    using Xin.SOMDiff;
    //using Xin.XsdToClass;

    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            #region Reference XmlDiffPatch.dll first if you want to run the following commented code.
            //bool fragments = false;
            //string sourceFile = @"C:\Users\v-liuxin\Desktop\test\Email-old.xsd";
            //string changedFile = @"C:\Users\v-liuxin\Desktop\test\email-new.xsd";
            //string diffgramFileName = @"C:\Users\v-liuxin\Desktop\test\EmailDiff.xml";

            //// create XmlTextWriter where the diffgram will be saved
            //XmlWriter diffgramWriter = new XmlTextWriter(diffgramFileName, Encoding.Unicode);

            //// create XmlDiff object & set the desired options and algorithm
            //XmlDiffAlgorithm algorithm = XmlDiffAlgorithm.Precise;
            //XmlDiffOptions options = XmlDiffOptions.None;
            //XmlDiff xmlDiff = new XmlDiff(options);
            //xmlDiff.Algorithm = algorithm;

            //// Compare the XML files
            //bool bEqual = false;
            //try
            //{
            //    bEqual = xmlDiff.Compare(sourceFile, changedFile, fragments, diffgramWriter);
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine("Error:" + e.Message);
            //    return;
            //}

            //if (bEqual)
            //{
            //    Console.WriteLine("Files are identical.");
            //}
            //else
            //{
            //    Console.WriteLine("Files are different.");
            //}

            //if (diffgramWriter != null)
            //{
            //    diffgramWriter.Close();
            //    Console.WriteLine("XDL diffgram has been saved to " + diffgramFileName + ".");
            //}
            #endregion



            


            #region Step1: SOMDiff invocation

            SOMDiff sdiff = new SOMDiff();

            // Get the dependency graph
            string sourcepath = @"D:\8-GitHub\TSEA\TSEA\ConsoleApplication1\ConsoleApplication1\Resources\OriginalXSD\Request";
            string changepath = @"D:\8-GitHub\TSEA\TSEA\ConsoleApplication1\ConsoleApplication1\Resources\ChangedXSD\Request";

            sdiff.ParseSchemaDependency(sourcepath);
            sdiff.ParseSchemaDependency(changepath);

            // Diff a specific pair of XSD files
            string sourefile = @"D:\8-GitHub\TSEA\TSEA\ConsoleApplication1\ConsoleApplication1\Resources\OriginalXSD\Request\Calendar.xsd";
            string changefile = @"D:\8-GitHub\TSEA\TSEA\ConsoleApplication1\ConsoleApplication1\Resources\ChangedXSD\Request\cal.xsd";
            sdiff.DiffSchemas(sourefile, changefile);

            Console.Read();

            #endregion


            #region Step2: Launch xsd.exe to generate proxy class automatically

            string directory = @"D:\8-GitHub\TSEA\TSEA\ConsoleApplication1\ConsoleApplication1\Resources\OriginalXSD\Response";
            //directory = @"C:\Users\v-liuxin\Desktop\Newfolder\Request";
            string xsdpath = @"D:\8-GitHub\TSEA\TSEA\ConsoleApplication1\ConsoleApplication1\Resources\OriginalXSD\Response";
            //xsdpath = @"C:\Users\v-liuxin\Desktop\Newfolder\Request";
            string[] xsdfiles = Directory.GetFiles(xsdpath, "*.xsd", SearchOption.TopDirectoryOnly);

            StringBuilder arguments = new StringBuilder();

            foreach (string file in xsdfiles)
            {
                arguments.Append(file);
                arguments.Append(" ");
            }

            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = @"C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0A\bin\NETFX 4.0 Tools\xsd.exe";
            processStartInfo.Arguments = arguments.ToString() + "/classes /language:cs /n:TSEA.Original.Response";
            processStartInfo.WorkingDirectory = directory;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.UseShellExecute = false;

            // Start xsd.exe
            Process xsdtool = new Process();
            xsdtool.StartInfo = processStartInfo;
            xsdtool.Start();
            string output = xsdtool.StandardOutput.ReadToEnd();
            xsdtool.WaitForExit();

            string[] lines = output.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                Console.WriteLine(line);
            }

            Console.Read();

            #endregion

            


            #region Test alternative methods
            

            //TestExpandRef();
            //Console.Read();



            //string sourefile = @"D:\8-GitHub\TSEA\TSEA\ConsoleApplication1\ConsoleApplication1\Resources\OriginalXSD\Email2.xsd";
            string path = @"D:\8-GitHub\TSEA\TSEA\ConsoleApplication1\ConsoleApplication1\Resources\OriginalXSD\result.xsd";

            XmlTextReader reader = new XmlTextReader(sourefile);
            XmlSchema myschema = XmlSchema.Read(reader, ValidationCallBack);

            // Add the customer schema to a new XmlSchemaSet and compile it.
            // Any schema validation warnings and errors encountered reading or 
            // compiling the schema are handled by the ValidationEventHandler delegate.
            XmlSchemaSet schemaSet = new XmlSchemaSet();
            schemaSet.ValidationEventHandler += new ValidationEventHandler(ValidationCallBack);
            schemaSet.Add(myschema);
            schemaSet.Compile();

            // Retrieve the compiled XmlSchema object from the XmlSchemaSet
            // by iterating over the Schemas property.
            XmlSchema customerSchema = null;
            foreach (XmlSchema schema in schemaSet.Schemas())
            {
                customerSchema = schema;
            }

            // Iterate over each XmlSchemaElement in the Values collection
            // of the Elements property.
            foreach (XmlSchemaElement element in customerSchema.Elements.Values)
            {

                Console.WriteLine("Element: {0}", element.Name);

                // Get the complex type of the Customer element.
                XmlSchemaComplexType complexType = element.ElementSchemaType as XmlSchemaComplexType;

                // If the complex type has any attributes, get an enumerator 
                // and write each attribute name to the console.
                if (complexType.AttributeUses.Count > 0)
                {
                    IDictionaryEnumerator enumerator =
                        complexType.AttributeUses.GetEnumerator();

                    while (enumerator.MoveNext())
                    {
                        XmlSchemaAttribute attribute =
                            (XmlSchemaAttribute)enumerator.Value;

                        Console.WriteLine("Attribute: {0}", attribute.Name);
                    }
                }

                // Get the sequence particle of the complex type.
                XmlSchemaSequence sequence = complexType.ContentTypeParticle as XmlSchemaSequence;

                // Iterate over each XmlSchemaElement in the Items collection.
                foreach (XmlSchemaElement childElement in sequence.Items)
                {
                    Console.WriteLine("Element: {0}", childElement.Name);
                }
            }


            //try
            //{
            //    string sourefile = @"D:\8-GitHub\TSEA\TSEA\ConsoleApplication1\ConsoleApplication1\Resources\OriginalXSD\Email2.xsd";
            //    string path = @"D:\8-GitHub\TSEA\TSEA\ConsoleApplication1\ConsoleApplication1\Resources\OriginalXSD\result.xsd";

            //    XmlTextReader reader = new XmlTextReader(sourefile);
            //    XmlSchema myschema = XmlSchema.Read(reader, ValidationCallBack);
                
            //    // output the schema to console
            //    myschema.Write(Console.Out);

            //    // output the schema to a file
            //    FileStream file = new FileStream(path, FileMode.Create, FileAccess.ReadWrite);
            //    XmlTextWriter xwriter = new XmlTextWriter(file, new UTF8Encoding());
            //    xwriter.Formatting = Formatting.Indented;
            //    myschema.Write(xwriter);
            //}
            //catch (System.Exception ex)
            //{
            //    Console.WriteLine(ex);
            //}


            Console.Read();

            #endregion


            #region XmlDiff invocation

            string soureFile = @"D:\8-GitHub\TSEA\TSEA\ConsoleApplication1\ConsoleApplication1\Resources\OriginalXSD\SettingsResponse.xsd";
            string changedFile = @"D:\8-GitHub\TSEA\TSEA\ConsoleApplication1\ConsoleApplication1\Resources\ChangedXSD\setres.xsd";
            
            XmlDiff diff = new XmlDiff(soureFile, changedFile);

            diff.Parse();

            diff.Diff();

            Console.Read();

            #endregion

        }

        private static void ValidationCallBack(object sender, ValidationEventArgs args)
        {
            if (args.Severity == XmlSeverityType.Warning)
                Console.Write("WARNING: ");
            else if (args.Severity == XmlSeverityType.Error)
                Console.Write("ERROR: ");

            Console.WriteLine(args.Message);
        }

        private static void TestExpandRef()
        {
            // output the schema to a file
            string path = @"D:\8-GitHub\TSEA\TSEA\ConsoleApplication1\ConsoleApplication1\Resources\OriginalXSD\result.xsd";
            FileStream file = new FileStream(path, FileMode.Create, FileAccess.ReadWrite);
            XmlTextWriter xwriter = new XmlTextWriter(file, new UTF8Encoding());
            xwriter.Formatting = Formatting.Indented;

            string email = @"D:\8-GitHub\TSEA\TSEA\ConsoleApplication1\ConsoleApplication1\Resources\OriginalXSD\Email.xsd";
            string email2 = @"D:\8-GitHub\TSEA\TSEA\ConsoleApplication1\ConsoleApplication1\Resources\OriginalXSD\Email2.xsd";

            XmlTextReader reader1 = new XmlTextReader(email);
            XmlSchema emailSchema = XmlSchema.Read(reader1, ValidationCallBack);

            XmlTextReader reader2 = new XmlTextReader(email2);
            XmlSchema email2Schema = XmlSchema.Read(reader2, ValidationCallBack);

            // Add the customer and address schemas to a new XmlSchemaSet and compile them.
            // Any schema validation warnings and errors encountered reading or 
            // compiling the schemas are handled by the ValidationEventHandler delegate.
            XmlSchemaSet schemaSet = new XmlSchemaSet();
            schemaSet.ValidationEventHandler += new ValidationEventHandler(ValidationCallBack);
            schemaSet.Add(emailSchema);
            schemaSet.Add(email2Schema);
            schemaSet.Compile();

            //// Retrieve the compiled XmlSchema objects for the customer and
            //// address schema from the XmlSchemaSet by iterating over 
            //// the Schemas property.
            //XmlSchema customerSchema = null;
            //XmlSchema addressSchema = null;
            //foreach (XmlSchema schema in schemaSet.Schemas())
            //{
            //    if (schema.TargetNamespace == "http://www.tempuri.org")
            //        customerSchema = schema;
            //    else if (schema.TargetNamespace == "http://www.example.com/IPO")
            //        addressSchema = schema;
            //}
            
            // Create an XmlSchemaImport object, set the Namespace property
            // to the namespace of the address schema, the Schema property 
            // to the address schema, and add it to the Includes property
            // of the customer schema.
            XmlSchemaImport import = new XmlSchemaImport();
            import.Namespace = email2Schema.TargetNamespace;
            import.Schema = email2Schema;
            emailSchema.Includes.Add(import);

            // Reprocess and compile the modified XmlSchema object 
            // of the customer schema and write it to the console.    
            schemaSet.Reprocess(emailSchema);
            schemaSet.Compile();
            emailSchema.Write(xwriter);

            // Recursively write all of the schemas imported into the
            // customer schema to the console using the Includes 
            // property of the customer schema.
            RecurseExternals(emailSchema);
        }

        private static void RecurseExternals(XmlSchema schema)
        {
            foreach (XmlSchemaExternal external in schema.Includes)
            {
                if (external.SchemaLocation != null)
                {
                    Console.WriteLine("External SchemaLocation: {0}", external.SchemaLocation);
                }

                if (external is XmlSchemaImport)
                {
                    XmlSchemaImport import = external as XmlSchemaImport;
                    Console.WriteLine("Imported namespace: {0}", import.Namespace);
                }

                if (external.Schema != null)
                {
                    external.Schema.Write(Console.Out);
                    RecurseExternals(external.Schema);
                }

                Console.WriteLine();
            }
        }
    }
}


namespace ConsoleApplication1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;


    using System.Xml;
    using Sam.XmlDiff;

    class Program
    {
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

            string soureFile = @"D:\8-GitHub\TSEA\TSEA\ConsoleApplication1\ConsoleApplication1\Resources\OriginalXSD\Calendar.xsd";
            string changedFile = @"D:\8-GitHub\TSEA\TSEA\ConsoleApplication1\ConsoleApplication1\Resources\ChangedXSD\cal.xsd";
            
            XmlDiff diff = new XmlDiff(soureFile, changedFile);

            diff.Parse();

            diff.Diff();

            Console.Read();
            
        }
    }
}

using System.Xml;
using TelltaleLocalizationTool.Telltale.FileTypes.LDB;

namespace TelltaleLocalizationTool;

internal class Program
{
    private const StringComparison oic = StringComparison.OrdinalIgnoreCase;

    private static void Main(string[] args)//For now, this is for test
    {
        if (args.Length > 0)
        {
            if ((!Path.GetExtension(args[0]).Equals(".landb", oic) && !Path.GetExtension(args[0]).Equals(".xml", oic)) || !File.Exists(args[0]))
            {
                Console.WriteLine("A .landb | .xml file must exist to proceed.");
                Environment.Exit(1);
            }
            string inputLandb = args[0];
            string ouputLandb = GetOutputLandb(inputLandb, ".landb");
            string outoutXmlLandb = GetOutputLandb(inputLandb, ".xml");
            ProccessLandbFile(inputLandb, ouputLandb, outoutXmlLandb);
        }
    }

    private static void ProccessLandbFile(string inputLandb, string outputLandb, string outputLandbXml)
    {
        string extension = Path.GetExtension(inputLandb);
        if (extension.Equals(".landb", oic))
        {
            LandbParseWrite(inputLandb, outputLandb, outputLandbXml);
            if (!IsEquals(inputLandb, outputLandb))
            {
                throw new Exception("Input and output files do not match!(.landb)");
            }
        }
        else if (extension.Equals(".xml"))
        {
            LandbParseWriteXml(inputLandb, outputLandb);
            string newTemp = Path.GetTempFileName();
            File.Delete(newTemp);
            string newLandb = Path.ChangeExtension(newTemp, ".landb");
            File.Create(newLandb).Close();
            string newXml = Path.ChangeExtension(newTemp, ".xml");
            File.Create(newXml).Close();
            ProccessLandbFile(outputLandb, newTemp, newXml);
            File.Delete(newXml);
            File.Delete(newLandb);
        }
    }


    private static void LandbParseWriteXml(string inputLandbXml, string outputLandb)
    {
        XmlDocument landbXmlDoc = new();
        landbXmlDoc.Load(inputLandbXml);
        XmlElement root = landbXmlDoc.DocumentElement ?? throw new InvalidDataException("The input XML file has no elements!");
        using FileStream outputLandbStream = new(outputLandb, FileMode.Create, FileAccess.Write);
        using BinaryWriter outputBinStreamWriter = new(outputLandbStream);
        Landb landb = new();
        landb.Parse(root, nameof(landb));
        landb.Write(outputBinStreamWriter);
    }

    private static void LandbParseWrite(string inputLandb, string outputLandb, string outputLandbXml)
    {
        using FileStream inputLandbStream = new(inputLandb, FileMode.Open, FileAccess.Read);
        using BinaryReader inputLansbBinStreamReader = new(inputLandbStream);
        using FileStream outputLandbStream = new(outputLandb, FileMode.Create, FileAccess.Write);
        using BinaryWriter outputBinStreamWriter = new(outputLandbStream);
        Landb landb = new();
        landb.Parse(inputLansbBinStreamReader);
        landb.Write(outputBinStreamWriter);
        XmlDocument doc = new();
        doc.AppendChild(doc.AppendChild(doc.CreateElement("Root"))!);
        landb.Write(doc.DocumentElement!, nameof(landb));
        doc.Save(outputLandbXml);
    }

    private static bool IsEquals(string file1, string file2)
    {
        {
            using FileStream fs1 = new(file1, FileMode.Open, FileAccess.Read, FileShare.Read);
            using FileStream fs2 = new(file2, FileMode.Open, FileAccess.Read, FileShare.Read);

            if (fs1.Length != fs2.Length)
            {
                return false;
            }

            const int bufferSize = 4096;
            byte[] buffer1 = new byte[bufferSize];
            byte[] buffer2 = new byte[bufferSize];

            int bytesRead1;
            while ((bytesRead1 = fs1.Read(buffer1, 0, bufferSize)) > 0)
            {
                int bytesRead2 = fs2.Read(buffer2, 0, bufferSize);

                if (bytesRead1 != bytesRead2)
                {
                    return false;
                }

                if (!buffer1.AsSpan(0, bytesRead1).SequenceEqual(buffer2.AsSpan(0, bytesRead1)))
                {
                    return false;
                }
            }

            return true;
        }
    }
    private static string GetOutputLandb(string fileName, string? extension = null)
    {
        string name = Path.GetFileNameWithoutExtension(fileName);
        extension ??= Path.GetExtension(fileName);
        string inputDirectory = Path.GetDirectoryName(fileName) ?? throw new InvalidDataException($"Couldn't find the folder associated with the \"{fileName}\" path.");
        string outputLandb = Path.Combine(inputDirectory, $"{name}_test{extension}");
        return outputLandb;
    }
}

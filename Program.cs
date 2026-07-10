using System.Xml;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Convert;
using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Versions;
using TelltaleLocalizationTool.Telltale.FileTypes.Font;
using TelltaleLocalizationTool.Telltale.FileTypes.LDB;

namespace TelltaleLocalizationTool;

internal class Program
{
    public static Config Config { get; } = new Config();

    private const StringComparison oic = StringComparison.OrdinalIgnoreCase;

    private static void Main(string[] args)//For now, this is for test
    {
        if (args.Length > 0)
        {
            if (Path.GetExtension(args[0]).Equals(".d3dtx"))
            {
                TestD3dtx(args[0]);
                return;
            }
            if (Path.GetExtension(args[0]).Equals(".font"))
            {
                TestFont(args[0]);
                return;
            }
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

    private static void TestD3dtx(string inputPath)
    {
        string firstOutputPath = Path.ChangeExtension(inputPath, ".first_output.d3dtx");
        string xmlOutputPath = Path.ChangeExtension(inputPath, ".xml");
        string secondOutputPath = Path.ChangeExtension(inputPath, ".from_xml.d3dtx");
        _ = Path.ChangeExtension(inputPath, Formats.ddsExtension);
        // Step 1: Read original, write first binary output
        D3DTXV9 d3 = new();
        using (FileStream input = new(inputPath, FileMode.Open, FileAccess.Read))
        using (FileStream output1 = new(firstOutputPath, FileMode.Create, FileAccess.Write))
        using (BinaryReader reader = new(input))
        using (BinaryWriter writer = new(output1))
        {
            d3.Parse(reader);
            d3.Write(writer);
        }
        string outputBmp = Path.ChangeExtension(firstOutputPath, Formats.bmpExtension);
        string outputD3dtxFromBmp = Path.Combine(Path.GetDirectoryName(outputBmp)!, $"{Path.GetFileNameWithoutExtension(outputBmp)}.from_bmp{Formats.d3dtxExtension}");
        string outputDdsFromBmpETC2_RGBA = Path.Combine(Path.GetDirectoryName(outputBmp)!, $"{Path.GetFileNameWithoutExtension(outputBmp)}.from_Bmp_ETC2_RGBA{Formats.ddsExtension}");
        _ = Path.Combine(Path.GetDirectoryName(outputBmp)!, $"{Path.GetFileNameWithoutExtension(outputBmp)}.from_dds_ETC2_RGBA{Formats.d3dtxExtension}");
        Converter.ConvertTextureFromD3DtxToOthersV9(firstOutputPath, outputBmp, new());
        Converter.ConvertTextureFromOthersToD3DtxV9(outputBmp, outputD3dtxFromBmp, new(), Path.ChangeExtension(firstOutputPath, Formats.xmlExtension));
        _ = Converter.CompressWithCompressonatorCli(outputBmp, outputDdsFromBmpETC2_RGBA);
        // Step 2: Byte-by-byte comparison of original vs first output
        Console.WriteLine("Comparing original <-> first binary output...");
        bool firstEqual = CompareFilesByteByByte(inputPath, firstOutputPath);
        if (!firstEqual)
        {
            throw new Exception($"Byte mismatch between original file and first binary output: '{inputPath}' vs '{firstOutputPath}'");
        }

        // Step 3: Save XML
        D3DTXV9 d3ForXml = new();
        using (FileStream input = new(inputPath, FileMode.Open, FileAccess.Read))
        using (BinaryReader reader = new(input))
        {
            d3ForXml.Parse(reader);
        }
        XmlDocument doc = new();
        XmlElement root = doc.CreateElement("Root");
        _ = doc.AppendChild(root);
        d3ForXml.Write(root, "Texture");
        doc.Save(xmlOutputPath);
        Console.WriteLine($"XML saved to: {xmlOutputPath}");

        // Step 4: Load XML and write second binary output
        XmlDocument doc2 = new();
        doc2.Load(xmlOutputPath);
        D3DTXV9 d3FromXml = new();
        d3FromXml.Parse(doc2.DocumentElement!, "Texture");
        using (FileStream output2 = new(secondOutputPath, FileMode.Create, FileAccess.Write))
        using (BinaryWriter writer2 = new(output2))
        {
            d3FromXml.Write(writer2);
        }

        // Step 5: Compare first output vs second output
        Console.WriteLine("\nComparing first binary output <-> XML‑roundtripped binary output...");
        bool secondEqual = CompareFilesByteByByte(firstOutputPath, secondOutputPath);
        if (!secondEqual)
        {
            throw new Exception($"Byte mismatch between first binary output and XML‑roundtripped output: '{firstOutputPath}' vs '{secondOutputPath}'");
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\nAll round‑trip tests passed successfully.");
        Console.ResetColor();
    }

    private static void TestFont(string inputPath)
    {
        string outputDir = Path.GetDirectoryName(inputPath) ?? throw new NullReferenceException("I can't find your font dir!");
        string exportImportDir = Path.Combine(outputDir, "ExportImport");
        string ExportFnt = Path.Combine(exportImportDir, "Export", $"{Path.GetFileNameWithoutExtension(inputPath)}{Formats.fntExtension}");
        string ImportFnt = Path.Combine(exportImportDir, "Import", $"{Path.GetFileNameWithoutExtension(inputPath)}{Formats.fntExtension}");
        string fontName = Path.GetFileNameWithoutExtension(inputPath);
        string outputPath = Path.Combine(outputDir, $"{fontName}_test.font");
        string outputXmlFile = Path.Combine(outputDir, $"{fontName}_test.xml");
        string outputXmlFontFile = Path.Combine(outputDir, $"{fontName}_test.xml.font");

        TFont font = new();
        using (FileStream inputStream = new(inputPath, FileMode.Open, FileAccess.Read))
        using (BinaryReader reader = new(inputStream))
        using (FileStream outputStream = new(outputPath, FileMode.Create, FileAccess.Write))
        using (BinaryWriter writer = new(outputStream))
        {
            font.Parse(reader);
            font.Write(writer);
            _ = Directory.CreateDirectory(exportImportDir);
            _ = Directory.CreateDirectory(Path.GetDirectoryName(ExportFnt)!);
            _ = Directory.CreateDirectory(Path.GetDirectoryName(ImportFnt)!);
            font.Export(ExportFnt);
#if false //Export and Import aren't fully implemented yet, so for now we're just focusing on exporting font atlases until the .fnt file is fully built for export — later, that same file and its atlases can be imported.
            File.Copy(ExportFnt, ImportFnt, true);
            font.Import(ImportFnt);
#endif
        }

        if (!CompareFilesByteByByte(inputPath, outputPath))
        {
            throw new Exception("Binary write-back does not match original file.");
        }

        XmlDocument odoc = new();
        _ = odoc.AppendChild(odoc.CreateElement("Root"));
        if (odoc.DocumentElement == null)
        {
            throw new NullReferenceException("I can't find your root element! WTF<!>?");
        }

        font.Write(odoc.DocumentElement, "myfont");
        odoc.Save(outputXmlFile);

        TFont rxmlFont = new();
        XmlDocument idoc = new();
        idoc.Load(outputXmlFile);
        if (idoc.DocumentElement == null)
        {
            throw new NullReferenceException("I can't find your root element! WTF<!>?");
        }

        rxmlFont.Parse(idoc.DocumentElement, "myfont");

        using (FileStream xmlFontStream = new(outputXmlFontFile, FileMode.Create, FileAccess.Write))
        using (BinaryWriter xmlWriter = new(xmlFontStream))
        {
            rxmlFont.Write(xmlWriter);
        }

        if (!CompareFilesByteByByte(inputPath, outputXmlFontFile))
        {
            throw new Exception("XML round-trip output does not match original file.");
        }
    }

    // Helper: compare two files byte by byte, printing colored results
    private static bool CompareFilesByteByByte(string file1, string file2)
    {
        byte[] bytes1 = File.ReadAllBytes(file1);
        byte[] bytes2 = File.ReadAllBytes(file2);

        int maxLen = Math.Max(bytes1.Length, bytes2.Length);
        bool identical = true;

        for (int i = 0; i < maxLen; i++)
        {
            byte b1 = i < bytes1.Length ? bytes1[i] : (byte)0;
            byte b2 = i < bytes2.Length ? bytes2[i] : (byte)0;

            if (b1 != b2)
            {
                identical = false;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Offset {i:X8}: {b1:X2} != {b2:X2}");
            }
            else
            {
                // Optional: print progress or only differences
                // To keep output manageable, we skip printing every equal byte.
            }
        }

        Console.ResetColor();
        if (identical)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Files are identical.");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Mismatch count: found differences.");
            Console.ResetColor();
        }

        return identical;
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
        _ = doc.AppendChild(doc.AppendChild(doc.CreateElement("Root"))!);
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

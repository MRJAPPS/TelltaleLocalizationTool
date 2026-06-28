using Microsoft.Extensions.Configuration;

namespace TelltaleLocalizationTool.Helper;
public class Config
{
    private const string appsettingsDefault = "appsettings.json";
    private readonly string appsettingsJsonFileName = appsettingsDefault;
    private readonly IConfiguration config;

    #region Common constants
    public const uint Msv6Magic = 0x4d535636;
    #endregion

    public Config(string appsettingsJsonFileName, IConfiguration config)
    {
        this.appsettingsJsonFileName = appsettingsJsonFileName;
        this.config = config;
    }

    public Config(string appsettingsJsonFileName)
    {
        this.appsettingsJsonFileName = appsettingsJsonFileName;
        config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(appsettingsJsonFileName, optional: false, reloadOnChange: true)
            .Build();
    }

    public Config() : this(appsettingsDefault) { }

    #region Props

    public string CompressonatorPath
    {
        get
        {
            string ret = config["CompressonatorPath"];
            return !File.Exists(ret)
                ? throw new FileNotFoundException("Set a valid CompressonatorPath in appsettings.json! Keep in mind, this app can't do a thing without compressonatorcli version 4.5.52!")
                : ret;
        }
    }

    #endregion
}

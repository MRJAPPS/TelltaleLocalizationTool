using TelltaleLocalizationTool.Telltale.FileTypes.D3dtx.Enums;
using TelltaleLocalizationTool.Telltale.Meta;
using TelltaleLocalizationTool.Telltale.Meta.Enums;
using TelltaleLocalizationTool.Telltale.Meta.Versions;

namespace TelltaleLocalizationTool.Helper;
public static class Utils
{
    #region Map

    public static T3SurfaceFormat GetDefaultFormat(Games game)
    {
        return game switch
        {
            Games.WDS3NF => T3SurfaceFormat.ETC2_RGBA,
            Games.WDS1 => T3SurfaceFormat.ARGB8,
            _ => throw new NotImplementedException("WTF? what is this game? should I use RGBA8?? no... I don't..."),
        };
    }

    #endregion

    #region Checking

    public static void CheckHeader(Games game, IMetaHeader? header)
    {
        switch (game)
        {
            case Games.WDS3NF:
                if (header is not MSV6)
                {
                    throw new InvalidDataException($"Bro, are you living in a parallel universe? Because in my world {nameof(Games.WDS3NF)} uses {nameof(MSV6)}! Your file isn't MSV6!");
                }

                break;
            case Games.WDS1:
                if (header is not MSV5)
                {
                    throw new InvalidDataException($"Bro, are you living in a parallel universe? Because in my world {nameof(Games.WDS1)} uses {nameof(MSV5)}! Your file isn't MSV6!");
                }

                break;
            default:
                throw new NotImplementedException($"Your input is not valid! USE {nameof(Games.WDS3NF)} or  {nameof(Games.WDS1)}");
        }
    }


    #endregion
}

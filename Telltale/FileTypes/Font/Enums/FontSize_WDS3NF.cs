namespace TelltaleLocalizationTool.Telltale.FileTypes.Font.Enums;

/// I don't think we actually need to use this enum — it's more of a reference note to check sizes later.
public enum FontSize_WDS3NF // mWiiScale="0.5" mFontScale="1"
{
    // MenuBody*
    MenuBodyLarge = 90,   // 44
    MenuBodyMedium = 62,  // 30
    MenuBodySmall = 43,   // 22

    // MenuDisplay*
    MenuDisplayLarge = 85,  // 41
    MenuDisplayMedium = 51, 
    MenuDisplaySmall = 35,  // 18

    // GameplayBody*
    GameplayBodyLarge = 67,   // 33
    GameplayBodyMedium = 48,  // 24
    GameplayBodySmall = 36,   // 19

    // GameplayDisplay*
    GameplayDisplayLarge = 64,   // 32
    GameplayDisplayMedium = 40,  // 20
    GameplayDisplaySmall = 28,   // 15

    // Unrelated / last
    Default = 16,             // 16
    DCCSharpDistress = -1,    // We don't use this
}
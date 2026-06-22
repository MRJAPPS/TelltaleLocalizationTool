namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.Enum;

/// <summary>
/// Home for all the enum types used in the Landb file parsing.
/// Nothing fancy, just some named numbers.
/// </summary>
public static class Enums
{
    #region ERecordingStatus

    /// <summary>
    /// How far along the recording of a voice line is. Goes from “not started” to “delivered”.
    /// </summary>
    public enum ERecordingStatus : int
    {
        eRecordingStatus_None = 0,
        eRecordingStatus_AtStudio = 1,
        eRecordingStatus_Recorded = 2,
        eRecordingStatus_Delivered = 3
    };

    #endregion

    #region ETextItemFlags

    /// <summary>
    /// Bit flags that describe a text item – is it voice‑acted? should we hide subtitles? etc.
    /// </summary>
    [Flags]
    public enum ETextItemFlags : uint
    {
        Unknown = 0,
        Synthesized = 1,
        IgnoreTextLipSync = 2,
        HideSubtitles = 4,
        IsVoiced = 8,
        IsSilent = 16,
        NoAnimation = 32,
        MenuOrDisplayOrIDK = 64,
    }

    #endregion
}
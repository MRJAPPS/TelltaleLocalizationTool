namespace TelltaleLocalizationTool.Telltale.FileTypes.LandbTypes.Enum;

public static class Enums
{
    public enum ERecordingStatus : int
    {
        eRecordingStatus_None = 0,
        eRecordingStatus_AtStudio = 1,
        eRecordingStatus_Recorded = 2,
        eRecordingStatus_Delivered = 3
    };

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
}

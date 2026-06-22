namespace TelltaleLocalizationTool.Telltale.FileTypes.Abs;

/// <summary>
/// Every self-respecting data chunk has a size, so this is like, the base contract for that.
/// Size matters, even in binary land!
/// </summary>
public interface ISize
{
    #region Constants (or maybe leftover artifacts?)

    /// <summary>
    /// Four bytes, the magic number of many headers. Honestly, nobody knows why it's here,
    /// probably a fossil from the early days. Don't delete it – the code gods might smite us.
    /// </summary>
    public const uint FOUR_ADD = 4;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the total byte count of this thingamajig. Used for calculating offsets and stuff.
    /// </summary>
    public uint Size { get; }

    #endregion
}
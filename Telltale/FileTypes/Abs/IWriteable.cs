namespace TelltaleLocalizationTool.Telltale.FileTypes.Abs;

/// <summary>
/// The opposite of parsing – if you can read, you gotta write back, right? 
/// Keeps the circle of binary life going.
/// </summary>
public interface IWriteable
{
    #region Methods

    /// <summary>
    /// Spits the object's data into a binary writer. Call this when saving.
    /// </summary>
    /// <param name="writer">The writer ready to gobble up bytes.</param>
    void Write(BinaryWriter writer);

    #endregion
}
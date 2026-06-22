namespace TelltaleLocalizationTool.Telltale.FileTypes.Abs;

/// <summary>
/// Marks a type as "hey, I can read myself from a binary stream!" 
/// Also inherits <see cref="ISize"/> because you gotta know your byte footprint.
/// </summary>
public interface IParsable : ISize, IWriteable, IPosition
{
    #region Properties

    #endregion

    #region Methods

    /// <summary>
    /// Reads and populates this object from the given binary reader. 
    /// Make sure the stream is at the right spot, or kaboom.
    /// </summary>
    /// <param name="reader">The reader positioned at the start of this chunk's data.</param>
    void Parse(BinaryReader reader);

    #endregion
}
namespace TelltaleLocalizationTool.Telltale.FileTypes.Abs;

public interface IParsable : ISize, IWriteable, IPosition
{
    #region Properties

    #endregion

    #region Methods

    void Parse(BinaryReader reader);

    #endregion
}
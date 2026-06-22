namespace TelltaleLocalizationTool.Telltale.FileTypes.Abs;

public interface IWriteable : IPosition
{
    #region Methods

    void Write(BinaryWriter writer);

    #endregion
}
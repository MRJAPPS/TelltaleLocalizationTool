namespace TelltaleLocalizationTool.Telltale.Meta;

/// <summary>
/// Interface for serializing a meta header.
/// </summary>
public interface IMetaHeader : ITelltaleSerializable
{
    //Older headers don't have separate debug or async sections.
    uint mDebugSectionChunkSize => 0;
    uint mAsyncSectionChunkSize => 0;

    /// <summary>
    /// Gets the size of the header in bytes.
    /// </summary>
    /// <returns></returns>
    uint GetHeaderByteSize();

    /// <summary>
    /// Sets the chunk sections.
    /// </summary>
    /// <param name="defaultSectionChunkSize"></param>
    /// <param name="debugSectionChunkSize"></param>
    /// <param name="asyncSectionChunkSize"></param>
    void SetMetaSectionChunkSizes(uint defaultSectionChunkSize, uint debugSectionChunkSize, uint asyncSectionChunkSize);
}

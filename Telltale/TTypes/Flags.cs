using System.ComponentModel;
using System.Xml;
using TelltaleLocalizationTool.Helper;
using TelltaleLocalizationTool.Telltale.FileTypes.Abs;
using TelltaleLocalizationTool.Telltale.Meta;

namespace TelltaleLocalizationTool.Telltale.TTypes;

[Description("Processing Flags(4 byte)")]
public class Flags<T> : ReportBase, ITelltaleObject
    where T : struct, IConvertible
{
    #region Private Fields

    private readonly Type ValueType = typeof(T).IsEnum ? Enum.GetUnderlyingType(typeof(T)) : typeof(T);

    #endregion

    #region Constructors

    public Flags(T value)
    {
        if (ValueType != typeof(uint) && ValueType != typeof(int))
        {
            throw new NotSupportedException($"Flags type {typeof(T)} must be uint, int or a 32-bit enum");
        }

        Value = value;
    }

    public Flags() : this(default) { }

    #endregion

    #region Public Properties

    public T Value { get; set; }

    public override string ToString() => Value.ToString()!;

    #endregion

    #region Implement

    public MetaStreamContext? msCtx { get; protected set; }

    MetaStreamContext? ITelltaleObject.msCtx
    {
        get => msCtx;
        set => msCtx = value;
    }

    public uint Size => sizeof(uint);

    public long Position { get; private set; } = -1;

    public void Parse(BinaryReader reader)
    {
        Position = reader.BaseStream.Position;
        object rawValue = ValueType == typeof(int) ? (object)reader.ReadInt32() : reader.ReadUInt32();
        Value = typeof(T).IsEnum ? (T)Enum.ToObject(typeof(T), rawValue) : (T)rawValue;
        Report(this);
    }

    public void Parse(XmlElement reader, string name)
    {
        XmlElement myElement = reader.GetSingleElement(nameof(Flags<T>), name);
        string value = myElement.GetAttribute(nameof(Value));
        Value = typeof(T).IsEnum ? (T)Enum.Parse(typeof(T), value) : (T)Convert.ChangeType(value, typeof(T));
    }

    public void Write(BinaryWriter writer)
    {
        Position = writer.BaseStream.Position;
        if (ValueType == typeof(int))
        {
            writer.Write(Convert.ToInt32(Value));
        }
        else
        {
            writer.Write(Convert.ToUInt32(Value));
        }
        Report(this, true);
    }

    public void Write(XmlElement element, string name)
    {
        XmlElement myElement = element.OwnerDocument.CreateElement(nameof(Flags<T>));
        myElement.SetAttribute(nameof(name), name);

        XmlAttribute Value = myElement.OwnerDocument.CreateAttribute(nameof(Value));
        Value.Value = this.Value.ToString();
        _ = myElement.Attributes.Append(Value);

        _ = element.AppendChild(myElement);
    }

    #endregion

    #region Implicit Conversions

    public static implicit operator Flags<T>(T value) => new(value);

    public static implicit operator T(Flags<T> flags) => flags.Value;

    #endregion
}

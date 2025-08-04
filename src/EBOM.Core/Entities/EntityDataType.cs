namespace EBOM.Core.Entities;

public class EntityDataType : BaseEntity
{
    public int DataTypeID { get; set; }
    public string DataTypeName { get; set; } = string.Empty;
    public string? DataTypeDescription { get; set; }
    public string? DataTypeFormat { get; set; }
}
using NewAgePOSModels.Utilities;
using System;

namespace NewAgePOSModels.Models
{
  public class TransactionModel
  {
    private MethodEnum _method;
    private TypeEnum _type;
    private DateTime _created;

    public int Id { get; set; }
    public int SaleId { get; set; }
    public int? GiftCardId { get; set; }
    public float Amount { get; set; }
    public MethodEnum Method {
      get { return _method; }
      set { _method = (MethodEnum)Enum.Parse(typeof(MethodEnum), value.ToString()); }
    }
    public TypeEnum Type {
      get { return _type; }
      set { _type = (TypeEnum)Enum.Parse(typeof(TypeEnum), value.ToString()); }
    }
    public DateTime Created {
      get { return _created.UTCtoPST(); }
      set { _created = value; }
    }
  }
}

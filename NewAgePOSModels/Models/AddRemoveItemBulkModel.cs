namespace NewAgePOSModels.Models
{
  public class AddRemoveItemBulkModel
  {
    public string Code { get; set; }
    public string LocationCode { get; set; }
    public int Quantity { get; set; }
    public string Reason { get; set; }
  }
}

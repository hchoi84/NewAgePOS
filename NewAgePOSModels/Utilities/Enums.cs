namespace NewAgePOSModels.Models
{
  public enum MethodEnum
  {
    GiftCard,
    Cash,
    Give,
    Change
  }

  public enum TypeEnum
  {
    Checkout,
    Refund
  }

  public enum StatusEnum
  {
    Pending,
    Ready,
    Picking,
    Complete
  }

  public enum TransferPageTypeEnum
  {
    Single,
    Batch,
    Review
  }
}

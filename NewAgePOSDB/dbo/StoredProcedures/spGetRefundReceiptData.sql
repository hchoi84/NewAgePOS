CREATE PROCEDURE [dbo].[spGetRefundReceiptData]
	@transactionId int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT s.Id AS SaleId, st.Id AS TransactionId, st.Amount, st.PaymentType, st.Created, sl.DiscAmt, sl.DiscPct, rl.Qty AS RefundQty, sl.Price, t.TaxPct
	FROM dbo.Transactions st
	INNER JOIN dbo.RefundLines rl ON st.Id = rl.TransactionId
	INNER JOIN dbo.SaleLines sl ON rl.SaleLineId = sl.Id
	INNER JOIN dbo.Sales s ON st.SaleId = s.Id
	INNER JOIN dbo.Taxes t ON s.TaxId = t.Id
	WHERE st.Id = @transactionId;
END
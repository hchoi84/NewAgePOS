CREATE PROCEDURE [dbo].[spGetRefundReceiptData]
	@saleTransactionId int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT s.Id AS SaleId, st.Id AS SaleTransactionId, st.Amount, st.PaymentType, st.Created, sl.DiscAmt, sl.DiscPct, rl.Qty AS RefundQty, p.Price, t.TaxPct
	FROM dbo.SaleTransactions st
	INNER JOIN dbo.RefundLines rl ON st.Id = rl.SaleTransactionId
	INNER JOIN dbo.SaleLines sl ON rl.SaleLineId = sl.Id
	INNER JOIN dbo.Products p ON sl.ProductId = p.Id
	INNER JOIN dbo.Sales s ON st.SaleId = s.Id
	INNER JOIN dbo.Taxes t ON s.TaxId = t.Id
	WHERE st.Id = @saleTransactionId;
END
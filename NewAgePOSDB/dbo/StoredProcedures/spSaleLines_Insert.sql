CREATE PROCEDURE [dbo].[spSaleLines_Insert]
	@saleId INT,
	@productId INT,
	@qty INT,
	@discAmt INT,
	@discPct INT,
	@refundQty INT
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO dbo.SaleLines(SaleId, ProductId, Qty, DiscAmt, DiscPct, RefundQty)
	VALUES (@saleId, @productId, @qty, @discAmt, @discPct, @refundQty)
END

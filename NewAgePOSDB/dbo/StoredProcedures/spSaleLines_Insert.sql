CREATE PROCEDURE [dbo].[spSaleLines_Insert]
	@saleId int, 
	@productId int, 
	@qty int
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO dbo.SaleLines (SaleId, ProductId, Qty, DiscAmt, DiscPct, RefundQty)
	VALUES (@saleId, @productId, @qty, 0, 0, 0);
END
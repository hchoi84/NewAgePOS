CREATE PROCEDURE [dbo].[spSaleLines_Update]
	@id int,
	@qty int,
	@discAmt float,
	@discPct float
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE dbo.SaleLines
	SET Qty = @qty, DiscAmt = @discAmt, DiscPct = @discPct
	WHERE Id = @id
END
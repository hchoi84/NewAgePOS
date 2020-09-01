CREATE PROCEDURE [dbo].[spSaleLines_Update]
	@id int,
	@qty int,
	@discPct float
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE dbo.SaleLines
	SET Qty = @qty, DiscPct = @discPct
	WHERE Id = @id
END
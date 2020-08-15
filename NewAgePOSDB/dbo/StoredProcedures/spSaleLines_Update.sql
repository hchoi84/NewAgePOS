CREATE PROCEDURE [dbo].[spSaleLines_Update]
	@id INT,
	@qty INT,
	@discAmt INT,
	@discPct INT
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE dbo.SaleLines
	SET Qty = @qty, DiscAmt = @discAmt, DiscPct = @discPct, Updated = GETDATE()
	WHERE Id = @id;
END

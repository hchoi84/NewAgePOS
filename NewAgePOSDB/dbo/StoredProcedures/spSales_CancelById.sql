CREATE PROCEDURE [dbo].[spSales_CancelById]
	@id int
AS
BEGIN
	SET NOCOUNT ON;

	DELETE FROM dbo.SaleLines WHERE SaleId = @id;

	DELETE FROM dbo.Sales WHERE Id = @id;

END
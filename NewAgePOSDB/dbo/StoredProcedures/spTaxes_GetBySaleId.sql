CREATE PROCEDURE [dbo].[spTaxes_GetBySaleId]
	@saleId int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TaxPct
	FROM dbo.Taxes
	WHERE Id = 
		(SELECT TaxId
		FROM dbo.Sales
		WHERE Id = @saleId)
END
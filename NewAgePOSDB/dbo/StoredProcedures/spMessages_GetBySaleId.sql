CREATE PROCEDURE [dbo].[spMessages_GetBySaleId]
	@saleId int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT m.*
	FROM dbo.Sales s
	INNER JOIN dbo.Messages m ON m.SaleId = s.Id
	WHERE s.Id = @saleId;
END
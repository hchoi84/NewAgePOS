CREATE PROCEDURE [dbo].[spSales_GetById]
	@id INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT s.Created, s.Id,
		CONCAT(c.FirstName, ' ', c.LastName) AS FullName,
		c.EmailAddress, c.PhoneNumber
	FROM dbo.Sales s
	INNER JOIN dbo.Customers c ON s.CustomerId = c.Id
	WHERE s.Id = @id;
END
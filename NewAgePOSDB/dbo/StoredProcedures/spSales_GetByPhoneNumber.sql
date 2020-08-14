CREATE PROCEDURE [dbo].[spSales_GetByPhoneNumber]
	@phoneNumber VARCHAR(10)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT s.Created, s.Id,
		CONCAT(c.FirstName, ' ', c.LastName) AS FullName,
		c.EmailAddress, c.PhoneNumber
	FROM dbo.Customers c
	INNER JOIN dbo.Sales s ON c.Id = s.CustomerId
	WHERE PhoneNumber = @phoneNumber;
END
CREATE PROCEDURE [dbo].[spSearchSales]
	@saleId int, 
	@lastName varchar(50), 
	@emailAddress varchar(100), 
	@phoneNumber varchar(10)
AS
BEGIN
	SET NOCOUNT ON;

	IF (@saleId > 0)
		BEGIN
			SELECT s.Id AS SaleId, s.Created, 
						 CONCAT(c.FirstName, ' ', c.LastName) AS FullName, 
						 c.EmailAddress, c.PhoneNumber
			FROM dbo.Sales s
			INNER JOIN dbo.Customers c ON s.CustomerId = c.Id
			WHERE s.Id = @saleId;
		END
	ELSE IF (@lastName <> '')
		BEGIN
			SELECT s.Id AS SaleId, s.Created, 
						 CONCAT(c.FirstName, ' ', c.LastName) AS FullName, 
						 c.EmailAddress, c.PhoneNumber
			FROM dbo.Customers c
			INNER JOIN dbo.Sales s ON c.Id = s.CustomerId
			WHERE c.LastName = @lastName;
		END
	ELSE IF (@emailAddress <> '')
		BEGIN
			SELECT s.Id AS SaleId, s.Created, 
						 CONCAT(c.FirstName, ' ', c.LastName) AS FullName, 
						 c.EmailAddress, c.PhoneNumber
			FROM dbo.Customers c
			INNER JOIN dbo.Sales s ON c.Id = s.CustomerId
			WHERE c.EmailAddress = @emailAddress;
		END
	ELSE IF (@phoneNumber <> '')
		BEGIN
			SELECT s.Id AS SaleId, s.Created, 
						 CONCAT(c.FirstName, ' ', c.LastName) AS FullName, 
						 c.EmailAddress, c.PhoneNumber
			FROM dbo.Customers c
			INNER JOIN dbo.Sales s ON c.Id = s.CustomerId
			WHERE c.PhoneNumber = @phoneNumber;
		END
	ELSE
		BEGIN
			RETURN NULL;
		END
END

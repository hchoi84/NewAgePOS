CREATE PROCEDURE [dbo].[spCustomers_Update]
	@id int,
	@firstName VARCHAR(50),
	@lastName VARCHAR(50),
	@emailAddress VARCHAR(100),
	@phoneNumber VARCHAR(10)
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE dbo.Customers
	SET FirstName = @firstName, LastName = @lastName, EmailAddress = @emailAddress, PhoneNumber = @phoneNumber
	WHERE Id = @id;
END
CREATE PROCEDURE [dbo].[spCustomers_Insert]
	@firstName    VARCHAR (50),
  @lastName     VARCHAR (50),
  @emailAddress VARCHAR (100),
  @phoneNumber  VARCHAR (10)
AS
BEGIN
	SET NOCOUNT ON;

  INSERT INTO dbo.Customers (FirstName, LastName, EmailAddress, PhoneNumber)
  OUTPUT inserted.Id
  VALUES (@firstName, @lastName, @emailAddress, @phoneNumber);
END

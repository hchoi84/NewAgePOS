CREATE PROCEDURE [dbo].[spCustomers_Insert]
	@firstName    VARCHAR (50),
  @lastName     VARCHAR (50),
  @emailAddress VARCHAR (100),
  @phoneNumber  VARCHAR (10)
AS
BEGIN
	SET NOCOUNT ON;

  DECLARE @id int;

  SELECT @id = Id
  FROM dbo.Customers
  WHERE EmailAddress = @emailAddress OR PhoneNumber = @phoneNumber;

  IF (@id IS NULL)
    BEGIN
      INSERT INTO dbo.Customers (FirstName, LastName, EmailAddress, PhoneNumber)
      OUTPUT inserted.Id
      VALUES (@firstName, @lastName, @emailAddress, @phoneNumber);
    END
  ELSE
    BEGIN
      SELECT @id
    END
END

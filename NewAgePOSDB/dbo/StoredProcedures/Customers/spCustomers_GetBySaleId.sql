﻿CREATE PROCEDURE [dbo].[spCustomers_GetBySaleId]
	@saleId int
AS
BEGIN
 SET NOCOUNT ON;

 SELECT *
 FROM dbo.Customers
 WHERE Id = 
	(SELECT CustomerId
	FROM dbo.Sales
	WHERE Id = @saleId);
END

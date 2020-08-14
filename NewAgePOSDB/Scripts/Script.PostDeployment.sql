/*
Post-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.		
 Use SQLCMD syntax to include a file in the post-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the post-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/

IF NOT EXISTS (SELECT 1 FROM dbo.Customers)
BEGIN
  INSERT INTO dbo.Customers(FirstName, LastName,  EmailAddress,         PhoneNumber)
                     VALUES ('Unknown', 'Unknown', 'unknown@email.com', '5555555555');
END

IF NOT EXISTS (SELECT 1 FROM dbo.Products)
BEGIN
  INSERT INTO dbo.Products(Sku, Upc, Cost, Price, AllName)
    VALUES ('ANN0052_001', '8809221911698', 15.00, 27.99, 'Saintnine Misty Golf Balls 2019 Women 1 Dozen Red'),
           ('ANN0195_002', '888167652306', 14.35, 24.99, 'TaylorMade Tour Preferred Golf Gloves 2020 Regular White Fit to Left Hand Medium'),
           ('ANN0584_002', '706843403021', 18.00, 22.99, 'Titleist TruFeel Golf Balls 1 Dozen White');
END

IF NOT EXISTS (SELECT 1 FROM dbo.Taxes)
BEGIN
  INSERT INTO dbo.Taxes(TaxPct) VALUES (0.00);
END
CREATE TABLE [History] ([String] nvarchar(200) NOT NULL  
, [Category] nvarchar(30) NOT NULL  
, [LastModifiedDate] datetime NOT NULL   
);
GO
CREATE INDEX [Category] ON [History] ([Category] Asc);
GO
CREATE INDEX [Date] ON [History] ([LastModifiedDate] Asc);
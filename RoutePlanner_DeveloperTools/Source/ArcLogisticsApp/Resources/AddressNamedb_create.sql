CREATE TABLE [AddressName] ([Name] nvarchar(50) NOT NULL
, [AddressLine] nvarchar(100) NOT NULL
, [Locality1] nvarchar(50) NOT NULL
, [Locality2] nvarchar(50) NOT NULL
, [Locality3] nvarchar(50) NOT NULL
, [CountyPrefecture] nvarchar(50) NOT NULL
, [PostalCode1] nvarchar(50) NOT NULL
, [PostalCode2] nvarchar(50) NOT NULL
, [StateProvince] nvarchar(50) NOT NULL
, [Country] nvarchar(50) NOT NULL
, [Unit] nvarchar(50) NOT NULL
, [FullAddress] nvarchar(250) NOT NULL
, [X] real NOT NULL
, [Y] real NOT NULL
, [MAddressLine] nvarchar(100) NOT NULL
, [MLocality1] nvarchar(50) NOT NULL
, [MLocality2] nvarchar(50) NOT NULL
, [MLocality3] nvarchar(50) NOT NULL
, [MCountyPrefecture] nvarchar(50) NOT NULL
, [MPostalCode1] nvarchar(50) NOT NULL
, [MPostalCode2] nvarchar(50) NOT NULL
, [MStateProvince] nvarchar(50) NOT NULL
, [MCountry] nvarchar(50) NOT NULL
, [MUnit] nvarchar(50) NOT NULL
, [MFullAddress] nvarchar(250) NOT NULL
, [MatchMethod] nvarchar(100) NOT NULL
);
GO
CREATE INDEX [Name] ON [AddressName] ([Name] Asc);
GO
CREATE INDEX [AddressLine] ON [AddressName] ([AddressLine] Asc);
GO
CREATE INDEX [Locality1] ON [AddressName] ([Locality1] Asc);
GO
CREATE INDEX [Locality2] ON [AddressName] ([Locality2] Asc);
GO
CREATE INDEX [Locality3] ON [AddressName] ([Locality3] Asc);
GO
CREATE INDEX [CountyPrefecture] ON [AddressName] ([CountyPrefecture] Asc);
GO
CREATE INDEX [PostalCode1] ON [AddressName] ([PostalCode1] Asc);
GO
CREATE INDEX [PostalCode2] ON [AddressName] ([PostalCode2] Asc);
GO
CREATE INDEX [StateProvince] ON [AddressName] ([StateProvince] Asc);
GO
CREATE INDEX [Country] ON [AddressName] ([Country] Asc);
GO
CREATE INDEX [Unit] ON [AddressName] ([Unit] Asc);
GO
CREATE INDEX [FullAddress] ON [AddressName] ([FullAddress] Asc);
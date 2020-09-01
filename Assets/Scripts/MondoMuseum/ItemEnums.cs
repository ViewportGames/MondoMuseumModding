public enum ItemType{
	Path,
	WallMaterial,
	FloorMaterial,
	ExhibitItem,
	Light,
	WallItem,
	Placeable,
	Fence
}

public enum ItemSubType{
	None,
	Door,
	Window,
	Carpet,
	VisitorRestItem,
	VisitorDrinkItem,
	VisitorFoodItem,
	VisitorToiletItem,
	WallFrame,
	Stanchion,
	ExhibitSign,
	Decorative,
	VisitorGiftShopItem,
	VisitorTicketPurchaseItem
}

[System.Flags]
public enum ItemUISortTag{
	Architecture = (1 << 1),
	Landscaping  = (1 << 2),
	Services = (1 << 3),
	Exhibits = (1 << 4),
	Paint = (1 << 5),
	Windows = (1 << 6),
	Doors = (1 << 7),
	Columns = (1 << 8),
	Lights = (1 << 9),
	Plants = (1 << 10),
	OutdoorPlants = (1 << 11),
	Paths = (1 << 12),
	BenchesEtc = (1 << 13),
	Lobby = (1 << 14),
	GiftShop = (1 << 15),
	Cafe = (1 << 16),
	Restroom = (1 << 17),
	WorkLab = (1 << 18),
	ExhibitHalls = (1 << 19),
	ExhibitItems = (1 << 20),
	FloorPaint = (1 << 21),
	WallPaint = (1 << 22)
	
}

public enum TileFootprint{
	_1x1 = 0,
	_1x2 = 1,
	_1x3 = 2,
	_1x4 = 3,
	_2x2 = 4,
	_2x3 = 5,
	_2x4 = 6,
	_3x3 = 7,
	_3x4 = 8,
	_4x4 = 9
}

public enum TileRestriction{
	Interior,
	Exterior,
	InteriorAndExterior
}
public enum WallRestriction{
	_Straight = 0,
	_2Corner = 1,
	_3Corner = 2,
	_4Corner = 3
}

public enum ExhibitItemRarity{
	Common = 0,
	Rare = 1,
	Unique = 2
}
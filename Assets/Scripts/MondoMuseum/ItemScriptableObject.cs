using UnityEngine;

namespace Viewport.MondoMuseum {

	[CreateAssetMenu(fileName = "New Item Data", menuName = "Mondo Museum Assets/Placeable Item", order = 40)]
	[System.Serializable]
	public class ItemScriptableObject : ScriptableObject {
		[SerializeField] private ItemType _itemType = ItemType.ExhibitItem;
		public ItemType ItemType{
			get { return _itemType; }
			set { _itemType = value; }
		}
		[SerializeField] private ItemSubType _itemSubType;
		public ItemSubType ItemSubType{
			get { return _itemSubType; }
			set { _itemSubType = value; }
		}
		[SerializeField] private ItemUISortTag _itemUISortTags;
		public ItemUISortTag ItemUISortTags{
			get { return _itemUISortTags; }
			set { _itemUISortTags = value; }
		}
		[SerializeField] private string _itemName;
		public string ItemName{
			get { return _itemName; }
			set { _itemName = value; }
		}
		[SerializeField] private string _itemDescription;
		public string ItemDescription{
			get { return _itemDescription; }
			set { _itemDescription = value; }
		}

		[SerializeField] private Sprite _iconSprite;
		public Sprite IconSprite{
			get { return _iconSprite; }
			set { _iconSprite = value; }
		}
		
		public GameObject MainPrefab;
		[Tooltip("For stanchions, this is the rope piece.")]
		public GameObject SecondaryPrefab;
		public TileRestriction PlacementRestriction;
		[Tooltip("Size of the item on the grid, for collision purposes. For exhibit items, this also determines the size of the display case.")]
		
		public TileFootprint PlacementSize;

		[SerializeField] private bool[] _viewingArray;
		public bool[] ViewingArray{
			get { return _viewingArray; }
			set{ _viewingArray = value; }
		}

		[Tooltip("For items that visitors physically interact with, this ensures they move to the precise spot required.")]
		public bool VisitorInteractsWithItem;
		[Tooltip("Visitors will be considered to have reached their destination when they're this far away from an item viewing spot.")]
		public float VisitorStoppingDistance = 0.25f;
		[Tooltip("Visitors will not search for this item if it will require taking a path longer than this distance.")]
		public float MaxVisitorSearchDistance = 5f;
		[Tooltip("The number of visitors allowed to concurrently occupy a single viewing spot.")]
		public int MaxVisitorsAllowedPerSpot = 1;

		/* EXHIBIT ITEM */
		[Tooltip("In format \"Category/Collection\"")]
		public string CollectionId;
		public ExhibitItemRarity Rarity;
		[Tooltip("Dimensions of the exhibit item image.")]
		public Vector2 WallFrameItemDimensions;
		[Tooltip("The space between the exhibit item image and the frame.")]
		public float WallFramePadding = 0.1f;

		/* MATERIAL ITEM */
		public Material[] MaterialVariants;

		/* PATH ITEM */
		public Material Material;

		/* WALL ITEM */
		public WallRestriction WallConstraint;
		public int WindowWallTypeId;
		public int DoorWallTypeId;
		public bool HasDoorFrame;
	}
}
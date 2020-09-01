using UnityEditor;
using UnityEngine;

namespace Viewport.MondoMuseum {

	[CustomEditor(typeof(ItemScriptableObject), true)]
	[CanEditMultipleObjects]
	public class ItemScriptableObjectEditor : Editor{
		
		/* ASSETS */
		SerializedProperty _iconSprite;
		SerializedProperty MainPrefab;

		SerializedProperty SecondaryPrefab;
		
		/* PROPERTIES */
		SerializedProperty PlacementRestriction;
		SerializedProperty PlacementSize;

		SerializedProperty WallFrameItemDimensions;
		SerializedProperty WallFramePadding;

		SerializedProperty VisitorInteractsWithItem;		
		SerializedProperty VisitorStoppingDistance;
		SerializedProperty MaxVisitorSearchDistance;
		SerializedProperty MaxVisitorsAllowedPerSpot;

		/* EXHIBIT ITEM */
		SerializedProperty CollectionId;
		SerializedProperty Rarity;

		/* MATERIAL ITEM */
		SerializedProperty MaterialVariants;

		/* PATH ITEM */
		SerializedProperty Material;

		/* WALL ITEM */
		SerializedProperty WallConstraint;
		string[] _windowWallOptions = new string [3]{"Rounded", "Rectangular", "Full" };
		string[] _doorWallOptions = new string [2]{"Standard", "Full" };
		SerializedProperty HasDoorFrame;
		
		ItemScriptableObject _script;
		int itemHeight = 0;
		int itemWidth = 0;

		int _typeId = 0;
		string[] _typeOptions;
		int _exhibitItemSubTypeId = 0;
		string[] _exhibitItemOptions;
		int _floorMaterialSubTypeId = 0;
		string[] _floorMaterialOptions;
		int _wallItemSubTypeId = 0;
		string[] _wallItemOptions;
		int _placeableItemSubTypeId = 0;
		string[] _placeableItemOptions;

		private Vector2 _itemDescriptionScrollArea;

		private void OnEnable(){
			_script = target as ItemScriptableObject;

			_typeOptions = new string[8]{"Exhibit Item", "Wall Material", "Floor Material", "Path", "Placeable Item", "Placeable Wall Item", "Light", "Fence"};
			_exhibitItemOptions = new string[2]{"Display Case", "Wall Frame"};
			_floorMaterialOptions = new string[2]{"None", "Carpet"};
			_wallItemOptions = new string[3]{"Decorative", "Door", "Window"};
			_placeableItemOptions = new string[9]{"Decorative", "Exhibit Hall Sign", "Stanchion (Rope Barrier)", "Visitor Rest Item", "Visitor Drink Item", "Visitor Food Item", "Visitor Toilet Item", "Visitor Ticket Item", "Visitor Gift Shop Item"};

			_typeId = GetPopupIndexFromItemType(_script.ItemType);
			_exhibitItemSubTypeId = GetExhibitItemIndexFromSubtype(_script.ItemSubType);
			_floorMaterialSubTypeId = GetFloorMaterialIndexFromSubtype(_script.ItemSubType);
			_wallItemSubTypeId = GetWallItemIndexFromSubtype(_script.ItemSubType);
			_placeableItemSubTypeId = GetPlaceableItemIndexFromSubtype(_script.ItemSubType);

			CollectionId = serializedObject.FindProperty("CollectionId");
			Rarity = serializedObject.FindProperty("Rarity");
			
			_iconSprite = serializedObject.FindProperty("_iconSprite");
			MainPrefab = serializedObject.FindProperty("MainPrefab");
			SecondaryPrefab = serializedObject.FindProperty("SecondaryPrefab");
			
			PlacementRestriction = serializedObject.FindProperty("PlacementRestriction");
			PlacementSize = serializedObject.FindProperty("PlacementSize");
			WallFrameItemDimensions = serializedObject.FindProperty("WallFrameItemDimensions");
			WallFramePadding = serializedObject.FindProperty("WallFramePadding");

			VisitorInteractsWithItem = serializedObject.FindProperty("VisitorInteractsWithItem");
			VisitorStoppingDistance = serializedObject.FindProperty("VisitorStoppingDistance");
			MaxVisitorSearchDistance = serializedObject.FindProperty("MaxVisitorSearchDistance");
			MaxVisitorsAllowedPerSpot = serializedObject.FindProperty("MaxVisitorsAllowedPerSpot");

			MaterialVariants = serializedObject.FindProperty("MaterialVariants");

			Material = serializedObject.FindProperty("Material");

			WallConstraint = serializedObject.FindProperty("WallConstraint");
			HasDoorFrame = serializedObject.FindProperty("HasDoorFrame");

			SetItemViewingTileArray();
		}

		public override void OnInspectorGUI(){
			EditorStyles.textField.wordWrap = true;
			serializedObject.Update();
			
			/* ITEM TYPE */
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Item Type");
			EditorGUI.BeginDisabledGroup(true);
            _typeId = EditorGUILayout.Popup(_typeId, _typeOptions);
			_script.ItemType = GetItemTypeFromPopupIndex(_typeId);
			EditorGUI.EndDisabledGroup();
			EditorGUILayout.EndHorizontal();

			/* ITEM SUBTYPE */
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Sub Type");
			if(_script.ItemType == ItemType.ExhibitItem){
				ExhibitItemSubTypeOnGUI();
			}
			else if(_script.ItemType == ItemType.FloorMaterial){
				FloorMaterialSubTypeOnGUI();
			}
			else if(_script.ItemType == ItemType.WallItem){
				WallItemSubTypeOnGUI();
			}
			else if(_script.ItemType == ItemType.Placeable){
				PlaceableItemSubTypeOnGUI();
			}
			else{
				EditorGUI.BeginDisabledGroup(true);
				_script.ItemSubType = ItemSubType.None;
				EditorGUILayout.Popup(0, new string[1]{"None"});
				EditorGUI.EndDisabledGroup();
			}
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.Separator();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("UI Sorting Tags:");
			_script.ItemUISortTags = (ItemUISortTag)EditorGUILayout.EnumFlagsField(_script.ItemUISortTags);
			EditorGUILayout.EndHorizontal();

			/* ITEM NAME */
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Name:");
			_script.ItemName = EditorGUILayout.TextField(_script.ItemName);
			EditorGUILayout.EndHorizontal();

			/* ITEM DESCRIPTION */
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Description:");
			_itemDescriptionScrollArea = EditorGUILayout.BeginScrollView(_itemDescriptionScrollArea, false, false);
			_script.ItemDescription = EditorGUILayout.TextArea(_script.ItemDescription, GUILayout.ExpandHeight(true), GUILayout.MinHeight(100));
			EditorGUILayout.EndScrollView();
			EditorGUILayout.EndHorizontal();

			/* EXHIBIT ITEM INFO */
			if(_script.ItemType == ItemType.ExhibitItem){
				EditorGUILayout.PropertyField(CollectionId);
				EditorGUILayout.PropertyField(Rarity);
			}

			/* GENERAL ASSETS */
			EditorGUILayout.Separator();

			if(_script.ItemType != ItemType.FloorMaterial && _script.ItemType != ItemType.WallMaterial){
				EditorGUILayout.PropertyField(_iconSprite);
				EditorGUILayout.PropertyField(MainPrefab);
				if(_script.ItemSubType == ItemSubType.Stanchion){
					EditorGUILayout.PropertyField(SecondaryPrefab);
				}
				if(_script.ItemType == ItemType.Path){
					EditorGUILayout.PropertyField(Material);
				}

				EditorGUILayout.Separator();

				if(_script.ItemSubType == ItemSubType.WallFrame){
					EditorGUILayout.PropertyField(WallFrameItemDimensions);
					EditorGUILayout.PropertyField(WallFramePadding);
				}

				EditorGUILayout.Separator();

				if(_script.ItemType != ItemType.Path && _script.ItemType != ItemType.WallItem){
					if(_script.ItemType == ItemType.ExhibitItem || _script.ItemSubType == ItemSubType.ExhibitSign || _script.ItemSubType == ItemSubType.Stanchion){
						_script.PlacementRestriction = TileRestriction.Interior;
						EditorGUI.BeginDisabledGroup(true);
					}
					EditorGUILayout.PropertyField(PlacementRestriction);
					EditorGUI.EndDisabledGroup();

					if(_script.ItemSubType == ItemSubType.Stanchion){
						_script.PlacementSize = TileFootprint._1x1;
						EditorGUI.BeginDisabledGroup(true);
					}

					EditorGUILayout.PropertyField(PlacementSize);

					if(_script.ItemSubType == ItemSubType.Stanchion){
						EditorGUI.EndDisabledGroup();
					}

					if(_script.ItemSubType != ItemSubType.Stanchion){
						EditorGUILayout.PrefixLabel("Visitor Viewing Spots");
						GUILayoutOption[] toggleOptions = new GUILayoutOption[]{ GUILayout.MaxWidth(14f), GUILayout.MaxHeight(14f) };
						GUILayoutOption[] spaceOptions = new GUILayoutOption[]{ GUILayout.MaxWidth(12f), GUILayout.MaxHeight(14f) };
						SetItemViewingTileArray();
						for (int y = 0; y < itemHeight; y++) {
							EditorGUILayout.BeginHorizontal(toggleOptions);
							for (int x = 0; x < itemWidth; x++) {
								if((x > 0 && x < itemWidth-1) && (y > 0 && y < itemHeight-1)){
									EditorGUI.BeginDisabledGroup(true);
								}

								_script.ViewingArray[(y * itemWidth) + x] = EditorGUILayout.Toggle(_script.ViewingArray[(y * itemWidth) + x], toggleOptions);
								EditorGUI.EndDisabledGroup();
							}
							EditorGUILayout.EndHorizontal();
						}

						EditorGUILayout.Separator();

						if(_script.ItemType == ItemType.ExhibitItem){
							_script.VisitorInteractsWithItem = false;
							EditorGUI.BeginDisabledGroup(true);
						}
						EditorGUILayout.PropertyField(VisitorInteractsWithItem);
						if(_script.ItemType == ItemType.ExhibitItem){
							EditorGUI.EndDisabledGroup();
						}

						EditorGUILayout.PropertyField(VisitorStoppingDistance);

						if(_script.ItemType == ItemType.ExhibitItem){
							_script.MaxVisitorSearchDistance = 0;
							EditorGUI.BeginDisabledGroup(true);
						}
						EditorGUILayout.PropertyField(MaxVisitorSearchDistance);
						if(_script.ItemType == ItemType.ExhibitItem){
							EditorGUI.EndDisabledGroup();
						}
						
						EditorGUILayout.PropertyField(MaxVisitorsAllowedPerSpot);
					}
					else{
						for(int i = 0; i < _script.ViewingArray.Length; i++){
							_script.ViewingArray[i] = false;
						}
					}
				}
			}

			if(_script.ItemType == ItemType.FloorMaterial || _script.ItemType == ItemType.WallMaterial){
				EditorGUILayout.PropertyField(MaterialVariants, true);
			}

			if(_script.ItemType == ItemType.WallItem){
				if(_script.ItemSubType == ItemSubType.Door || _script.ItemSubType == ItemSubType.Window){
					_script.WallConstraint = WallRestriction._Straight;
					EditorGUI.BeginDisabledGroup(true);
				}
				EditorGUILayout.PropertyField(WallConstraint);
				if(_script.ItemSubType == ItemSubType.Door || _script.ItemSubType == ItemSubType.Window){
					EditorGUI.EndDisabledGroup();
				}

				if(_script.ItemSubType == ItemSubType.Window){
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.PrefixLabel("Window Shape");
					_script.WindowWallTypeId = EditorGUILayout.Popup(_script.WindowWallTypeId, _windowWallOptions);
					EditorGUILayout.EndHorizontal();
				}
				
				if(_script.ItemSubType == ItemSubType.Door){
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.PrefixLabel("Door Shape");
					_script.DoorWallTypeId = EditorGUILayout.Popup(_script.DoorWallTypeId, _doorWallOptions);
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.PropertyField(HasDoorFrame);
				}
				
			}

			EditorGUILayout.Separator();
			EditorGUILayout.Separator();

			CheckForOrphanAssetReferences();
			
			serializedObject.ApplyModifiedProperties();

			if(GUI.changed){
				EditorUtility.SetDirty(_script);
			}
		}

		private void CleanAssetReferences(){
			if(_script.ItemType == ItemType.FloorMaterial || _script.ItemType == ItemType.WallMaterial){
				_script.IconSprite = null;
				_script.MainPrefab = null;
				_script.SecondaryPrefab = null;
			}
			else{
				_script.MaterialVariants = null;
			}

			if(_script.ItemType != ItemType.Path){
				_script.Material = null;
			}
			if(_script.ItemSubType != ItemSubType.Stanchion){
				_script.SecondaryPrefab = null;
			}
		}

		private void CheckForOrphanAssetReferences(){
			var showAssetRefWarning = false;
			if(_script.ItemType == ItemType.FloorMaterial || _script.ItemType == ItemType.WallMaterial){
				if(_script.IconSprite != null || _script.IconSprite != null || _script.SecondaryPrefab != null){
					showAssetRefWarning = true;
				}
			}
			else if(_script.MaterialVariants != null && _script.MaterialVariants.Length > 0){
				showAssetRefWarning = true;
			}

			if(_script.ItemType != ItemType.Path){
				if(_script.Material != null){
					showAssetRefWarning = true;
				}
			}
			if(_script.ItemSubType != ItemSubType.Stanchion){
				if(_script.SecondaryPrefab != null){
					showAssetRefWarning = true;
				}
			}

			if(showAssetRefWarning){
				EditorGUILayout.HelpBox("Switching the Item Type with assets assigned created some uneccesary asset references. These should be cleaned up to prevent unused files from being loaded.", MessageType.Warning);
				if(GUILayout.Button("Clean Asset References")){
					CleanAssetReferences();
				}
			}
		}

		private void ExhibitItemSubTypeOnGUI(){
			_exhibitItemSubTypeId = EditorGUILayout.Popup(_exhibitItemSubTypeId, _exhibitItemOptions);
			_script.ItemSubType = GetExhibitItemSubTypeFromIndex(_exhibitItemSubTypeId);
		}
		private void FloorMaterialSubTypeOnGUI(){
			_floorMaterialSubTypeId = EditorGUILayout.Popup(_floorMaterialSubTypeId, _floorMaterialOptions);
			_script.ItemSubType = GetFloorMaterialSubTypeFromIndex(_floorMaterialSubTypeId);
		}
		private void WallItemSubTypeOnGUI(){
			_wallItemSubTypeId = EditorGUILayout.Popup(_wallItemSubTypeId, _wallItemOptions);
			_script.ItemSubType = GetWallItemSubTypeFromIndex(_wallItemSubTypeId);
		}
		private void PlaceableItemSubTypeOnGUI(){
			_placeableItemSubTypeId = EditorGUILayout.Popup(_placeableItemSubTypeId, _placeableItemOptions);
			_script.ItemSubType = GetPlaceableItemSubTypeFromIndex(_placeableItemSubTypeId);
		}

		private ItemType GetItemTypeFromPopupIndex(int value){
			if(value == 0){
				return ItemType.ExhibitItem;
			}
			else if(value == 1){
				return ItemType.WallMaterial;
			}
			else if(value == 2){
				return ItemType.FloorMaterial;
			}
			else if(value == 3){
				return ItemType.Path;
			}
			else if(value == 4){
				return ItemType.Placeable;
			}
			else if(value == 5){
				return ItemType.WallItem;
			}
			else if(value == 6){
				return ItemType.Light;
			}
			else if(value == 7){
				return ItemType.Fence;
			}
			return ItemType.ExhibitItem;
		}
		private int GetPopupIndexFromItemType(ItemType value){
			if(value == ItemType.ExhibitItem){
				return 0;
			}
			else if(value == ItemType.WallMaterial){
				return 1;
			}
			else if(value == ItemType.FloorMaterial){
				return 2;
			}
			else if(value == ItemType.Path){
				return 3;
			}
			else if(value == ItemType.Placeable){
				return 4;
			}
			else if(value == ItemType.WallItem){
				return 5;
			}
			else if(value == ItemType.Light){
				return 6;
			}
			else if(value == ItemType.Fence){
				return 7;
			}
			return 0;
		}

		private ItemSubType GetExhibitItemSubTypeFromIndex(int value){
			if(value == 1){
				return ItemSubType.WallFrame;
			}
			return ItemSubType.None;
		}
		private int GetExhibitItemIndexFromSubtype(ItemSubType value){
			if(value == ItemSubType.WallFrame){
				return 1;
			}
			return 0;
		}
		private ItemSubType GetFloorMaterialSubTypeFromIndex(int value){
			if(value == 1){
				return ItemSubType.Carpet;
			}
			return ItemSubType.None;
		}
		private int GetFloorMaterialIndexFromSubtype(ItemSubType value){
			if(value == ItemSubType.Carpet){
				return 1;
			}
			return 0;
		}
		private ItemSubType GetWallItemSubTypeFromIndex(int value){
			if(value == 1){
				return ItemSubType.Door;
			}
			else if(value == 2){
				return ItemSubType.Window;
			}
			return ItemSubType.Decorative;
		}
		private int GetWallItemIndexFromSubtype(ItemSubType value){
			if(value == ItemSubType.Door){
				return 1;
			}
			else if(value == ItemSubType.Window){
				return 2;
			}
			return 0;
		}
		private ItemSubType GetPlaceableItemSubTypeFromIndex(int value){
			if(value == 0){
				return ItemSubType.Decorative;
			}
			else if(value == 1){
				return ItemSubType.ExhibitSign;
			}
			else if(value == 2){
				return ItemSubType.Stanchion;
			}
			else if(value == 3){
				return ItemSubType.VisitorRestItem;
			}
			else if(value == 4){
				return ItemSubType.VisitorDrinkItem;
			}
			else if(value == 5){
				return ItemSubType.VisitorFoodItem;
			}
			else if(value == 6){
				return ItemSubType.VisitorToiletItem;
			}
			else if(value == 7){
				return ItemSubType.VisitorTicketPurchaseItem;
			}
			else if(value == 8){
				return ItemSubType.VisitorGiftShopItem;
			}
			return ItemSubType.None;
		}
		private int GetPlaceableItemIndexFromSubtype(ItemSubType value){
			if(value == ItemSubType.Decorative){
				return 0;
			}
			else if(value == ItemSubType.ExhibitSign){
				return 1;
			}
			else if(value == ItemSubType.Stanchion){
				return 2;
			}
			else if(value == ItemSubType.VisitorRestItem){
				return 3;
			}
			else if(value == ItemSubType.VisitorDrinkItem){
				return 4;
			}
			else if(value == ItemSubType.VisitorFoodItem){
				return 5;
			}
			else if(value == ItemSubType.VisitorToiletItem){
				return 6;
			}
			else if(value == ItemSubType.VisitorTicketPurchaseItem){
				return 7;
			}
			else if(value == ItemSubType.VisitorGiftShopItem){
				return 8;
			}
			return 0;
		}

		private void SetItemViewingTileArray(){
			SetArraySizeFromItemSize();
			if(_script.ViewingArray == null || _script.ViewingArray.GetLength(0) != itemHeight * itemWidth){
				_script.ViewingArray = new bool[itemWidth*itemHeight];
				for (int y = 0; y < itemHeight; y++) {
					for (int x = 0; x < itemWidth; x++) {
						if((x > 0 && x < itemWidth-1) && (y > 0 && y < itemHeight-1)){
							//Item
							_script.ViewingArray[(y * itemWidth) + x] = false;
						}
						else{
							//Slots
							_script.ViewingArray[(y * itemWidth) + x] = false;
						}
						
					}
				}
			}
		}

		private void SetArraySizeFromItemSize(){
			if((int)_script.PlacementSize == 0){
				itemWidth = 3;
				itemHeight = 3;
			}
			else if((int)_script.PlacementSize == 1){
				itemWidth = 4;
				itemHeight = 3;
			}
			else if((int)_script.PlacementSize == 2){
				itemWidth = 5;
				itemHeight = 3;
			}
			else if((int)_script.PlacementSize == 3){
				itemWidth = 6;
				itemHeight = 3;
			}
			else if((int)_script.PlacementSize == 4){
				itemWidth = 4;
				itemHeight = 4;
			}
			else if((int)_script.PlacementSize == 5){
				itemWidth = 5;
				itemHeight = 4;
			}
			else if((int)_script.PlacementSize == 6){
				itemWidth = 6;
				itemHeight = 4;
			}
			else if((int)_script.PlacementSize == 7){
				itemWidth = 5;
				itemHeight = 5;
			}
			else if((int)_script.PlacementSize == 8){
				itemWidth = 6;
				itemHeight = 5;
			}
			else if((int)_script.PlacementSize == 9){
				itemWidth = 6;
				itemHeight = 6;
			}
			// else if((int)_script.PlacementSize == 10){
			// 	itemWidth = 3;
			// 	itemHeight = 4;
			// }
		}
		
	}

}

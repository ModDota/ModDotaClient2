﻿package  {
	
	import flash.display.MovieClip;
	import ModDotaLib.Utils.AssetUtils;
	import flash.text.TextField;
	import scaleform.clik.events.ButtonEvent;
	import scaleform.clik.events.ListEvent;
	import scaleform.clik.data.DataProvider;
	import scaleform.clik.controls.ScrollingList;
	import scaleform.clik.interfaces.IDataProvider;
	import flash.geom.Point;

	public class ModRow extends MovieClip {
		
		public var MainText:TextField;
		public var s_row_container:MovieClip;
		public var fancyButton:MovieClip;
		
		public var dropdownMC:ScrollingList;
		public var dropdownDataProvider:IDataProvider;
		
		public function ModRow(mainWidth:int, mainHeight:int, i:int) {
			trace("MODROW_a");
			var oldwidth = MainText.width;
			trace("MODROW_b");
			s_row_container = AssetUtils.ReplaceAsset(s_row_container, "s_row_container");
			trace("MODROW_c");
			width = 21 * s_row_container.width / 32;
			trace("MODROW_d");
			fancyButton = AssetUtils.ReplaceAsset(fancyButton, "chrome_arrow_button_right");
			trace("MODROW_e");
			fancyButton.addEventListener(ButtonEvent.CLICK, fancyButtonClicked);
			trace("MODROW_f");
			
			var menuItems:Array = [];
			menuItems.push({
               "label":"View Mod Info",
               "option":1
            });
			menuItems.push({
               "label":"Remove Mod",
               "option":2
            });
			dropdownDataProvider = new DataProvider(menuItems);
			
			dropdownMC = AssetUtils.CreateAsset("ScrollingListSkinned");
			dropdownMC.dataProvider = dropdownDataProvider;
			dropdownMC.addEventListener(ListEvent.ITEM_PRESS, dropdownClicked);
			dropdownMC.visible = false;
		}
		public function fancyButtonClicked(e:ButtonEvent) {
			trace("##HELPME Hello World");
			var localCoords:Point = globalToLocal(new Point(root.mouseX, root.mouseY));
			addChild(dropdownMC);
			dropdownMC.visible = true;
			dropdownMC.x = localCoords.x;
			dropdownMC.y = localCoords.y;
		}
		public function dropdownClicked(e:ListEvent) {
			trace("##HELPME You clicked "+e.index +" which is... "+e.target.dataProvider[e.index]);
		}
	}
	
}

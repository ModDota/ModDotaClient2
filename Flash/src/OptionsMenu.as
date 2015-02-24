package  {
	
	import flash.display.MovieClip;
	import ModDotaLib.Utils.AssetUtils;
	
	public class OptionsMenu extends MovieClip {
		
		public var bg:MovieClip;
		public var modBG:MovieClip;
		public var mainBG:MovieClip;
		public var mods:Array;
		public var modEntry:ModEntry;
		
		public function OptionsMenu() {
			bg = AssetUtils.CreateAsset("DB_inset");
			addChild(bg);
			bg.x = 0 - (width/50);
			bg.y = 0 - (height/50);
			bg.width = width;
			bg.height = height;
			bg.visible = true;
			trace("Created BG");
			modBG = AssetUtils.CreateAsset("newlayout_inset");
			addChild(modBG);
			modBG.x = 0;
			modBG.y = height / 125;
			modBG.height = 7*(height / 50);
			modBG.width = 4*(width / 50);
			modBG.visible = true;
			trace("Created modBG");
			mainBG = AssetUtils.CreateAsset("newlayout_inset");
			addChild(mainBG);
			mainBG.x = width / 12;
			mainBG.y = height / 125;
			mainBG.height = 7*(height / 50);
			mainBG.width = 7.5*(width / 50);
			mainBG.visible = true;
			trace("Created MainBG");
			mods = new Array();
			var i:int = 0;
			for(i=1; i <= 17; i++) {
				trace("Creating mod "+i);
				var mod:ModRow = new ModRow(width, height, i);
				addChild(mod);
				mod.x = 0;
				mod.y = i*(6*height/125);
				mod.height = 6*height/125;
				mod.width = width/4;
				mod.visible = true;
				mods.push(mod);
			}
			
			//Time for the right half
			//TODO: Initiate this based on left half
			modEntry = new ModEntry();
			addChild(modEntry);
			modEntry.x = width / 5 + width/500;
			modEntry.y = height / 62.5 +  + height/500;
			modEntry.height = 19*(height / 50);
			modEntry.width = 16*(width / 50);
		}
	}
	
}

package  {
	
	import flash.display.MovieClip;
	import ModDotaLib.Utils.AssetUtils;
	
	public class OptionsMenu extends MovieClip {
		
		
		public function OptionsMenu() {
			var bg:MovieClip = AssetUtils.CreateAsset("DB_inset");
			addChild(bg);
			bg.x = 0 - (width/50);
			bg.y = 0 - (height/50);
			bg.width = width;
			bg.height = height;
			bg.visible = true;
			trace("Created BG");
			var modBG:MovieClip = AssetUtils.CreateAsset("newlayout_inset");
			addChild(modBG);
			modBG.x = 0;
			modBG.y = height / 125;
			modBG.height = 7*(height / 50);
			modBG.width = 4*(width / 50);
			modBG.visible = true;
			trace("Created modBG");
			var mainBG:MovieClip = AssetUtils.CreateAsset("newlayout_inset");
			addChild(mainBG);
			mainBG.x = width / 12;
			mainBG.y = height / 125;
			mainBG.height = 7*(height / 50);
			mainBG.width = 7.5*(width / 50);
			mainBG.visible = true;
			trace("Created MainBG");
			var mods:Array = new Array();
			var i:int = 0;
			for(i=0; i < 10; i++) {
				trace("Creating mod "+i);
				var mod:ModRow = new ModRow(width, height, i);
				trace("a" + i);
				addChild(mod);
				mod.x = 0;
				mod.y = i*(6*height/125);
				mod.height = 6*height/125;
				mod.width = width/4;
				trace("e" + i);
				mod.visible = true;
				trace("f" + i);
				mods.push(mod);
			}
		}
	}
	
}

package  {
	
	import flash.display.MovieClip;
	import ValveLib.Globals;
	import ModDotaLib.Utils.AssetUtils;
	import scaleform.clik.events.ButtonEvent;
	
	public class ModEntry extends MovieClip {
		
		public var imagelocation:MovieClip;
		public var downloadButton:MovieClip;
		public var adButton1:MovieClip;
		public var adButton2:MovieClip;
		
		public function ModEntry() {
			trace("####HELPME Loading test.png");
			adButton1 = AssetUtils.ReplaceAsset(adButton1, "AdPickerButton");
			adButton1["AdButtonID"] = 1;
			adButton1.addEventListener(ButtonEvent.CLICK, adButtonClicked);
			adButton2 = AssetUtils.ReplaceAsset(adButton2, "AdPickerButton");
			adButton2["AdButtonID"] = 2;
			adButton2.addEventListener(ButtonEvent.CLICK, adButtonClicked);
			
			downloadButton = AssetUtils.ReplaceAsset(downloadButton, "button_big2");
			downloadButton.label = "Download & Install";
			downloadButton.scaleX = 0.5;
			downloadButton.scaleY = 0.5;
			downloadButton.gotoAndStop(0);
			
			Globals.instance.LoadImageWithCallback("ModDotaClient2/test.png", imageLocation, true, imageLoaded);
		}
		public function imageLoaded() {
			imageLocation.visible = true;
		}
		public function adButtonClicked(e:ButtonEvent) {
			var buttonID = e.target["AdButtonID"];
			adButton1.selected = (buttonID == adButton1["AdButtonID"]);
			adButton2.selected = (buttonID == adButton2["AdButtonID"]);
		}
	}
	
}

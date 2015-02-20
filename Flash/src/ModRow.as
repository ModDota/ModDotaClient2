package  {
	
	import flash.display.MovieClip;
	import ModDotaLib.Utils.AssetUtils;
	import flash.text.TextField;
	public class ModRow extends MovieClip {
		
		public var MainText:TextField;
		public var s_row_container:MovieClip;
		
		public function ModRow(mainWidth:int, mainHeight:int, i:int) {
			s_row_container = AssetUtils.ReplaceAsset(s_row_container, "s_row_container");
		}
	}
	
}

package  {
	
	import flash.display.MovieClip;
	import ValveLib.Globals;
	import ModDotaLib.Utils.AssetUtils;
	import scaleform.clik.events.ButtonEvent;
	import flash.text.TextField;
	import flash.text.TextFormat;
	import flash.geom.Point;
	
	public class ModEntry extends MovieClip {
		
		public var imagelocation:MovieClip;
		public var mainName:TextField;
		public var description:TextField;
		public var downloadButton:MovieClip;
		public var adButton1:MovieClip;
		public var adButton2:MovieClip;
		
		public var descriptionWidth:Number;
		public var descriptionHeight:Number;
		
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
			
			mainName.scaleX = 0.75;
			mainName.width = localToGlobal(new Point(adButton1.x,0)).subtract(localToGlobal(new Point(mainName.x,0))).x;
			Autosize(mainName);
			
			descriptionWidth = description.width;
			descriptionHeight = description.height;
			Autosize(description, true);
			
			Globals.instance.LoadImageWithCallback("ModDotaClient2/test.png", imageLocation, true, imageLoaded);
			
			SetName("Lobby Explorer");
			SetDescription("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec non massa nisi. Fusce eros quam, blandit ut est eu, pellentesque bibendum nibh. Nulla a aliquam massa. Pellentesque eu orci a nunc tincidunt rutrum. Suspendisse rutrum, enim eget sodales lacinia, odio dolor imperdiet metus, sed suscipit metus leo eget ligula. Suspendisse vitae felis elementum, congue purus vitae, cursus dolor. Sed sit amet fringilla turpis, non finibus dolor.\r\n Nam convallis ultricies dui, ac eleifend nisl consectetur in. Sed maximus condimentum elementum. Cras id odio dignissim, euismod dui eget, vulputate nisl. Aliquam erat volutpat. Integer a dolor pretium, porttitor dolor eget, vestibulum dui. Nunc dictum erat vitae erat laoreet ullamcorper. Nunc blandit rutrum turpis, sed vehicula urna malesuada a. Vestibulum lectus tortor, dictum et faucibus non, scelerisque ac velit. Cras bibendum felis vitae semper euismod. Fusce non vulputate libero. Proin a lacus ac erat facilisis condimentum tempus non dui. Suspendisse rutrum sollicitudin pellentesque. Pellentesque ut augue a diam ultrices luctus. Ut eget purus ut ante mattis imperdiet. \r\n Duis vel sodales tellus. Duis sodales libero a lacus elementum ullamcorper. Aenean vestibulum rutrum lacus. Integer est odio, suscipit eu lacus eget, lobortis euismod orci. Duis sed nunc feugiat, rutrum mi eu, condimentum odio. Nulla congue, nulla ac tempus elementum, lorem metus imperdiet lectus, in consectetur orci ipsum vel orci. Etiam accumsan commodo urna id porttitor. Nullam venenatis massa vestibulum felis accumsan, sit amet sodales sapien tristique. Maecenas a sapien ut ex consectetur placerat vitae id ligula. Donec accumsan facilisis dapibus. Suspendisse sit amet nibh et sem mattis hendrerit. Mauris velit libero, convallis sed condimentum a, mollis vitae nisi. Suspendisse aliquam, turpis et cursus lacinia, nibh velit blandit nisl, eget ultrices sapien erat ut nibh. \r\n In dapibus vestibulum neque, sed varius libero semper vitae. Fusce congue accumsan consectetur. Curabitur aliquet leo ac erat iaculis rutrum. Sed tristique velit vel nisi suscipit commodo. Vivamus laoreet sed sapien quis sollicitudin. Etiam ultrices neque nec tellus posuere, vitae dictum est volutpat. Mauris at leo ornare, posuere elit sed, lacinia urna. \r\n Donec tristique dictum nunc eu porttitor. In nec maximus ante. Vivamus tincidunt nec diam viverra faucibus. Vivamus eu mi magna. Proin imperdiet, nulla sit amet rhoncus luctus, felis nunc sagittis dui, non facilisis nunc eros vitae odio. Donec congue luctus purus quis consequat. Cras vitae ligula mauris. Maecenas at faucibus nisi. Vestibulum sed lorem nec diam laoreet dignissim ac et nisl. Donec nec posuere quam.");
		}
		public function imageLoaded() {
			imageLocation.visible = true;
		}
		public function SetName(nameText:String) {
			mainName.text = nameText;
			Autosize(mainName);
		}
		public function SetDescription(descriptionText:String) {
			description.text = descriptionText;
			Autosize(description, true);
		}
		public function adButtonClicked(e:ButtonEvent) {
			var buttonID = e.target["AdButtonID"];
			adButton1.selected = (buttonID == adButton1["AdButtonID"]);
			adButton2.selected = (buttonID == adButton2["AdButtonID"]);
		}
		
		//from http://stackoverflow.com/a/812128 with some tweaks
		function Autosize(txt:TextField, description:Boolean = false):void 
		{
			var maxTextWidth:int;
			var maxTextHeight:int;
			if (description) {
				maxTextWidth = descriptionWidth;
				maxTextHeight = descriptionHeight;
			} else {
				maxTextWidth = txt.width; 
				maxTextHeight = txt.height;
			}
			
			var f:TextFormat = txt.getTextFormat();
			
			//decrease font size until the text fits  
			while (txt.textWidth > maxTextWidth || txt.textHeight > maxTextHeight) {
				f.size = int(f.size) - 1;
				txt.setTextFormat(f);
			}
		}
	}
	
}

package  {
	
	import flash.display.MovieClip;
	import ModDotaLib.Utils.AssetUtils;
	import flash.events.Event;
	import flash.display.Loader;
	import flash.net.URLRequest;
	import flash.utils.Timer;
	import flash.events.TimerEvent;
	import flash.events.MouseEvent;
	
	public class ModDotaClient extends MovieClip {
		//Game API stuff
        public var gameAPI:Object;
        public var globals:Object;
        public var elementName:String;
		
		public var entryButton:MovieClip;
		public var optionsMenu:OptionsMenu;
		
		public function ModDotaClient() {
			// constructor code
		}
		
		public function onLoaded() : void {
			//Do stuff before we need to be last
			this.gameAPI.OnReady();
			//Do stuff when we dont mind a wait
			trace("Hello World!");
			
			LoadFlash("DebugHelper.swf");
			trace("###Loaded DebugHelper");
			LoadFlash("LobbyExplorer.swf");
			trace("###Loaded LobbyExplorer");
			globals.Loader_top_bar.movieClip.gameAPI.DashboardSwitchToSection(7);
			
			
			var miniTimer = new Timer(500,1);
			miniTimer.addEventListener(TimerEvent.TIMER, populateOptions);
			miniTimer.start();
		}
		public function populateOptions(e:TimerEvent) {
			entryButton = AssetUtils.CreateAsset("d_RadioButton_2nd");
			globals.Loader_settings_v2.movieClip.SettingsWindow.addChild(entryButton);
			entryButton.x = globals.Loader_settings_v2.movieClip.SettingsWindow.VideoPanelButton.x+globals.Loader_settings_v2.movieClip.SettingsWindow.VideoPanelButton.width;
			entryButton.y = globals.Loader_settings_v2.movieClip.SettingsWindow.VideoPanelButton.y;
			entryButton.width = globals.Loader_settings_v2.movieClip.SettingsWindow.VideoPanelButton.width;
			entryButton.height = globals.Loader_settings_v2.movieClip.SettingsWindow.VideoPanelButton.height;
			entryButton.visible = true;
			entryButton.label = "MODS";
			
			optionsMenu = new OptionsMenu();
			globals.Loader_settings_v2.movieClip.SettingsWindow.PanelsExpose.addChild(optionsMenu);
			optionsMenu.x = globals.Loader_settings_v2.movieClip.SettingsWindow.PanelsExpose.Game_Main.x;
			optionsMenu.y = globals.Loader_settings_v2.movieClip.SettingsWindow.PanelsExpose.Game_Main.y;
			optionsMenu.width = globals.Loader_settings_v2.movieClip.SettingsWindow.PanelsExpose.Game_Main.width;
			optionsMenu.height = globals.Loader_settings_v2.movieClip.SettingsWindow.PanelsExpose.Game_Main.height;
			optionsMenu.visible = false;
			
			entryButton.addEventListener(MouseEvent.CLICK, openOptions);
			globals.Loader_settings_v2.movieClip.SettingsWindow.HotKeysPanelButton.addEventListener(MouseEvent.CLICK, closeOptions);
			globals.Loader_settings_v2.movieClip.SettingsWindow.GamePanelButton.addEventListener(MouseEvent.CLICK, closeOptions);
			globals.Loader_settings_v2.movieClip.SettingsWindow.VideoPanelButton.addEventListener(MouseEvent.CLICK, closeOptions);
			trace("###CREATED_ENTRY_BUTTON");
		}
		public function openOptions(e:MouseEvent) {
			var settingsMC = globals.Loader_settings_v2.movieClip;
			settingsMC.SettingsWindow.PanelsExpose.HotKeys_Main.visible = false;
         	settingsMC.SettingsWindow.PanelsExpose.Game_Main.visible = false;
         	settingsMC.updateGamePanel();
         	settingsMC.SettingsWindow.PanelsExpose.Video_Main.visible = false;
         	settingsMC.updateVideoPanel();
         	settingsMC.SettingsWindow.PanelsExpose.Audio_Main.visible = false;
         	settingsMC.updateAudioPanel();
			optionsMenu.visible = true;
		}
		public function closeOptions(e:MouseEvent) {
			optionsMenu.visible = false;
		}
		
		public function LoadFlash(fileName:String) {
			var loader:Loader = new Loader();
			loader.load(new URLRequest(fileName));
			
			var loadedFlash:Function = function(e:Event){
				addChild(loader);
				var mc:MovieClip = loader.content as MovieClip;
				mc.globals = globals;
				mc.gameAPI = gameAPI;
				mc.elementName = elementName;
				mc.onLoaded();
				trace("Added "+fileName+" to play_weekend_tourney");
			}
			loader.contentLoaderInfo.addEventListener(Event.COMPLETE, loadedFlash);
		}
	}
	
}

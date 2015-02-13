using System;
using System.Collections.Generic;
using System.IO;

namespace ModDotaHelper
{
    /// <summary>
    /// One of the two big features of ModDotaHelper is the mod manager.
    /// This basically uses Pdub's gameinfo-based override method to set
    /// a new vpk as an override for your existing vpk.
    /// </summary>
    public class ModManager
    {
        string dotapath;
        VPK archive;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dotapath">The path ending in "dota 2 beta"</param>
        /// <param name="archivepath">The archive (VPK) to use for mods</param>
        public ModManager(string dotapath, string archivepath)
        {
            this.dotapath = dotapath;
            if (!Directory.Exists(dotapath + "/moddota"))
            {
                Directory.CreateDirectory(dotapath + "/moddota/");
            }
            archive = new VPK(archivepath);
        }
        /// <summary>
        /// Checks gameinfo.txt (and later gameinfo.gi) to make sure that we are
        /// actually overriding the vpk, and our changes haven't been nuked by a
        /// "verify local game cache" use.
        /// </summary>
        public void CheckGameInfo()
        {
            string contents = "";
            KV.KeyValue gameinfo = null;
            if (File.Exists(dotapath + "/dota/gameinfo.txt"))
            {
                using (FileStream fs = File.OpenRead(dotapath + "/dota/gameinfo.txt"))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        contents = sr.ReadToEnd();
                    }
                }
                gameinfo = KV.KVParser.ParseKeyValue(contents);
            }
            Console.WriteLine(gameinfo.ToString());
            // Make sure our entry is there
            bool found = false;
            if (gameinfo != null && gameinfo["FileSystem"] != null && gameinfo["FileSystem"]["SearchPaths"] != null)
            {
                foreach (KV.KeyValue f in gameinfo["FileSystem"]["SearchPaths"].Children)
                {
                    if (f.Key == "Game" && f.GetString() == "moddota")
                    {
                        found = true;
                        break;
                    }
                }
            }
            if (!found)
            {
                gameinfo = GetFixedGameInfo();
                // Write to the file
                using (FileStream fs = File.Open(dotapath + "/dota/gameinfo.txt", FileMode.Truncate, FileAccess.Write))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        sw.Write(gameinfo.ToString());
                    }
                }
            }
        }
        /// <summary>
        /// Get a gameinfo with our override added.
        /// </summary>
        /// <returns>A KV structure describing the gameinfo.</returns>
        private static KV.KeyValue GetFixedGameInfo()
        {
            // This seemed like a good way to do it at the time... remind me to
            // add a more syntactically-compact way of doing this or just use a
            // string representation instead - this is 3x longer and 3000x more
            // complex/unreadable/error-prone.
            KV.KeyValue gameinfo = new KV.KeyValue("GameInfo");
            KV.KeyValue game = new KV.KeyValue("game");
            game.Set("DOTA 2");
            gameinfo.AddChild(game);
            KV.KeyValue gamelogo = new KV.KeyValue("gamelogo");
            gamelogo.Set(1);
            gameinfo.AddChild(gamelogo);
            KV.KeyValue type = new KV.KeyValue("type");
            type.Set("multiplayer_only");
            gameinfo.AddChild(type);
            KV.KeyValue nomodels = new KV.KeyValue("nomodels");
            nomodels.Set(1);
            gameinfo.AddChild(nomodels);
            KV.KeyValue nohimodel = new KV.KeyValue("nohimodel");
            nohimodel.Set(1);
            gameinfo.AddChild(nohimodel);
            KV.KeyValue nocrosshair = new KV.KeyValue("nocrosshair");
            nocrosshair.Set(0);
            gameinfo.AddChild(nocrosshair);
            KV.KeyValue gamedata = new KV.KeyValue("GameData");
            gamedata.Set("dota.fgd");
            gameinfo.AddChild(gamedata);
            KV.KeyValue supportsdx8 = new KV.KeyValue("SupportsDX8");
            supportsdx8.Set(0);
            gameinfo.AddChild(supportsdx8);

            KV.KeyValue filesystem = new KV.KeyValue("FileSystem");
            KV.KeyValue SteamAppId = new KV.KeyValue("SteamAppId");
            SteamAppId.Set(816);
            filesystem.AddChild(SteamAppId);
            KV.KeyValue ToolsAppId = new KV.KeyValue("ToolsAppId");
            ToolsAppId.Set(211);
            filesystem.AddChild(ToolsAppId);

            KV.KeyValue SearchPaths = new KV.KeyValue("SearchPaths");
            KV.KeyValue game0 = new KV.KeyValue("Game");
            game0.Set("moddota");
            SearchPaths.AddChild(game0);
            KV.KeyValue game1 = new KV.KeyValue("Game");
            game1.Set("|gameinfo_path|.");
            SearchPaths.AddChild(game1);
            KV.KeyValue game2 = new KV.KeyValue("Game");
            game2.Set("platform");
            SearchPaths.AddChild(game2);

            filesystem.AddChild(SearchPaths);

            gameinfo.AddChild(filesystem);

            KV.KeyValue ToolsEnvironment = new KV.KeyValue("ToolsEnvironment");
            KV.KeyValue Engine = new KV.KeyValue("Engine");
            Engine.Set("Souce");
            ToolsEnvironment.AddChild(Engine);
            KV.KeyValue UseVPlatform = new KV.KeyValue("UseVPlatform");
            UseVPlatform.Set(1);
            ToolsEnvironment.AddChild(UseVPlatform);
            KV.KeyValue PythonVersion = new KV.KeyValue("PythonVersion");
            PythonVersion.Set("2.7");
            ToolsEnvironment.AddChild(PythonVersion);
            KV.KeyValue PythonHomeDisable = new KV.KeyValue("PythonHomeDisable");
            PythonVersion.Set(1);
            ToolsEnvironment.AddChild(PythonVersion);

            gameinfo.AddChild(ToolsEnvironment);

            return gameinfo;
        }
        /// <summary>
        /// Validate the installed content against the information from ModDota.
        /// This should fix a variety of issues related to bad downloads.
        /// </summary>
        /// <param name="deepverify">Whether or not we should CRC all mod files to verify them.</param>
        public void ValidateInstalledMods(bool deepverify = false)
        {
            // Make sure that we're in gameinfo
            CheckGameInfo();
            // Load installed mod descriptions
            List<ModSpecification> modspecs = LoadModSpecifications();
            // Check VPK
            archive.Validate(modspecs, deepverify);
        }
        /// <summary>
        /// Load the specifications of all installed mods.
        /// </summary>
        /// <returns>A List of ModSpecifications</returns>
        List<ModSpecification> LoadModSpecifications()
        {
            // Mod specifications are kv files stored in /moddota/mods/
            // the specifications just list filename keys, containing a CRC kv each (and maybe a path?)
            // there's also a single "version" key, which lists the version of the mod (for comparison with md)
            string folderpath = dotapath + "/moddota/mods/";
            if(!Directory.Exists(folderpath))
            {
                Directory.CreateDirectory(folderpath);
            }
            IEnumerable<string> mods = Directory.EnumerateFiles(folderpath, "*.mod");
            List<ModSpecification> modspecs = new List<ModSpecification>();
            foreach (string modfilename in mods)
            {
                Console.WriteLine(modfilename);
                KV.KeyValue thisone = null;
                using (FileStream fs = File.OpenRead(modfilename))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        thisone = KV.KVParser.ParseKeyValue(sr.ReadToEnd());
                    }
                }
                if(thisone == null)
                {
                    // should queue-up re-download?
                    continue;
                }
                ModSpecification ms = new ModSpecification();
                ms.name = thisone.Key;
                ms.version = new Version(-1,-1);
                ms.files = new List<ModResource>();
                foreach(KV.KeyValue k in thisone.Children)
                {
                    if(k.Key == "version")
                    {
                        ms.version = new Version(k.GetString());
                    }
                    if(k.Key == "resource")
                    {
                        ModResource mr = new ModResource();
                        foreach(KV.KeyValue v in k.Children)
                        {
                            switch (k.Key)
                            {
                                case "CRC":
                                    mr.CRC = UInt32.Parse(k.GetString());
                                    break;
                                case "internalpath":
                                    mr.internalpath = k.GetString();
                                    break;
                                case "downloadurl":
                                    mr.downloadurl = k.GetString();
                                    break;
                            }
                        }
                        ms.files.Add(mr);
                    }
                }
            }
            return modspecs;
        }
        /// <summary>
        /// Get the mod list from the local cache.
        /// </summary>
        public void GetModList()
        {

        }
    }
}

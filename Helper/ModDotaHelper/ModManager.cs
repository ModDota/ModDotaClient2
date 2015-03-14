using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

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
        public CryptoChainValidator CCV;
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
            CCV = new CryptoChainValidator();
            Thread workerthread = new Thread(ModWorker);
        }
        /// <summary>
        /// The worker thread for the ModManager.
        /// Basically just does periodic checks for mod updates and the like.
        /// </summary>
        /// <param name="state">Ignored.</param>
        private void ModWorker(object state)
        {
            ModDotaHelper.workersactive.AddCount();
            int counter = 0;
            TimeSpan checkdifference = new TimeSpan(0, 1, 0);
            while (!ModDotaHelper.closedown.WaitOne(checkdifference))
            {
                if (counter % 1 == 0)
                {
                    // Check gameinfo
                    CheckGameInfo();
                }
                if (counter % 5 == 0)
                {
                    // Check mods for updates
                    ValidateInstalledMods(true, false);
                }
                counter++;
            }
            ModDotaHelper.workersactive.Signal();
        }
        /// <summary>
        /// Checks gameinfo.txt (and later gameinfo.gi) to make sure that we are
        /// actually overriding the vpk, and our changes haven't been nuked by a
        /// "verify local game cache" use.
        /// </summary>
        public void CheckGameInfo()
        {
            KV.KeyValue gameinfo = null;
            if (File.Exists(dotapath + "/dota/gameinfo.txt"))
            {
                gameinfo = KV.KVParser.ParseKeyValueFile(dotapath + "/dota/gameinfo.txt");
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
                File.WriteAllText(dotapath + "/dota/gameinfo.txt", gameinfo.ToString());
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
        /// <param name="downloadlists">Whether or not we should check for new mod specifications.</param>
        /// <param name="deepverify">Whether or not we should CRC all mod files to verify them.</param>
        public void ValidateInstalledMods(bool downloadlists = false, bool deepverify = false)
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
        /// <returns>A List of ModSpecifications, one per installed mod.</returns>
        List<ModSpecification> LoadModSpecifications()
        {
            // Mod specifications are kv files stored in /moddota/mods/
            // the specifications just list filename keys, containing a CRC kv each (and maybe a path?)
            // there's also a single "version" key, which lists the version of the mod (for comparison with md)
            string folderpath = dotapath + "/moddota/mods/";
            if (!Directory.Exists(folderpath))
            {
                Directory.CreateDirectory(folderpath);
            }
            IEnumerable<string> mods = Directory.EnumerateFiles(folderpath, "*.mod");
            List<ModSpecification> modspecs = new List<ModSpecification>();
            foreach (string modfilename in mods)
            {
                Console.WriteLine("Parsing data for mod at " + modfilename);
                KV.KeyValue thisone = null;
                try
                {
                    thisone = KV.KVParser.ParseKeyValueFile(modfilename);
                }
                catch (KV.KVParser.KeyValueParsingException)
                {
                    Console.WriteLine("Error while KV parsing " + modfilename);
                }
                if (thisone == null)
                {
                    // should queue-up re-download?
                    continue;
                }
                ModSpecification ms = new ModSpecification(thisone);
                ms.name = thisone.Key;
                ms.version = new Version();
                ms.files = new List<ModResource>();
                foreach (KV.KeyValue k in thisone.Children)
                {
                    if (k.Key == "version")
                    {
                        ms.version = new Version(k.GetString());
                    }
                    if (k.Key == "resource")
                    {
                        ms.files.Add(new ModResource(k));
                    }
                }
            }
            return modspecs;
        }
        /// <summary>
        /// Get the mod list from the local cache.
        /// </summary>
        /// <param name="nodownload">If true, don't download new versions for missing/incorrect/outdated directories.</param>
        /// <param name="forcereacquire">If true, force a new download of all directories.</param>
        public ModDirectory GetModDirectories(bool nodownload = false, bool forcereacquire = false)
        {
            // First get the list of directories we need to look at
            if (!File.Exists(dotapath + "/moddota/directories.kv"))
            {
                return null;
            }
            string contents = null;
            try
            {
                contents = File.ReadAllText(dotapath + "/moddota/directories.kv");
            }
            catch (Exception)
            {
                Console.WriteLine("Couldn't access directories file!");
            }
            KV.KeyValue kv = null;
            try
            {
                kv = KV.KVParser.ParseKeyValue(contents);
            }
            catch (KV.KVParser.KeyValueParsingException)
            {
                Console.WriteLine("poorly formatted kv file for directories, using default");
                kv = new KV.KeyValue("directories");
                KV.KeyValue defaultentry = new KV.KeyValue("moddota");

                KV.KeyValue defaultdirectory = new KV.KeyValue("directory");
                defaultdirectory.Set("https://moddota.com/mdc/directory.kv");
                defaultentry.AddChild(defaultdirectory);

                KV.KeyValue defaultversion = new KV.KeyValue("version");
                defaultversion.Set("https://moddota.com/mdc/directory.version");
                defaultentry.AddChild(defaultversion);

                kv.AddChild(defaultentry);
            }
            ModDirectoryList mdl = new ModDirectoryList(kv);
            ModDirectory basemd = new ModDirectory();
            foreach (Tuple<string, Uri, Uri> tpl in mdl.directories)
            {
                try
                {
                    string directoryname = dotapath + "/moddota/dirs/" + tpl.Item1 + ".dir";
                    bool trieddownload = false;
                    // If we don't have that particular directory, force a re-acquire
                    if (!File.Exists(directoryname) || forcereacquire)
                    {
                        if (nodownload)
                        {
                            // don't have it and can't get it, just go to the next one.
                            continue;
                        }
                        else
                        {
                            TryDownloadModDirectory(tpl.Item1, tpl.Item2);
                            trieddownload = true;
                        }
                    }
                    ModDirectory md = null;
                    // Try to parse the file, since we have one
                    try
                    {
                        md = new ModDirectory(KV.KVParser.ParseKeyValueFile(directoryname), tpl.Item2.Host);
                    }
                    catch (CryptoChainValidator.SignatureException)
                    {
                        // The signature was wrong.
                        if (nodownload)
                        {
                            // Since we can't download a new version, we might as well just give up on this one
                            continue;
                        }
                        else
                        {
                            md = TryDownloadModDirectory(tpl.Item1, tpl.Item2);
                            trieddownload = true;
                        }
                    }
                    catch (KV.KVParser.KeyValueParsingException)
                    {
                        // The format was wrong.
                        if (nodownload)
                        {
                            // Since we can't download a new version, we might as well just give up on this one
                            continue;
                        }
                        else
                        {
                            md = TryDownloadModDirectory(tpl.Item1, tpl.Item2);
                            trieddownload = true;
                        }
                    }
                    // Version check - don't check the version if we aren't going to download a new one anyway, and don't check it if we already just downloaded it.
                    if (!nodownload && !trieddownload)
                    {
                        Version remoteversion;
                        try
                        {
                            using (WebClient client = new WebClient())
                            {
                                remoteversion = new Version(client.DownloadString(tpl.Item3));
                            }
                            if (remoteversion > md.version)
                            {
                                if (nodownload)
                                {
                                    // don't actually skip this time - we just let people play offline with the old version.
                                }
                                else
                                {
                                    ModDirectory newmd = TryDownloadModDirectory(tpl.Item1, tpl.Item2);
                                    if (newmd != null)
                                    {
                                        md = newmd;
                                    }
                                    trieddownload = true;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("Encountered exception while parsing files for " + tpl.Item1 + "'s directory version information");
                        }
                    }
                    basemd.add(md);
                }
                catch (Exception)
                {
                    Console.WriteLine("Failed to acquire or parse the ModDirectory from " + tpl.Item2.ToString());
                }
            }
            return basemd;
        }
        /// <summary>
        /// Try to download a single ModDirectory, and update the stored file if necessary.
        /// </summary>
        /// <param name="name">The name of the ModDirectory, used for debug output and the storage file.</param>
        /// <param name="location">The location from which the ModDirectory can theoretically be downloaded.</param>
        /// <returns></returns>
        private ModDirectory TryDownloadModDirectory(string name, Uri location)
        {
            string directoryname = dotapath + "/moddota/dirs/" + name + ".dir";
            string downloadcontents;
            ModDirectory md;
            try
            {
                using (WebClient client = new WebClient())
                {
                    downloadcontents = client.DownloadString(location);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error downloading the mod directory for " + name + " to be downloaded from " + location.ToString() + ". Exception was: " + e.ToString());
                return null;
            }
            try
            {
                md = new ModDirectory(KV.KVParser.ParseKeyValue(downloadcontents), location.Host);
                // at this point we've already jumped down to one of the two exception handlers below if there's something wrong with the data, so it won't write bad stuff.
                try
                {
                    File.WriteAllText(directoryname, downloadcontents);
                }
                catch (Exception e)
                {
                    // This is actually one from which we can nicely recover.
                    Console.WriteLine("Error writing the moddirectory for " + name + " to disk; using in-memory version. Encountered this exception: " + e.ToString());
                }
                return md;
            }
            catch (CryptoChainValidator.SignatureException)
            {
                // Can't do much about it here
                Console.WriteLine("Mod Directory for " + name + " failed signature check, skipping...");
            }
            catch (KV.KVParser.KeyValueParsingException)
            {
                // Well, can't do anything about it here.
                Console.WriteLine("Error parsing the new download version ModDirectory for " + name + "...");
            }
            return null;
        }
    }
}

using System;
using System.Reflection;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Xml;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

using UnityEngine;

namespace ReikaKalseki.DIDrones {

	public static class DSUtil {
	    
	    public static readonly Assembly diDLL = Assembly.GetExecutingAssembly();
	    public static readonly Assembly gameDLL = Assembly.GetAssembly(typeof(DungeonInfo));
	    public static readonly Assembly gameDLL2 = Assembly.GetAssembly(typeof(MeshContainer));
	    
	    public static readonly string gameDir = Directory.GetParent(gameDLL.Location).Parent.Parent.FullName; //managed -> _Data -> root
		//public static readonly string savesDir = "C:\\Users\\Reika\\AppData\\LocalLow\\MijuGames\\Planet Crafter";

        public static bool allowDIDLL = false;
	    
	    private static bool checkedReikaPC;
	    private static bool savedIsReikaPC;
	    
	    private static readonly HashSet<Assembly> assembliesToSkip = new HashSet<Assembly>(){
            diDLL,
            gameDLL,
            gameDLL2
        };
	    
	    static DSUtil() {
			
	    }
	    
	    private static bool evaluateReikaPC() {
	    	try {
		    	OperatingSystem os = Environment.OSVersion;
		    	if (os.Platform != PlatformID.Win32NT || os.Version.Major != 10)
		    		return false;
		    	if (!System.Security.Principal.WindowsIdentity.GetCurrent().Name.EndsWith("\\Reika", StringComparison.InvariantCultureIgnoreCase)) //windows username
		    		return false;
		    	//if (System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture != System.Runtime.InteropServices.Architecture.X64)
		    	//	return false;
		    	return true;//Steamworks.SteamUser..m_SteamID == 76561198068058411;
	    	}
	    	catch (Exception e) {
	    		log("Error evaluating PC: "+e.ToString(), diDLL);
	    		return false;
	    	}
	    }
	    
	    public static bool isReikaPC() {
	    	if (!checkedReikaPC)
	    		savedIsReikaPC = evaluateReikaPC();
	    	checkedReikaPC = true;
	    	return savedIsReikaPC;
	    }
		
		internal static Assembly tryGetModDLL(bool acceptDI = false) {
	    	try {
				Assembly di = Assembly.GetExecutingAssembly();
				StackFrame[] sf = new StackTrace().GetFrames();
		        if (sf == null || sf.Length == 0)
		        	return Assembly.GetCallingAssembly();
		        foreach (StackFrame f in sf) {
		        	Assembly a = f.GetMethod().DeclaringType.Assembly;
		        	if ((a != di || acceptDI || allowDIDLL) && a != gameDLL && a != gameDLL2 && a.Location.Contains("BepInEx"))
		                return a;
		        }
		        log("Could not find valid mod assembly: "+sf.Select<StackFrame, string>(s => s.GetMethod()+" in "+s.GetMethod().DeclaringType).toDebugString("\n"), diDLL);
	    	}
	    	catch (Exception e) {
	    		log("Failed to find a DLL due to an exception: "+e, diDLL);
	    		return diDLL;
	    	}
	        return Assembly.GetCallingAssembly();
		}
		
		public static void log(string s, Assembly a = null, int indent = 0) {
			while (s.Length > 4096) {
				string part = s.Substring(0, 4096);
				log(part, a);
				s = s.Substring(4096);
			}
			string id = (a != null ? a : tryGetModDLL()).GetName().Name.ToUpperInvariant();
			if (indent > 0) {
				s = s.PadLeft(s.Length+indent, ' ');
			}
			UnityEngine.Debug.Log(id+": "+s);
		}
    
	    public static bool canUseDebug() {
	    	return isReikaPC();
	    }
		
		public static bool checkPiracy() {
			HashSet<string> files = new HashSet<string> {"steam_api64.cdx", "steam_api64.ini", "steam_emu.ini", "valve.ini", "chuj.cdx", "SteamUserID.cfg", "Achievements.bin", "steam_settings", "user_steam_id.txt", "account_name.txt", "ScreamAPI.dll", "ScreamAPI32.dll", "ScreamAPI64.dll", "SmokeAPI.dll", "SmokeAPI32.dll", "SmokeAPI64.dll", "Free Steam Games Pre-installed for PC.url", "Torrent-Igruha.Org.URL", "oalinst.exe"};
            foreach (string file in files) {
                if (File.Exists(Path.Combine(Environment.CurrentDirectory, file)))
                	return true;
            }
			return false;
		}
		
		public static bool match(string s, string seek) {
			return s == seek || (!string.IsNullOrEmpty(s) && !string.IsNullOrEmpty(seek) && (seek[0] == '*' && s.EndsWith(seek.Substring(1), StringComparison.InvariantCulture)) || (seek[seek.Length-1] == '*' && s.StartsWith(seek.Substring(0, seek.Length-1), StringComparison.InvariantCulture)));
		}
		
	}
}

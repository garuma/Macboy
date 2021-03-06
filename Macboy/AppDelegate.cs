using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;

using MonoMac.Foundation;
using MonoMac.AppKit;

using Tomboy;

namespace MacSuperBoy
{
	public partial class AppDelegate : NSApplicationDelegate
	{
		public AppDelegate ()
		{
			// TODO, set it in a generic way
			Tomboy.DiskStorage.Instance.SetPath ("/Users/jeremie/projects/MacBoy/test-notes");
			NoteEngine = new Engine (Tomboy.DiskStorage.Instance);
			Notes = NoteEngine.GetNotes ();

			// Create our cache directory
			if (!Directory.Exists (BaseUrlPath))
				Directory.CreateDirectory (BaseUrlPath);
		}

		public override void FinishedLaunching (NSObject notification)
		{

		}

		public static string BaseUrlPath {
			get {
				return Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal),
				                     "Library", "Cache", "Tomboy");
			}
		}

		public static Tomboy.Engine NoteEngine {
			get;
			set;
		}

		public static Dictionary<string, Note> Notes {
			get;
			set;
		}
	}
}


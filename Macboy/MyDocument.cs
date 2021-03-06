using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using MonoMac.Foundation;
using MonoMac.AppKit;

using Tomboy;

namespace MacSuperBoy
{
	public partial class MyDocument : MonoMac.AppKit.NSDocument
	{
		List<string> history = new List<string> ();
		int currentHistoryPosition;

		// A unique identifier for a note
		string currentNoteID;
		Note currentNote;

		public MyDocument (IntPtr handle) : base (handle)
		{
		}

		[Export ("initWithCoder:")]
		public MyDocument (NSCoder coder) : base (coder)
		{
		}

		public override void WindowControllerDidLoadNib (NSWindowController windowController)
		{
			base.WindowControllerDidLoadNib (windowController);
			UpdateBackForwardSensitivity ();
			noteWebView.FinishedLoad += HandleFinishedLoad;
		}

		void HandleFinishedLoad (object sender, MonoMac.WebKit.WebFrameEventArgs e)
		{
			var dom = e.ForFrame.DomDocument;

			if (!string.IsNullOrEmpty (currentNote.Title)) {
				this.WindowForSheet.Title = currentNote.Title + " — Tomboy";
			} else {
				// Update the title of the current page from HTML
				var es = dom.GetElementsByTagName ("title");
				if (es.Count > 0 && !string.IsNullOrWhiteSpace (es [0].TextContent))
					this.WindowForSheet.Title = es [0].TextContent + " — Tomboy";
			}
		}

		void LoadNote (string newNoteId, bool withHistory = true)
		{
			Console.WriteLine ("Trying to load note {0}", newNoteId);
			var note = AppDelegate.Notes[newNoteId];
			if (note == null)
				return;
			currentNote = note;
			currentNoteID = newNoteId;
			InvalidateRestorableState ();
			noteWebView.MainFrame.LoadHtmlString (note.Text,
			                                      new NSUrl (AppDelegate.BaseUrlPath));
			if (withHistory) {
				if (currentHistoryPosition < history.Count - 1)
					history.RemoveRange (currentHistoryPosition + 1,
					                     history.Count - (currentHistoryPosition + 1));
				history.Add (currentNoteID);
				currentHistoryPosition = history.Count - 1;
			}
			UpdateBackForwardSensitivity ();
		}

		partial void BackForwardAction (MonoMac.AppKit.NSSegmentedControl sender)
		{
			var selected = sender.SelectedSegment;
			if (selected == 0)
				LoadNote (history[--currentHistoryPosition], false);
			else
				LoadNote (history[++currentHistoryPosition], false);
			sender.SetSelected (false, 0);
			sender.SetSelected (false, 1);
			
			UpdateBackForwardSensitivity ();
		}

		void UpdateBackForwardSensitivity ()
		{
			bool canGoBack = history.Count > 0 && currentHistoryPosition > 0;
			bool canGoForward = history.Count > 0 && currentHistoryPosition < history.Count - 1;
			backForwardControl.SetEnabled (canGoBack, 0);
			backForwardControl.SetEnabled (canGoForward, 1);
		}

		partial void StartSearch (MonoMac.AppKit.NSSearchField sender)
		{
			var noteResults = AppDelegate.NoteEngine.GetNotes (sender.StringValue, true);
			NSMenu noteSearchMenu = new NSMenu ("Search Results");
			var action = new MonoMac.ObjCRuntime.Selector ("searchResultSelected");
			foreach (var name in noteResults.Values.Select (n => n.Title))
				noteSearchMenu.AddItem (name, action, string.Empty);
			Console.WriteLine (sender.Frame);
			Console.WriteLine (sender.Superview.Frame);
			Console.WriteLine (sender.Superview.Superview.Frame);
			NSEvent evt = NSEvent.OtherEvent (NSEventType.ApplicationDefined,
			                                  new PointF (sender.Frame.Left, sender.Frame.Top),
			                                  (NSEventModifierMask)0,
			                                  0,
			                                  sender.Window.WindowNumber,
			                                  sender.Window.GraphicsContext,
			                                  (short)NSEventType.ApplicationDefined,
			                                  0, 0);
			NSMenu.PopUpContextMenu (noteSearchMenu, evt, searchField);
		}

		[Export ("searchResultSelected")]
		void SearchResultSelected (NSObject sender)
		{

		}

		partial void ShowNotes (NSObject sender)
		{
			using (NSPopover popover = new NSPopover ()) {
				var controller = new ShowNotesPopupController ();
				controller.NoteNodeClicked += (s, e) => LoadNote (e.NoteId);
				popover.Behavior = NSPopoverBehavior.Transient;
				popover.ContentViewController = controller;
				popover.Show (RectangleF.Empty, sender as NSView, NSRectEdge.MaxYEdge);
			}
		}

		partial void DeleteNote (NSObject sender)
		{
			NSAlert alert = new NSAlert () {
				MessageText = "Really delete this note?",
				InformativeText = "You are about to delete this note, this operation cannot be undone",
				AlertStyle = NSAlertStyle.Warning
			};
			alert.AddButton ("OK");
			alert.AddButton ("Cancel");
			alert.BeginSheet (WindowForSheet,
			                  this,
			                  new MonoMac.ObjCRuntime.Selector ("alertDidEnd:returnCode:contextInfo:"),
			                  IntPtr.Zero);
		}

		[Export ("alertDidEnd:returnCode:contextInfo:")]
		void AlertDidEnd (NSAlert alert, int returnCode, IntPtr contextInfo)
		{
			if (((NSAlertButtonReturn)returnCode) == NSAlertButtonReturn.First) {
				AppDelegate.NoteEngine.DeleteNote (currentNote);
				currentNote = null;
				currentNoteID = null;
				Close ();
			}
		}

		public override void EncodeRestorableState (NSCoder coder)
		{
			base.EncodeRestorableState (coder);
			if (!string.IsNullOrEmpty (currentNoteID))
				coder.Encode (new NSString (currentNoteID), "savedNoteId");
		}

		public override void RestoreState (NSCoder coder)
		{
			base.RestoreState (coder);
			if (!coder.ContainsKey ("savedNoteId"))
				return;
			var id = (NSString)coder.DecodeObject ("savedNoteId");
			if (!string.IsNullOrEmpty (id))
				LoadNote (id);
		}

		public override NSData GetAsData (string documentType, out NSError outError)
		{
			outError = NSError.FromDomain (NSError.OsStatusErrorDomain, -4);
			return null;
		}

		public override bool ReadFromData (NSData data, string typeName, out NSError outError)
		{
			outError = NSError.FromDomain (NSError.OsStatusErrorDomain, -4);
			return false;
		}

		public override string WindowNibName { 
			get {
				return "MyDocument";
			}
		}

	}
}


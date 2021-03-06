// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoMac.Foundation;

namespace MacSuperBoy
{
	[Register ("MyDocument")]
	partial class MyDocument
	{
		[Outlet]
		MonoMac.WebKit.WebView noteWebView { get; set; }

		[Outlet]
		MonoMac.AppKit.NSSegmentedControl backForwardControl { get; set; }

		[Outlet]
		MonoMac.AppKit.NSSearchField searchField { get; set; }

		[Action ("BackForwardAction:")]
		partial void BackForwardAction (MonoMac.AppKit.NSSegmentedControl sender);

		[Action ("StartSearch:")]
		partial void StartSearch (MonoMac.AppKit.NSSearchField sender);

		[Action ("ShowNotes:")]
		partial void ShowNotes (MonoMac.Foundation.NSObject sender);

		[Action ("DeleteNote:")]
		partial void DeleteNote (MonoMac.Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (noteWebView != null) {
				noteWebView.Dispose ();
				noteWebView = null;
			}

			if (backForwardControl != null) {
				backForwardControl.Dispose ();
				backForwardControl = null;
			}

			if (searchField != null) {
				searchField.Dispose ();
				searchField = null;
			}
		}
	}
}

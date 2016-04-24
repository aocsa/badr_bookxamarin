// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;
using UIKit;

namespace BookXamarin
{
	[Register ("BookXamarinViewController")]
	partial class BookXamarinViewController
	{
		[Outlet]
		UIScrollView chapterScroll { get; set; }

		[Outlet]
		UIPageControl pageControl { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (chapterScroll != null) {
				chapterScroll.Dispose ();
				chapterScroll = null;
			}

			if (pageControl != null) {
				pageControl.Dispose ();
				pageControl = null;
			}
		}
	}
}

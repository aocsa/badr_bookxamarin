using System;
using UIKit;
using System.Drawing;
using System.Collections.Generic;
namespace BookXamarin
{
	public class ChapterViewController : UIViewController
	{
		public class ChapterVCDataSource : UIPageViewControllerDataSource
		{
			public override UIViewController GetPreviousViewController (UIPageViewController pageViewController, UIViewController referenceViewController)
			{
				if (pages == null)
					return null;
				var chapterVC = (ChapterViewController)referenceViewController;
				int currentIndex = chapterVC.currentPage.pageInfo.indexInBook;
				Console.WriteLine (currentIndex - 1 + " / " + pages.Count);
				if (currentIndex == 0)
					return null;
				var previousVC = new ChapterViewController (pages [currentIndex - 1]);
				return previousVC;
			}

			public override UIViewController GetNextViewController (UIPageViewController pageViewController, UIViewController referenceViewController)
			{
				if (pages == null)
					return null;
				var chapterVC = (ChapterViewController)referenceViewController;
				int currentIndex = chapterVC.currentPage.pageInfo.indexInBook;
				Console.WriteLine (currentIndex + 1 + " / " + pages.Count);
				if (currentIndex == pages.Count - 1)
					return null;
				var nextVC = new ChapterViewController (pages [currentIndex + 1]);
				return nextVC;
			}
		}

		public PageController currentPage { get; set; }
		public static List<PageController> pages { get; set; }

		public ChapterViewController (PageController currentPage) : base ()
		{
			this.currentPage = currentPage;
			this.View.Frame = Constants.mainFrame;
			UIView largeView = this.currentPage.getLargeView ();
			largeView.Frame = this.View.Frame;
			largeView.Hidden = false;
			this.View.AddSubview (largeView);
		}
	}
}


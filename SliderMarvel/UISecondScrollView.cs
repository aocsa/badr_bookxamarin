using System;
using UIKit;
using System.Drawing;
using System.Collections.Generic;
using CoreGraphics;

namespace BookXamarin
{
	public class UISecondScrollView : UIScrollView
	{
		public class UISecondScrollViewController : UIScrollViewDelegate
		{
			public override void Scrolled (UIScrollView scrollView)
			{
				UISecondScrollView tmp = (UISecondScrollView)scrollView;
				if(!chapterIsScrolling)
				{
					if (pagesAreScrolling)
					{
						tmp.checkLeftOrRightMost (scrollView);
						if (tmp.currentPageScrollIsLeftMost || tmp.currentPageScrollIsRightMost) 
						{
							tmp.secondScrollDelegate.secondScrollDidScroll (tmp);
							alignChapter = true;
						}
						else if (alignChapter) 
						{
							alignChapter = false;
							tmp.secondScrollDelegate.alignChapterScrollView();
						}
						tmp.evaluateChapterBounds (scrollView);
						if(!tmp.Tracking)
						{
							if(tmp.moveToRight || tmp.moveToLeft)
							{
								tmp.secondScrollDelegate.updateChapterScroll(tmp);
							}
						}
					}
				}

			}

			public override void DraggingStarted(UIScrollView scrollView)
			{
				UISecondScrollView tmp = (UISecondScrollView)scrollView;
				chapterIsScrolling = false;
				pagesAreScrolling = true;
				tmp.checkLeftOrRightMost(tmp);
				tmp.secondScrollDelegate.secondScrollWillBeginDragging(tmp);
			}

			public override void DraggingEnded (UIScrollView scrollView, bool willDecelerate)
			{
				UISecondScrollView tmp = (UISecondScrollView)scrollView;
				tmp.secondScrollDelegate.secondScrollDidEndDragging(tmp);
			}

			public override void DecelerationEnded (UIScrollView scrollView)
			{
				UISecondScrollView tmp = (UISecondScrollView)scrollView;
				pagesAreScrolling = false;
				tmp.secondScrollDelegate.secondScrollDidEndDragging(tmp);
			}
				
		}

		public List<PageController> pagesController;
		public UIView view;
		int totalPages;
		ChapterController currentChapterController;
		public int currentChapter;
		public SecondScrollDelegate secondScrollDelegate;
		public UISecondScrollViewController controller;

		public bool moveToLeft;
		public bool moveToRight;
		public bool currentPageScrollIsLeftMost;
		public bool currentPageScrollIsRightMost;
		public static bool alignChapter = false;
		public static bool pagesAreScrolling;
		public static bool chapterIsScrolling;


		public UISecondScrollView (List<PageController> p)
		{
			controller = new UISecondScrollViewController ();

			this.pagesController = p;
			this.totalPages = p.Count;
			this.ClipsToBounds = true;

			this.ExclusiveTouch = true;
			this.MultipleTouchEnabled = false;
			this.ShowsHorizontalScrollIndicator = false;
			this.ShowsVerticalScrollIndicator = false;

			this.currentPageScrollIsLeftMost = true;
			this.currentPageScrollIsRightMost = false;
			pagesAreScrolling = false;
			chapterIsScrolling = false;
			this.moveToLeft = false;
			this.moveToRight = false;
			this.Delegate = controller;

			this.currentChapter = 0;
		}

		public void loadView ()
		{
			this.view = new UIView (new CGRect(0, 0, Constants.SECOND_IMAGE_WIDTH * this.totalPages,  this.Frame.Size.Height));
			this.view.ClipsToBounds = true;
			this.view.MultipleTouchEnabled = false;


			nfloat offset = 0;
			foreach(PageController page in this.pagesController)
			{
				UIView smallView = page.initSmallImageFeatures(this.Frame, offset);
				this.view.AddSubview (smallView);
				offset += smallView.Frame.Size.Width;
			}
			this.ContentSize = new CGSize (this.view.Frame.Size.Width, this.view.Frame.Size.Height);
			this.AddSubview (this.view);
		}

		#region SecondScrollMethods
		public void setOffset(CGPoint point)
		{
			chapterIsScrolling = true;
			this.SetContentOffset (point, false);
		}


		public void moveToChapter(ChapterController chapter)
		{
			this.setCurrentChapterController(chapter);
			CGRect f = this.view.Frame;
			f.X = -chapter.pagesStartX;

			UIView.Animate (
				duration: 0.25, 
				delay: 0, 
				options: UIViewAnimationOptions.CurveEaseOut | UIViewAnimationOptions.BeginFromCurrentState,
				animation: () => {
					this.view.Frame = f;
					this.SetContentOffset (new PointF(0, 0), false);
				},
				completion: () => {
				}
			);
		}

		public void setCurrentChapterController(ChapterController currentChapterController)
		{
			this.currentChapterController = currentChapterController;
			this.currentChapter = currentChapterController.indexInBook;

			CGSize contentSize = this.ContentSize;
			contentSize.Width = this.currentChapterController.pagesFinishX - this.currentChapterController.pagesStartX;
			this.ContentSize = contentSize;

		}

		public void evaluateChapterBounds(UIScrollView scrollView)
		{
			if(scrollView.ContentOffset.X <= -Constants.LIMIT_TO_CHANGE_CHAPTER)
				this.moveToLeft = true;
			else this.moveToLeft = false;

			if(scrollView.ContentOffset.X + this.Frame.Width >= this.ContentSize.Width + Constants.LIMIT_TO_CHANGE_CHAPTER)
				this.moveToRight = true;
			else this.moveToRight = false;
		}

		public void checkLeftOrRightMost(UIScrollView scrollView)
		{
			currentPageScrollIsLeftMost = (scrollView.ContentOffset.X <= 0 && this.currentChapter == 0) ? true : false;

			int lastChapter = (this.pagesController[this.pagesController.Count - 1]).pageInfo.chapter;

			currentPageScrollIsRightMost = (scrollView.ContentOffset.X + 
				scrollView.Frame.Width > this.currentChapterController.pagesFinishX - 
				this.currentChapterController.pagesStartX && this.currentChapter == lastChapter) ? true : false;
		}
			
		#endregion

	}
}
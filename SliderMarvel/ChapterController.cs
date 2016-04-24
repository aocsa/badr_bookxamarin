using System;
using System.Collections;
using System.Collections.Generic;
using UIKit;
using System.Drawing;
using CoreGraphics;

namespace BookXamarin
{
	public class ChapterController
	{
		public List<PageController> pagesController { set; get; }
		public UIImageView chapterImage;
		public nfloat pagesStartX;
		public nfloat pagesFinishX;
		public int indexInBook;
		public int numberOfPages;

		public ChapterController (List<PageController> pc, string imageName,int index)
		{
			this.pagesController = pc;
			foreach (PageController p in pc)
				Console.WriteLine ("number of chapter + " + p.pageInfo.chapter);
			this.chapterImage = new UIImageView (UIImage.FromFile(imageName));
			this.indexInBook = index;
		}

		public UIView addChapterView (CGRect frame, nfloat offset)
		{
			this.chapterImage.Bounds = new CGRect (Constants.MAIN_ORIGIN_X, Constants.MAIN_ORIGIN_Y,
				frame.Size.Width, frame.Size.Height);
			
			this.chapterImage.Frame = new CGRect (offset, Constants.MAIN_ORIGIN_Y, 
				frame.Size.Width, frame.Size.Height);

			return this.chapterImage;
		}

		public void zoomIn()
		{
			UIView.Animate (duration: Constants.CHAPTER_TIME_TO_ZOOM,
				animation: () => {
					chapterImage.Transform = CGAffineTransform.MakeScale(Constants.CHAPTER_ZOOM, Constants.CHAPTER_ZOOM);
				});
		}
		public void zoomOut()
		{
			UIView.Animate (duration: Constants.CHAPTER_TIME_TO_ZOOM,
				animation: () => {
					chapterImage.Transform = CGAffineTransform.MakeIdentity ();
				});
		}
	}
}


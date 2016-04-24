using System;
using System.Drawing;
using Foundation;
using UIKit;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using CoreGraphics;

using UIKit;
using System.Collections.Generic;
using Foundation;
using CoreGraphics;
using CoreAnimation;

using PlistCS;

namespace BookXamarin
{

	public class PageModel  {
		public String portThumbContent;
		public  String portMediumContent;
		public  String portraitLargeContent;
		public  String portLargeTempContent;
		public  String portAnimationFile;
		//public  NSString *landscapeThumbContent;
		//public  NSString *landMediumContent;
		//public  NSString *landscapeLargeContent;
		//public  NSString *landscapeLargeTempContent;
		//public  NSString *landscapeAnimationFile;
		public nuint number;
		public int type;
	}

	public class Chapter  {
		public  int idChapter;
		public  NSString nameChapter;


		public nuint number;
		public  String title;
		public  String videoPath;
		public  String productIdentifier;
		public  String portMediumContent;
		public  String portLargeContent;
		//	@property (nonatomic, getter = isPurchased) BOOL purchased;

		public List<PageModel> pages = new List<PageModel>();

		/*public  NSString coverChapter;
			public  NSString productIdentifier;
			public  NSString price;
			public  NSString descriptionChapter;
			public  NSString state;
			public  bool itWasBought;*/
		public  Book book;

		public void addPage(PageModel ch) {
			pages.Add(ch);
		}
	}


	public class Book  {
		public int idBook;
		public NSString nameBook;

		public int chapterSelected;
		public List<Chapter> chapters = new List<Chapter>();

		public void addChapter(Chapter ch) {
			chapters.Add(ch);
		}

	}


	public partial class BookXamarinViewController : UIViewController, SecondScrollDelegate, GesturesDelegate
	{
		List <ChapterController> chaptersController;
		UISecondScrollView secondScroll;
		int currentPage;
		ChapterController currentChapter;
		bool chapterChanged;
		bool secondScrollIsLeftMost;
		bool secondScrollIsRightMost;
		bool secondScrollIsScrolling;
		bool canBeginGestures;
		bool setOffsetForChange;
		bool isPossibleBigToSmallAnimation;
		UIPageViewController chapterVC;

		public BookXamarinViewController () : base ("BookXamarinViewController", null)
		{
		}

		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();

			// Release any cached data, images, etc that aren't in use.
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			// Perform any additional setup after loading the view, typically from a nib.

			try
			{
				chaptersController = new List<ChapterController> ();

				//loadPropertiesFile ();
				loadPropertiesFileFromPList();


				chapterChanged = false;
				setOffsetForChange = false;
				secondScrollIsScrolling = false;
				canBeginGestures = true;
				isPossibleBigToSmallAnimation = true;

				List<PageController> allPagesController = new List<PageController> ();
				foreach(ChapterController chapter in this.chaptersController)
				{
					foreach(PageController page in chapter.pagesController)
					{
						page.gesturesDelegate = this;
						allPagesController.Add(page);
					}
				}

				this.secondScroll = new UISecondScrollView(allPagesController);

				this.chapterScroll.Frame = new CGRect(Constants.MAIN_ORIGIN_X, Constants.MAIN_ORIGIN_Y, 
					Constants.MAIN_WIDTH, Constants.MAIN_HEIGHT);
				
				this.secondScroll.Frame = new RectangleF(Constants.SECOND_ORIGIN_X, Constants.SECOND_ORIGIN_Y,
					Constants.SECOND_WIDTH, Constants.SECOND_HEIGHT);

				this.pageControl.Pages = this.chaptersController.Count;
				this.currentPage = 0;
				this.pageControl.CurrentPage = this.currentPage;
				this.currentChapter = this.chaptersController[this.currentPage];

				//initial properties for second scroll
				this.secondScroll.currentChapter = this.currentPage;
				this.chapterScroll.WeakDelegate = this;
				this.secondScroll.secondScrollDelegate = this;
			}
			catch(Exception e)
			{
				Console.WriteLine (e.ToString ());
				Console.WriteLine ("file bookInfo.plist could not be opened");
			}
		}

		public override void ViewWillAppear (bool animated)
		{

			this.chapterScroll.Frame = new CGRect(Constants.MAIN_ORIGIN_X, Constants.MAIN_ORIGIN_Y, 
			Constants.MAIN_WIDTH, Constants.MAIN_HEIGHT);

			base.ViewWillAppear (animated);

			nfloat xOffset = 0;
			foreach(ChapterController chapter in this.chaptersController)
			{
				this.chapterScroll.ContentSize = new CGSize (this.chapterScroll.Frame.Size.Width + xOffset,
					this.chapterScroll.Frame.Size.Height);
				this.chapterScroll.AddSubview(chapter.addChapterView(this.chapterScroll.Frame, xOffset));
				xOffset += this.chapterScroll.Frame.Width;
			}
			this.secondScroll.loadView ();
			this.View.AddSubview (this.secondScroll);

			updateChaptersControllerXPositions();
			this.secondScroll.setCurrentChapterController (this.currentChapter);


		}


		#region MyMethods methods




		public void initChapterViewController(PageController page)
		{
			if (chapterVC == null) {
				//initial chapterVC
				ChapterViewController.pages = secondScroll.pagesController;
				chapterVC = new UIPageViewController (UIPageViewControllerTransitionStyle.Scroll, UIPageViewControllerNavigationOrientation.Horizontal, UIPageViewControllerSpineLocation.Min);
				chapterVC.DataSource = new ChapterViewController.ChapterVCDataSource ();
				chapterVC.View.Frame = Constants.mainFrame;
				chapterVC.DidFinishAnimating += (sender, e) => {
					if (e.Finished) {
						if (e.Completed) {
							foreach (ChapterViewController c in e.PreviousViewControllers) {
								this.secondScroll.view.AddSubview (c.currentPage.smallImage);
								c.currentPage.smallImageToPosition (secondScroll, this.currentChapter.pagesStartX);
								var currentVC = (ChapterViewController)chapterVC.ViewControllers [chapterVC.ViewControllers.Length - 1];
								currentVC.currentPage.smallImage.Hidden = true;
								Console.WriteLine("current + " + currentVC.currentPage.pageInfo.chapter + "   prev " + c.currentPage.pageInfo.chapter);
								if (currentVC.currentPage.pageInfo.chapter != c.currentPage.pageInfo.chapter) {
									Console.WriteLine("chapterchanged");
									int chapter = currentVC.currentPage.pageInfo.chapter;
									this.currentPage = chapter;
									this.currentChapter = this.chaptersController [chapter];
									this.drawCurrentChapter ();
								}

								fixSecondScrollPosition(currentVC.currentPage);

							}
						}
					}
				};
			}

			ChapterViewController contentViewController = new ChapterViewController (page);

			var viewControllers = new List<ChapterViewController>();
			viewControllers.Add (contentViewController);
			chapterVC.SetViewControllers(viewControllers.ToArray(), UIPageViewControllerNavigationDirection.Forward, false, null);

			this.AddChildViewController (this.chapterVC);
			chapterVC.DidMoveToParentViewController (this);
			this.View.AddSubview (chapterVC.View);
			this.chapterScroll.UserInteractionEnabled = false;
			this.secondScroll.UserInteractionEnabled = false;
		}

		public void fixSecondScrollPosition (PageController page)
		{
			CGPoint offset = this.secondScroll.ContentOffset;

			if(page.smallImage.Frame.X + Constants.SECOND_IMAGE_WIDTH > this.currentChapter.pagesStartX + this.secondScroll.ContentOffset.X + this.secondScroll.Frame.Width)
			{
				offset.X += (page.smallImage.Frame.X + Constants.SECOND_IMAGE_WIDTH - this.currentChapter.pagesStartX - this.secondScroll.ContentOffset.X - this.secondScroll.Frame.Width);
				this.secondScroll.ContentOffset = offset;
			}
			else if (page.smallImage.Frame.X < offset.X + this.currentChapter.pagesStartX)
			{
				offset.X = page.smallImage.Frame.X - (nfloat)this.currentChapter.pagesStartX;
				this.secondScroll.ContentOffset = offset;
			}

		}
		public void updateChaptersControllerXPositions ()
		{
			ChapterController prev = this.chaptersController[0];

			prev.pagesStartX = 0;
			prev.pagesFinishX = Constants.SECOND_IMAGE_WIDTH * prev.pagesController.Count;

			for(int i = 1; i < this.chaptersController.Count; i++)
			{
				ChapterController tmp = this.chaptersController[i];
				tmp.pagesStartX = prev.pagesFinishX;
				tmp.pagesFinishX = tmp.pagesStartX + Constants.SECOND_IMAGE_WIDTH * tmp.pagesController.Count;
				prev = tmp;
			}
		}

		private void updateSecondScroll()
		{
			nfloat offsetX = this.chapterScroll.ContentOffset.X - this.currentChapter.chapterImage.Frame.Location.X;
			Console.WriteLine ("updateSecondScroll offset " + offsetX +  "   " + currentPage * Constants.MAIN_WIDTH);
			if (offsetX >= 0 && secondScrollIsLeftMost)
				this.secondScroll.setOffset (new Point (0, 0));
			else if (secondScrollIsLeftMost) {
				if (offsetX <= 0) {
					this.secondScroll.setOffset (new CGPoint (offsetX, 0));
				}
			} else if (secondScrollIsRightMost) {
				if (offsetX >= 0) {
					offsetX += this.secondScroll.ContentSize.Width - this.secondScroll.Frame.Width;
					this.secondScroll.setOffset (new CGPoint (offsetX, 0));
					Console.WriteLine ("offset should be close to " + offsetX);
				} else 
				{
					nfloat tmp = this.currentChapter.pagesFinishX - this.currentChapter.pagesStartX - Constants.MAIN_WIDTH;
					Console.WriteLine ("tmp " + tmp);
					this.secondScroll.setOffset (new CGPoint (tmp, 0));
				}
			}
			else return;
		}

		public bool updateCurrentChapter()
		{
			int chapter = (int)Math.Round(this.chapterScroll.ContentOffset.X / this.chapterScroll.Frame.Width);

			if (chapter >= this.chaptersController.Count || chapter < 0)
			{
				chapterChanged = false;
				return chapterChanged;
			}

			if (chapter != this.currentPage)
			{
				this.currentPage = chapter;
				this.currentChapter = this.chaptersController[this.currentPage];
				this.pageControl.CurrentPage = this.currentPage;

				this.secondScroll.currentChapter = this.currentPage;

				secondScrollIsScrolling = false;

				chapterChanged = true;
			}
			else
				chapterChanged = false;

			return chapterChanged;
		}

		public void drawCurrentChapter()
		{
			Console.WriteLine ("drawing");
			CGPoint moveChapterOffset = new CGPoint(this.currentPage * this.chapterScroll.Frame.Width,
				this.chapterScroll.ContentOffset.Y);

			this.chapterScroll.SetContentOffset(moveChapterOffset,true);
			Console.WriteLine (moveChapterOffset);
			this.secondScroll.moveToChapter(this.currentChapter);
			killScroll();

		}

		public void killScroll()
		{
			CGPoint offset = this.secondScroll.ContentOffset;
			this.secondScroll.SetContentOffset(offset, false);
		}

		#endregion

		#region ChapterScrollDelegate methods
		[Export("scrollViewDidScroll:")]
		public void scrollviewDidScroll(UIScrollView scrollView)
		{
			if (!secondScrollIsScrolling)
			{
				if (scrollView.Tracking)
				{
					updateSecondScroll();
				}
				else
				{
					if (updateCurrentChapter())
					{
						setOffsetForChange = true;
						this.secondScroll.moveToChapter(this.currentChapter);
						this.secondScroll.SetContentOffset (new PointF (0, 0), false);
					}
					else
					{
						if (setOffsetForChange)
							this.secondScroll.SetContentOffset (new PointF (0, 0), false);
						else
							updateSecondScroll();
					}
				}
			}

		}

		[Export("scrollViewWillBeginDragging:")]

		public void scrollViewWillBeginDragging(UIScrollView scrollView)
		{
			this.secondScroll.UserInteractionEnabled = false;
			//to avoid problems when trying to get crazy the scroll
			secondScrollIsLeftMost = (this.secondScroll.ContentOffset.X == 0) ? true : false;
			secondScrollIsRightMost = (this.secondScroll.ContentOffset.X == 
				(this.secondScroll.ContentSize.Width - this.secondScroll.Frame.Width) ) ? true : false;
			secondScrollIsScrolling = false;
		}


		[Export ("scrollViewDidEndDragging:willDecelerate:")]
		public void DidEndDragging (UIScrollView scrollView, bool willDecelerate)
		{
			this.secondScroll.UserInteractionEnabled = true;

			if(chapterChanged)
			{
				this.secondScroll.SetContentOffset( new PointF(0, 0), false);
			}    
		}

		[Export ("scrollViewDidEndDecelerating:")]
		public void DidEndDecelerating (UIScrollView scrollView)
		{
			this.secondScroll.UserInteractionEnabled = true;

			//if current chapter changes, the second one should be located at the begining of the current chapter
			if(setOffsetForChange)
			{
				this.secondScroll.SetContentOffset (new PointF(0, 0), false);
				setOffsetForChange = false;
			}

		}

		#endregion

		#region SecondScrollDelegate

		public void secondScrollWillBeginDragging(UISecondScrollView scrollView)
		{
			secondScrollIsScrolling = true;
			this.chapterScroll.UserInteractionEnabled = false;
		}

		public void secondScrollDidScroll(UISecondScrollView scrollView)
		{
			if(scrollView.currentPageScrollIsLeftMost && scrollView.ContentOffset.X < 0)
			{
				CGPoint offset = this.chapterScroll.ContentOffset;
				offset.X = scrollView.ContentOffset.X + this.currentPage * Constants.MAIN_WIDTH;
				this.chapterScroll.ContentOffset = offset;
			}
			else if(scrollView.currentPageScrollIsRightMost && scrollView.ContentOffset.X > 
				scrollView.ContentSize.Width - scrollView.Frame.Width)
			{
				nfloat diff = scrollView.ContentOffset.X - scrollView.ContentSize.Width;
				CGPoint offset = this.chapterScroll.ContentOffset;
				offset.X = (this.currentPage + 1) * Constants.MAIN_WIDTH + diff;
				this.chapterScroll.ContentOffset = offset;
			}
			this.chapterScroll.SetNeedsDisplay ();

		}

		public void secondScrollDidEndDragging(UISecondScrollView scrollView)
		{
			this.chapterScroll.UserInteractionEnabled = true;
			this.secondScroll.evaluateChapterBounds(scrollView);
		}

		public void alignChapterScrollView()
		{
			RectangleF frame = new RectangleF (this.currentPage * Constants.MAIN_WIDTH, Constants.MAIN_ORIGIN_Y, Constants.MAIN_WIDTH, Constants.MAIN_HEIGHT);
			Console.WriteLine (frame);
			this.chapterScroll.ScrollRectToVisible (frame, false);
		}
		public void updateChapterScroll(UISecondScrollView scrollView)
		{
			if(scrollView.moveToLeft)
			{
				if(this.currentPage == 0)
					return;
				else
				{
					this.currentPage--;
					this.pageControl.CurrentPage = this.currentPage;
					this.currentChapter = this.chaptersController[this.currentPage];
					drawCurrentChapter();
					scrollView.moveToLeft = false;
				}
			}
			else if(scrollView.moveToRight)
			{
				if(this.currentPage == this.chaptersController.Count - 1)
					return;
				else
				{
					this.currentPage++;
					this.pageControl.CurrentPage = this.currentPage;
					this.currentChapter = this.chaptersController[this.currentPage];
					drawCurrentChapter();
					scrollView.moveToRight = false;
				}

			}

		}
		#endregion

		#region GesturesDelegate

		public void loadLargeViewWithPage(PageController page)
		{
			UIView largeView = page.getLargeView();
			largeView.Frame = new RectangleF(Constants.MAIN_ORIGIN_X, Constants.MAIN_ORIGIN_Y, 
				Constants.MAIN_WIDTH, Constants.CONTENT_HEIGHT);

			largeView.BackgroundColor = UIColor.Clear;
			
			this.View.AddSubview(largeView);
			//this.View bringSubviewToFront:largeView];
			largeView.Hidden = false;
			page.smallImage.Hidden = true;

			this.View.BackgroundColor = UIColor.Clear;

		}

		public void manageTap(PageController page)
		{
			if(!page.didGrow)
			{
				UIView pageView = page.smallImage;

				CGRect modifiedFrame = pageView.Frame;
				modifiedFrame.X = modifiedFrame.X - this.secondScroll.ContentOffset.X - this.currentChapter.pagesStartX;
				modifiedFrame.Y = Constants.CONTENT_HEIGHT;
				pageView.Frame = modifiedFrame;

				RectangleF newFrame = new RectangleF(Constants.MAIN_ORIGIN_X, Constants.MAIN_ORIGIN_Y, 
					Constants.MAIN_WIDTH, Constants.MAIN_HEIGHT);

				UIView.Animate (
					duration: Constants.ANIMATION_DURATION,
					animation: () => {
						pageView.Frame = newFrame;
					},
					completion: () => {
						this.loadLargeViewWithPage(page);
						initChapterViewController(page);
					}
				);

				page.didGrow = true;
			}

		}

		public void manageRotationInView(UIView view,float angle)
		{

		}
		public void managePanInView(UIView view, UIPanGestureRecognizer panRecognizer)
		{

		}
		public void manageScaleInView(UIView view, float scale, UIPinchGestureRecognizer pinch)
		{

		}

		public bool beginGestures(PageController page)
		{
			if(canBeginGestures)
			{
				currentChapter.zoomIn ();
				this.secondScroll.ScrollEnabled = false;
				this.chapterScroll.ScrollEnabled = false;
				this.chapterScroll.UserInteractionEnabled = false;
				this.secondScroll.UserInteractionEnabled = false;

				//locating the view as sibling of second scroll and chapter scroll
				this.View.AddSubview(page.smallImage);
				CGRect tmp = page.smallImage.Frame;
				tmp.Y = Constants.CONTENT_HEIGHT;

				if(!page.didTap)
					tmp.X -= this.secondScroll.ContentOffset.X;

				page.smallImage.Frame = tmp;
				canBeginGestures = false;
				return true;
			}

			return canBeginGestures;
		}

		public void gesturesFinished(PageController page, string name)
		{

		}
		public void largeGesturesFinished(PageController page, string name)
		{
			Console.WriteLine (name);
			if (isPossibleBigToSmallAnimation) 
			{
				currentChapter.zoomOut ();
				isPossibleBigToSmallAnimation = false;

				if (page.largeView.Frame.Width > Constants.REQUIRED_WIDTH &&
					page.largeView.Frame.Height > Constants.REQUIRED_HEIGHT) {
					var newFrame = page.largeView.Frame;

					newFrame.Width = Constants.MAIN_WIDTH;
					newFrame.Height = Constants.MAIN_HEIGHT;
					newFrame.X = Constants.MAIN_ORIGIN_X;
					newFrame.Y = Constants.MAIN_ORIGIN_Y;


					UIView.Animate (
						duration: Constants.ANIMATION_DURATION,
						animation: () => {
							page.largeView.Transform = CGAffineTransform.MakeIdentity ();
							page.largeView.Frame = newFrame;
							page.largeView.Bounds = newFrame;
							page.didGrow = true;
						},
						completion: () => {
							isPossibleBigToSmallAnimation = true;
							//********//
							initChapterViewController(page);
						}
					);
				} else {
					//se reduce

					Console.WriteLine ("frame " + page.largeView.Frame);
					var tmp = page.initialFrame;
					tmp.X -= this.secondScroll.ContentOffset.X + this.currentChapter.pagesStartX;
					tmp.Y = Constants.CONTENT_HEIGHT;
					Console.WriteLine (tmp.ToString ());
					this.putInPlace (page, tmp);
				}

			}

		}
		public void putInPlace(PageController page, CGRect tmp)
		{
			UIView.Animate (
				duration: Constants.ANIMATION_DURATION,
				animation: () => {
					page.largeView.Transform = CGAffineTransform.MakeIdentity ();
					page.largeView.Frame = tmp;
					page.largeView.Bounds = page.initialBounds;
				},
				completion: () => {

					this.secondScroll.view.AddSubview(page.smallImage);
					page.smallImageToPosition(secondScroll,currentChapter.pagesStartX);

					page.largeView.Hidden = true;
					page.largeView.RemoveFromSuperview();
					secondScroll.UserInteractionEnabled = true;
					secondScroll.ScrollEnabled = true;
					chapterScroll.UserInteractionEnabled = true;
					chapterScroll.ScrollEnabled = true;
					UISecondScrollView.pagesAreScrolling = false;
					canBeginGestures = true;
					secondScrollIsScrolling = false;
					isPossibleBigToSmallAnimation = true;

					//delete pageviewcontroller
					this.chapterVC.View.RemoveFromSuperview();
					this.chapterVC.RemoveFromParentViewController();
				}
			);
		}
		public void beginLargeGestures(PageController page)
		{
		}

		#endregion


		private static NSUrl fileURLOfCachedBooksFile(){
			return NSUrl.CreateFileUrl(NSBundle.MainBundle.PathForResource("itourperuBooks","plist"),false,null);
		}



		public void loadPropertiesFileFromPList () {


 
			NSDictionary _booksDic = NSDictionary.FromUrl( fileURLOfCachedBooksFile());

			//NSDictionary book =  _booksDic;

			NSArray arrayOfBooks = _booksDic.ObjectForKey(new NSString("books") ) as NSArray;

			if (arrayOfBooks.Count > 0) {

				this.createBooksFromDictionary( arrayOfBooks.GetItem<NSDictionary>(0));

			}

		}
		Book book = new Book();

		void  createBooksFromDictionary(NSDictionary dic) {

			foreach (var pair in dic) {
				
				//this.setAttibute(key, dic.ObjectForKey(key), book);
				if (pair.Key.ToString ().Equals ("IdBook"))
					book.idBook = Int32.Parse((pair.Value as NSString).ToString());

				if (pair.Key.ToString ().Equals ("NameBook"))
					book.nameBook = pair.Value as NSString;
			}


			NSArray array = dic.ObjectForKey( new NSString("ChapterList")) as NSArray;

			for (nuint i=0; i < array.Count; i++) {
				NSDictionary chapterDic = array.GetItem<NSDictionary> (i);

				Chapter chapter = new Chapter();
				foreach (var pair in chapterDic) {
					//[self setAttibute:key Value:[chapterDic objectForKey:key] Object:chapter];
					if (pair.Key.ToString().Equals("IdChapter")){
						chapter.idChapter = Int32.Parse((pair.Value as NSString).ToString());


					}
					else if (pair.Key.ToString().Equals("NameChapter")) {
						chapter.nameChapter =  pair.Value as NSString;
					}



				}
				book.addChapter(chapter);
				initWithBookModel (chapter, i);
			}


			int pageNumber = 0;
			int chapterIndex = 0;

			List<PageController> pagesController = new List<PageController> ();
			for (int idChapter = 0; idChapter < book.chapters.Count ; idChapter++)
			{
				Chapter chapter = book.chapters [idChapter];

				for (int page = 0; page < chapter.pages.Count; page++) {
					PageModel pageModel = chapter.pages [page];

					PageInformation pi = new PageInformation ();
					pi.indexInChapter = page;
					pi.indexInBook = pageNumber;
					pi.chapter = chapterIndex;
					Console.WriteLine ("page number " + page);
					PageController pc = new PageController (pi, pageModel.portThumbContent, pageModel.portLargeTempContent, pageModel);
					pagesController.Add (pc);
					pageNumber++;
				}
				ChapterController chc = new ChapterController (pagesController,  book.chapters [idChapter].portLargeContent, chapterIndex);
				chaptersController.Add (chc);
				chapterIndex++;

			}

		}

		static string  BOOKS = "books";
		static string  VIDEO_NAME  = "video.m4v";
		static string  CHAPTER_LIST_DIR = "ChapterList";

		static string   PAGE_PORTRAIT_ANIMATION_FILE = "animation.pex";
		static string   PAGE_PORTRAIT_THUMB_CONTENT  = "thumb.jpg";
		static string   PAGE_PORTRAIT_MEDIUM_CONTENT = "medium.jpg";
		static string   PAGE_PORTRAIT_LARGE_TEMP_CONTENT = "fullpage.jpg";
		static string   PORTRAIT_DIR = "portrait";

		static string   CHAPTER_LARGE_CONTENT  = "large.png";
			static string   CHAPTER_MEDIUM_CONTENT  = "medium.png";





		void initWithBookModel(Chapter chapter, nuint idChapter) {
 

			//NSFileManager fileManager = new NSFileManager();

 
			string comicDir =  ("" + book.idBook);
			 
			string chapterDir =  ( ""+  chapter.idChapter);

			//---------------------------------------------------
			// Chapter portrait Content.plist
			//---------------------------------------------------
			String contentPlistPath = String.Format("{0}/{1}/{2}/{3}/{4}/{5}", BOOKS, comicDir, CHAPTER_LIST_DIR, chapterDir, PORTRAIT_DIR, "Content");

			Console.WriteLine (contentPlistPath);

			string  filePath = NSBundle.MainBundle.PathForResource (contentPlistPath, "plist");

			NSDictionary chapterDictionary = NSDictionary.FromFile (filePath);


			NSArray portPagesArray = chapterDictionary.ObjectForKey(new NSString("Pages")) as NSArray;

			nuint pageCount = 0;

			for (nuint i = 0; i < portPagesArray.Count; i++) // PORTRAIT PAGES
			{
				NSDictionary pageDictionary = portPagesArray.GetItem<NSDictionary> (i);

				NSString pageDir = pageDictionary.ObjectForKey(new NSString("PageContentDir")) as NSString;

				PageModel pm = new PageModel();

				String animationFile = String.Format("{0}/{1}/{2}/{3}/{4}/{5}/{6}",BOOKS, comicDir, CHAPTER_LIST_DIR, chapterDir, PORTRAIT_DIR, pageDir, PAGE_PORTRAIT_ANIMATION_FILE);
				//pm.portAnimationFile = [fileManager fileExistsAtPath:animationFile] ? animationFile : nil;
				pm.portThumbContent = String.Format("{0}/{1}/{2}/{3}/{4}/{5}/{6}", BOOKS, comicDir, CHAPTER_LIST_DIR, chapterDir, PORTRAIT_DIR, pageDir, PAGE_PORTRAIT_THUMB_CONTENT) ;
				pm.portMediumContent= String.Format("{0}/{1}/{2}/{3}/{4}/{5}/{6}", BOOKS, comicDir, CHAPTER_LIST_DIR, chapterDir, PORTRAIT_DIR, pageDir, PAGE_PORTRAIT_MEDIUM_CONTENT) ;
				pm.portraitLargeContent = String.Format("{0}/{1}/{2}/{3}/{4}/{5}", BOOKS, comicDir, CHAPTER_LIST_DIR, chapterDir, PORTRAIT_DIR, pageDir) ;
				pm.portLargeTempContent = String.Format("{0}/{1}/{2}/{3}/{4}/{5}/{6}", BOOKS, comicDir, CHAPTER_LIST_DIR, chapterDir, PORTRAIT_DIR, pageDir, PAGE_PORTRAIT_LARGE_TEMP_CONTENT) ;
				pm.number = pageCount++;
				//pm.type = type;
				//type = PAGE_TYPE_NORMAL;

				chapter.addPage(pm);
			}			
			chapter.number = idChapter;
			//chapter.title  = [chapterDictionary objectForKey:@"VolumeName"];
			chapter.videoPath = String.Format("{0}/{1}/{2}/{3}/{4}/{5}", BOOKS, comicDir, CHAPTER_LIST_DIR, chapterDir, PORTRAIT_DIR, VIDEO_NAME);
			chapter.portMediumContent= String.Format("{0}/{1}/{2}/{3}/{4}/{5}", BOOKS, comicDir, CHAPTER_LIST_DIR, chapterDir, PORTRAIT_DIR, CHAPTER_MEDIUM_CONTENT);
			chapter.portLargeContent = String.Format("{0}/{1}/{2}/{3}/{4}/{5}", BOOKS, comicDir, CHAPTER_LIST_DIR, chapterDir, PORTRAIT_DIR, CHAPTER_LARGE_CONTENT);
			//chapter.productIdentifier = chapter.productIdentifier;
			//chapter.purchased = chapter.itWasBought;



		}

		public void loadPropertiesFile()
		{
			string documentsPath = "load.txt";

			Dictionary<string, object> dict = (Dictionary<string, object>)Plist.readPlist (documentsPath);

			foreach (var content in dict)
			{
				List<object> root = (List <object> )content.Value;
				int pageNumber = 0;
				int chapterIndex = 0;

				foreach (var arrayElement in root) 
				{
					Dictionary <string,object> bookContent = (Dictionary <string, object>)arrayElement;
					string imagesPath = "Images/";
					string chapterImageName = "";
					foreach (var chapterContent in bookContent)
					{
						if(chapterContent.Key.Equals("ChapterImage"))
						{
							chapterImageName += imagesPath + (string)chapterContent.Value; 
						}
						else if (chapterContent.Key.Equals("Pages"))
						{
							List<object> pName = (List<object>)chapterContent.Value;
							List<string> pagesNames = new List<string> ();
							foreach (var page in pName) 
							{
								pagesNames.Add(imagesPath + (string)page);
							}
							pagesNames.Insert(0,chapterImageName);

							List<PageController> pagesController = new List<PageController> ();
							for (int page = 0; page < pagesNames.Count ; page++)
							{
								PageInformation pi = new PageInformation();
								pi.indexInChapter = page;
								pi.indexInBook = pageNumber;
								pi.chapter = chapterIndex;
								Console.WriteLine ("page number " + page);
								PageController pc = new PageController (pi, pagesNames[page], pagesNames[page], null);
								pagesController.Add (pc);
								pageNumber++;
							}

							ChapterController chapter = new ChapterController (pagesController, chapterImageName, chapterIndex);
							chaptersController.Add (chapter);
							chapterIndex++;
						}
					}

				}
			}

		}
	}
}


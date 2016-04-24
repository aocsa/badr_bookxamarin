using System;
using UIKit;
using System.Drawing;
using CoreGraphics;
using System.Collections.Generic;
using Foundation;
using CoreAnimation;

namespace BookXamarin
{
	public class PageContentView  : UIView {


		public class Pair   
		{
			public UIView  view;
			public NSArray glosary;
			public nint currentKey;

		}

		public PageContentView (CGRect rect, PageModel model )
			: base(rect)
		{

			this.pagePath = model.portraitLargeContent;

		} 

		Dictionary<nint, List<Pair> > myDictionary;
		List<UIImageView> myItems;
		int currentItem;
		int currentItemText;
		float nextTextTime;
		List<object> _timerColecction;
		List<object> _timerTextColecction;

		public string            pagePath;
		bool              _landViewCreated;
		bool              _highlighted;


		nuint animationLayerIndex;
		bool isLandscape;

		public CGRect CGRectFromString(String frame)
		{

			CGRect rect= new CGRect();
			frame = frame.Replace (" ", "");

			frame = frame.Replace ("{{", "");
			frame = frame.Replace ("}}", "");
			frame = frame.Replace ("},{", " ");
			frame = frame.Replace (",", " ");

			var myList = new List<string>(frame.Split(' '));
			rect.X = (int)Double.Parse(myList[0]); 
			rect.Y = (int)Double.Parse(myList[1]); 
			rect.Width = (int)Double.Parse(myList[2]); 
			rect.Height = (int)Double.Parse(myList[3]); 

			return rect;
		}



		public override void Draw(CGRect mainrect)
		{
			myDictionary = new Dictionary<nint, List<Pair> >(); 
			myItems = new List<UIImageView>(); 
			nint key = 0;
			List<Pair> array = null;
			currentItem = 0;
			currentItemText = 0;
			nextTextTime = 1.25f; 

			string filePath = pagePath + "/index" ;

			filePath = NSBundle.MainBundle.PathForResource (filePath,"plist");



			NSArray contentsArray = NSArray.FromFile (filePath);
			nint itemCount = 0;
			for (nuint i = 0; i < contentsArray.Count; i++) {
				NSDictionary item = contentsArray.GetItem<NSDictionary>(i);
				NSString animation = item.ValueForKey(new NSString("animationLayer" )) as NSString;
				if (animation != null) {
					this.animationLayerIndex = (nuint)itemCount + 2;
				}
				NSString frame  = item.ValueForKey(new NSString ( "frame" ) ) as NSString;
				NSString file =  item.ValueForKey (new NSString ( "image") ) as NSString;
				NSString file2 = new NSString(pagePath.ToString() + "/" + file.ToString());

				CGRect rect = CGRectFromString( frame ); 

				NSString hfile = new NSString( file.ToString ().Replace (".png", "-old.png") );
				hfile =   new NSString(pagePath  + "/" + hfile);

				UIImageView subview = new UIImageView (rect);
				subview.HighlightedImage = new UIImage(file2);  
				//subview.Image = new UIImage(hfile);    
				subview.Highlighted = _highlighted;
				subview.UserInteractionEnabled = true;
				subview.MultipleTouchEnabled = true;
				subview.Tag = itemCount;
				this.InsertSubview (subview, itemCount); 
				itemCount++;


				if (array != null) {
					myDictionary.Add (key, array);
				}
				array = new List<Pair>();
				key =  subview.Tag;
				myItems.Add(subview);

				NSArray childrenArray = item.ObjectForKey (new NSString("children")) as NSArray; 

				if (childrenArray != null) {
					for (nuint j = 0; j < childrenArray.Count; j++) {
						NSDictionary child = childrenArray.GetItem<NSDictionary> (j);

						frame = child.ObjectForKey (new NSString ("frame")) as NSString;
						file = child.ObjectForKey (new NSString ("image")) as NSString;

						file = new NSString (pagePath + "/" + file);

						rect = CGRectFromString (frame); 

						subview = new UIImageView (rect);

						subview.Image = new UIImage (file);
						subview.UserInteractionEnabled = true;
						subview.MultipleTouchEnabled = true;
						this.AddSubview (subview);

						subview.Transform = CGAffineTransform.MakeScale (1.00f, 1.00f);

						subview.Alpha = 0.0f;

						UITapGestureRecognizer tap = new UITapGestureRecognizer (t => {
							handleTapGestureText (t);
						});

						subview.AddGestureRecognizer (tap);

						Pair pair = new Pair ();
						pair.view = subview;
						pair.glosary = child.ObjectForKey (new NSString ("glosary")) as NSArray;
						pair.currentKey = 0;

						array.Add (pair);
					}
				}

			}
			if (array != null) {
				myDictionary.Add(key, array );
			}

			NSString sepiaFile = new NSString(pagePath  +  "/medium.jpg");
			UIImageView sepiaImage = new UIImageView ( new CGRect(0, 0, 1024, 646));
			sepiaImage.Image =  new UIImage(sepiaFile);  

			this.InsertSubview (sepiaImage, 0); 

			UITapGestureRecognizer gestureRe =  new UITapGestureRecognizer( t => {
				handleTapGesture(t);
			});
			this.AddGestureRecognizer(gestureRe);  


			base.Draw (mainrect);
			// Perform any additional setup after loading the view, typically from a nib.
		}
		void handleTapGestureText (UITapGestureRecognizer tapRecognizer) {

			UIImageView  currentText = tapRecognizer.View as UIImageView;


			int selectedItem = 0; 
			int selectedText = 0; 

			for( int i = 0; i < myItems.Count; i++) {
				UIImageView  image =  myItems[i];
				List<Pair>  currItems = myDictionary [image.Tag];

				for (int j = 0; j < currItems.Count;  j++) {

					Pair cpair =  currItems[j];
					UIView  item =  cpair.view;

					if ( currentText == item )  {
						selectedText = j;
						selectedItem = i;
						continue;
					}
					item.Transform = CGAffineTransform.MakeScale(1.00f, 1.00f);
				}
			}
			if( myItems.Count  == 0)
				return ;

			UIImageView img =  myItems[selectedItem];
			nint key = img.Tag;
			List<Pair> items = myDictionary [ key];
			if( items.Count == 0)
				return ;
			Pair pair = items[selectedText];
			NSArray glosary = pair.glosary;

			if ( glosary.Count == 0){
				return;
			}
			if(pair.currentKey == (nint)glosary.Count){
				pair.currentKey = 0;
				//[self dismissAllPopTipViews];
				return;
			}



		}
		void handleTapGesture (object sender) {
			//[self dismissAllPopTipViews];

			nextTextTime = 1.25f;

			// stop animation
			//this.stopAnimation();

			if (currentItem == myDictionary.Count ) {
				currentItemText = 0;
				return;
			}

			UIImageView  img =  myItems[currentItem];

			this.showItem(img); 
			currentItem++;

		}

		double  TEXT_TIME =  0.000075f;


		double showItem (UIImageView img){

			double Alltime = 0.0;
			CATransition transition = new CATransition ();

			transition.Duration = 0.5f;

			transition.TimingFunction = CAMediaTimingFunction.FromName(
				CAMediaTimingFunction.EaseInEaseOut);
			transition.Type = CATransition.TransitionFade;

			img.Highlighted = false;
			img.Layer.AddAnimation(transition, null);
			img.Highlighted = true;             
			img.Alpha = 1.0f;


			nint key =  img.Tag;
			List<Pair> items = myDictionary[key];

			if (items.Count  > 0) {
				float localTime =  0.25f;

				//NSNumber * value2 = [[NSNumber alloc] initWithInt:0 ];
				//NSNumber * value3 = [[NSNumber alloc] initWithFloat:localTime ];

				NSTimer timer = NSTimer.CreateScheduledTimer (localTime, (t) => {
					showTextItem(t, items, 0);
				});
				//NSTimer timer =  [NSTimer scheduledTimerWithTimeInterval:[value3 floatValue] target:self selector:@selector(showTextItem:) userInfo:[NSDictionary dictionaryWithObjectsAndKeys:items, @"value1", value2, @"value2",  value3, @"value3", nil]  repeats:NO];
				//[_timerTextColecction addObject:timer];
			}

			for (int i = 0; i < items.Count; i++) {
				Pair pair = items[i];
				UIView  item =  pair.view;

				Alltime  =  (item.Frame.Size.Width * item.Frame.Size.Width ) *  TEXT_TIME + 0.70;
			}
			return Alltime;

		}



		void showTextItem (NSTimer theTimer,List<Pair> array, int index) {
			Pair  pair ;
			UIView item;
			if( index  > 0) {
				pair = array[index- 1];
				item =  pair.view;

				UIView.BeginAnimations("CLOSEFULLSCREEN");
				UIView.SetAnimationDuration(0.35f);
				UIView.SetAnimationTransition(UIViewAnimationTransition.None, item, true);

				item.Transform = CGAffineTransform.MakeScale(1.00f, 1.00f);

				UIView.SetAnimationDelegate(this);    
				UIView.CommitAnimations();
			}

			if ( index  == array.Count ) {

				return;
			}
			pair = array[index ];
			item = pair.view;


			float time = 0.35f;

			UIView.BeginAnimations("CLOSEFULLSCREEN");
			UIView.SetAnimationDuration(0.35f);
			UIView.SetAnimationTransition(UIViewAnimationTransition.None, item, true);


			item.Alpha = 1.0f;
			item.Transform = CGAffineTransform.MakeScale(1.15f, 1.15f);

			UIView.SetAnimationDelegate(this);    
			UIView.CommitAnimations();

			double Alltime = 0.0f;

			Alltime  =  (item.Frame.Size.Width * item.Frame.Size.Height ) *  TEXT_TIME;


			NSTimer timer = NSTimer.CreateScheduledTimer (Alltime + theTimer.TimeInterval, (t) => {
				showTextItem(t, array, index + 1);
			});

		} 



	} 


	public class PageController : UIView
	{
	
		public string smallImageName { set; get; }
		public string largeImageName { set; get; }
		public UIImageView smallImage { set; get; }
		public UIImageView largeImage { set; get; }
		public UIView largeView { set; get; }
		public PageInformation pageInfo { set; get; }
		public GesturesDelegate gesturesDelegate;
		public bool didTap;
		public bool didGrow;
		public CGRect initialBounds;
		public CGRect initialFrame;

		public PageController (PageInformation pi, string sIName, string lIName, PageModel model)
		{
			this.pageInfo = pi;
			this.smallImageName = sIName;
			this.largeImageName = lIName;
			this.largeView = new PageContentView(new CGRect(Constants.MAIN_ORIGIN_X, Constants.MAIN_ORIGIN_Y, 
				Constants.MAIN_WIDTH, Constants.CONTENT_HEIGHT), model);
			
			this.largeImage = new UIImageView(UIImage.FromFile(this.largeImageName));
			this.smallImage = new UIImageView (UIImage.FromFile (this.smallImageName));
			this.largeImage.AutoresizingMask = UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleWidth;

		}


		public UIView getLargeView()
		{
			this.largeView.UserInteractionEnabled = true;
			this.largeView.MultipleTouchEnabled = true;
			this.largeView.ClipsToBounds = true;
			this.largeView.Frame = new CGRect (Constants.MAIN_ORIGIN_X, Constants.MAIN_ORIGIN_Y, 
				Constants.MAIN_WIDTH, Constants.CONTENT_HEIGHT);
			//adding largeImage

			this.largeImage.BackgroundColor = UIColor.Red;

			this.largeImage.Frame = this.largeView.Frame;
			//this.largeImage.UserInteractionEnabled = true;
			//this.largeImage.MultipleTouchEnabled = true;

			var panGesture = new UIPanGestureRecognizer((g) =>
				{
					if(g.State == UIGestureRecognizerState.Changed && g.NumberOfTouches == 2)
					{
						var t = g.TranslationInView(largeView);
						var imageViewPosition = largeView.Center;
						imageViewPosition.X += t.X;
						imageViewPosition.Y += t.Y;
						largeView.Center = imageViewPosition;
						g.SetTranslation(new PointF(),largeView.Superview);
					}
					if(g.State == UIGestureRecognizerState.Cancelled || g.State == UIGestureRecognizerState.Ended)
					{
						largeTouchesEnd("pan");
					}
				});
			panGesture.MinimumNumberOfTouches = 2;
			panGesture.MaximumNumberOfTouches = 2;
			var pinchGesture = new UIPinchGestureRecognizer((g) =>
				{
					if(g.State == UIGestureRecognizerState.Began)
					{
						largeView.Transform = CGAffineTransform.MakeIdentity();
						g.Scale = 1.0f;
					}
					if(g.State == UIGestureRecognizerState.Cancelled || g.State == UIGestureRecognizerState.Ended)
					{
						largeTouchesEnd("scale");
					}
					//_scale = g.Scale;
					if(g.State == UIGestureRecognizerState.Changed)
					{
						var midPoint = g.LocationInView(largeView.Superview);
						CGAffineTransform s = largeView.Transform;
						s.Scale(g.Scale, g.Scale);
						largeView.Center = midPoint;
						largeView.Transform = s;
						//Console.WriteLine("affine +     " + s);
						g.Scale = 1.0f;
					}
				});
			var rotateGesture = new UIRotationGestureRecognizer((g) =>
				{
					//_angle = g.Rotation;
					if(g.State == UIGestureRecognizerState.Changed)
					{
						CGAffineTransform r = largeView.Transform;
						var midPoint = g.LocationInView(largeView.Superview);
						largeView.Center = midPoint;
						r.Rotate(g.Rotation);
						largeView.Transform = r; //.Rotate(_angle);// = MakeTransform();
						g.Rotation = 0.0f;
					}
					if(g.State == UIGestureRecognizerState.Cancelled || g.State == UIGestureRecognizerState.Ended)
					{
						largeTouchesEnd("rotate");
					}
				});

			panGesture.ShouldRecognizeSimultaneously = (gesture1, gesture2) => true;
			pinchGesture.ShouldRecognizeSimultaneously = (gesture1, gesture2) => true;
			rotateGesture.ShouldRecognizeSimultaneously = (gesture1, gesture2) => true;

			largeView.AddGestureRecognizer(panGesture);
			largeView.AddGestureRecognizer(pinchGesture);
			largeView.AddGestureRecognizer(rotateGesture);

			//largeview will manage the view when it belongs to the pageviewcontroller
			// @todo 
			//this.largeView.AddSubview(this.largeImage);

			return this.largeView;
		}


		public void smallImageToPosition (UIScrollView UIScrollView, nfloat offset)
		{
			smallImage.Bounds = initialBounds;
			var tmp = initialFrame;

			tmp.Y = 0;
			smallImage.Frame = tmp;
			didGrow = false;
			smallImage.Hidden = false;
		}

		public UIView initSmallImageFeatures(CGRect frame, nfloat offset)
		{
			this.smallImage.BackgroundColor = UIColor.White;
			this.smallImage.Bounds = new CGRect (0, 0, Constants.SECOND_IMAGE_WIDTH, frame.Size.Height);
			this.smallImage.Frame = new CGRect (offset, 0, Constants.SECOND_IMAGE_WIDTH, frame.Size.Height);
			this.smallImage.UserInteractionEnabled = true;
			this.smallImage.MultipleTouchEnabled = true;

			this.didGrow = false;
			this.didTap = false;
			UITapGestureRecognizer tap = new UITapGestureRecognizer (handleSingleTap);
			this.smallImage.AddGestureRecognizer (tap);


			initialFrame = smallImage.Frame;
			initialBounds = smallImage.Bounds;

			return this.smallImage;
		}

		#region SmallGestures
		public void handleSingleTap(UITapGestureRecognizer tap)
		{
			didTap = true;
			if (gesturesDelegate.beginGestures (this)) {
				gesturesDelegate.manageTap (this);
				didTap = false;
			} else
				didTap = false;
		}
		#endregion

		#region touchesFinished

		public void largeTouchesEnd(string name)
		{
			this.gesturesDelegate.largeGesturesFinished(this, name);
		}
		#endregion
	}
}


using System;
using UIKit;

namespace BookXamarin
{
	public interface GesturesDelegate
	{
		void manageTap(PageController page);
		void manageRotationInView(UIView view,float angle);
		void managePanInView(UIView view, UIPanGestureRecognizer panRecognizer);
		void manageScaleInView(UIView view, float scale, UIPinchGestureRecognizer pinch);
		bool beginGestures(PageController page);
		void gesturesFinished(PageController page, string name);
		void largeGesturesFinished(PageController page, string name);
		void beginLargeGestures(PageController page);
	}
}


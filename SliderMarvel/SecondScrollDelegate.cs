using System;

namespace BookXamarin
{
	public interface SecondScrollDelegate
	{
		void secondScrollWillBeginDragging(UISecondScrollView scrollView);
		void secondScrollDidScroll(UISecondScrollView scrollView);
		void secondScrollDidEndDragging(UISecondScrollView scrollView);
		void updateChapterScroll(UISecondScrollView scrollView);
		void alignChapterScrollView();
	}
}


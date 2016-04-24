using System;
using System.Drawing;
using CoreGraphics;

namespace BookXamarin

{
	public class Constants
	{
		public static int MAIN_WIDTH = 1024;
		public static int MAIN_HEIGHT = 768;

		public static int MAIN_ORIGIN_X = 0;
		public static int MAIN_ORIGIN_Y = 0;

		public static int SECOND_WIDTH = 1024;
		public static int SECOND_HEIGHT = 102;

		public static int SECOND_ORIGIN_X = 0;
		public static int SECOND_ORIGIN_Y = 646;  // size 1024 * 646 (contentPage)

		public static int CONTENT_HEIGHT = 768;

		public static int SECOND_IMAGE_WIDTH = (int)(SECOND_HEIGHT * 1.585); // 102 * 1,6

		public static int REQUIRED_WIDTH = 427;
		public static int REQUIRED_HEIGHT = 320;

		public static int LIMIT_TO_CHANGE_CHAPTER = 175;

		public static float DELAY_IN_LARGE_VIEW = 0.25f;
		public static float ANIMATION_DURATION = 0.55f;
		public static float CHAPTER_TIME_TO_ZOOM = 0.4f;
		public static float CHAPTER_ZOOM = 1.04f;
		public static CGRect mainFrame = new CGRect (MAIN_ORIGIN_X, MAIN_ORIGIN_Y, MAIN_WIDTH, MAIN_HEIGHT);
	}
}


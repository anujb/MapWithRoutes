using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace MapWithRoutes.Sample
{
	public class Application
	{
		static void Main (string[] args)
		{
			UIApplication.Main (args);
		}
	}
	
	public partial class AppDelegate : UIApplicationDelegate
	{
        MapWithRouteViewController controller;

		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
            
            controller = new MapWithRouteViewController();
            window.RootViewController = controller;
            
			window.MakeKeyAndVisible ();
			return true;
		}
	}
}


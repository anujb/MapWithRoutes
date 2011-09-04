using System;
using MonoTouch.UIKit;
using System.Drawing;

namespace MapWithRoutes.Sample
{
    public class MapWithRouteViewController : UIViewController
    {
        MapView _MapView { get; set; }
        
        public MapWithRouteViewController ()
        {
        }
        
        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();
            
            _MapView = new MapView(new RectangleF(0, 0, View.Frame.Width, View.Frame.Height));
            _MapView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
            
            
            View.AddSubview(_MapView);               
			
            var home = new Place()
			{
				Name = "Home",
				Description = "Boring Home Town",
				Latitude = 32.725410,
				Longitude = -97.320840,
			};
            
            var office = new Place()
			{
				Name = "Austin",
				Description = "Super Awesome Town",
				Latitude = 30.26710,
				Longitude = -97.744546,
			};
            
            _MapView.ShowRouteFrom(office, home);
        }
        
        public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
        {
            return true;
        }
    }
}


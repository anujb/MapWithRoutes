using System;
using MonoTouch.CoreLocation;
using MonoTouch.MapKit;

namespace MapWithRoutes
{
    public class PlaceMark : MKAnnotation
    {
        public Place Place { get; set; }
        public override CLLocationCoordinate2D Coordinate { get; set; }
        
        public override string Subtitle 
        {
            get 
            {
                return this.Place.Description;
            }
        }
        
        public override string Title 
        {
            get 
            {
                return this.Place.Name;
            }
        }
        
        public PlaceMark(Place place)
        {
            this.Place = place;
            this.Coordinate = new CLLocationCoordinate2D(place.Latitude, place.Longitude);
            
        }
    }
}


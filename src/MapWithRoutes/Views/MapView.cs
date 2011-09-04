using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.MapKit;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.CoreLocation;
using MonoTouch.CoreGraphics;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using MonoTouch.Foundation;
using RestSharp;
using ServiceStack.Text;

namespace MapWithRoutes
{
	public class MapView : UIView
	{
		public UIColor LineColor { get; set; }
		public Action<PlaceMark, PlaceMark> CalculateRoutesAction { get; private set; }
		
		MKMapView _MapView { get; set; }
		UIImageView _RouteView { get; set; }
		CLLocation[] _Routes { get; set; }
		
//		HttpWebRequest Request { get; set; }
		
		RestClient _Client { get; set; }
		RestRequest _Request { get; set; }
		RestResponse _Response { get; set; }
		RouteResult _RouteResult { get; set; }
		
		public MapView()
		{
		}
		
		public MapView(RectangleF frame)
			: base(frame)
		{
			Initialize();	
		}
		
		private void Initialize()
		{
			_MapView = new MKMapView();
			_MapView.Frame = new RectangleF(0, 0, Frame.Width, Frame.Height);
			_MapView.ShowsUserLocation = true;
			_MapView.Delegate = new MapViewDelegate(this);
			
			_RouteView = new UIImageView();
			_RouteView.Frame = new RectangleF(0, 0, _MapView.Frame.Width, _MapView.Frame.Height);
			_RouteView.UserInteractionEnabled = false;
			
			this.LineColor = UIColor.FromWhiteAlpha(0.2f, 0.5f);
			
			_Routes = new CLLocation[] { };
			
			CalculateRoutesAction = CalculateRoutes;
				
			_Client = new RestClient(@"http://maps.googleapis.com/maps/api/directions/");
			_Client.AddDefaultUrlSegment("sensor", "true");
			
			_MapView.AddSubview(_RouteView);
			this.AddSubview(_MapView);
		}
		
		public void ShowRouteFrom(Place from, Place to)
		{
			if(_Routes.Any())
			{
				_MapView.RemoveAnnotations(_MapView.Annotations as MKAnnotation[]);
				_Routes = new CLLocation[] { };
			}
				
			var placeMarkFrom = new PlaceMark(from);
			var placeMarkTo = new PlaceMark(to);
			
			CalculateRoutesAction.BeginInvoke(placeMarkFrom, placeMarkTo, (ar) => 
			{
				CalculateRoutesAction.EndInvoke(ar);
				
			}, null);
		}
		
		private void CalculateRoutesFromAction_Handler(PlaceMark from, PlaceMark to)
		{
			CalculateRoutes(from.Coordinate, to.Coordinate);
			
			using(var pool = new NSAutoreleasePool())
			{
				_MapView.AddAnnotation(from);
				_MapView.AddAnnotation(to);
				
				UpdateRouteView();
				CenterMap();
			}
		}
		
		
		public void UpdateRouteView()
		{
			using(var context = new CGBitmapContext(null, (int)_RouteView.Frame.Width, (int)_RouteView.Frame.Height, 8,
				(int)(4 * _RouteView.Frame.Width), CGColorSpace.CreateDeviceRGB(), CGImageAlphaInfo.PremultipliedLast))
			{
				context.SetStrokeColor(LineColor.CGColor);
				context.SetFillColor(0.0f, 0.0f, 1.0f, 1.0f);
				context.SetLineWidth(3.0f);
				
				for (int i = 0; i < _Routes.Count(); i++) 
				{
					var route = _Routes[i];
					var point = _MapView.ConvertCoordinate(route.Coordinate, _RouteView);
					
					if(i == 0)
						context.MoveTo(point.X, _RouteView.Frame.Height - point.Y);
					else
						context.AddLineToPoint(point.X, _RouteView.Frame.Height - point.Y);
				}
				
				context.StrokePath();
				
				var cgImage = context.ToImage();
				var image = UIImage.FromImage(cgImage);
				
				_RouteView.Image = image;
			}
		}
		
		private void CalculateRoutes(PlaceMark from, PlaceMark to)
		{
			CalculateRoutes(from.Coordinate, to.Coordinate);
		}
		
		private void CalculateRoutes(CLLocationCoordinate2D from, CLLocationCoordinate2D to)
		{
			try 
			{
				_Request = new RestRequest("json", Method.GET);
				_Request.AddParameter("sensor", "false");

				var origin = string.Format(@"{0},{1}", from.Latitude, from.Longitude);
				_Request.AddParameter("origin", origin);
				
				var dest = string.Format(@"{0},{1}", to.Latitude, to.Longitude);
				_Request.AddParameter("destination", dest);
				
				_Response = _Client.Execute(_Request);
				
				if(!string.IsNullOrWhiteSpace(_Response.Content))
				{
					_RouteResult = ParseRouteJson(_Response.Content);
					Console.WriteLine("Directions Response Status: {0}", _RouteResult.Status);
					
					if(_RouteResult.Routes.Any())
					{
						var point = _RouteResult.Routes.FirstOrDefault();
						if(point != null)
							_Routes = DecodePolyLine(point.Overview_Polyline.Points).ToArray();
					}
				}
			}
			catch (Exception ex) 
			{
				throw ex;
			}
		}
		
		private static RouteResult ParseRouteJson(string json)
		{
			var result = JsonObject.Parse(json).ConvertTo(x=> new RouteResult
			{
				Status = x.Get("status"),
				Routes = x.ArrayObjects("routes").ConvertAll(r=>new Route
				{
					Overview_Polyline = r.Object("overview_polyline").ConvertTo(p => 
						new Polyline
						{
							Points = p.Get("points")
						})
				})
			});
			
			return result;
		}
		
		private IEnumerable<CLLocation> CalculateRoutesFrom(CLLocationCoordinate2D from, CLLocationCoordinate2D to)
		{
//			var saddr = string.Format(@"{0},{1}", from.Latitude, from.Longitude);
//			var daddr = string.Format(@"{0},{1}", to.Latitude, to.Longitude);
//			
//			saddr = "dallas,tx";
//			daddr = "austin,tx";
//			
//			var uri = new Uri(string.Format(@"http://maps.google.com/maps?output=dragdir&saddr={0}&daddr={1}", saddr, daddr));
//			Request = new HttpWebRequest(uri);
			
			var apiResponse = string.Empty;
//			var response = Request.GetResponse();
//			
//			using(var stream = response.GetResponseStream())
//			{
//				using(var rdr = new StreamReader(stream))
//				{
//					apiResponse = rdr.ReadToEnd();
//				}
//			}
//			
//			var regex = new Regex("points:\\\"([^\\\"]*)\\\"");
//			var match = regex.Match(apiResponse);
//		    apiResponse = match.Groups[1].Value;
			
			return DecodePolyLine(apiResponse);
		}
		
		public void CenterMap()
		{
			double maxLatitude = -90;
			double maxLongitude = -180;
			double minLatitude = 90;
			double minLongitude = 180;
			
			MKCoordinateRegion region;
		
			for(int i = 0; i < _Routes.Count(); i++) 
			{
				var location = _Routes[i];
				
				if(location.Coordinate.Latitude > maxLatitude)
					maxLatitude = location.Coordinate.Latitude;
				
				if(location.Coordinate.Latitude < minLatitude)
					minLatitude = location.Coordinate.Latitude;
				
				if(location.Coordinate.Longitude > maxLongitude)
					maxLongitude = location.Coordinate.Longitude;
				
				if(location.Coordinate.Longitude < minLongitude)
					minLongitude = location.Coordinate.Longitude;
				
				region.Center.Latitude = (maxLatitude + minLatitude) / 2;
				region.Center.Longitude = (maxLongitude + minLongitude) / 2;
				
				region.Span.LatitudeDelta = maxLatitude - minLatitude;
				region.Span.LongitudeDelta = maxLongitude - minLongitude;
				
				_MapView.SetRegion(region, true);
			}
		}
		
		private List<CLLocation> DecodePolyLine(string encoded)
		{
			var array = new List<CLLocation> { };
			encoded = encoded.Replace(@"\\\\", @"\\");
			
			int lat = 0; 
			int lng = 0;
			int index = 0;
			
			while (index < encoded.Length)
			{
				int b;
				int shift = 0;
				int result = 0;
				do 
				{
					b = encoded[index++] - 63;
					result |= (b & 0x1f) << shift;
					shift += 5;
					
				} while (b >= 0x20);
				
				int dlat = ((result & 1) == 1 ? ~(result >> 1) : (result >> 1));
				lat += dlat;
				
				shift = 0;
				result = 0;
				
				do 
				{
					b = encoded[index++] - 63;
					result |= (b & 0x1f) << shift;
					shift += 5;
				} 
				while (b >= 0x20);
				
				int dlng = ((result & 1) == 1 ? ~(result >> 1) : (result >> 1));
				lng += dlng;

				double latitude = lat * 1e-5;
				double longitude = lng * 1e-5;
				
				Console.WriteLine("Latitude: {0}", latitude);
				Console.WriteLine("Longitude: {0}", longitude);
				
				var location = new CLLocation(latitude, longitude);
				array.Add(location);
			}
			
			return array;
		}
		
		
		public class MapViewDelegate : MKMapViewDelegate
		{
			MapView _MapView { get; set; }
			
			public MapViewDelegate(MapView mapView)
			{
				this._MapView = mapView;
			}
			
			public override void RegionWillChange(MKMapView mapView, bool animated)
			{
				_MapView._RouteView.Hidden = true;
			}
			
			public override void RegionChanged(MKMapView mapView, bool animated)
			{
				_MapView.UpdateRouteView();
				_MapView._RouteView.Hidden = false;
				_MapView._RouteView.SetNeedsDisplay();
			}
		}
			
	}
}


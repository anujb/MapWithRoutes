using System;
using System.Collections.Generic;
using ServiceStack.Text;

namespace MapWithRoutes
{
	public class RouteResult
	{
		public string Status { get; set; }
		public List<Route> Routes { get; set; }
	}

	public class Route
	{
		public Polyline Overview_Polyline { get; set; }
	}
	
	public class Polyline
	{
		public string Points { get; set; }
		public string Levels { get; set; }	
	}
	
}


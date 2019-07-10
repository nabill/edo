using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Locations.Google
{
    public readonly struct Geometry
    {
        [JsonConstructor]
        public Geometry(GeoPoint location, Viewport viewport)
        {
            Location = location;
            Viewport = viewport;
        }


        public GeoPoint Location { get; }
        public Viewport Viewport { get; }
    }
}

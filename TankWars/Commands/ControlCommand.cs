using Newtonsoft.Json;
using System;
using TankWars;

namespace Commands
{
    /// <summary>
    /// Nested helper class used for sending JSON control commands to the server.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class ControlCommand
    {
        [JsonProperty(PropertyName = "moving")]
        private string moving;

        [JsonProperty(PropertyName = "fire")]
        private string firing;

        [JsonProperty(PropertyName = "tdir")]
        private Vector2D orientation;

        public ControlCommand(string _moving, string _firing, Vector2D _orientation)
        {
            moving = _moving;
            firing = _firing;
            orientation = _orientation;
        }

        public string Moving
        {
            get
            {
                return moving;
            }
        }

        public string Firing
        {
            get
            {
                return firing;
            }
        }

        public Vector2D Orientation
        {
            get
            {
                return orientation;
            }
        }
    }
}

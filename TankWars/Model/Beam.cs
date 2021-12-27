// Ryan Garcia and Jordy Larrea 

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using TankWars;

namespace Model
{
    /// <summary>
    /// Class that represents a beam, i.e. the alternative fire of a tank. 
    /// </summary>
    
    [JsonObject(MemberSerialization.OptIn)]
    public class Beam
    {
        [JsonProperty(PropertyName = "beam")]
        private int projID;

        [JsonProperty(PropertyName = "org")]
        private Vector2D location;

        [JsonProperty(PropertyName = "dir")]
        private Vector2D orientation;

        [JsonProperty(PropertyName = "owner")]
        private int ownerID;
        /// <summary>
        /// Constructor for the beam class
        /// </summary>
        /// <param name="_projID"></param>
        /// <param name="_location"></param>
        /// <param name="_orientation"></param>
        /// <param name="_ownerID"></param>
        public Beam(int _projID, Vector2D _location, Vector2D _orientation, int _ownerID)
        {
            projID = _projID;
            location = _location;
            orientation = _orientation;
            ownerID = _ownerID;
        }
        // Properties for important beam fields
        [JsonIgnore]
        public Vector2D Location
        {
            get
            {
                return location;
            }
            set
            {
                location = value;
            }
        }
        [JsonIgnore]
        public Vector2D Orientation
        {
            get
            {
                return orientation;
            }
        }
        [JsonIgnore]
        public int Owner
        {
            get
            {
                return ownerID;
            }
        }
    }
}

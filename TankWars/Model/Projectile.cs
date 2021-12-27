// Ryan Garcia and Jordy Larrea

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using TankWars;

namespace Model
{
    /// <summary>
    /// Class that represents a projectile.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Projectile
    {
        [JsonProperty(PropertyName = "proj")]
        private int projID;

        [JsonProperty(PropertyName = "loc")]
        private Vector2D location;

        [JsonProperty(PropertyName = "dir")]
        private Vector2D orientation;

        [JsonProperty(PropertyName = "died")]
        private bool died;

        [JsonProperty(PropertyName = "owner")]
        private int ownerID;
        /// <summary>
        /// Constructor for the projectile class
        /// </summary>
        /// <param name="_projID"></param>
        /// <param name="_location"></param>
        /// <param name="_orientation"></param>
        /// <param name="_ownerID"></param>
        public Projectile(int _projID, Vector2D _location, Vector2D _orientation, int _ownerID)
        {
            projID = _projID;
            location = _location;
            orientation = _orientation;
            ownerID = _ownerID;
        }
        // Properties for important projectile fields.
        [JsonIgnore]
        public Vector2D Orientation
        {
            get
            {
                return orientation;
            }
        }
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
        public int Owner
        {
            get
            {
                return ownerID;
            }
        }
        [JsonIgnore]
        public int ProjID
        {
            get
            {
                return projID;
            }
        }
        [JsonIgnore]
        public bool Died
        {
            get
            {
                return died;
            }
            set
            {
                died = value;
            }
        }
    }
}

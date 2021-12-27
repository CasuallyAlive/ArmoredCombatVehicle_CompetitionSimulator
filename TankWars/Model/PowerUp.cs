// Ryan Garcia and Jordy Larrea 

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using TankWars;

namespace Model
{
    /// <summary>
    /// Class that represents the power-up onject.
    /// </summary>
    public class PowerUp
    {
        [JsonProperty(PropertyName = "power")]
        private int powerID;

        [JsonProperty(PropertyName = "loc")]
        private Vector2D location;

        [JsonProperty(PropertyName = "died")]
        private bool died;


        /// <summary>
        /// Constructor for the Power-up object. 
        /// </summary>
        /// <param name="_powerID"></param>
        /// <param name="_location"></param>
        public PowerUp(int _powerID, Vector2D _location)
        {
            powerID = _powerID;
            location = _location;
        }

        // Properties for important projectile fields
        [JsonIgnore]
        public int PowerID
        {
            get
            {
                return powerID;
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

        [JsonIgnore]
        public Vector2D Location
        {
            get
            {
                return location;
            }
        }
    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
// Ryan Garcia and Jordy
using System.Text;
using TankWars;

namespace Model
{
    /// <summary>
    /// Class that represents a wall object in this client. 
    /// </summary>
    public class Wall
    {
        [JsonProperty(PropertyName = "wall")]
        private int wallID;

        [JsonProperty(PropertyName = "p1")]
        private Vector2D endpointOne;

        [JsonProperty(PropertyName = "p2")]
        private Vector2D endpointTwo;
        /// <summary>
        /// Constructor for the wall class.
        /// </summary>
        /// <param name="_wallID"></param>
        /// <param name="_endpointOne"></param>
        /// <param name="_endpointTwo"></param>
        public Wall(int _wallID, Vector2D _endpointOne, Vector2D _endpointTwo)
        {
            wallID = _wallID;
            endpointOne = _endpointOne;
            endpointTwo = _endpointTwo;
        }

        //Properties for the wall class
        [JsonIgnore]
        public int WallID
        {
            get
            {
                return wallID;
            }
        }
        [JsonIgnore]
        public Vector2D Side1
        {
            get
            {
                return endpointOne;
            }
        }
        [JsonIgnore]
        public Vector2D Side2
        {
            get
            {
                return endpointTwo;
            }
        }
    }
}

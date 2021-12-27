// Ryan Garcia and Jordy Larrea

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using TankWars;

namespace Model
{
    /// <summary>
    /// Class that represents the tanks in this instance of the tankwars client.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Tank
    {
        [JsonProperty(PropertyName = "tank")]
        private int ID;

        [JsonProperty(PropertyName = "loc")]
        private Vector2D location;

        [JsonProperty(PropertyName = "bdir")]
        private Vector2D orientation;

        [JsonProperty(PropertyName = "tdir")]
        private Vector2D aiming;

        [JsonProperty(PropertyName = "name")]
        private string name;

        [JsonProperty(PropertyName = "hp")]
        private int hitPoints;

        [JsonProperty(PropertyName = "score")]
        private int score = 0;

        [JsonProperty(PropertyName = "died")]
        private bool died;

        [JsonProperty(PropertyName = "dc")]
        private bool disconnected;

        [JsonProperty(PropertyName = "join")]
        private bool joined;

        [JsonIgnore]
        private int shotFramesElapsed;

        [JsonIgnore]
        private bool canfire = true;

        [JsonIgnore]
        private int deathFramesElapsed;

        [JsonIgnore]
        private int powerUpCounts = 0;

        [JsonIgnore]
        private bool isDrunk = false;

        [JsonIgnore]
        private int drunkFrames;

        /// <summary>
        /// Constructor for a tank object.
        /// </summary>
        /// <param name="_ID"></param>
        /// <param name="_location"></param>
        /// <param name="_orientation"></param>
        /// <param name="_name"></param>
        public Tank(int _ID, Vector2D _location, Vector2D _orientation, Vector2D _aiming, string _name, int _hitPoints)
        {
            hitPoints = _hitPoints;
            ID = _ID;
            location = _location;
            orientation = _orientation;
            aiming = _aiming;
            name = _name;
        }
        /// <summary>
        /// Property for the boolean that represents whether a tank has activated, or has "drank to much", its special ability
        /// </summary>
        [JsonIgnore]
        public bool IsDrunk
        {
            get
            {
                return isDrunk;
            }
            set
            {
                isDrunk = value;
            }
        }
        /// <summary>
        /// Method used to increment this tanks score.
        /// </summary>
        public void incrementScore()
        {
            score++;
        }
        
        /// <summary>
        /// Method used to decrement this tanks HP.
        /// </summary>
        public void DecrementHP()
        {
            hitPoints--;
        }

        /// <summary>
        /// Method used to update this tanks location based on the given velocity.
        /// </summary>
        /// <param name="velocity"></param>
        public void UpdateLocation(Vector2D velocity)
        {
            location += velocity;
        }

        /// <summary>
        /// Method used to update this tanks orientation based on the given orientation.
        /// </summary>
        /// <param name="newOrientation"></param>
        public void UpdateOrientation(Vector2D newOrientation)
        {
            orientation = newOrientation;
        }

        /// <summary>
        /// Method used to update this tanks turret orientation based on the given orientation.
        /// </summary>
        /// <param name="newTOrientation"></param>
        public void UpdateTurretOrientation(Vector2D newTOrientation)
        {
            aiming = newTOrientation;
        }

        // Properties for all tha fields associated with a tank object.
        [JsonIgnore]
        public string Name
        {
            get
            {
                return name;
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
        public int HitPoints
        {
            get
            {
                return hitPoints;
            }
            set
            {
                hitPoints = value;
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
        public int TankID
        {
            get
            {
                return ID;
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
        public Vector2D Aim
        {
            get
            {
                return aiming;
            }
        }

        [JsonIgnore]
        public int Score
        {
            get
            {
                return score;
            }
        }

        [JsonIgnore]
        public bool Joined
        {
            get
            {
                return joined;
            }
        }

        [JsonIgnore]
        public bool Disconnected
        {
            get
            {
                return disconnected;
            }
            set
            {
                disconnected = value;
            }
        }

        [JsonIgnore]
        public int ShotFramesElapsed
        {
            get
            {
                return shotFramesElapsed;
            }
            set
            {
                shotFramesElapsed = value;
            }
        }

        [JsonIgnore]
        public int DeathFramesElapsed
        {
            get
            {
                return deathFramesElapsed;
            }
            set
            {
                deathFramesElapsed = value;
            }
        }

        [JsonIgnore]
        public bool CanFire
        {
            get
            {
                return canfire;
            }
            set
            {
                canfire = value;
            }
        }

        [JsonIgnore]
        public int PowerUpCounts
        {
            get
            {
                return powerUpCounts;
            }
            set
            {
                powerUpCounts = value;
            }
        }
        /// <summary>
        /// Property for the integer value that represents how many frames the "Drunk" ability is elapsed.
        /// </summary>
        [JsonIgnore]
        public int DrunkFrames
        {
            get
            {
                return drunkFrames;
            }
            set
            {
                drunkFrames = value;
            }
        }
    }
}
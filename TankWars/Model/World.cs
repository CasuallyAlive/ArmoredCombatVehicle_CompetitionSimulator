// Authors: Ryan Garcia and Jordy Larrea
using System;
using System.Collections.Generic;
using TankWars;

namespace Model
{
    /// <summary>
    /// Class that represents the world logic in this iteration of TankWars.
    /// </summary>
    public class World
    {
        // Dictionaries with appropriate objects of this world such as tanks, walls, etc. The objects are accessed by a unique id. 
        private Dictionary<int, Tank> tanks;
        private Dictionary<int, Wall> walls;
        private Dictionary<int, Projectile> projectiles;
        private Dictionary<int, PowerUp> powerUps;
        private Dictionary<int, Beam> beams;
        // The id of this particular client
        private int clientID;
   
        private int worldSize;

        private int framesPerPowerUp;
        private int maxPowerUps;

        /// <summary>
        /// Constructor for the world class
        /// </summary>
        /// <param name="_worldSize"></param>
        /// <param name="_clientID"></param>
        public World(int _worldSize, int _clientID)
        {
            worldSize = _worldSize;
            clientID = _clientID;
            tanks = new Dictionary<int, Tank>();
            walls = new Dictionary<int, Wall>();
            projectiles = new Dictionary<int, Projectile>();
            powerUps = new Dictionary<int, PowerUp>();
            beams = new Dictionary<int, Beam>();
        }

        /// <summary>
        /// Constructor that allows for the defining of extra server related variables.
        /// </summary>
        /// <param name="_worldSize"></param>
        /// <param name="_walls"></param>
        /// <param name="_framesPerPowerUp"></param>
        /// <param name="_maxPowerUps"></param>
        public World(int _worldSize, Dictionary<int,Wall> _walls, int _framesPerPowerUp, int _maxPowerUps)
        {
            worldSize = _worldSize;

            walls = _walls;
            framesPerPowerUp = _framesPerPowerUp;
            maxPowerUps = _maxPowerUps;
            tanks = new Dictionary<int, Tank>();
            projectiles = new Dictionary<int, Projectile>();
            powerUps = new Dictionary<int, PowerUp>();
            beams = new Dictionary<int, Beam>();
        }

        /// <summary>
        /// Adds a tank to the dictionary of tanks if the tank isn't dead or disconnected. 
        /// </summary>
        /// <param name="tank"></param>
        public void AddTank(Tank tank)
        {
            if (tank.Disconnected || tank.Died)
                tanks.Remove(tank.TankID);
            else
            {
                if (!tanks.ContainsKey(tank.TankID))
                {
                    tanks.Add(tank.TankID, tank);
                    return;
                }
                tanks[tank.TankID] = tank;
            }
        }
        /// <summary>
        /// Adds a projectile to the dictionary of projectiles if the projectile isn't destroyed. 
        /// </summary>
        /// <param name="tank"></param>
        public void AddProjectile(Projectile projToAdd)
        {
            if (projToAdd.Died)
                projectiles.Remove(projToAdd.ProjID);
            else
            {
                if (!projectiles.ContainsKey(projToAdd.ProjID))
                {
                    projectiles.Add(projToAdd.ProjID, projToAdd);
                    return;
                }
                projectiles[projToAdd.ProjID] = projToAdd;
            }
        }

        /// <summary>
        /// Method used to remove the given projectile from this world.
        /// </summary>
        /// <param name="projToRemove"></param>
        public void RemoveProjectile(Projectile projToRemove)
        {
            projectiles.Remove(projToRemove.ProjID);
        }

        /// <summary>
        /// Method used to remove the given tank from this world.
        /// </summary>
        /// <param name="TankToRemove"></param>
        public void RemoveTank(Tank TankToRemove)
        {
            tanks.Remove(TankToRemove.TankID);
        }

        /// <summary>
        /// Adds a power-up to the dictionary of power-ups if the power-up isn't picked up. 
        /// </summary>
        /// <param name="tank"></param>
        public void AddPowerUp(PowerUp powToAdd)
        {
            if (powToAdd.Died)
                powerUps.Remove(powToAdd.PowerID);
            else
            {
                if (!powerUps.ContainsKey(powToAdd.PowerID))
                {
                    powerUps.Add(powToAdd.PowerID, powToAdd);
                    return;
                }
                powerUps[powToAdd.PowerID] = powToAdd;
            }
        }
        /// <summary>
        /// Adds a wall to the dictionary of walls. 
        /// </summary>
        /// <param name="tank"></param>
        public void AddWall(Wall wallToAdd)
        {
            if (!walls.ContainsKey(wallToAdd.WallID))
            {
                walls.Add(wallToAdd.WallID, wallToAdd);
                return;
            }
            walls[wallToAdd.WallID] = wallToAdd;
        }
        /// <summary>
        /// Adds a beam to the dictionary of beams.
        /// </summary>
        /// <param name="beam"></param>
        public void AddBeam(Beam beam)
        {

            if (!beams.ContainsKey(beam.Owner))
            {
                beams.Add(beam.Owner, beam);
                return;
            }
            beams[beam.Owner] = beam;
        }

        // Poperty that allows read-only access to the size of the world.
        public int WorldSize
        {
            get
            {
                return worldSize;
            }
        }
        //Getters for the members of this world
        public Dictionary<int,Tank> GetTanks()
        {
            return tanks;
        }
        public Dictionary<int,PowerUp> GetPowerUps()
        {
            return powerUps;
        }
        public Dictionary<int, Wall> GetWalls()
        {
            return walls;
        }
        public Dictionary<int, Projectile> GetProjectiles()
        {
            return projectiles;
        }
        public Dictionary<int, Beam> GetBeams()
        {
            return beams;
        }

        // Poperty that allows read-only access to the ID of this client.
        public int ClientID
        {
            get
            {
                return clientID;
            }
        }

        //Property for the amount of frames in between powerup respawns.
        public int FramesPerPowerUp
        {
            get
            {
                return framesPerPowerUp;
            }

            set
            {
                framesPerPowerUp = value;
            }
        }
        
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Model;
using TankWars;

namespace ServerController
{
    public class ServerSettings
    {
        // Important fields that define behavior in this TankWars server.
        public readonly int universeSize;
        public readonly int msPerFrame;
        public readonly int framesPerShot;
        public readonly int respawnRate;
        public readonly int hitPoints;
        public readonly int projectileSpeed;
        public readonly int tankSpeed;
        public readonly int tankSize;
        public readonly int wallSize;
        public readonly int maxPowerUps;
        public readonly int maxPowerUpDelay;
        public readonly bool drunkTankMode;
        public readonly int drunkFrames;

        // A dictionary containing the walls that are sent to a client during the handshake.
        public readonly Dictionary<int, Wall> walls = new Dictionary<int, Wall>();

        /// <summary>
        /// Helper class used to read and store the servers settings, this includes the world layout and size.
        /// </summary>
        /// <param name="filepath"></param>
        public ServerSettings(string filepath)
        {
            hitPoints = 3;
            projectileSpeed = 25;
            tankSpeed = 3;
            tankSize = 60;
            wallSize = 50;
            maxPowerUps = 2;
            maxPowerUpDelay = 1650;
            drunkTankMode = false;
            drunkFrames = 600;
            //Simple XML reader that will load the settings for this server.
            try
            {
                using (XmlReader settingsReader = XmlReader.Create(filepath + "settings.xml"))
                {
                    settingsReader.MoveToContent();
                    int i = 0;
                    double x1 = 0;
                    double x2 = 0;
                    double y1 = 0;
                    double y2 = 0;
                    while (settingsReader.Read())
                    {
                        if (settingsReader.IsStartElement())
                        {
                            switch (settingsReader.Name)
                            {

                                case "UniverseSize":
                                    settingsReader.Read();
                                    if (!Int32.TryParse(settingsReader.Value, out int _universeSize))
                                    {
                                        throw new ArgumentException();
                                    }
                                    universeSize = _universeSize;
                                    break;

                                case "MSPerFrame":
                                    settingsReader.Read();
                                    if (!Int32.TryParse(settingsReader.Value, out int _msPerFrame))
                                    {
                                        throw new ArgumentException();
                                    }
                                    msPerFrame = _msPerFrame;
                                    break;

                                case "FramesPerShot":
                                    settingsReader.Read();
                                    if (!Int32.TryParse(settingsReader.Value, out int _framesPerShot))
                                    {
                                        throw new ArgumentException();
                                    }
                                    framesPerShot = _framesPerShot;
                                    break;

                                case "RespawnRate":
                                    settingsReader.Read();
                                    if (!Int32.TryParse(settingsReader.Value, out int _respawnRate))
                                    {
                                        throw new ArgumentException();
                                    }
                                    respawnRate = _respawnRate;
                                    break;
                                case "Hitpoints":
                                    settingsReader.Read();
                                    if (!Int32.TryParse(settingsReader.Value, out int _hitPoints))
                                        throw new ArgumentException();
                                    hitPoints = _hitPoints;
                                    break;
                                case "ProjectileSpeed":
                                    settingsReader.Read();
                                    if (!Int32.TryParse(settingsReader.Value, out int _projectileSpeed))
                                        throw new ArgumentException();

                                    projectileSpeed = _projectileSpeed;
                                    break;
                                case "TankSpeed":
                                    settingsReader.Read();
                                    if (!Int32.TryParse(settingsReader.Value, out int _tankSpeed))
                                        throw new ArgumentException();

                                    tankSpeed = _tankSpeed;
                                    break;
                                case "TankSize":
                                    settingsReader.Read();
                                    if (!Int32.TryParse(settingsReader.Value, out int _tankSize))
                                        throw new ArgumentException();

                                    tankSize = _tankSize;
                                    break;
                                case "WallSize":
                                    settingsReader.Read();
                                    if (!Int32.TryParse(settingsReader.Value, out int _wallSize))
                                        throw new ArgumentException();

                                    wallSize = _wallSize;
                                    break;
                                case "MaxPowerUps":
                                    settingsReader.Read();
                                    if (!Int32.TryParse(settingsReader.Value, out int _maxPowerUps))
                                        throw new ArgumentException();

                                    maxPowerUps = _maxPowerUps;
                                    break;
                                case "MaxPowerUpDelay":
                                    settingsReader.Read();
                                    if (!Int32.TryParse(settingsReader.Value, out int _maxPowerUpDelay))
                                        throw new ArgumentException();

                                    maxPowerUpDelay = _maxPowerUpDelay;
                                    break;
                                case "DrunkTankMode":
                                    settingsReader.Read();
                                    if (!bool.TryParse(settingsReader.Value, out bool _drunkTankMode))
                                        throw new ArgumentException();

                                    drunkTankMode = _drunkTankMode;
                                    break;
                                case "DrunkFrames":
                                    settingsReader.Read();
                                    if (!Int32.TryParse(settingsReader.Value, out int _drunkFrames))
                                        throw new ArgumentException();

                                    drunkFrames = _drunkFrames;
                                    break;
                                case "p1":
                                    settingsReader.Read();
                                    settingsReader.Read();
                                    if (!Double.TryParse(settingsReader.Value, out x1))
                                    {
                                        throw new ArgumentException();
                                    }
                                    settingsReader.Read();
                                    settingsReader.Read();
                                    settingsReader.Read();
                                    if (!Double.TryParse(settingsReader.Value, out y1))
                                        throw new ArgumentException();
                                    break;

                                case "p2":
                                    settingsReader.Read();
                                    settingsReader.Read();
                                    if (!Double.TryParse(settingsReader.Value, out x2))
                                    {
                                        throw new ArgumentException();
                                    }
                                    settingsReader.Read();
                                    settingsReader.Read();
                                    settingsReader.Read();
                                    if (!Double.TryParse(settingsReader.Value, out y2))
                                        throw new ArgumentException();
                                    break;

                            }
                        }
                        else // end of an element
                        {
                            if (settingsReader.Name == "Wall") //If the block read defined a wall
                            {
                                walls.Add(i, new Wall(i++, new Vector2D(x1, y1), new Vector2D(x2, y2)));
                                x1 = 0;
                                x2 = 0;
                                y1 = 0;
                                y2 = 0;
                            }
                        }

                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Invalid xml settings file! Please check the file for proper syntax.");
            }
        }
    }
}

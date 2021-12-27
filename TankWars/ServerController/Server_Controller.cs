// Jordy Larrea and Ryan Garcia

using Commands;
using Model;
using NetworkUtil;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using TankWars;

namespace ServerController
{
    /// <summary>
    /// Class that informs the world and clients of any interactions that occur on this server
    /// </summary>
    public class Server_Controller
    {        
        // Dictionary that maps clients to specific ID's
        private Dictionary<long, SocketState> clients;

        // Field for the world of this server       
        private World theWorld;

        // Field for the TCPListener of this server
        private TcpListener server;

        // Field that encapsulates important constants such as the amount of time elapsed per frame and so forth
        private ServerSettings settings;

        // The port used by this server
        private const int port = 11000;

        // RNG object used throughout this class for random processes
        private Random rng;

        // A field containing the serialized data that is sent once per frame by the server
        private string serializedData;

        // A numerical identifier that is incremented such as to create new unique power-ups
        private long powerUpID;

        // Event that alerts the server-app that a client has connected.
        public delegate void ClienConnectedHandler(long clientID);
        public event ClienConnectedHandler ClientConnected;



        // Event that alerts the server-app that a client has disconnected.
        public delegate void ClientDisconnectedHandler(long clientID);
        public event ClientDisconnectedHandler ClientDisconnected;

        // A Dictionary that is used to maintain one control command per client per frame
        private Dictionary<long, ControlCommand> clientCommands;
        private int projID;

        /// <summary>
        /// Constructor for the server controller.
        /// </summary>
        /// <param name="_settings"></param>
        public Server_Controller(ServerSettings _settings)
        {
            rng = new Random();
            powerUpID = 0;
            projID = 0;
            settings = _settings;
            theWorld = new World(settings.universeSize, settings.walls, rng.Next(settings.maxPowerUpDelay + 1), settings.maxPowerUps);
            clients = new Dictionary<long, SocketState>();
            clientCommands = new Dictionary<long, ControlCommand>();
        }

        /// <summary>
        /// Updates the world on every frame. Collisions, movements, object field updates, etc are handled implicitly inside this method. 
        /// 
        /// </summary>
        public void UpdateWorld()
        {

            serializedData = "";

            UpdateTanks();
            UpdateProjectiles();
            UpdatePowerUps();
            if(!settings.drunkTankMode)
                UpdateBeams();

            // Reset the client-command of each client
            foreach (SocketState client in clients.Values)
            {
                lock (clientCommands)
                {
                    clientCommands[client.ID] = null;
                }
            }
        }

        /// <summary>
        /// Helper method that encodes active beams once per frame. Alters tank fields on hit, i.e. any tank that is hit with a beam is destroyed and the score of the player that fired the beam is updated. 
        /// 
        /// </summary>
        private void UpdateBeams()
        {
            foreach (Beam beam in new HashSet<Beam>(theWorld.GetBeams().Values))
            {
                serializedData += JsonConvert.SerializeObject(beam) + '\n';

                foreach (Tank tank in theWorld.GetTanks().Values)
                {
                    if (BeamCollidesWithTank(beam.Location, beam.Orientation, tank.Location, settings.tankSize / 2))
                    {
                        tank.HitPoints = 0;
                        tank.Died = true;
                        theWorld.GetTanks()[beam.Owner].incrementScore();
                    }
                }

                theWorld.GetBeams().Remove(beam.Owner);
            }
        }

        /// <summary>
        /// Helper method that encodes active powerUps once per frame. A given power-up is removed if and only if a tank comes into contact with the power-up, such a collision results in the tank incrementing its power-up reserve. 
        /// 
        /// </summary>
        private void UpdatePowerUps()
        {
            foreach (PowerUp pow in new HashSet<PowerUp>(theWorld.GetPowerUps().Values))
            {

                if (PowCollidesWithTank(pow.Location, pow.PowerID, out Tank tank))
                {
                    tank.PowerUpCounts++;
                    pow.Died = true;
                }

                serializedData += JsonConvert.SerializeObject(pow) + '\n';
                if (pow.Died)
                    theWorld.GetPowerUps().Remove(pow.PowerID);
            }
            // Spawns a power-up at random at delays up to maxframes = maxPowerUps
            if (--theWorld.FramesPerPowerUp == 0 && theWorld.GetPowerUps().Count < settings.maxPowerUps)
            {
                spawnPowerUp(powerUpID++);
                theWorld.FramesPerPowerUp = rng.Next(settings.maxPowerUpDelay + 1);
            }
        }

        /// <summary>
        /// Helper method that encodes active projectiles and updates there position once per frame. Alters tank fields on hit, i.e. any tank that is hit with a projectile gets its health buffer decremented. 
        /// 
        /// </summary>
        private void UpdateProjectiles()
        {
            foreach (Projectile proj in new HashSet<Projectile>(theWorld.GetProjectiles().Values))
            {
                UpdateProjectilePosition(proj.ProjID, proj);
                if (CollidesWithWall(proj.Location, 0))
                    proj.Died = true;
                else if (ProjCollidesWithTank(proj.Location, proj.Owner, out Tank t))
                {
                    proj.Died = true;
                    t.DecrementHP();
                    if (t.HitPoints == 0)
                    {
                        theWorld.GetTanks()[proj.Owner].incrementScore();
                        t.Died = true;
                    }
                }
                else if (ExceedsWorldBorder(proj.Location))
                    proj.Died = true;

                serializedData += JsonConvert.SerializeObject(proj) + '\n';
                if (proj.Died)
                    theWorld.RemoveProjectile(proj);
            }
        }
        /// <summary>
        ///  Helper method that encodes active tanks and updates there position and orientation once per frame.  
        ///  
        /// </summary>
        private void UpdateTanks()
        {
            foreach (Tank tank in new HashSet<Tank>(theWorld.GetTanks().Values))
            {
                // Only transform tanks and accept commands from tanks when the tank is alive
                if (tank.HitPoints > 0)
                {
                    if (!tank.IsDrunk)
                        TransformTank(tank.TankID, tank);
                    else
                    {
                        DrunkTransform(tank.TankID, tank);
                        tank.DrunkFrames--;
                    }
                }
                if (tank.CanFire && tank.HitPoints > 0 && !tank.IsDrunk)
                    processFiringCommands(tank.TankID, tank);
                // Spawn 
                else if (tank.IsDrunk && tank.CanFire && tank.HitPoints > 0)
                {
                    drunkFire(tank.TankID, tank);
                }
                else
                {
                    // Resets the fire-rate of a tank as specified by the server settings
                    if (--tank.ShotFramesElapsed == 0)
                        tank.CanFire = true;
                }

                if(tank.IsDrunk)
                {
                    if (tank.DrunkFrames == 0)
                        tank.IsDrunk = false;
                }

                // If a tank died, start its respawn counter
                if (tank.Died)
                    tank.DeathFramesElapsed = settings.respawnRate;

                serializedData += JsonConvert.SerializeObject(tank) + '\n';
                // Disconnected tanks are removed from the world
                if (tank.Disconnected)
                    theWorld.GetTanks().Remove(tank.TankID);
                // decrement the respawn counter of dead tank, respawn the tank once the counter has expired
                if (tank.HitPoints <= 0)
                {
                    tank.Died = false;
                    if (--tank.DeathFramesElapsed <= 0)
                    {
                        tank.HitPoints = settings.hitPoints;
                        spawnTank(tank.TankID, tank.Name);
                    }
                }
            }
        }

        /// <summary>
        /// Method used for performing firing commands when a tank is "drunk."
        /// Drunk firing fires eight projectiles at 45 degree intervals.
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="tank"></param>
        private void drunkFire(int ID, Tank tank)
        {
            if (!clientCommands.ContainsKey(ID) || clientCommands[ID] == null)
                return;
            Vector2D orientation;
            switch (clientCommands[ID].Firing)
            {
                case "main":
                    for(int i = 0; i < 8; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                orientation = new Vector2D(0, -1);
                                break;
                            case 1:
                                orientation = new Vector2D(0, 1);
                                break;
                            case 2:
                                orientation = new Vector2D(1, 0);
                                break;
                            case 3:
                                orientation = new Vector2D(-1, 0);
                                break;
                            case 4:
                                orientation = new Vector2D(-.5, -0.5);
                                break;
                            case 5:
                                orientation = new Vector2D(.5, -0.5);
                                break;
                            case 6:
                                orientation = new Vector2D(0.5, .5);
                                break;
                            case 7:
                                orientation = new Vector2D(-0.5, .5);
                                break;
                            default:
                                orientation = new Vector2D(0, 1);
                                break;
                        }
                        orientation.Normalize();
                        theWorld.AddProjectile(new Projectile(projID++, tank.Location, orientation, (int)ID));
                    }
                    // Set a delay on fire commands from this client as specified in the server settings
                    tank.ShotFramesElapsed = settings.framesPerShot;
                    tank.CanFire = false;
                    break;
                case "alt":
                    tank.IsDrunk = true;
                    tank.DrunkFrames = settings.drunkFrames;
                    tank.PowerUpCounts--;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Helper method that updates the position of a projectile proj
        /// 
        /// </summary>
        /// <param name="projID"></param>
        /// <param name="proj"></param>
        private void UpdateProjectilePosition(long projID, Projectile proj)
        {
            Vector2D distanceTraveled = proj.Orientation * settings.projectileSpeed;
            proj.Location += distanceTraveled;
        }

        /// <summary>
        /// Helper method that processes firing commands from a given client. 
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="tank"></param>
        private void processFiringCommands(long ID, Tank tank)
        {
            if (!clientCommands.ContainsKey(ID) || clientCommands[ID] == null)
                return;

            Vector2D orientation = clientCommands[ID].Orientation;
            switch (clientCommands[ID].Firing)
            {
                case "main":
                    theWorld.AddProjectile(new Projectile((int)ID, tank.Location, orientation, (int)ID));
                    // Set a delay on fire commands from this client as specified in the server settings
                    tank.ShotFramesElapsed = settings.framesPerShot;
                    tank.CanFire = false;
                    break;
                case "alt":
                    // Beam is fired by this client if the client has power-ups in there tank's power-up reserves
                    if (tank.PowerUpCounts > 0) {
                        if (!settings.drunkTankMode)
                        {
                            theWorld.AddBeam(new Beam((int)ID, tank.Location, tank.Aim, (int)ID));
                            tank.PowerUpCounts--;
                        }
                        else
                        {
                            tank.IsDrunk = true;
                            tank.DrunkFrames = settings.drunkFrames;
                            tank.PowerUpCounts--;
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// The encoded world is sent once per frame and disconnected clients are removed from the world once per frame. 
        /// 
        /// </summary>
        public void SendWorld()
        {
            HashSet<long> disconnectedClients = new HashSet<long>();

            foreach (SocketState client in new HashSet<SocketState>(clients.Values))
            {
                if (client.OnNetworkAction == OnReceive && !Networking.Send(client.TheSocket, serializedData))
                {
                    disconnectedClients.Add(client.ID);
                }
            }

            foreach (long id in disconnectedClients)
            {
                lock (clients)
                {
                    clients.Remove(id);
                }

                lock (theWorld)
                {
                    theWorld.GetTanks().TryGetValue((int)id, out Tank tank);
                    tank.Disconnected = true;
                    tank.Died = true;
                    tank.HitPoints = 0;
                }

                ClientDisconnected(id);
            }
        }

        /// <summary>
        /// The on-network-action for the connection stage of the callback.
        /// </summary>
        /// <param name="client"></param>
        private void NewClientConnected(SocketState client)
        {
            if (client.ErrorOccurred)
            {
                return;
            }

            client.OnNetworkAction = InitialReceive;
            Networking.GetData(client);
        }

        /// <summary>
        /// On-network-action that is set once a client has initiated the handshake process
        /// 
        /// </summary>
        /// <param name="client"></param>
        private void InitialReceive(SocketState client)
        {
            if (client.ErrorOccurred)
            {
                return;
            }

            ProcessMessages(client);

            Networking.GetData(client);
        }
        /// <summary>
        /// On-network-action that is set once a client has finalized the handshake process
        /// </summary>
        /// <param name="client"></param>
        private void OnReceive(SocketState client)
        {
            if (client.ErrorOccurred)
            {
                return;
            }
            ProcessMessages(client);
            Networking.GetData(client);
        }

        /// <summary>
        /// Process any buffered messages separated by '\n'
        /// Then inform the view
        /// </summary>
        /// <param name="state"></param>
        private void ProcessMessages(SocketState client)
        {
            string totalData = client.GetData();
            string[] parts = Regex.Split(totalData, @"(?<=[\n])");

            // Loop until we have processed all messages.
            // We may have received more than one.

            List<string> newMessages = new List<string>();

            foreach (string p in parts)
            {
                // Ignore empty strings added by the regex splitter
                if (p.Length == 0)
                    continue;

                // The regex splitter will include the last string even if it doesn't end with a '\n',
                // So we need to ignore it if this happens. 
                if (p[p.Length - 1] != '\n')
                    break;

                // build a list of messages to send to the view
                newMessages.Add(p);

                // Then remove it from the SocketState's growable buffer
                client.RemoveData(0, p.Length);
            }
            if (client.OnNetworkAction == InitialReceive)
            {
                ProcessName(newMessages, client);
                return;
            }
            ProcessCommands(newMessages, client.ID);
        }

        /// <summary>
        /// Processes commands as they are received from client and adds them to a dictionary of commands that are associated to a given ID
        /// </summary>
        /// <param name="newMessages"></param>
        /// <param name="ID"></param>
        private void ProcessCommands(List<string> newMessages, long ID)
        {
            foreach (string message in newMessages)
            {
                JObject deseralizedObject = JObject.Parse(message);
                if (deseralizedObject["moving"] != null)
                {
                    ControlCommand newCommand = JsonConvert.DeserializeObject<ControlCommand>(message);
                    lock (clientCommands)
                    {
                        if (!clientCommands.ContainsKey(ID))
                            clientCommands.Add(ID, newCommand);
                        else
                            clientCommands[ID] = newCommand;
                    }
                }
            }
        }

        /// <summary>
        /// Updates the orientation, position, and turret orientation of a tank with the given ID.
        /// 
        /// </summary>
        /// <param name="ID"></param>
        public void TransformTank(long ID, Tank t)
        {
            if (!clientCommands.ContainsKey(ID) || clientCommands[ID] == null)
                return;

            Vector2D velocity;
            Vector2D orientation;
            string direction = clientCommands[ID].Moving;
            switch (direction)
            {
                case "right":
                    velocity = new Vector2D(1, 0) * settings.tankSpeed;
                    orientation = new Vector2D(1, 0);
                    break;
                case "left":
                    velocity = new Vector2D(-1, 0) * settings.tankSpeed;
                    orientation = new Vector2D(-1, 0);
                    break;
                case "up":
                    velocity = new Vector2D(0, -1) * settings.tankSpeed;
                    orientation = new Vector2D(0, -1);
                    break;
                case "down":
                    velocity = new Vector2D(0, 1) * settings.tankSpeed;
                    orientation = new Vector2D(0, 1);
                    break;
                default:
                    velocity = new Vector2D(0, 0);
                    orientation = t.Orientation;
                    break;
            }


            lock (theWorld)
            {
                Vector2D newLocation = t.Location + velocity;
                if (!CollidesWithWall(newLocation, settings.tankSize))
                {
                    // Update a tank normally if it does not cross the boundaries of the world; otherwise, reflect the tank across the corrosponding axis
                    if (!ExceedsWorldBorder(newLocation))
                    {
                        t.UpdateLocation(velocity);
                    }
                    else
                    {
                        Vector2D reflectedLocation;
                        switch (direction)
                        {
                            case "right":
                                reflectedLocation = new Vector2D(-t.Location.GetX(), t.Location.GetY());
                                break;
                            case "left":
                                reflectedLocation = new Vector2D(-t.Location.GetX(), t.Location.GetY());
                                break;
                            case "up":
                                reflectedLocation = new Vector2D(t.Location.GetX(), -t.Location.GetY());
                                break;
                            case "down":
                                reflectedLocation = new Vector2D(t.Location.GetX(), -t.Location.GetY());
                                break;
                            default:
                                reflectedLocation = t.Location;
                                break;
                        }

                        if (!CollidesWithWall(reflectedLocation, settings.tankSize))
                            t.Location = reflectedLocation;
                    }
                }
                t.UpdateOrientation(orientation);
                t.UpdateTurretOrientation(clientCommands[ID].Orientation);
            }
        }
        /// <summary>
        /// Inverts and speeds up the movement of a tank that has actived its power-up in "Drunk Mode" 
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="t"></param>
        public void DrunkTransform(long ID, Tank t)
        {
            if (!clientCommands.ContainsKey(ID) || clientCommands[ID] == null)
                return;

            Vector2D velocity;
            Vector2D orientation;
            string direction = clientCommands[ID].Moving;
            switch (direction)
            {
                case "right":
                    velocity = new Vector2D(-1, 0) * settings.tankSpeed * 3;
                    orientation = new Vector2D(1, 0);
                    break;
                case "left":
                    velocity = new Vector2D(1, 0) * settings.tankSpeed * 3;
                    orientation = new Vector2D(-1, 0);
                    break;
                case "up":
                    velocity = new Vector2D(0, 1) * settings.tankSpeed * 3;
                    orientation = new Vector2D(0, -1);
                    break;
                case "down":
                    velocity = new Vector2D(0, -1) * settings.tankSpeed * 3;
                    orientation = new Vector2D(0, 1);
                    break;
                default:
                    velocity = new Vector2D(0, 0);
                    orientation = t.Orientation;
                    break;
            }


            lock (theWorld)
            {
                Vector2D newLocation = t.Location + velocity;
                if (!CollidesWithWall(newLocation, settings.tankSize))
                {
                    if (!ExceedsWorldBorder(newLocation))
                    {
                        t.UpdateLocation(velocity);
                    }
                    else
                    {
                        Vector2D reflectedLocation;
                        switch (direction)
                        {
                            case "right":
                                reflectedLocation = new Vector2D(-t.Location.GetX(), t.Location.GetY());
                                break;
                            case "left":
                                reflectedLocation = new Vector2D(-t.Location.GetX(), t.Location.GetY());
                                break;
                            case "up":
                                reflectedLocation = new Vector2D(t.Location.GetX(), -t.Location.GetY());
                                break;
                            case "down":
                                reflectedLocation = new Vector2D(t.Location.GetX(), -t.Location.GetY());
                                break;
                            default:
                                reflectedLocation = t.Location;
                                break;
                        }

                        if (!CollidesWithWall(reflectedLocation, settings.tankSize))
                            t.Location = reflectedLocation;
                    }
                }
                t.UpdateOrientation(orientation);
                t.UpdateTurretOrientation(clientCommands[ID].Orientation);
            }
        }

        /// <summary>
        /// Method used to check whether or not the given location exceeds the world border.
        /// Used for removing projectiles and tank wrap-around.
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        private bool ExceedsWorldBorder(Vector2D location)
        {

            return (location.GetX() > theWorld.WorldSize / 2) || (location.GetY() > theWorld.WorldSize / 2) || 
                (location.GetX() < -(theWorld.WorldSize / 2)) || (location.GetY() < -(theWorld.WorldSize / 2));
        }

        /// <summary>
        /// Checks if an object is colliding with a wall, i.e. returns true if the object associated with the parameter vector lies in the area of occupied by a wall; otherwise, false is returned.
        /// </summary>
        /// <param name="objLocation"></param>
        /// <returns></returns>
        private bool CollidesWithWall(Vector2D objLocation, int objectSize)
        {
            foreach (Wall wall in theWorld.GetWalls().Values)
            {
                Vector2D wallCenter = new Vector2D((wall.Side1.GetX() + wall.Side2.GetX()) / 2, (wall.Side1.GetY() + wall.Side2.GetY()) / 2);
                int width = (wall.Side1.GetY() == wall.Side1.GetY()) ? (int)Math.Abs(wall.Side1.GetX() - wall.Side2.GetX()) + settings.wallSize : settings.wallSize;
                int height = (wall.Side1.GetX() == wall.Side1.GetX()) ? (int)Math.Abs(wall.Side1.GetY() - wall.Side2.GetY()) + settings.wallSize : settings.wallSize;

                int xUpperBound = (int)(wallCenter.GetX() + width / 2 + objectSize / 2);
                int yLowerBound = (int)(wallCenter.GetY() + height / 2 + objectSize / 2);
                int xLowerBound = (int)(wallCenter.GetX() - width / 2 - objectSize / 2);
                int yUpperBound = (int)(wallCenter.GetY() - height / 2 - objectSize / 2);

                bool xCollision = xUpperBound >= objLocation.GetX() && xLowerBound <= objLocation.GetX();
                bool yCollision = yUpperBound <= objLocation.GetY() && yLowerBound >= objLocation.GetY();

                if (xCollision && yCollision)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Helper method used to check for a projectile colliding with a tank that is not its owner.
        /// </summary>
        /// <param name="objLocation"></param>
        /// <param name="ownerID"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        private bool ProjCollidesWithTank(Vector2D objLocation, int ownerID, out Tank t)
        {
            bool CollisionDetected = CollidesWithTank(objLocation,ownerID,out t);
            return CollisionDetected && (ownerID != t.TankID);
        }

        /// <summary>
        /// Helper method used to check for a powerups collision with a tank.
        /// </summary>
        /// <param name="objLocation"></param>
        /// <param name="objectID"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        private bool PowCollidesWithTank(Vector2D objLocation, int objectID, out Tank t)
        {
            return CollidesWithTank(objLocation, objectID, out t);
        }

        /// <summary>
        /// Method used to check if a provided object location collides with any tank.
        /// </summary>
        /// <param name="objLocation"></param>
        /// <param name="objectID"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        private bool CollidesWithTank(Vector2D objLocation, int objectID, out Tank t)
        {
            foreach (Tank tank in theWorld.GetTanks().Values)
            {

                int xUpperBound = (int)(tank.Location.GetX() + settings.tankSize / 2);
                int yLowerBound = (int)(tank.Location.GetY() + settings.tankSize / 2);
                int xLowerBound = (int)(tank.Location.GetX() - settings.tankSize / 2);
                int yUpperBound = (int)(tank.Location.GetY() - settings.tankSize / 2);

                bool xCollision = xUpperBound >= objLocation.GetX() && xLowerBound <= objLocation.GetX();
                bool yCollision = yUpperBound <= objLocation.GetY() && yLowerBound >= objLocation.GetY();

                if (xCollision && yCollision)
                {
                    t = tank;
                    return true;
                }

            }

            t = null;
            return false;
        }

        /// <summary>
        /// Determines if a ray interescts a circle
        /// </summary>
        /// <param name="rayOrig">The origin of the ray</param>
        /// <param name="rayDir">The direction of the ray</param>
        /// <param name="center">The center of the circle</param>
        /// <param name="r">The radius of the circle</param>
        /// <returns></returns>
        public static bool BeamCollidesWithTank(Vector2D rayOrig, Vector2D rayDir, Vector2D center, double r)
        {
            // ray-circle intersection test
            // P: hit point
            // ray: P = O + tV
            // circle: (P-C)dot(P-C)-r^2 = 0
            // substituting to solve for t gives a quadratic equation:
            // a = VdotV
            // b = 2(O-C)dotV
            // c = (O-C)dot(O-C)-r^2
            // if the discriminant is negative, miss (no solution for P)
            // otherwise, if both roots are positive, hit

            double a = rayDir.Dot(rayDir);
            double b = ((rayOrig - center) * 2.0).Dot(rayDir);
            double c = (rayOrig - center).Dot(rayOrig - center) - r * r;

            // discriminant
            double disc = b * b - 4.0 * a * c;

            if (disc < 0.0)
                return false;

            // find the signs of the roots
            // technically we should also divide by 2a
            // but all we care about is the sign, not the magnitude
            double root1 = -b + Math.Sqrt(disc);
            double root2 = -b - Math.Sqrt(disc);

            return (root1 > 0.0 && root2 > 0.0);
        }

        /// <summary>
        /// Processes the name of the passed in client.
        /// </summary>
        /// <param name="newMessages"></param>
        /// <param name="client"></param>
        private void ProcessName(List<string> newMessages, SocketState client)
        {
            string playerName = newMessages[0].Substring(0, newMessages[0].Length - 1);
            spawnTank(client.ID, playerName);
            string InitialMessage = client.ID + "\n" + theWorld.WorldSize + "\n";

            lock (theWorld)
            {
                if (!clients.ContainsKey((int)client.ID))
                {
                    clients.Add((int)client.ID, client);
                    ClientConnected(client.ID);
                }
            }
            InitialMessage += SerializeWalls();
            //Console.WriteLine(InitialMessage);
            Networking.Send(client.TheSocket, InitialMessage);
            client.OnNetworkAction = OnReceive;
        }

        /// <summary>
        /// Helper method that adds tank to the world at a random location.
        /// 
        /// </summary>
        private void spawnTank(long clientID, string playerName)
        {
            TankWars.Vector2D newLocation = new TankWars.Vector2D(GetRandomCoordinate(), GetRandomCoordinate());
            Dictionary<int, Tank> tanks = theWorld.GetTanks();
            if (!CollidesWithWall(newLocation, settings.tankSize))
            {
                if (!tanks.ContainsKey((int)clientID))
                    theWorld.AddTank(new Tank((int)clientID, newLocation, new TankWars.Vector2D(0, -1), new TankWars.Vector2D(0, -1), playerName,settings.hitPoints));
                else
                {
                    Tank t;
                    tanks.TryGetValue((int)clientID, out t);
                    t.Location = newLocation;
                }
                return;
            }
            spawnTank(clientID, playerName);
        }

        /// <summary>
        /// Method used to spawn a powerup, checks whether the spawn point collides with a wall.
        /// Continues to creat new spawn points until a non-collision happens.
        /// </summary>
        /// <param name="powID"></param>
        private void spawnPowerUp(long powID)
        {
            Vector2D newLocation = new Vector2D(GetRandomCoordinate(), GetRandomCoordinate());
            if (!CollidesWithWall(newLocation, 0))
            {
                PowerUp p = new PowerUp((int)powID, newLocation);
                theWorld.AddPowerUp(p);
                return;
            }
            spawnPowerUp(powID);
        }

        /// <summary>
        /// Returns a random double that lies in the range of the universe.
        /// 
        /// </summary>
        /// <returns></returns>
        private double GetRandomCoordinate()
        {
            return Math.Pow(-1, rng.Next(2)) * rng.Next((settings.universeSize / 2) + 1);
        }

        /// <summary>
        /// Serializes the walls to a Json format as defined by the world.
        /// 
        /// </summary>
        /// <returns></returns>
        private string SerializeWalls()
        {
            string walls = "";
            foreach (Wall wall in theWorld.GetWalls().Values)
                walls += JsonConvert.SerializeObject(wall) + "\n";
            return walls;
        }

        /// <summary>
        /// Wrapper method for the Networking library's start-server method
        /// </summary>
        public void StartServer()
        {
            server = Networking.StartServer(NewClientConnected, port);
        }
    }
}

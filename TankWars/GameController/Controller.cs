// Authors: Ryan Garcia and Jordy Larrea
using Commands;
using Model;
using NetworkUtil;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using TankWars;

namespace GameController
{
    public class Controller
    {
        // Controller events that the view can subscribe to
        public delegate void MessageHandler(IEnumerable<string> messages);
        public event MessageHandler MessagesArrived;

        public delegate void ConnectedHandler();
        public event ConnectedHandler Connected;

        public delegate void BeamFiredHandler(Beam beam);
        public event BeamFiredHandler BeamFired;

        public delegate void OnDeathHandler(Vector2D location);
        public event OnDeathHandler OnDeath;

        public delegate void OnDisconnectHandler();
        public event OnDisconnectHandler OnDisconnect;

        // World is a simple container for Players and Powerups
        private World theWorld;

        //Used for tracking two movement keys inputed by the player
        private Queue<string> movements = new Queue<string>();

        private int worldSize;

        /// <summary>
        /// Property used for accessing theWorld.
        /// </summary>
        public World TheWorld
        {
            get
            {
                return theWorld;
            }
        }

        private bool AllWallsReceived = false; //Flag for when walls are fully received and normal JSON processing can begin.
        private string playerName;
        private int playerID;
        private string firing = "none";
        private Vector2D orientation = new Vector2D(0, 1); //Default tank orientation

        /// <summary>
        /// Property used for accessing this players name.
        /// </summary>
        public string PlayerName
        {
            get
            {
                return playerName;
            }
        }

        /// <summary>
        /// Property used for accessing this players ID.
        /// </summary>
        public int PlayerID
        {
            get
            {
                return playerID;
            }
        }

        //Handler and event for server updates
        public delegate void ServerUpdateHandler();
        public event ServerUpdateHandler UpdateArrived;

        /// <summary>
        /// State representing the connection with the server
        /// </summary>
        SocketState theServer = null;

        //Handler and event for errors
        public delegate void ErrorOccurredHandler(string errorMessage);
        public event ErrorOccurredHandler ErrorOccured;

        /// <summary>
        /// Begins the process of connecting to the server
        /// </summary>
        /// <param name="addr"></param>
        public void Connect(string _playerName, string addr)
        {
            Networking.ConnectToServer(OnConnect, addr, 11000);
            playerName = _playerName;
        }


        /// <summary>
        /// Method to be invoked by the networking library when a connection is made
        /// </summary>
        /// <param name="state"></param>
        private void OnConnect(SocketState state)
        {
            if (state.ErrorOccurred)
            {
                if (!state.TheSocket.Connected)
                    OnDisconnect();
                // inform the view
                else
                    ErrorOccured(state.ErrorMessage);
                return;
            }

            // inform the view
            Connected();

            theServer = state;
            Networking.Send(state.TheSocket, PlayerName + "\n"); // send the server this client's name

            // Start an event loop to receive messages from the server
            state.OnNetworkAction = InitialMessage;
            Networking.GetData(state);
        }

        /// <summary>
        /// Callback for Handshake. Handles servers intial data send. 
        /// </summary>
        /// <param name=""></param>
        private void InitialMessage(SocketState state)
        {
            if (state.ErrorOccurred)
            {
                if (!state.TheSocket.Connected)
                    OnDisconnect();
                // inform the view
                else
                    ErrorOccured(state.ErrorMessage);
                return;
            }
            ProcessMessages(state);

            state.OnNetworkAction = ReceiveMessage;
            Networking.GetData(state);
        }

        /// <summary>
        /// Method to be invoked by the networking library when 
        /// data is available.
        /// </summary>
        /// <param name="state"></param>
        private void ReceiveMessage(SocketState state)
        {
            if (state.ErrorOccurred)
            {
                if (!state.TheSocket.Connected)
                    OnDisconnect();
                // inform the view
                else
                    ErrorOccured(state.ErrorMessage);
                return;
            }

            //Inform the view
            UpdateArrived();
            ProcessMessages(state);

            ProcessInputs();
            if (firing == "alt") //If the alt firing key is held down, we do not want to continue sending beams to the server.
            {
                firing = "none";
            }

            // Continue the event loop
            // state.OnNetworkAction has not been changed, 
            // so this same method (ReceiveMessage) 
            // will be invoked when more data arrives
            try
            {
                Networking.GetData(state);
            }
            catch (Exception)
            {
                OnDisconnect();
            }
        }

        /// <summary>
        /// Process any buffered messages separated by '\n'
        /// Then inform the view
        /// </summary>
        /// <param name="state"></param>
        private void ProcessMessages(SocketState state)
        {
            string totalData = state.GetData();
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
                state.RemoveData(0, p.Length);
            }

            // inform the view
            if (state.OnNetworkAction == InitialMessage)
            {
                ProcessInitialMessage(newMessages);
                return;
            }
            if (!AllWallsReceived)
            {
                ProcessJsonOfWalls(newMessages);
                return;
            }
            ProcessJson(newMessages);
        }


        /// <summary>
        /// Method used for processing the intial handshake data.
        /// </summary>
        /// <param name="messages"></param>
        private void ProcessInitialMessage(List<string> messages)
        {
            if (Int32.TryParse(messages[0], out int result1))
                playerID = result1;
            if (Int32.TryParse(messages[1], out int result2))
                worldSize = result2;

            theWorld = new World(worldSize, playerID);

            if (messages.Count > 2)
                ProcessJsonOfWalls(messages.GetRange(2, messages.Count - 2));

            //Inform the view
            UpdateArrived();
        }

        /// <summary>
        /// Method used for processing wall JSON data from the server.
        /// </summary>
        /// <param name="walls"></param>
        private void ProcessJsonOfWalls(List<string> walls)
        {
            lock (theWorld)
            {
                //We need a way to track the point at which we encounter a non-wall.
                int index = 0;
                foreach (string serializedObject in walls)
                {
                    JObject deseralizedObjects = JObject.Parse(serializedObject);

                    if (deseralizedObjects["wall"] != null)
                        //If we encounter a wall we want to add it to the world and continue proccessing until a non-wall is encountered.
                        theWorld.AddWall(JsonConvert.DeserializeObject<Wall>(serializedObject));
                    else
                    {
                        //Update our flag since walls are all sent before any other JSON data.
                        AllWallsReceived = true;
                        theServer.OnNetworkAction = ReceiveMessage;
                        //Get the sublist of non-wall objects to be processed.
                        ProcessJson(walls.GetRange(index, walls.Count - index));
                        return;
                    }
                    index++;
                }
            }

        }

        /// <summary>
        /// Method used for processing general JSON data from the server.
        /// </summary>
        /// <param name="messages"></param>
        private void ProcessJson(List<string> messages)
        {

            lock (theWorld)
            {
                foreach (string serializedObject in messages)
                {
                    JObject deseralizedObjects = JObject.Parse(serializedObject);
                    if (deseralizedObjects["tank"] != null)
                    {
                        Tank tank = JsonConvert.DeserializeObject<Tank>(serializedObject);
                        theWorld.AddTank(tank);
                        //If a tank died, we want to inform the view and begin drawing the explosion.
                        if (tank.Died)
                            OnDeath(tank.Location);
                    }
                    else if (deseralizedObjects["proj"] != null)
                        theWorld.AddProjectile(JsonConvert.DeserializeObject<Projectile>(serializedObject));
                    else if (deseralizedObjects["beam"] != null)
                    {
                        Beam beam = JsonConvert.DeserializeObject<Beam>(serializedObject);
                        theWorld.AddBeam(beam);
                        //Inform the view that a beam is being fired and that we need to draw it.
                        BeamFired(beam);
                    }
                    else if (deseralizedObjects["power"] != null)
                        theWorld.AddPowerUp(JsonConvert.DeserializeObject<PowerUp>(serializedObject));

                }
            }

        }

        /// <summary>
        /// Closes the connection with the server
        /// </summary>
        public void Close()
        {
            theServer?.TheSocket.Close();
        }


        /// <summary>
        /// Method used to process and send the inputs received from the View to the server.
        /// </summary>
        public void ProcessInputs()
        {
            if (theServer != null)
                //If our movement queue has data we want to send the top of the queue as a movement command
                if (movements.Count > 0)
                {
                    Networking.Send(theServer.TheSocket, JsonConvert.SerializeObject(new ControlCommand(movements.Peek(), firing, orientation)) + "\n");
                }
                //Otherwise, send a command with movement as "none"
                else
                {
                    Networking.Send(theServer.TheSocket, JsonConvert.SerializeObject(new ControlCommand("none", firing, orientation)) + "\n");

                }
        }


        /// <summary>
        /// Method used to queue movement inputs received from the view.
        /// </summary>
        /// <param name="e"></param>
        public void MovementInputs(KeyEventArgs e)
        {
            string moving = GetStringRepresentationOfKey(e);
            //Check if the queue is empty.
            if (movements.Count > 0)
            {
                //If the queue is not full, the key pressed was a valid movement, and the current movement command is not the same then queue the new command.
                if (movements.Count < 2 && moving != "" && movements.Peek() != moving)
                    movements.Enqueue(moving);
            }
            //Otherwise, just check if the movement command is valid.
            else if (moving != "")
                movements.Enqueue(moving);
        }

        /// <summary>
        /// Method used to dequeue movement commands.
        /// </summary>
        /// <param name="e"></param>
        public void CancelMoveRequest(KeyEventArgs e)
        {
            string key = GetStringRepresentationOfKey(e);
            if (key == "")
                return;
            if (movements.Count > 0)
            {
                movements.Dequeue();
            }
        }

        /// <summary>
        /// Helper method used to discern if the key pressed was a valid movement command.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private string GetStringRepresentationOfKey(KeyEventArgs e)
        {
            if (e.KeyData == Keys.W)
                return "up";
            if (e.KeyData == Keys.A)
                return "left";
            if (e.KeyData == Keys.S)
                return "down";
            if (e.KeyData == Keys.D)
                return "right";
            return "";
        }

        /// <summary>
        /// Method used to cancel the firing state of a tank.
        /// </summary>
        public void CancelFireRequest()
        {
            firing = "none";
        }

        /// <summary>
        /// Method used to discern what mouse button was presed and appropriately assign the firing mode.
        /// </summary>
        /// <param name="e"></param>
        public void MouseClick(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                firing = "main";
            else if (e.Button == MouseButtons.Right)
            {
                firing = "alt";
            }
        }

        /// <summary>
        /// Method used to adjust and normalize the mouse input vector.
        /// </summary>
        /// <param name="e"></param>
        public void MouseMovement(MouseEventArgs e)
        {
            Vector2D newOrientation = new Vector2D(e.X - 450, e.Y - 450);
            newOrientation.Normalize();
            orientation = newOrientation;
        }
    }
}

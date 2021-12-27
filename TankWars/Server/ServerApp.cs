// Ryan Garcia and Jordy Larrea

using Model;
using ServerController;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using TankWars;

namespace Server
{
    class ServerApp
    {
        /// <summary>
        /// Main server loop begins and ends here, intiates file reading/loading.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Stopwatch watch = new Stopwatch();
            ServerSettings settings = new ServerSettings("..\\..\\..\\..\\Resources\\");

            Server_Controller serverController = new Server_Controller(settings);
            serverController.ClientConnected += clientConnectedHandler;
            serverController.ClientDisconnected += clientDisconnectedHandler;

            serverController.StartServer();
            watch.Start();

            Console.WriteLine("Server is running. Accepting clients " + "(Drunk Mode: " + settings.drunkTankMode + ").");
            // Busy loop for the server that updates the world once per frame
            while (true)
            {
                serverController.UpdateWorld();
                while (watch.ElapsedMilliseconds < settings.msPerFrame)
                {

                }
                watch.Restart();
                serverController.SendWorld();
            }
        }

        /// <summary>
        /// Helper method for printing disconnected client messages.
        /// </summary>
        /// <param name="clientID"></param>
        private static void clientDisconnectedHandler(long clientID)
        {
            Console.WriteLine("Client(" + clientID + ")" + " has disconnected");
        }

        /// <summary>
        /// Helper method for printing connected client messages.
        /// </summary>
        /// <param name="clientID"></param>
        private static void clientConnectedHandler(long clientID)
        {
            Console.WriteLine("Player(" + clientID + ")" + " has connected");
        }


    }
}

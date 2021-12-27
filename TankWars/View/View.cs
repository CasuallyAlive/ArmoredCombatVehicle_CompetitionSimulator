// Authors: Ryan Garcia and Jordy Larrea
using GameController;
using Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TankWars;

namespace View
{
    public partial class View : Form
    {
        // Controller object for this instance of the client.
        private Controller controller; 
        private World theWorld;
        private const int viewSize = 900;
        private const int menuHeight = 40;
        private DrawingPanel drawingPanel;

        //booleans used to check if a text box is empty.
        private bool serverTB = false;
        private bool playerTB = false;

        /// <summary>
        /// Constructor, Intializes view by adding a controller and its associated events, as well as the drawing panel and its associated events.
        /// </summary>
        public View()
        {
            InitializeComponent();
            controller = new Controller();
            controller.UpdateArrived += UpdateArrived;
            controller.ErrorOccured += ErrorOccurredHandler;
            controller.BeamFired += BeamFiredHandler;
            controller.OnDeath += OnDeathHandler;
            controller.OnDisconnect += OnDisconnectHandler;
            theWorld = controller.TheWorld;

            drawingPanel = new DrawingPanel(PlayerNameTB.Text);
            drawingPanel.Location = new Point(0, menuHeight);
            drawingPanel.Size = new Size(viewSize, viewSize);
            this.Controls.Add(drawingPanel);
            drawingPanel.MouseDown += HandleMouseDown;
            drawingPanel.MouseUp += HandleMouseUp;
            drawingPanel.MouseMove += HandleMouseMove;

            this.KeyDown += HandleKeyDown;
            this.KeyUp += HandleKeyUp;

            controller.Connected += OnConnect;

            ClientSize = new Size(viewSize, viewSize + menuHeight);
        }

        /// <summary>
        /// Displays control information within the view.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void controlsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("W: Move Up\nA: Move Left\nS: Move Down\nD: Move Right\nMouse: Aim\nLeft Click: Fire Projectile\nRight Click: Fire Beam", "Controls", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Displays 'about' information within the view.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("TankWars student solution\nArtwork by Jolie UK, Alex Smith, and Sinestasia(Explosions)\nGame design and Implementation by Ryan Garcia and Jordy Larrea\nCS 3500 Spring 2021, University of Utah", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Handles disconnects and displays a message box with a detailed message.
        /// </summary>
        private void OnDisconnectHandler()
        {
            Invoke(new MethodInvoker(() => ServerAddressTB.Enabled = false));
            Invoke(new MethodInvoker(() => ConnectButton.Enabled = false));
            MethodInvoker invalidator = new MethodInvoker(() => this.Invalidate(true));
            Invoke(new MethodInvoker(() => PlayerNameTB.Enabled = false));
            MessageBox.Show("Error Occurred!\n" + "Disconnected from server. Please restart client", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        
        /// <summary>
        /// Handles tank death and draws explosion animation.
        /// </summary>
        /// <param name="location"></param>
        private void OnDeathHandler(Vector2D location)
        {
            Invoke(new MethodInvoker(() => { drawingPanel.AddExplosionAnimation(new ExplosionAnimation(location)); }));
        }

        /// <summary>
        /// Handles beam firing and draws beams upon firing.
        /// </summary>
        /// <param name="beam"></param>
        private void BeamFiredHandler(Beam beam)
        {
            Invoke(new MethodInvoker(() => { drawingPanel.AddBeamAnimation(new BeamAnimation(beam.Location, beam.Orientation)); }));
        }

        /// <summary>
        /// Handles errors and displays a message box with a detailed message.
        /// </summary>
        /// <param name="errorMessage"></param>
        private void ErrorOccurredHandler(string errorMessage)
        {
            MessageBox.Show("Error Occurred!\n" + errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Invoke(new MethodInvoker(() => ServerAddressTB.Enabled = true)); //We want to disable the text boxes and the connect button when errors occur
            Invoke(new MethodInvoker(() => ConnectButton.Enabled = true));
            Invoke(new MethodInvoker(() => PlayerNameTB.Enabled = true));
            Invoke(new MethodInvoker(() => this.Invalidate(true)));
        }

        /// <summary>
        /// Handles when a key is released, sends a cancel movement request to the controller.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleKeyUp(object sender, KeyEventArgs e)
        {
            controller.CancelMoveRequest(e);
        }

        /// <summary>
        /// Handles when a key is pushed, sends a movement request to the controller.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleKeyDown(object sender, KeyEventArgs e)
        {
            controller.MovementInputs(e);
            e.SuppressKeyPress = true;
            e.Handled = true;
        }

        /// <summary>
        /// Handles when a mouse button is clicked, sends a mouse click to the controller.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleMouseDown(object sender, MouseEventArgs e)
        {
            controller.MouseClick(e);
        }

        /// <summary>
        /// Handles when a mouse button is released, sends a CancelFireRequest to the controller.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleMouseUp(object sender, MouseEventArgs e)
        {
            controller.CancelFireRequest();
        }

        /// <summary>
        /// Handles mouse movement on the drawingPanel, sends a MouseMovement to the controller.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleMouseMove(object sender, MouseEventArgs e)
        {
            controller.MouseMovement(e);
        }

        /// <summary>
        /// Handler for the OnConnect event from the controller. 
        /// Disables text boxes and connect button.
        /// </summary>
        private void OnConnect()
        {
            Invoke(new MethodInvoker(() => ServerAddressTB.Enabled = false));
            Invoke(new MethodInvoker(() => ConnectButton.Enabled = false));
            MethodInvoker invalidator = new MethodInvoker(() => this.Invalidate(true));
            Invoke(new MethodInvoker(() => PlayerNameTB.Enabled = false));
            this.Invoke(invalidator);
        }

        /// <summary>
        /// Handler for the UpdateArrived event from the controller.
        /// Calls for the world to be redrawn on every update.
        /// </summary>
        private void UpdateArrived()
        {
            if (theWorld == null)
                drawingPanel.TheWorld = controller.TheWorld;
            MethodInvoker invalidator = new MethodInvoker(() => this.Invalidate(true));
            try
            {
                this.Invoke(invalidator);
            }
            catch (Exception)
            {
                return;
            }
        }

        /// <summary>
        /// Invokes the controllers Connect method when the 'connect' button is pressed in the View.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConnectButton_Click(object sender, EventArgs e)
        {
            controller.Connect(PlayerNameTB.Text, ServerAddressTB.Text);
        }

        /// <summary>
        /// When the text is changed in the ServerAddressTB checks to make sure text is entered,
        /// otherwise disables connect button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ServerAddressTB_TextChanged(object sender, EventArgs e)
        {
            serverTB = ServerAddressTB.Text != "";
            playerTB = PlayerNameTB.Text != "";

            ConnectButton.Enabled = serverTB && playerTB;
        }

        /// <summary>
        /// When the text is changed in the PlayerNameTB checls to make sure text is entered,
        /// otherwise disables connect button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlayerNameTB_TextChanged(object sender, EventArgs e)
        {
            playerTB = PlayerNameTB.Text != "";
            serverTB = ServerAddressTB.Text != "";

            ConnectButton.Enabled = serverTB && playerTB;
        }

        /// <summary>
        /// Nested helper class for managing beam animations.
        /// </summary>
        public class BeamAnimation
        {
            private Vector2D orientation;
            private Vector2D location;
            private int numFrames = 0;
            
            /// <summary>
            /// Property for accessing numFrames.
            /// </summary>
            public int NumFrames
            {
                get
                {
                    return numFrames;
                }
            }

            /// <summary>
            /// Method used to increment the number of frames.
            /// </summary>
            public void IncrementFrames()
            {
                numFrames++;
            }

            /// <summary>
            /// Property for acessing the orientation of a beam animation.
            /// </summary>
            public Vector2D Orientation
            {
                get
                {
                    return orientation;
                }
            }

            /// <summary>
            /// Property for acessing the origin of a beam animation.
            /// </summary>
            public Vector2D Location
            {
                get
                {
                    return location;
                }
            }

            /// <summary>
            /// Constructor, intializes the beam animation fields.
            /// </summary>
            /// <param name="_location"></param>
            /// <param name="_orientation"></param>
            public BeamAnimation(Vector2D _location, Vector2D _orientation)
            {
                location = _location;
                orientation = _orientation;
            }
        }

        /// <summary>
        /// Nested helper class for Explosion Animations.
        /// </summary>
        public class ExplosionAnimation
        {
            private Vector2D location;
            /// <summary>
            /// used to track the number of elapsed frames for an animation.
            /// </summary>
            private int numFrames = 0;

            /// <summary>
            /// Constructor, intializes an explosions location.
            /// </summary>
            /// <param name="_location"></param>
            public ExplosionAnimation(Vector2D _location)
            {
                location = _location;
            }

            /// <summary>
            /// Property for accessing an explosions Location.
            /// </summary>
            public Vector2D Location
            {
                get
                {
                    return location;
                }
            }

            /// <summary>
            /// Property for accessing the number of elapsed frames for this explosion.
            /// </summary>
            public int NumFrames
            {
                get
                {
                    return numFrames;
                }
            }

            /// <summary>
            /// Method used to increment the number of frames.
            /// </summary>
            public void IncrementFrames()
            {
                numFrames++;
            }
        }
    }
}

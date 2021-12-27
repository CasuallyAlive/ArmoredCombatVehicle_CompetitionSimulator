// Authors: Ryan Garcia and Jordy Larrea
using Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TankWars;

namespace View
{
    class DrawingPanel : Panel
    {
        private HashSet<View.BeamAnimation> beamAnimations = new HashSet<View.BeamAnimation>();

        public void AddBeamAnimation(View.BeamAnimation beamAnimation) 
        {
            beamAnimations.Add(beamAnimation);
        }

        private HashSet<View.ExplosionAnimation> explosionAnimations = new HashSet<View.ExplosionAnimation>();

        public void AddExplosionAnimation(View.ExplosionAnimation explosionAnimation)
        {
            explosionAnimations.Add(explosionAnimation);
        }

        private Image theBackground;

        private Dictionary<int, Image> tankImages;

        private Dictionary<int, Image> turretImages;

        private Dictionary<int, Image> shotImages;

        private List<Image> ExplosionFrames = new List<Image>();

        private Image wallSprite;

        private World theWorld;
        private string filePath = "..\\..\\..\\Resources\\Images\\";
        private string playerName;

        /// <summary>
        /// Constructor, intializes the drawing panel fields and loads images to be drawn.
        /// </summary>
        /// <param name="_playerName"></param>
        public DrawingPanel(string _playerName)
        {
            playerName = _playerName;

            DoubleBuffered = true;

            theBackground = LoadImage("Background.png");
            wallSprite = LoadImage("WallSprite.png");

            tankImages = new Dictionary<int, Image>();
            turretImages = new Dictionary<int, Image>();
            shotImages = new Dictionary<int, Image>();

            LoadTankImages();
            LoadTurretImages();
            LoadShotImages();
            LoadExplosionImages();

            this.BackColor = Color.Black;
        }

        /// <summary>
        /// Property used to access the world.
        /// </summary>
        public World TheWorld
        {
            set
            {
                theWorld = value;
            }
        }

        /// <summary>
        /// Helper method used to load an image with a passed file name.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private Image LoadImage(string filename)
        {
            try
            {
                return Image.FromFile(filePath + filename);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Loads different colored tank images.
        /// </summary>
        private void LoadTankImages()
        {
            //Since we can reuse colors after 8 players join we only need the first 8 valid tank IDs of 0-7
            tankImages.Add(0, LoadImage("BlueTank.png"));
            tankImages.Add(1, LoadImage("DarkTank.png"));
            tankImages.Add(2, LoadImage("GreenTank.png"));
            tankImages.Add(3, LoadImage("LightGreenTank.png"));
            tankImages.Add(4, LoadImage("OrangeTank.png"));
            tankImages.Add(5, LoadImage("PurpleTank.png"));
            tankImages.Add(6, LoadImage("RedTank.png"));
            tankImages.Add(7, LoadImage("YellowTank.png"));
        }


        /// <summary>
        /// Loads the images of the different colored turrets for each tank.
        /// </summary>
        private void LoadTurretImages()
        {
            turretImages.Add(0, LoadImage("BlueTurret.png"));
            turretImages.Add(1, LoadImage("DarkTurret.png"));
            turretImages.Add(2, LoadImage("GreenTurret.png"));
            turretImages.Add(3, LoadImage("LightGreenTurret.png"));
            turretImages.Add(4, LoadImage("OrangeTurret.png"));
            turretImages.Add(5, LoadImage("PurpleTurret.png"));
            turretImages.Add(6, LoadImage("RedTurret.png"));
            turretImages.Add(7, LoadImage("YellowTurret.png"));
        }

        /// <summary>
        /// Loads the images of the different colored shots for each tank.
        /// </summary>
        private void LoadShotImages()
        {
            shotImages.Add(0, LoadImage("shot-blue.png"));
            shotImages.Add(1, LoadImage("shot-grey.png"));
            shotImages.Add(2, LoadImage("shot-green.png"));
            shotImages.Add(3, LoadImage("shot-brown.png"));
            shotImages.Add(4, LoadImage("shot-white.png"));
            shotImages.Add(5, LoadImage("shot-violet.png"));
            shotImages.Add(6, LoadImage("shot-red.png"));
            shotImages.Add(7, LoadImage("shot-yellow.png"));
        }

        /// <summary>
        /// Loads the images of the explosion frames.
        /// </summary>
        private void LoadExplosionImages()
        {
            for (int i = 1; i <= 38; i++)
            {
                ExplosionFrames.Add(LoadImage("Frame" + i + ".png"));
            }
        }

        // A delegate for DrawObjectWithTransform
        // Methods matching this delegate can draw whatever they want using e  
        public delegate void ObjectDrawer(object o, PaintEventArgs e);


        /// <summary>
        /// This method performs a translation and rotation to drawn an object in the world.
        /// </summary>
        /// <param name="e">PaintEventArgs to access the graphics (for drawing)</param>
        /// <param name="o">The object to draw</param>
        /// <param name="worldX">The X coordinate of the object in world space</param>
        /// <param name="worldY">The Y coordinate of the object in world space</param>
        /// <param name="angle">The orientation of the objec, measured in degrees clockwise from "up"</param>
        /// <param name="drawer">The drawer delegate. After the transformation is applied, the delegate is invoked to draw whatever it wants</param>
        private void DrawObjectWithTransform(PaintEventArgs e, object o, double worldX, double worldY, double angle, ObjectDrawer drawer)
        {
            // "push" the current transform
            System.Drawing.Drawing2D.Matrix oldMatrix = e.Graphics.Transform.Clone();

            e.Graphics.TranslateTransform((int)worldX, (int)worldY);
            e.Graphics.RotateTransform((float)angle);
            drawer(o, e);

            // "pop" the transform
            e.Graphics.Transform = oldMatrix;
        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// After performing the necessary transformation (translate/rotate)
        /// DrawObjectWithTransform will invoke this method
        /// 
        /// Used to draw tanks. Tanks are drawn with a corresponding image to that tanks ID.
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void TankDrawer(object o, PaintEventArgs e)
        {
            Tank p = o as Tank;

            int width = 60;
            int height = 60;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Rectangles are drawn starting from the top-left corner.
            // So if we want the rectangle centered on the player's location, we have to offset it
            // by half its size to the left (-width/2) and up (-height/2)
            Rectangle r = new Rectangle(-(width / 2), -(height / 2), width, height);

            if (tankImages.TryGetValue(p.TankID % 8, out Image tankImage))
            {
                e.Graphics.DrawImage(tankImage, r);
            }
            
        }


        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// After performing the necessary transformation (translate/rotate)
        /// DrawObjectWithTransform will invoke this method
        /// 
        /// Used to draw tank names. Names are slightly below a tank.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void NameDrawer(object o, PaintEventArgs e)
        {
            Tank t = o as Tank;
            
            string drawString = t.Name + ": " + t.Score.ToString();
            int x = -32;
            int y = 30;

            using (System.Drawing.Font drawFont = new System.Drawing.Font("Arial", 10))
            using (System.Drawing.SolidBrush drawBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black))
            using (System.Drawing.StringFormat drawFormat = new System.Drawing.StringFormat())
            {
                e.Graphics.DrawString(drawString, drawFont, drawBrush, new Point(x, y));
            }
        }


        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// After performing the necessary transformation (translate/rotate)
        /// DrawObjectWithTransform will invoke this method
        /// 
        /// Used to draw tank health bars. Health bars a slightly above the tank and are redrawn when a tanks HP changes.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void HealthDrawer(object o, PaintEventArgs e)
        {
            Tank t = o as Tank;

            int x = -25;
            int y = -55;

            using(System.Drawing.SolidBrush greenBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Green))
            using (System.Drawing.SolidBrush orangeBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Orange))
            using (System.Drawing.SolidBrush redBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Red))
            using (System.Drawing.Pen blackPen = new System.Drawing.Pen(System.Drawing.Color.Black))
            {
                switch (t.HitPoints)
                {
                    case 3:
                        Rectangle fullHealth = new Rectangle(x, y, 48, 10);
                        e.Graphics.DrawRectangle(blackPen, fullHealth);
                        e.Graphics.FillRectangle(greenBrush, fullHealth);
                        break;
                    case 2:
                        Rectangle midHealth = new Rectangle(x, y, 32, 10);
                        e.Graphics.DrawRectangle(blackPen, midHealth);
                        e.Graphics.FillRectangle(orangeBrush, midHealth);
                        break;
                    case 1:
                        Rectangle lowHealth = new Rectangle(x, y, 16, 10);
                        e.Graphics.DrawRectangle(blackPen, lowHealth);
                        e.Graphics.FillRectangle(redBrush, lowHealth);
                        break;
                }
            }
        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// After performing the necessary transformation (translate/rotate)
        /// DrawObjectWithTransform will invoke this method
        /// 
        /// Used to draw turrets. Draws turrets centered on the given tanks location. Redraws the turret with a new orientation.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void TurretDrawer(object o, PaintEventArgs e)
        {
            Tank t = o as Tank;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            Rectangle r = new Rectangle(-25, -25, 50, 50);

            if (turretImages.TryGetValue(t.TankID % 8, out Image turretImage))
                e.Graphics.DrawImage(turretImage, r);
        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// After performing the necessary transformation (translate/rotate)
        /// DrawObjectWithTransform will invoke this method
        /// 
        /// Used to draw walls. Draws walls by utilizing their enpoints to create a length of wall.
        /// That length is then painted over with a TextureBrush of the given wall image and then outlined in black.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void WallDrawer(object o, PaintEventArgs e)
        {
            Wall w = o as Wall;

            int width = (w.Side1.GetX() == w.Side2.GetX()) ? 50 : (int)Math.Abs(w.Side1.GetX() - w.Side2.GetX()) + 50;
            int height = (w.Side1.GetY() == w.Side2.GetY()) ? 50 : (int)Math.Abs(w.Side1.GetY() - w.Side2.GetY()) + 50;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            TextureBrush tBrush = new TextureBrush(wallSprite);

            Rectangle r = new Rectangle(-(width / 2), -(height / 2), width, height);

            using (System.Drawing.Pen blackPen = new System.Drawing.Pen(System.Drawing.Color.Black))
            {
                e.Graphics.FillRectangle(tBrush, r);
                e.Graphics.DrawRectangle(blackPen, r);
            }
        }


        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// After performing the necessary transformation (translate/rotate)
        /// DrawObjectWithTransform will invoke this method
        /// 
        /// Used to draw the world background.
        /// </summary>
        /// <param name="e"></param>
        private void BackgroundDrawer(PaintEventArgs e)
        {

            int width = theWorld.WorldSize;
            int height = theWorld.WorldSize;

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (System.Drawing.SolidBrush blueBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Blue))
            {
                // Rectangles are drawn starting from the top-left corner.
                // So if we want the rectangle centered, we have to offset it
                // by half its size to the left (-width/2) and up (-height/2)
                Rectangle r = new Rectangle(-(width / 2), -(height / 2), width, height);

                e.Graphics.DrawImage(theBackground, r);
            }
        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// After performing the necessary transformation (translate/rotate)
        /// DrawObjectWithTransform will invoke this method
        /// 
        /// Used to draw projectiles. Will draw a projectile each frame until it reaches a tank, wall, or end of the world.
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void ProjectileDrawer(object o, PaintEventArgs e)
        {
            Projectile proj = o as Projectile;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            Rectangle r = new Rectangle(-15, -15, 30, 30);

            if (shotImages.TryGetValue(proj.Owner % 8, out Image shotImage))
                e.Graphics.DrawImage(shotImage, r);
        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// After performing the necessary transformation (translate/rotate)
        /// DrawObjectWithTransform will invoke this method
        /// 
        /// Used to draw beam animations. Will draw a beam of intial width of 8 that then shrinks.
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void BeamDrawer(object o, PaintEventArgs e)
        {
            View.BeamAnimation beam = o as View.BeamAnimation;

            int width = 8;
            int height = 8;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (Pen pen = new Pen(Color.White, 20 - beam.NumFrames))
            {
                Rectangle r = new Rectangle(-(width / 2), -(height / 2), width, height);
                e.Graphics.DrawLine(pen, new Point(0,0), new Point(0, -theWorld.WorldSize));
                
            }
            beam.IncrementFrames();
        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// After performing the necessary transformation (translate/rotate)
        /// DrawObjectWithTransform will invoke this method
        /// 
        /// Used to draw explosion animations. Will draw the explosion frames upon a tanks death for ~37 frames.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void ExplosionAnimationDrawer(object o, PaintEventArgs e)
        {
            View.ExplosionAnimation explosionAnimation = o as View.ExplosionAnimation;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            Rectangle r = new Rectangle(-128, -128, 256, 256);
            e.Graphics.DrawImage(ExplosionFrames[explosionAnimation.NumFrames], r);
            explosionAnimation.IncrementFrames();
        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// After performing the necessary transformation (translate/rotate)
        /// DrawObjectWithTransform will invoke this method
        /// 
        /// Used to draw power ups. Will draw the power up at a specified location by the server.
        /// Power ups are drawn as red circles.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void PowerUpDrawer(object o, PaintEventArgs e)
        {
            PowerUp p = o as PowerUp;

            int width = 8;
            int height = 8;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (System.Drawing.SolidBrush redBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Red))
            using (System.Drawing.SolidBrush yellowBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Yellow))
            using (System.Drawing.SolidBrush blackBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black))
            {
                // Circles are drawn starting from the top-left corner.
                // So if we want the circle centered on the powerup's location, we have to offset it
                // by half its size to the left (-width/2) and up (-height/2)
                Rectangle r = new Rectangle(-(width / 2), -(height / 2), width, height);

                e.Graphics.FillEllipse(redBrush, r);
            }
        }


        /// <summary>
        /// This method is invoked when the DrawingPanel needs to be re-drawn.
        /// Draws everything in theWorld in this order: Background, walls, tanks, powerups, projectiles, beams, and explosions.
        /// If a tank is at 0 hitpoints, a powerup or projectile has "died", or a beam and/or explosion has exceeded the number of drawing frames,
        /// this method will not draw them.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            // Center the view on the middle of the world,
            // since the image and world use different coordinate systems
            int viewSize = Size.Width; // view is square, so we can just use width

            if (theWorld != null)
            {
                if (theWorld.GetTanks().TryGetValue(theWorld.ClientID, out Tank t))
                {
                    e.Graphics.TranslateTransform((float)-t.Location.GetX() + viewSize / 2, (float)-t.Location.GetY() + viewSize / 2);
                }
                lock (theWorld)
                {
                    //Draw the background
                    BackgroundDrawer(e);
                    
                    //Draw the walls
                    foreach (Wall wall in theWorld.GetWalls().Values)
                    {
                        if (wall.Side1.GetX() == wall.Side2.GetX())
                            DrawObjectWithTransform(e, wall, wall.Side1.GetX(), (wall.Side2.GetY() + wall.Side1.GetY()) / 2.0, 0, WallDrawer);
                        else if (wall.Side1.GetY() == wall.Side2.GetY())
                            DrawObjectWithTransform(e, wall, (wall.Side2.GetX() + wall.Side1.GetX()) / 2.0, wall.Side1.GetY(), 0, WallDrawer);
                    }

                    // Draw the tanks
                    foreach (Tank tank in theWorld.GetTanks().Values)
                    {
                        if ((tank.HitPoints != 0) || tank.Disconnected)
                        {
                            DrawObjectWithTransform(e, tank, tank.Location.GetX(), tank.Location.GetY(), tank.Orientation.ToAngle(), TankDrawer);
                            DrawObjectWithTransform(e, tank, tank.Location.GetX(), tank.Location.GetY(), tank.Aim.ToAngle(), TurretDrawer);
                            DrawObjectWithTransform(e, tank, tank.Location.GetX(), tank.Location.GetY(), 0, NameDrawer);
                            DrawObjectWithTransform(e, tank, tank.Location.GetX(), tank.Location.GetY(), 0, HealthDrawer);
                        }
                    }

                    // Draw the powerups
                    foreach (PowerUp pow in theWorld.GetPowerUps().Values)
                    {
                        if (!pow.Died)
                        {
                            DrawObjectWithTransform(e, pow, pow.Location.GetX(), pow.Location.GetY(), 0, PowerUpDrawer);
                        }
                    }

                    // Draw the projectiles
                    foreach (Projectile proj in theWorld.GetProjectiles().Values)
                    {
                        if (!proj.Died)
                            DrawObjectWithTransform(e, proj, proj.Location.GetX(), proj.Location.GetY(), proj.Orientation.ToAngle(), ProjectileDrawer);
                    }

                    //Draw the beams
                    foreach(View.BeamAnimation beamAnimation in new HashSet<View.BeamAnimation>(beamAnimations))
                    {
                        DrawObjectWithTransform(e, beamAnimation, beamAnimation.Location.GetX(), beamAnimation.Location.GetY(), beamAnimation.Orientation.ToAngle(), BeamDrawer);
                        if (beamAnimation.NumFrames > 30)
                            beamAnimations.Remove(beamAnimation);
                            
                    }

                    //Draw the explosions
                    foreach (View.ExplosionAnimation explosionAnimation in new HashSet<View.ExplosionAnimation>(explosionAnimations))
                    {
                        DrawObjectWithTransform(e, explosionAnimation, explosionAnimation.Location.GetX(), explosionAnimation.Location.GetY(), 0, ExplosionAnimationDrawer);
                        if (explosionAnimation.NumFrames >= 37)
                            explosionAnimations.Remove(explosionAnimation);

                    }
                }
            }
            // Do anything that Panel (from which we inherit) needs to do
            base.OnPaint(e);
        }
    }
}


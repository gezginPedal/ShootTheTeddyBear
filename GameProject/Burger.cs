using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameProject
{
    /// <summary>
    /// A burger
    /// </summary>
    public class Burger
    {
        #region Fields

        // graphic and drawing info
        Texture2D sprite;
        Rectangle drawRectangle;

        // burger stats
        int health = 100;

        // shooting support
        bool canShoot = true;
        int elapsedCooldownMilliseconds = 0;

        // sound effect
        SoundEffect shootSound;

        #endregion

        #region Constructors

        /// <summary>
        ///  Constructs a burger
        /// </summary>
        /// <param name="contentManager">the content manager for loading content</param>
        /// <param name="spriteName">the sprite name</param>
        /// <param name="x">the x location of the center of the burger</param>
        /// <param name="y">the y location of the center of the burger</param>
        /// <param name="shootSound">the sound the burger plays when shooting</param>
        public Burger(ContentManager contentManager, string spriteName, int x, int y,
            SoundEffect shootSound)
        {
            LoadContent(contentManager, spriteName, x, y);
            this.shootSound = shootSound;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the collision rectangle for the burger
        /// </summary>
        public Rectangle CollisionRectangle
        {
            get { return drawRectangle; }

        }
        /// <summary>
        /// step 3 1/2
        /// health must upper zero always
        /// </summary>
        public int Health
        {
            get { return health; }
            set { health = value; }
        }

        #endregion


        #region Update Method Keyboard
        /// <summary>
        /// Updates the burger's location based on keyboard. Also fires 
        /// french fries as appropriate
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="keyboard"></param>
        public void Update(GameTime gameTime, KeyboardState keyboard)
        {
            
            // burger should only respond to input if it still has health
            if (health > 0)
            {
                //project increment 5 step 3
                //burger to up
                if (keyboard.IsKeyDown(Keys.W) || keyboard.IsKeyDown(Keys.Up))
                {
                    drawRectangle.Y -= GameConstants.BurgerMovementAmount;
                }
                //to down
                if (keyboard.IsKeyDown(Keys.S) || keyboard.IsKeyDown(Keys.Down))
                {
                    drawRectangle.Y += GameConstants.BurgerMovementAmount;
                }
                // to right
                if (keyboard.IsKeyDown(Keys.D) ||
                    keyboard.IsKeyDown(Keys.Right))
                {
                    drawRectangle.X += GameConstants.BurgerMovementAmount;
                }
                // to left
                if (keyboard.IsKeyDown(Keys.A) ||
                    keyboard.IsKeyDown(Keys.Left))
                {
                    drawRectangle.X -= GameConstants.BurgerMovementAmount;
                }

                

                // clamp burger in window
                if (drawRectangle.Left < 0)
                {
                    drawRectangle.X = 0;
                }
                if (drawRectangle.Right > GameConstants.WindowWidth)
                {
                    drawRectangle.X = GameConstants.WindowWidth - drawRectangle.Width;
                }
                if (drawRectangle.Top < 0)
                {
                    drawRectangle.Y = 0;
                }
                if (drawRectangle.Bottom > GameConstants.WindowHeight)
                {
                    drawRectangle.Y = GameConstants.WindowHeight - drawRectangle.Height;
                }
            }

            //update shooting allowed
            //timer concept(for animations) introduced in Chapter 7
            //project increment 5 step 3
            if (!canShoot)
            {
                elapsedCooldownMilliseconds += gameTime.ElapsedGameTime.Milliseconds;
                if (elapsedCooldownMilliseconds >= GameConstants.BurgerTotalCooldownMilliseconds ||
                    !keyboard.IsKeyDown(Keys.Space))
                {
                    canShoot = true;
                    elapsedCooldownMilliseconds = 0;
                    
                }
            }

            // shoot if appropriate
            //project increment 5 step 3
            if (health > 0 &&
                keyboard.IsKeyDown(Keys.Space) &&
                canShoot)
            {
                canShoot = false;
                Projectile projectile = new Projectile(ProjectileType.FrenchFries,
                    Game1.GetProjectileSprite(ProjectileType.FrenchFries),
                    drawRectangle.Center.X,
                    drawRectangle.Center.Y - GameConstants.FrenchFriesProjectileOffset,
                    -GameConstants.FrenchFriesProjectileSpeed);
                Game1.AddProjectile(projectile);

                //project increment 5 step 4 (3/5) add shot sound
                shootSound.Play();
            }

        }
        #endregion

        #region Draw
        /// <summary>
        /// Draws the burger
        /// </summary>
        /// <param name="spriteBatch">the sprite batch to use</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(sprite, drawRectangle, Color.White);
        }

#endregion



        #region Private methods

        /// <summary>
        /// Loads the content for the burger
        /// </summary>
        /// <param name="contentManager">the content manager to use</param>
        /// <param name="spriteName">the name of the sprite for the burger</param>
        /// <param name="x">the x location of the center of the burger</param>
        /// <param name="y">the y location of the center of the burger</param>
        private void LoadContent(ContentManager contentManager, string spriteName,
            int x, int y)
        {
            // load content and set remainder of draw rectangle
            sprite = contentManager.Load<Texture2D>(spriteName);
            drawRectangle = new Rectangle(x - sprite.Width / 2,
                y - sprite.Height / 2, sprite.Width,
                sprite.Height);
        }

        #endregion
    }
}

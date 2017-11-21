using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameProject
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // game objects. Using inheritance would make this
        // easier, but inheritance isn't a GDD 1200 topic
        Burger burger;
        List<TeddyBear> bears = new List<TeddyBear>();
        static List<Projectile> projectiles = new List<Projectile>();
        List<Explosion> explosions = new List<Explosion>();

        // projectile and explosion sprites. Saved so they don't have to
        // be loaded every time projectiles or explosions are created
        static Texture2D frenchFriesSprite;
        static Texture2D teddyBearProjectileSprite;
        static Texture2D explosionSpriteStrip;

        // scoring support
        int score = 0;
        string scoreString = GameConstants.ScorePrefix + 0;

        // health support
        string healthString = GameConstants.HealthPrefix +
            GameConstants.BurgerInitialHealth;
        bool burgerDead = false;

        // text display support
        SpriteFont font;

        // sound effects
        SoundEffect burgerDamage;
        SoundEffect burgerDeath;
        SoundEffect burgerShot;
        SoundEffect explosion;
        SoundEffect teddyBounce;
        SoundEffect teddyShot;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // set resolution
            graphics.PreferredBackBufferWidth = GameConstants.WindowWidth;
            graphics.PreferredBackBufferHeight = GameConstants.WindowHeight;
            IsMouseVisible = true;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            RandomNumberGenerator.Initialize();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // load audio content
            //project increment 5 step 4 (1/5)
            burgerDamage = Content.Load<SoundEffect>(@"audio\BurgerDamage");
            burgerDeath = Content.Load<SoundEffect>(@"audio\BurgerDeath");
            burgerShot = Content.Load<SoundEffect>(@"audio\BurgerShot");
            explosion = Content.Load<SoundEffect>(@"audio\Explosion");
            teddyBounce = Content.Load<SoundEffect>(@"audio\TeddyBounce");
            teddyShot = Content.Load<SoundEffect>(@"audio\TeddyShot");

            // load sprite font
            //project increment 5 step 1 (1/4)
            font = Content.Load<SpriteFont>(@"fonts\Arial20");

            // load projectile and explosion sprites
            teddyBearProjectileSprite = Content.Load<Texture2D>(@"graphics\teddybearprojectile");
            frenchFriesSprite = Content.Load<Texture2D>(@"graphics\frenchfries");
            explosionSpriteStrip = Content.Load<Texture2D>(@"graphics\explosion");

            // add initial game objects
            burger = new Burger(Content, @"graphics\burger",
                graphics.PreferredBackBufferWidth / 2,
                graphics.PreferredBackBufferHeight - graphics.PreferredBackBufferHeight / 8,
                //project increment 5 step 4 (2/5) change null to burgerShot
                burgerShot);

            for (int i = 0; i < GameConstants.MaxBears; i++)
            {
                SpawnBear();
            }

            // set initial health and score strings
            //project increment 5 step 1 (2/4)
            healthString = GameConstants.HealthPrefix + burger.Health;

            //project increment 5 step 2 (1/3)
            scoreString = GameConstants.ScorePrefix;

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            //project increment 5 step 3
            // get keyboard state for movement   
            KeyboardState keyboard = Keyboard.GetState();
            burger.Update(gameTime, keyboard);
            
            // update other game objects
            foreach (TeddyBear bear in bears)
            {
                bear.Update(gameTime);
            }
            foreach (Projectile projectile in projectiles)
            {
                projectile.Update(gameTime);
            }
            foreach (Explosion explosion in explosions)
            {
                explosion.Update(gameTime);
            }

            // check and resolve collisions between teddy bears
            for (int i = 0; i < bears.Count; i++)
            {
                for (int j = i + 1; j < bears.Count; j++)
                {
                    if (bears[i].Active &&
                        bears[j].Active)
                    {
                        CollisionResolutionInfo cri = CollisionUtils.CheckCollision(gameTime.ElapsedGameTime.Milliseconds,
                            GameConstants.WindowWidth, GameConstants.WindowHeight,
                            bears[i].Velocity, bears[i].DrawRectangle,
                            bears[j].Velocity, bears[j].DrawRectangle);
                        if (cri != null)
                        {
                            // resolve collision
                            if (cri.FirstOutOfBounds)
                            {
                                bears[i].Active = false;
                            }
                            else
                            {
                                bears[i].Velocity = cri.FirstVelocity;
                                bears[i].DrawRectangle = cri.FirstDrawRectangle;
                            }
                            if (cri.SecondOutOfBounds)
                            {
                                bears[j].Active = false;
                            }
                            else
                            {
                                bears[j].Velocity = cri.SecondVelocity;
                                bears[j].DrawRectangle = cri.SecondDrawRectangle;
                                
                                //project increment 5 step 5 (4/4)
                                teddyBounce.Play();
                            }
                        }
                    }
                }
            }

            // check and resolve collisions between burger and teddy bears
            //step 3 2/2
             foreach(TeddyBear tedBear in bears)
            {
                if (tedBear.Active &&
                    tedBear.CollisionRectangle.Intersects(burger.CollisionRectangle))
                {
                    //if burger intersect with teddy bear some time finally burger will stop because burger's health dead
                    burger.Health = burger.Health - GameConstants.BearDamage;
                    tedBear.Active = false;
                    //project increment 5 step 7 (3/3)
                    //add new explosion sound to last argument
                    explosions.Add(new Explosion(explosionSpriteStrip, tedBear.Location.X,
                            tedBear.Location.Y, explosion));
                    //project increment 5 step 1 (3/4)
                    healthString = GameConstants.HealthPrefix + burger.Health;

                    //project increment 5 step 6 (1/1)
                    burgerDamage.Play();
                    
                }
            }


            // check and resolve collisions between burger and projectiles
            //step 4
            
            foreach(Projectile projectile in projectiles)
            {
                if(projectile.Type == ProjectileType.TeddyBear &&
                    projectile.Active &&
                    projectile.CollisionRectangle.Intersects(burger.CollisionRectangle))
                    {
                       projectile.Active = false;
                    //if burger get damage enough from projectile. it will stop
                       burger.Health -= GameConstants.TeddyBearProjectileDamage;
                   
                    //project increment 5 step 1 (3/4)
                    healthString = GameConstants.HealthPrefix + burger.Health;
                    
                    //project increment 5 step 6 (1/1)
                    burgerDamage.Play();
                }

            }

            // check and resolve collisions between teddy bears and projectiles
            foreach (TeddyBear bear in bears)
            {
                foreach (Projectile projectile in projectiles)
                {
                    if (projectile.Type == ProjectileType.FrenchFries &&
                        bear.Active &&
                        projectile.Active &&
                        bear.CollisionRectangle.Intersects(projectile.CollisionRectangle))
                    {
                        bear.Active = false;
                        projectile.Active = false;

                        //project increment 5 step 7 (3/3)
                        //add new explosion sound to last argument
                        explosions.Add(new Explosion(explosionSpriteStrip, bear.Location.X,
                            bear.Location.Y, explosion));

                        //project increment 5 step 2 (2/3)
                        score +=  GameConstants.BearPoints;
                        scoreString = GameConstants.ScorePrefix + score;
                                              
                    }
                }
            }

            // clean out inactive teddy bears and add new ones as necessary
            for (int i = bears.Count - 1; i >= 0; i--)
            {
                if (!bears[i].Active)
                {
                    bears.RemoveAt(i);
                }
            }

            //step 1 
            //if any bear is dead spawn immediately new bear
            while (bears.Count < GameConstants.MaxBears) {

                SpawnBear();

            }

            // clean out inactive projectiles
            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                if (!projectiles[i].Active)
                {
                    projectiles.RemoveAt(i);
                }
            }

            // clean out finished explosions
            for (int i = explosions.Count - 1; i >= 0; i--)
            {
                if (explosions[i].Finished)
                {
                    explosions.RemoveAt(i);
                }
            }
            //project increment 5 step 8 (1/2)
            CheckBurgerKill();

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.SteelBlue);

            spriteBatch.Begin();

            // draw game objects
            burger.Draw(spriteBatch);
            foreach (TeddyBear bear in bears)
            {
                bear.Draw(spriteBatch);
            }
            foreach (Projectile projectile in projectiles)
            {
                projectile.Draw(spriteBatch);
            }
            foreach (Explosion explosion in explosions)
            {
                explosion.Draw(spriteBatch);
            }

            // draw score and health
            //project increment 5 step 1 (4/4)
            spriteBatch.DrawString(font, healthString, GameConstants.HealthLocation, Color.SpringGreen);

            //project increment 5 step 2 (1/3)
            spriteBatch.DrawString(font, scoreString, GameConstants.ScoreLocation, Color.Red);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        #region Public methods

        /// <summary>
        /// Gets the projectile sprite for the given projectile type
        /// </summary>
        /// <param name="type">the projectile type</param>
        /// <returns>the projectile sprite for the type</returns>
        public static Texture2D GetProjectileSprite(ProjectileType type)
        {
            // return correct projectile sprite based on projectile type
            if (type == ProjectileType.FrenchFries)
            {
                return frenchFriesSprite;
            }
            else
            {
                return teddyBearProjectileSprite;
            }
        }

        /// <summary>
        /// Adds the given projectile to the game
        /// </summary>
        /// <param name="projectile">the projectile to add</param>
        public static void AddProjectile(Projectile projectile)
        {
            projectiles.Add(projectile);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Spawns a new teddy bear at a random location
        /// </summary>
        private void SpawnBear()
        {
            // generate random location
            int x = GetRandomLocation(GameConstants.SpawnBorderSize,
                graphics.PreferredBackBufferWidth - 2 * GameConstants.SpawnBorderSize);
            int y = GetRandomLocation(GameConstants.SpawnBorderSize,
                graphics.PreferredBackBufferHeight - 2 * GameConstants.SpawnBorderSize);

            // generate random velocity
            float speed = GameConstants.MinBearSpeed +
                RandomNumberGenerator.NextFloat(GameConstants.BearSpeedRange);
            float angle = RandomNumberGenerator.NextFloat(2 * (float)Math.PI);
            Vector2 velocity = new Vector2(
                (float)(speed * Math.Cos(angle)), (float)(speed * Math.Sin(angle)));

            // create new bear
            //project increment 5 step 4 (4/5) change last argument null to sound effect
            //project increment 5 step 5 (1/4) change the second to last argument from null to sound effect
            TeddyBear newBear = new TeddyBear(Content, @"graphics\teddybear", x, y, velocity,
                teddyBounce, teddyShot);

            // make sure we don't spawn into a collision
            //step 2
            //create list for get collision rectangle
            List<Rectangle> collisionRectangle = GetCollisionRectangles();

            //check the if collision exist. Then new bear spawn on window randomly
            while(CollisionUtils.IsCollisionFree(newBear.CollisionRectangle, collisionRectangle) == false)
            {
                newBear.X = GetRandomLocation(GameConstants.SpawnBorderSize,
                    graphics.PreferredBackBufferWidth - (2 * GameConstants.SpawnBorderSize));

                newBear.Y = GetRandomLocation(GameConstants.SpawnBorderSize, 
                    graphics.PreferredBackBufferHeight - (2 * GameConstants.SpawnBorderSize));
            }


            // add new bear to list
            bears.Add(newBear);
        }

        /// <summary>
        /// Gets a random location using the given min and range
        /// </summary>
        /// <param name="min">the minimum</param>
        /// <param name="range">the range</param>
        /// <returns>the random location</returns>
        private int GetRandomLocation(int min, int range)
        {
            return min + RandomNumberGenerator.Next(range);
        }

        /// <summary>
        /// Gets a list of collision rectangles for all the objects in the game world
        /// </summary>
        /// <returns>the list of collision rectangles</returns>
        private List<Rectangle> GetCollisionRectangles()
        {
            List<Rectangle> collisionRectangles = new List<Rectangle>();
            collisionRectangles.Add(burger.CollisionRectangle);
            foreach (TeddyBear bear in bears)
            {
                collisionRectangles.Add(bear.CollisionRectangle);
            }
            foreach (Projectile projectile in projectiles)
            {
                collisionRectangles.Add(projectile.CollisionRectangle);
            }
            foreach (Explosion explosion in explosions)
            {
                collisionRectangles.Add(explosion.CollisionRectangle);
            }
            return collisionRectangles;
        }

        /// <summary>
        /// Checks to see if the burger has just been killed
        /// </summary>
        private void CheckBurgerKill()
        {
            //project increment 5 step 8 (2/2)
            if(burger.Health <= 0 && !burgerDead)
            {
                burgerDead = true;
                burgerDeath.Play();
            }
            
        }

        #endregion
    }
}

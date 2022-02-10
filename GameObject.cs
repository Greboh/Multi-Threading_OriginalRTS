using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace OriginalRTS
{
    public abstract class GameObject
    {
        #region Fields 

        protected Texture2D sprite;

        #region Float

        protected Vector2 origin;
        #endregion

        #region Misc
        protected Random myRandom = new Random();
        protected GameWorld myGameWorld;
        protected Color color = Color.White;
        #endregion

        #region int
        protected int scale;
        protected int workerID;
        protected WorkerJob workerJob;


        protected int workerCurrentInventory;
        protected int workerMaxInventory;

        protected float ressourceWeight;

        protected int mineCount = 0;
        protected int workerHealth;
        protected int workerMaxHealthDegradation = 50;

        protected bool fullInventory;
        protected bool workerIsAlive;

        protected int bankLevel;

        #endregion

        #endregion


        public abstract void Enter(object id);

        public abstract void FarmRessource(object id);

        public abstract void LoadContent(ContentManager content);

        /// <summary>
        /// Used to Draw in all subclasses. Abstract since we only need to use it in the subclasses
        /// </summary>
        /// <param name="spriteBatch"></param>
        public abstract void Draw(SpriteBatch spriteBatch);

        /// <summary>
        /// Used to update in all subclasses. Abstract since we only need to use it in the subclasses
        /// </summary>
        /// <param name="gameTime"></param>
        public abstract void Update(GameTime gameTime);

        /// <summary>
        /// Used to set our gameworld in the GameWorld class
        /// </summary>
        /// <param name="gameWorld"></param>
        public void SetGameWorld(GameWorld gameWorld)
        {
            myGameWorld = gameWorld; // Takes our variable myGameWorld and applies it to the method parameter
        }

        /// <summary>
        /// Used to instantiate an object into our Gameworld
        /// In reality it adds them to our list of objects to add in the GameWorld Class
        /// </summary>
        /// <param name="gameObject"></param>
        public void Instantiate(GameObject gameObject)
        {
            myGameWorld.Instantiate(gameObject);
        }

        /// <summary>
        /// Used to destroy an object in our GameWorld
        /// In reality it adds them to our list of objects to destroy in the GameWorld Class
        /// </summary>
        public void Destroy(GameObject gameObject)
        {
            myGameWorld.DestroyGameObject(gameObject);
        }
    }

}

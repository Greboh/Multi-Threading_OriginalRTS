using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Threading;

namespace OriginalRTS
{
    public enum WorkerJob
    {
        Miner,
        Farmer,
        Banker
    }

    public class GameWorld : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private List<GameObject> listOfCurrentObjects = new List<GameObject>(); // List of objects in the game
        private List<GameObject> listOfObjectsToAdd = new List<GameObject>(); // List of objects to add to the game
        private List<GameObject> listOfObjectsToDestroy = new List<GameObject>(); // List of objects to remove from the game

        private readonly int screenHeight = 890;    // Y size of window
        private readonly int screenWidth = 1215;    // X size of window

        private int gold;
        private int maxGold = 1500;

        private bool hasWorkerDeposted = false;

        private SpriteFont textFont;
        private Texture2D goldIngot;




        private static Semaphore semaMine = new Semaphore(0, 3); // Max 3 workers in mine
        private static Semaphore semaFarm = new Semaphore(0, 3); // Max 3 workers in mine


        private static Dictionary<int, WorkerJob> dictionaryOfWorkers = new Dictionary<int, WorkerJob>();


        private bool pIsPressed = false;
        private bool enterIsPressed = false;





        #region Properties

        public static Vector2 ScreenSize { get; set; } //Property for setting the ScreenSize for public use

        public static Semaphore SemaMine { get => semaMine; set => semaMine = value; }

        public static Dictionary<int, WorkerJob> DictionaryOfWorkers { get => dictionaryOfWorkers; set => dictionaryOfWorkers = value; }
        public int Gold { get => gold; set => gold = value; }
        public bool HasWorkerDeposted { get => hasWorkerDeposted; set => hasWorkerDeposted = value; }
        public static Semaphore SemaFarm { get => semaFarm; set => semaFarm = value; }
        public int MaxGold { get => maxGold; set => maxGold = value; }

        #endregion

        public GameWorld()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _graphics.PreferredBackBufferHeight = screenHeight;
            _graphics.PreferredBackBufferWidth = screenWidth;
            ScreenSize = new Vector2(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight); // Saves the Screen size
        }

        protected override void Initialize()
        {
            for (int i = 1; i < 6; i++)
            {
                Instantiate(new MineWorker(i));
            }

            //for (int i = 5; i < 10; i++)
            //{
            //    Instantiate(new FarmWorker(i));
            //}


            base.Initialize();
            SemaMine.Release(3);
            //semaFarm.Release(3);
        }


        protected override void Update(GameTime gameTime)
        {
            foreach (GameObject gameObject in listOfCurrentObjects)
            {
                gameObject.Update(gameTime);
            }




            if (Keyboard.GetState().IsKeyDown(Keys.Enter) && !enterIsPressed)
            {
                foreach (KeyValuePair<int, WorkerJob> kvp in dictionaryOfWorkers)
                {
                    Console.WriteLine("Key = {0}, Value = {1}",
                        kvp.Key, kvp.Value);
                }

                Console.WriteLine($"Current gold is: {gold}");

                enterIsPressed = true;
            }
            else if (Keyboard.GetState().IsKeyUp(Keys.Enter))
            {
                enterIsPressed = false;
            }         
            

            else if (Keyboard.GetState().IsKeyUp(Keys.P))
            {
                pIsPressed = false;
            }

            CallInstantiate();
            CallDestroy();


            base.Update(gameTime);
        }

       

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            textFont = Content.Load<SpriteFont>("TextFont");
            goldIngot = Content.Load<Texture2D>("GoldIngot");

        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            
            _spriteBatch.Begin(SpriteSortMode.FrontToBack);

            _spriteBatch.DrawString(textFont,(gold.ToString() + " / " + maxGold), new Vector2(55, 10), Color.White, 0, new Vector2(0, 0), 1, SpriteEffects.None, 1f);
            _spriteBatch.Draw(goldIngot, new Vector2(1, 1), null, Color.White, 0, new Vector2(0, 0), .75f, SpriteEffects.None, 1f);




            _spriteBatch.End();
            base.Draw(gameTime);
        }







        public void Instantiate(GameObject gameObject)
        {
            gameObject.SetGameWorld(this); // Sets our GameWorld to this GameWorld
            listOfObjectsToAdd.Add(gameObject); // Adds the GameObject to the list
        }

        /// Moves our Gameobjects to our list of objects to destroy
        /// </summary>
        /// <param name="gameObject">The object we want to destroy</param>
        public void DestroyGameObject(GameObject gameObject)
        {
            listOfObjectsToDestroy.Add(gameObject); // Adds the GameObject to the list
        }

        /// <summary>
        /// Checks if there are any objects to add from our add list
        /// If there is it loads their content and adds them to our current objects list
        /// </summary>
        private void CallInstantiate()
        {
            if (listOfObjectsToAdd.Count > 0)
            {
                foreach (GameObject addObj in listOfObjectsToAdd)
                {
                    addObj.LoadContent(Content); // Loads the content we wish to spawn in the game
                    listOfCurrentObjects.Add(addObj); // Adds them to the list of current objects in the game
                }

                listOfObjectsToAdd.Clear();
            }
        }

        /// <summary>
        /// Checks if there are any objects to destroy from our destroy list
        /// if there is it removes them from our current objects list
        /// </summary> 
        private void CallDestroy()
        {
            if (listOfObjectsToDestroy.Count > 0)
            {
                foreach (GameObject destroyObj in listOfObjectsToDestroy)
                {
                    listOfCurrentObjects.Remove(destroyObj); // Remove the objects we wish to remove from the game
                }
            }
        }


    }
}

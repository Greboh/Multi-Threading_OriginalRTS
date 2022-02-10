using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace OriginalRTS
{
    public enum WorkerJob
    {
        Miner,
        Farmer
    }

    public class GameWorld : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private static Semaphore semaMine = new Semaphore(0, 3); // Max 3 workers in mine
        private static Semaphore semaFarm = new Semaphore(0, 3); // Max 3 workers in mine

        private List<GameObject> listOfCurrentObjects = new List<GameObject>(); // List of objects in the game
        private List<GameObject> listOfObjectsToAdd = new List<GameObject>(); // List of objects to add to the game
        private List<GameObject> listOfObjectsToDestroy = new List<GameObject>(); // List of objects to remove from the game

        private static Dictionary<int, WorkerJob> dictionaryOfWorkers = new Dictionary<int, WorkerJob>();

        private readonly int screenHeight = 915;    // Y size of window
        private readonly int screenWidth = 1500;    // X size of window

        private static int currentGold = 10000;
        private static int maxGold;

        private static int currentWood = 10000;
        private static int maxWood;

        private static int currentBankLevel = 1;
        private static int maxBankLevel = 3;

        private static bool bankIsUpgrading = false;


        private int workerCost = 200;

        private static int bankGoldCost;
        private static int bankWoodCost;

        private static int countOfMiners = 0;
        private static int countOfFarmers = 0;

        private SpriteFont textFont;
        private Texture2D goldIngot;
        private Texture2D woodPile;
        private Texture2D banklevel1;
        private Texture2D banklevel2;
        private Texture2D banklevel3;
        private Texture2D minerIcon;
        private Texture2D farmerIcon;
        private Texture2D uiElement;


        private bool qIsPressed = false;
        private bool wIsPressed = false;
        private bool eIsPressed = false;

        private string minerBuyString;
        private string farmerBuyString;
        private string bankUpgradeString;



        #region Properties

        public static Vector2 ScreenSize { get; set; } //Property for setting the ScreenSize for public use
        public static Semaphore SemaMine { get => semaMine; set => semaMine = value; }
        public static Semaphore SemaFarm { get => semaFarm; set => semaFarm = value; }
        public static Dictionary<int, WorkerJob> DictionaryOfWorkers { get => dictionaryOfWorkers; set => dictionaryOfWorkers = value; }
        public static int Gold { get => currentGold; set => currentGold = value; }
        public static int MaxGold { get => maxGold; set => maxGold = value; }
        public static int CurrentWood { get => currentWood; set => currentWood = value; }
        public static int MaxWood { get => maxWood; set => maxWood = value; }
        public static int CountOfMiners { get => countOfMiners; set => countOfMiners = value; }
        public static int CountOfFarmers { get => countOfFarmers; set => countOfFarmers = value; }
        public static int CurrentBankLevel { get => currentBankLevel; set => currentBankLevel = value; }
        public static int MaxBankLevel { get => maxBankLevel; set => maxBankLevel = value; }
        public static bool BankIsUpgrading { get => bankIsUpgrading; set => bankIsUpgrading = value; }
        public static int BankGoldCost { get => bankGoldCost; set => bankGoldCost = value; }
        public static int BankWoodCost { get => bankWoodCost; set => bankWoodCost = value; }
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
            for (int i = 1; i < 3; i++) // instantiates 2 miners
            {
                Instantiate(new MineWorker(i));
                Instantiate(new FarmWorker(i));
            }

            Instantiate(new Bank(currentBankLevel));

            base.Initialize();
            SemaMine.Release(1);
            semaFarm.Release(1);

            minerBuyString = ($"miners cost {workerCost} gold each! - Press q to buy a new miner!");
            farmerBuyString = ($"farmers cost {workerCost} gold each! - Press w to buy a new farmer!");

        }


        protected override void Update(GameTime gameTime)
        {
            foreach (GameObject gameObject in listOfCurrentObjects)
            {
                gameObject.Update(gameTime);
            }

            if (!bankIsUpgrading) bankUpgradeString = ($"Next bank level cost {bankGoldCost} gold and {bankWoodCost} wood! - Press e to upgrade bank!");
            else if (currentBankLevel == maxBankLevel) bankUpgradeString = ($"The Bank is fully upgraded!");
            else bankUpgradeString = ($"bank is upgrading");

            Keybindings();
            CallInstantiate();
            CallDestroy();


            base.Update(gameTime);
        }

        private void Keybindings()
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Q) && !qIsPressed)
            {
                BuyNewMiner(workerCost);
                qIsPressed = true;
            }
            else if (Keyboard.GetState().IsKeyUp(Keys.Q))
            {
                qIsPressed = false;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.W) && !wIsPressed)
            {
                BuyNewFarmer(workerCost);
                wIsPressed = true;
            }
            else if (Keyboard.GetState().IsKeyUp(Keys.W))
            {

                wIsPressed = false;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.E) && !eIsPressed)
            {
                UpgradeBank(bankGoldCost, bankWoodCost);
                eIsPressed = true;
            }
            else if (Keyboard.GetState().IsKeyUp(Keys.E))
            {
                eIsPressed = false;
            }



        }

        private void BuyNewMiner(int cost)
        {
            if (currentGold >= cost)
            {
                Instantiate(new MineWorker(DictionaryOfWorkers.Count + 1));
                Console.WriteLine("You have bought a new worker!");
                currentGold -= cost;
            }
        }

        private void BuyNewFarmer(int cost)
        {
            if (currentGold >= cost)
            {
                Instantiate(new FarmWorker(DictionaryOfWorkers.Count + 1));
                Console.WriteLine("You have bought a new farmer!");
                currentGold -= cost;
            }
        }

        private void UpgradeBank(int goldCost, int woodCost)
        {
            if (currentGold >= goldCost && currentWood >= woodCost && CurrentBankLevel != maxBankLevel)
            {
                bankIsUpgrading = true;
                currentGold -= goldCost;
                currentWood -= woodCost;
            }
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            textFont = Content.Load<SpriteFont>("TextFont");
            goldIngot = Content.Load<Texture2D>("GoldIngot");
            woodPile = Content.Load<Texture2D>("woodPile");


            banklevel1 = Content.Load<Texture2D>("bank1");
            banklevel2 = Content.Load<Texture2D>("bank2");
            banklevel3 = Content.Load<Texture2D>("bank3");

            minerIcon = Content.Load<Texture2D>("minerIcon");
            farmerIcon = Content.Load<Texture2D>("farmerIcon");

            uiElement = Content.Load<Texture2D>("uiElement_2");

        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            
            _spriteBatch.Begin(SpriteSortMode.FrontToBack);

            _spriteBatch.Draw(uiElement, new Vector2(0, 0), null, Color.White, 0, new Vector2(0, 0), 1, SpriteEffects.None, 1);


            // Switch for changing out building asset when upgrading
            switch (currentBankLevel)
            {
                case 1:
                    _spriteBatch.Draw(banklevel1, new Vector2(780, 500), null, Color.White, 0, new Vector2(banklevel1.Width, banklevel1.Height), 1, SpriteEffects.None, 1);
                    break;
                case 2:
                    _spriteBatch.Draw(banklevel2, new Vector2(800, 500), null, Color.White, 0, new Vector2(banklevel2.Width, banklevel2.Height), 1, SpriteEffects.None, 1);

                    break;
                case 3:
                    _spriteBatch.Draw(banklevel3, new Vector2(830, 500), null, Color.White, 0, new Vector2(banklevel3.Width, banklevel3.Height), 1, SpriteEffects.None, 1);
                    break;
            }



            _spriteBatch.DrawString(textFont,(currentGold.ToString() + " / " + maxGold), new Vector2(85, 35), Color.White, 0, new Vector2(0, 0), 1, SpriteEffects.None, 1f);
            _spriteBatch.Draw(goldIngot, new Vector2(25, 25), null, Color.White, 0, new Vector2(0, 0), .75f, SpriteEffects.None, 1f);

            _spriteBatch.DrawString(textFont,(currentWood.ToString() + " / " + MaxWood), new Vector2(250, 35), Color.White, 0, new Vector2(0, 0), 1, SpriteEffects.None, 1f);
            _spriteBatch.Draw(woodPile, new Vector2(200, 25), null, Color.White, 0, new Vector2(0, 0), .75f, SpriteEffects.None, 1f);




            _spriteBatch.DrawString(textFont,"Bank lvl: " + (CurrentBankLevel.ToString() + " / " + MaxBankLevel), new Vector2(680, 500), Color.Black, 0, new Vector2(0, 0), 1, SpriteEffects.None, 1f);







            _spriteBatch.DrawString(textFont, countOfMiners.ToString(), new Vector2(1300, 850), Color.White, 0, new Vector2(0, 0), 1, SpriteEffects.None, 1f);

            _spriteBatch.DrawString(textFont, countOfFarmers.ToString(), new Vector2(1465, 850), Color.White, 0, new Vector2(0, 0), 1, SpriteEffects.None, 1f);


            _spriteBatch.DrawString(textFont,minerBuyString, new Vector2(5, 830), Color.White, 0, new Vector2(0, 0), 1, SpriteEffects.None, 1f);
            _spriteBatch.DrawString(textFont,farmerBuyString, new Vector2(5 , 860), Color.White, 0, new Vector2(0, 0), 1, SpriteEffects.None, 1f);
            _spriteBatch.DrawString(textFont,bankUpgradeString, new Vector2(5, 890), Color.White, 0, new Vector2(0, 0), 1, SpriteEffects.None, 1f);



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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace OriginalRTS
{
    class Bank : GameObject
    {
        


        public Bank(int currentBankLevel)
        {
            GameWorld.BankIsUpgrading = false;

            // Switch case changes bank capacity and upgrade cost according to current bank level
            switch (currentBankLevel)
            {
                case 1:
                    GameWorld.MaxGold = 1500;
                    GameWorld.MaxWood = 1000;
                    GameWorld.BankGoldCost = 1000;
                    GameWorld.BankWoodCost = 1000;
                        break;
                case 2:
                    GameWorld.MaxGold = 3000;
                    GameWorld.MaxWood = 2000;
                    GameWorld.BankGoldCost = 2000;
                    GameWorld.BankWoodCost = 1500;
                    break;
                case 3:
                    GameWorld.MaxGold = 5000;
                    GameWorld.MaxWood = 4000;
                    GameWorld.BankGoldCost = 3000;
                    GameWorld.BankWoodCost = 2500;
                    break;
            }

            Thread t = new Thread(UpgradeBank);
            t.IsBackground = true;
            t.Start();

        }

        /// <summary>
        /// Method for upgrading bank
        /// </summary>
        private void UpgradeBank()
        {
            while (!GameWorld.BankIsUpgrading)
            {
                // Do nothing while bank is not upgrading
            }

            GameWorld.CurrentBankLevel++; // increases bank level by 1

            Console.WriteLine($"Bank is upgrading to level {GameWorld.CurrentBankLevel}");

            Thread.Sleep(5000);

            Console.WriteLine($"Bank is now upgraded!");

            Instantiate(new Bank(GameWorld.CurrentBankLevel)); // instansiates new bank level
            Destroy(this); // destroys previous bank


        }

        public override void Draw(SpriteBatch spriteBatch)
        {
        }
        public override void Enter(object id)
        {
        }

        public override void FarmRessource(object id)
        {
        }

        public override void LoadContent(ContentManager content)
        {
        }

        public override void Update(GameTime gameTime)
        {

        }
    }
}

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
        private Texture2D banklevel1;
        private Texture2D banklevel2;
        private Texture2D banklevel3;


        public Bank(int currentBankLevel)
        {
            GameWorld.BankIsUpgrading = false;

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


        private void UpgradeBank()
        {
            while (!GameWorld.BankIsUpgrading)
            {
                // Do nothing while bank is not upgrading
            }

            GameWorld.CurrentBankLevel++;

            Console.WriteLine($"Bank is upgrading to level {GameWorld.CurrentBankLevel}");

            Thread.Sleep(5000);

            Console.WriteLine($"Bank is now upgraded!");

            Instantiate(new Bank(GameWorld.CurrentBankLevel));
            Destroy(this);


        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(banklevel1, new Vector2(500, 500), null, this.color, 0, origin, 1, SpriteEffects.None, 1);

            //spriteBatch.Draw(banklevel1, new Vector2(50, 50), Color.White);

        }
        public override void Enter(object id)
        {
        }

        public override void FarmRessource(object id)
        {
        }

        public override void LoadContent(ContentManager content)
        {

            banklevel1 = content.Load<Texture2D>("bank1");
            banklevel2 = content.Load<Texture2D>("bank2");
            banklevel3 = content.Load<Texture2D>("bank3");

        }

        public override void Update(GameTime gameTime)
        {

        }
    }
}

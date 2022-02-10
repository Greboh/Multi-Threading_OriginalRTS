using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace OriginalRTS
{
    class MineWorker : GameObject
    {
        private static Mutex depositMutex = new Mutex(); // creates mutex

        public MineWorker(int id)
        {
            //checks if id in dictionary is available
            try
            {
                GameWorld.DictionaryOfWorkers.Add(id, workerJob);
            }
            catch
            {
                Console.WriteLine("Id already taking!");
                GameWorld.DictionaryOfWorkers.Add(GameWorld.DictionaryOfWorkers.Count + 1, workerJob);
            }


            this.workerID = id;
            this.workerJob = WorkerJob.Miner;

            this.fullInventory = false;
            this.workerCurrentInventory = 0;
            this.workerMaxInventory = 200;
            this.workerIsAlive = true;
            this.workerHealth = 100;
            this.workerMaxHealthDegradation = 25;

            GameWorld.CountOfMiners++;

            new Thread(() => Enter(this.workerID)) { IsBackground = true }.Start();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
        }

        public override void LoadContent(ContentManager content)
        {
        }


        public override void Update(GameTime gameTime)
        {
        }



        public override void Enter(object id)
        {


            GameWorld.SemaMine.WaitOne(); //thread waits untill space inside semaphore is released. 
            Thread.Sleep(1000); // sleep before executing task. 

            Console.WriteLine("\nMiner " + id + " is entering mine\n");

            Thread.Sleep(1000);

            FarmRessource(id);



            Thread.Sleep(1250); //sleep before releasing space. 

            GameWorld.SemaMine.Release(); //Sema releases space inside the mine when thread has finished task - new thread can enter and execute

            //while loop runs while gold variable is at maximum. Thread sleeps until it can deposit its inventory
            while (this.fullInventory)
            {
                if (GameWorld.Gold <= GameWorld.MaxGold - 1)
                {
                    DepositThread();
                    Console.WriteLine($"Miner {this.workerID} has {this.workerHealth} hp left!");
                }
                else
                {
                    Console.WriteLine("\nGold is at max capacity!\n");
                    Thread.Sleep(5000);
                }
            }

            //kills worker if worker health is below 1, if not worker resets.
            if (workerHealth >= 1) new Thread(() => Enter(this.workerID)) { IsBackground = true }.Start(); 
            else
            {
                Console.WriteLine($"Miner {this.workerID} has died!");
                GameWorld.CountOfMiners--;
                Destroy(this);
            }
        }

        /// <summary>
        /// Method for farming/adding Ressource to workerCurrentinventory
        /// </summary>
        /// <param name="id"></param>
        public override void FarmRessource(object id)
        {
            while (this.workerMaxInventory > this.workerCurrentInventory) 
            {
                this.workerCurrentInventory += myRandom.Next(1, 75);//random value added to worker inventory to simulate ressource farming.

                Thread.Sleep(500);

                //statement to end task and remove health from worker. 
                if (this.workerCurrentInventory >= this.workerMaxInventory)
                {
                    this.workerCurrentInventory = this.workerMaxInventory; //worker inventory is now full = 100
                    this.fullInventory = true; 
                    this.workerHealth -= myRandom.Next(20, workerMaxHealthDegradation); // remove random number/value from worker health. 
                }

                //Console.WriteLine("\n" + id + " has mined 10 stones");
                //Console.WriteLine(id + "'s inventory now weights " + this.workerCurrentInventory);
            }
        }


        /// <summary>
        /// Method for depositing resources into bank. 
        /// </summary>
        private void DepositThread()
        {
            if (depositMutex.WaitOne(300)) // Mutex ensures only one thread/worker can acces bank/variable
            {
                Console.WriteLine($"\nMiner {this.workerID} is depositing gold!");

                GameWorld.Gold += workerCurrentInventory; //worker inventory added to bank/variable. 


                Thread.Sleep(1000); //worker sleeps before release of mutex

                depositMutex.ReleaseMutex(); //mutex is released
                this.workerCurrentInventory = 0; //resets worker inventory
                this.fullInventory = false; // resets worker inventory

            }
            else // thread sleeps if mutex isn't available 
            {
                Console.WriteLine($"\nMiner {this.workerID} didnt get the mutex\n");
                //Console.WriteLine($"{this.workerID} trying again in 3 second");
                Thread.Sleep(1500);
            }
        }

        

    }

}
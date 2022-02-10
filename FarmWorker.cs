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
    class FarmWorker : GameObject
    {
        private static Mutex depositMutex = new Mutex(); // creates mutex

        public FarmWorker(int id)
        {
            // checks if worker id is available
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
            this.workerJob = WorkerJob.Farmer;

            this.fullInventory = false;
            this.workerCurrentInventory = 0;
            this.workerMaxInventory = 200;
            this.workerIsAlive = true;
            this.workerHealth = 100;
            this.workerMaxHealthDegradation = 25;

            GameWorld.CountOfFarmers++;

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


        /// <summary>
        /// The enter method uses id to differentiate between workers - and the Semaphore from GameWorld is used to ensure only 3 can enter farm at any point
        /// </summary>
        /// <param name="id"></param>
        public override void Enter(object id)
        {
            GameWorld.SemaFarm.WaitOne();   // thread waits untill space inside semaphore is released 
            Thread.Sleep(1000);

            Console.WriteLine($"\n{workerJob} " + id + " is entering the farm\n");

            Thread.Sleep(1000);

            FarmRessource(id);  // farm resource method

            Thread.Sleep(1250);

            GameWorld.SemaFarm.Release(); // releases space for the next worker to enter

            // this while loop runs while the workers inventory is full, it tries to deposit, but if wood is at capacity, thread sleeps until it can deposit inventory
            while (this.fullInventory)
            {
                if (GameWorld.CurrentWood <= GameWorld.MaxWood - 1)
                {
                    DepositThread();
                    Console.WriteLine($"{workerJob} {this.workerID} has {this.workerHealth} hp left!");
                }
                else
                {
                    Console.WriteLine("\nWood is at max capacity!\n");
                    Thread.Sleep(5000);
                }
            }
            // Kills worker thread if workerHealth goes below 1.
            if (workerHealth >= 1) new Thread(() => Enter(this.workerID)) { IsBackground = true }.Start();
            else
            {
                Console.WriteLine($"{workerJob} {this.workerID} has died!");
                GameWorld.CountOfFarmers--;
                Destroy(this);
            }
        }

        /// <summary>
        /// Method runs while worker is in mine, and worker still has room in inventory ie. workerMaxInventory > workerCurrentInventory
        /// </summary>
        /// <param name="id"></param>
        public override void FarmRessource(object id)
        {
            while (this.workerMaxInventory > this.workerCurrentInventory)
            {
                this.workerCurrentInventory += myRandom.Next(1, 75); // random value added to worker inventory to simulate resource farming

                Thread.Sleep(500);

                // checks if inventory is full, if it is worker loses random amount of health, and the while loop is broken out of
                if (this.workerCurrentInventory >= this.workerMaxInventory)
                {
                    this.workerCurrentInventory = this.workerMaxInventory;
                    this.fullInventory = true;
                    this.workerHealth -= myRandom.Next(20, workerMaxHealthDegradation);
                }
            }
        }

        /// <summary>
        /// Method for worker depositing resources into bank
        /// </summary>
        private void DepositThread()
        {
            if (depositMutex.WaitOne(300)) // mutex insures only one thread / worker can access bank/variable at a time
            {
                Console.WriteLine($"\n{workerJob} {this.workerID} is depositing wood!");

                GameWorld.CurrentWood += workerCurrentInventory; // worker inventory added to bank/variable

                

                Thread.Sleep(1000);

                depositMutex.ReleaseMutex(); // mutex is released
                this.workerCurrentInventory = 0; // resets worker inventory
                this.fullInventory = false; // resets worker inventory

            }
            else // thread sleeps if mutex isn't available
            {
                Console.WriteLine($"\n{workerJob} {this.workerID} didnt get the mutex\n");
                
                Thread.Sleep(1500);
            }
        }



    }

}
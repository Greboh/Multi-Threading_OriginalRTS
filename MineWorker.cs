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
        private static Mutex depositMutex = new Mutex();

        //int workerID;
        //static Semaphore MySemaphore = new Semaphore(0, 2);

        public MineWorker(int id)
        {
            GameWorld.DictionaryOfWorkers.Add(id, workerJob);

            this.workerID = id;
            this.workerJob = WorkerJob.Miner;

            this.fullInventory = false;
            this.workerCurrentInventory = 0;
            this.workerMaxInventory = 100;
            this.workerIsAlive = true;
            this.workerHealth = 100;
            this.workerMaxHealthDegradation = 25;


            //Thread.Sleep(250);

            new Thread(Enter).Start(this.workerID);
            

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
            //Console.WriteLine(id + " is waiting to go to mine");
            GameWorld.SemaMine.WaitOne();
            Thread.Sleep(1000);

            Console.WriteLine("\nMiner" + id + " is entering mine\n");

            Thread.Sleep(1000);

            FarmRessource(id);


            //Console.WriteLine("\n" + id + " is leaving mine to deposit gold!");

            Thread.Sleep(1250);

            GameWorld.SemaMine.Release();


            while (this.fullInventory)
            {
                if (myGameWorld.Gold <= myGameWorld.MaxGold - 1)
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

          

            if (workerHealth >= 1) new Thread(Enter).Start(this.workerID);
            else
            {
                Console.WriteLine($"Miner {this.workerID} has died!");
                GameWorld.DictionaryOfWorkers.Remove(this.workerID);
                Destroy(this);
            }


        }

        public override void FarmRessource(object id)
        {
            while (this.workerMaxInventory > this.workerCurrentInventory)
            {
                this.workerCurrentInventory += myRandom.Next(1, 35);

                Thread.Sleep(500);

                if (this.workerCurrentInventory > 100)
                {
                    this.workerCurrentInventory = 100;
                    this.fullInventory = true;
                    this.workerHealth -= myRandom.Next(20, workerMaxHealthDegradation);
                }

                //Console.WriteLine("\n" + id + " has mined 10 stones");
                //Console.WriteLine(id + "'s inventory now weights " + this.workerCurrentInventory);
            }
        }


        private void DepositThread()
        {
            if (depositMutex.WaitOne(300))
            {
                Console.WriteLine($"\nMiner {this.workerID} is depositing gold!");

                if (myGameWorld.Gold <= myGameWorld.MaxGold)
                {
                    myGameWorld.Gold += workerCurrentInventory;
                }
                else
                {
                   myGameWorld.Gold = myGameWorld.MaxGold;
                }

                //Console.WriteLine("Current " + this.workerCurrentInventory);
                //Console.WriteLine("Max " + this.workerMaxInventory);

                //Console.WriteLine($"{this.workerID} got the mutex");



                Thread.Sleep(1000);


                depositMutex.ReleaseMutex();
                this.workerCurrentInventory = 0;
                this.fullInventory = false;

            }
            else
            {
                Console.WriteLine($"\n Miner {this.workerID} didnt get the mutex\n");
                //Console.WriteLine($"{this.workerID} trying again in 3 second");
                Thread.Sleep(1500);
            }
        }

        

    }

}
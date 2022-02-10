﻿using Microsoft.Xna.Framework;
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

        public MineWorker(int id)
        {
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


            GameWorld.SemaMine.WaitOne();
            Thread.Sleep(1000);

            Console.WriteLine("\nMiner " + id + " is entering mine\n");

            Thread.Sleep(1000);

            FarmRessource(id);



            Thread.Sleep(1250);

            GameWorld.SemaMine.Release();

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

            if (workerHealth >= 1) new Thread(() => Enter(this.workerID)) { IsBackground = true }.Start();
            else
            {
                Console.WriteLine($"Miner {this.workerID} has died!");
                GameWorld.CountOfMiners--;
                Destroy(this);
            }
        }

        public override void FarmRessource(object id)
        {
            while (this.workerMaxInventory > this.workerCurrentInventory)
            {
                this.workerCurrentInventory += myRandom.Next(1, 75);

                Thread.Sleep(500);

                if (this.workerCurrentInventory >= this.workerMaxInventory)
                {
                    this.workerCurrentInventory = this.workerMaxInventory;
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

                GameWorld.Gold += workerCurrentInventory;

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
                Console.WriteLine($"\nMiner {this.workerID} didnt get the mutex\n");
                //Console.WriteLine($"{this.workerID} trying again in 3 second");
                Thread.Sleep(1500);
            }
        }

        

    }

}
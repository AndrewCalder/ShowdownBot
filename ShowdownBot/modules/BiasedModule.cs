﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQA.Selenium;
using static ShowdownBot.Global;
using static ShowdownBot.GlobalConstants;
namespace ShowdownBot.modules
{
    class BiasedModule : BotModule
    {

        float M1WGT;
        float M2WGT;
        float M3WGT;
        float M4WGT;
        float weightTotal;
        
        public BiasedModule(Bot m, IWebDriver b)
            : base(m, b)
        {
            format = "ou";
            M1WGT = Global.m1wgt;
            M2WGT = Global.m2wgt;
            M3WGT = Global.m3wgt;
            M4WGT = Global.m4wgt;
            weightTotal = (M1WGT + M2WGT + M3WGT + M4WGT);
        }

        public override void battle()
        {
            int turn = 1;
            if (format != "randombattle")
            {
                while (elementExists(By.CssSelector("button[name='chooseTeamPreview']")))
                {
                    //todo terminate this if after a while.
                    wait();
                }
                pickLeadBiased();
            }

            do
            {
               battleBiased(ref turn);

            } while (activeState == State.BATTLE);

            //Done battling, but the battle isn't over.

            if (activeState == State.IDLE && !checkBattleEnd())
            {
                goMainMenu(true);
            }
            
        }



        private bool battleBiased(ref int turn)
        {
            int moveSelection;
            int pokeSelection;

            if (checkMove())
            {
                if (elementExists(By.Name("megaevo")))
                {
                    browser.FindElement(By.Name("megaevo")).Click();
                }
                wait();
                moveSelection = pickMoveBiased();
                cwrite("I'm selecting move " + moveSelection.ToString(), "[TURN " + turn.ToString() + "]", COLOR_BOT);
                browser.FindElement(By.CssSelector("button[value='" + moveSelection.ToString() + "']")).Click();
                turn++;
            }
            else if (checkSwitch())
            {
                //TODO: check if it's the first turn, and then select appropriate lead.
                cwrite("Switching pokemon.", COLOR_BOT);
                pokeSelection = pickPokeRandomly();
                cwrite("New pokemon selected: " + pokeSelection.ToString(), COLOR_BOT);
                browser.FindElement(By.CssSelector("button[value='" + pokeSelection.ToString() + "']")).Click();
                wait();
            }
            else if (checkBattleEnd())
            {
                return true;
            }
            else
            {
                wait();
            }
            return false;
        }

        private int pickMoveBiased()
        {
            HashSet<int> exclude = new HashSet<int>();
            int choice;
            choice = getIndexBiased();
            while (!elementExists(By.CssSelector("button[name=chooseMove][value='" + choice.ToString() + "']")))
            {
                //If the move we've chosen does not exist, just cycle through until we get one.
                cwrite("Bad move choice: " + choice.ToString() + "Picking another", "[DEBUG]", COLOR_OK);
                exclude.Add(choice);
                choice = GetRandomExcluding(exclude, 1, 4);
            }

            return choice;
        }



        /// <summary>
        /// Helper method for pickMoveBiased.
        /// </summary>
        /// <returns>Choice index based on the specified weights.</returns>
        private int getIndexBiased()
        {
            int choice = 1;
            Random rand = new Random();
            float cumulative = 0.0f;
            float percent = (float)rand.NextDouble()*(weightTotal);
            cwrite("Choosing move that meets " + percent.ToString(), "debug", COLOR_OK);
            List<float> weights = new List<float>{ M1WGT, M2WGT, M3WGT, M4WGT };
            weights.Sort();
            foreach (float wgt in weights)
            {
                percent -= wgt;
                if (percent <= 0)
                    return 5-choice;
                choice++;
            }
            return choice;

        }

        private void pickLeadBiased()
        {
            int lead = getIndexBiased() - 1;
            while (!elementExists(By.CssSelector("button[name='chooseTeamPreview']")))
            {
                wait();
                cwrite("Picking lead...", COLOR_BOT);
            }
            browser.FindElement(By.CssSelector("button[name='chooseTeamPreview'][value='" + lead + "']")).Click();
        }

        public override void printInfo()
        {
            cwrite("Biased mode info:\n" +
                    "Format: " + format +
                    "\nMove weight 1: "+M1WGT+
                    "\nMove weight 2: "+M2WGT+
                    "\nMove weight 3: "+M3WGT+
                    "\nMove weight 4: "+M4WGT+
                    "\nTotal: "+weightTotal
                    ,COLOR_BOT);
        }
    }
}

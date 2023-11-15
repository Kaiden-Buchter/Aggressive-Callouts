using CitizenFX.Core;
using CitizenFX.Core.Native;
using FivePD.API;
using FivePD.API.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgressiveCallouts
{
    [CalloutProperties("Bar Fight", "Husky", "v1.0")]
    internal class BarFight : Callout
    {
        public readonly Random rng = new Random();

        public List<Vector3> barLocations = new List<Vector3>()
        {
            new Vector3(1986.26f, 3051.22f, 46.22f),
        };
        public Vector3 barLocation;
        
        public List<String> barNames = new List<string>()
        {
            "Yellow Jack",
        };

        public string bName;
        

        public List<string> clipSets = new List<string>()
        {
            "MOVE_M@DRUNK@MODERATEDRUNK",
            "MOVE_M@DRUNK@MODERATEDRUNK_HEAD_UP",
            "MOVE_M@DRUNK@SLIGHTLYDRUNK",
            "MOVE_M@DRUNK@VERYDRUNK"
        };
        public string clipset;

        public Ped fighter1, fighter2;
        public PedData fighter1Data, fighter2Data;

        public BarFight()
        {
            {
                barLocation = barLocations.SelectRandom();
                bName = barNames.SelectRandom();
                clipset = clipSets.SelectRandom();
            }

            InitInfo(barLocation);
            ShortName = "Bar Fight";
            ResponseCode = 2;
            StartDistance = 125f;
            CalloutDescription = $"A Bar Fight has been reported in the {World.GetZoneLocalizedName(barLocation)} area.";
        }

        public override async Task OnAccept()
        {
            InitBlip();
            UpdateData();

            await Task.FromResult(0);
        }

        public override async void OnStart(Ped closest)
        {
            base.OnStart(closest);
            {
                API.RequestAnimSet(clipset);
                while (!API.HasAnimSetLoaded(clipset)) { await BaseScript.Delay(50); }

                fighter1 = await SpawnPed(RandomUtils.GetRandomPed(), barLocation.Around(0.5f));

                fighter2 = await SpawnPed(RandomUtils.GetRandomPed(), barLocation.Around(0.5f));
                API.SetPedMovementClipset(fighter2.Handle, clipset, 0.2f);
                
                fighter2Data = await fighter2.GetData();
                double bac2 = (double)rng.Next(10, 20) / 100;
                fighter2Data.BloodAlcoholLevel = bac2;
                fighter2.SetData(fighter2Data);
            }

            Fighter1Questions();
            Fighter2Questions();
            
            fighter1.Task.LookAt(fighter2);
            fighter2.Task.LookAt(fighter1);

            while(World.GetDistance(Game.PlayerPed.Position, fighter1.Position) > 45f) { await BaseScript.Delay(50); };

            fighter1.Task.FightAgainst(fighter2);
            fighter2.Task.FightAgainst(fighter1);

            fighter1.AttachBlip();
            fighter2.AttachBlip();

            await Task.FromResult(0);
        }

        //Victim
        public void Fighter1Questions()
        {
            PedQuestion q1, q2, q3, q4;
            q1 = new PedQuestion();
            q2 = new PedQuestion();
            //Non Juice Bar
            {
                q1.Question = "What's going on here?";
                q1.Answers = new List<string>()
                {
                    "I was just trying to enjoy a drink with my girlfriend.",
                    "I attempting to drink an ice cold beer after a long day at work.",
                    "Can't even get a beer in this city without shit going down.",
                    "I'm trying to defend myself from this drunk asshole."
                };

                q2.Question = "Did you start this fight?";
                q2.Answers = new List<string>()
                {
                    "No officer, I was just trying to relax after work.",
                    "Nope. He said I was looking at his girl, I don't even know who that is!",
                    "He started attacking me and I defended myself.",
                    "Not me, this drunk idiot came up to me and started punching me."
                };
            }

            PedQuestion[] questions = new PedQuestion[] { q1, q2 };
            AddPedQuestions(fighter1, questions);
        }

        //Suspect
        public void Fighter2Questions()
        {
            PedQuestion q1, q2;
            q1 = new PedQuestion();
            q2 = new PedQuestion();
            
            //Non Juice Bar
            
            {
                q1.Question = "What's going on here?";
                q1.Answers = new List<string>()
                {
                    "Huh?... Who are you? ~y~*person appears confused*~s~",
                    "I'm kicking this guys ass! ~y~*noticeable slurring of speech*~s~",
                    "Two men settling things like men!",
                    "I'm about to send this asshole to the hospital! ~y~*strong smell of alcohol*~s~"
                };

                q2.Question = "Did you start this fight?";
                q2.Answers = new List<string>()
                {
                    "He was hitting on my girlfriend. I had to kick his ass. ~y~*strong smell of alcohol*~s~",
                    "He started this by making me spill my beer. ~y~*strong smell of alcohol*~s~",
                    "This jerk was eyeballing me from across the bar. Can't let that slide. ~y~*noticeable slurring of speech*~s~",
                    "No way. This guy started talking shit and I punched him to shut him up."
                };
            }

            PedQuestion[] questions = new PedQuestion[] { q1, q2};
            AddPedQuestions(fighter2, questions);
        }
    }
}

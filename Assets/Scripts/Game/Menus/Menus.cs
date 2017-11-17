using Scripts.Model.Interfaces;
using Scripts.Model.Pages;
using Scripts.Model.Processes;
using Scripts.Model.Characters;
using Scripts.Presenter;
using UnityEngine;
using Scripts.View.Effects;
using Scripts.View.ObjectPool;
using Scripts.Game.Defined.Characters;
using System;
using Scripts.Game.Defined.Spells;
using Scripts.Game.Defined.Serialized.Spells;
using System.Collections.Generic;
using Scripts.Game.Undefined.Characters;
using Scripts.Model.TextBoxes;
using Scripts.Model.Acts;
using Scripts.Model.Stats;
using Scripts.Game.Defined.Serialized.Statistics;
using Scripts.Game.Serialized;
using Scripts.Game.Serialization;
using Scripts.Model.SaveLoad;
using UnityEngine.SceneManagement;

namespace Scripts.Game.Pages {

    /// <summary>
    /// Main menu.
    /// </summary>
    /// <seealso cref="Scripts.Model.Pages.PageGroup" />
    public class Menus : PageGroup {
        public const int DEBUGGING = 1;
        public const int NEW_GAME = 2;
        public const int UI_TUTORIAL = 3;
        private const string SECRET_PASSWORD = "hello";

        private string name;

        public Menus() : base(new Page("Main Menu")) {
            Register(DEBUGGING, new Page("Debug"));
            Register(NEW_GAME, new Page("New Game"));
            Register(UI_TUTORIAL, new Page("UI Tutorial"));
            StartPage();
            DebugPage();
            NewGameNameInputPage();
        }

        private string Name {
            get {
                return name;
            }
        }

        private void StartPage() {
            Page start = Get(ROOT_INDEX);
            start.Body = string.Format("Hello world.", Main.VERSION);
            start.OnEnter = () => {
                List<IButtonable> buttons = new List<IButtonable>() {
                    Get(NEW_GAME),
                    new LoadPages(start),
                    new Process("Back", "Return to Start Page.", () => SceneManager.LoadScene("Start"))
                };
                if (Util.IS_DEBUG) {
                    buttons.Add(Get(DEBUGGING));
                }
                start.Actions = buttons;
            };
        }

        private void DebugPage() {
            Page debug = Get(DEBUGGING);
            SubGrid submenu = new SubGrid("Go to submenu");
            SubGrid mainDebug = new SubGrid("Return to main menu");

            Character kitsune = CharacterList.TestEnemy();
            debug.AddCharacters(Side.LEFT, kitsune);
            int level = 0;

            mainDebug.List = new IButtonable[] {
                Get(ROOT_INDEX),
                new Process("Say", "Hello", () => Presenter.Main.Instance.TextBoxes.AddTextBox(new Model.TextBoxes.TextBox("Hello"))),
                new Process("AttDisb", () => Presenter.Main.Instance.TextBoxes.AddTextBox(new Model.TextBoxes.TextBox(kitsune.Stats.LongAttributeDistribution))),
                new Battle(debug, debug, Music.BOSS, "Battle Test", new Character[] { CharacterList.Hero("Debug"), CharacterList.TestEnemy(), CharacterList.TestEnemy()  }, new Character[] { CharacterList.TestEnemy(), CharacterList.TestEnemy() }),
                new Process("LongTalk Test", () => {
                    ActUtil.SetupScene(ActUtil.LongTalk(debug, kitsune, "<t>we have the best <b>guns</b><s>theaefaefef oieafoewjfoejfio oe foiawjefoawijef oj efjoiejfaoo oajeoaijfo wi best guns<a>the best gonzos the best gonzos the best gonzosthe best gonzos the best gonzos the best gonzos the best gonzos the best gonzos the best gonzos the best gonzos the best gonzos the best gonzos the best gonzos the best gonzos the best gonzos the best gonzos<a>helloworld<t>this is the captian speak"));
                }),
                new Process("Get level exp diff", () => {
                    Page.TypeText(new TextBox(string.Format("For level {0}-{1}: {2} exp", level, level + 1, Experience.GetExpDiffForLevel(level, level + 1))));
                    level++;
                }),
                new Process("ALL saves", () => SaveLoad.PrintSaves()),
                new Process("DELET all saves", () => SaveLoad.DeleteAllSaves()),
                new Process("move fox", () => { debug.Left.Clear(); debug.AddCharacters(Side.RIGHT, kitsune); }),
                new Process("test boss logo", () => ActUtil.SetupScene(new BossTransitionAct(Root, kitsune.Look))),
                new Process("test trophy", () => GameJolt.API.Trophies.Unlock(80273)),
                submenu
            };

            submenu.List = new IButtonable[] {
                new Process("Say hello", () => Presenter.Main.Instance.TextBoxes.AddTextBox(new Model.TextBoxes.TextBox("Hello"))),
                mainDebug
            };

            debug.AddCharacters(Side.LEFT, kitsune);
            debug.Actions = mainDebug.List;
        }

        private void NewGameNameInputPage() {
            Page page = BasicPage(NEW_GAME, ROOT_INDEX,
                new Process(
                "Confirm",
                () => {
                    this.name = Get(NEW_GAME).Input;
                    UITutorialPage(name);
                    Get(UI_TUTORIAL).Invoke();
                },
                () => 2 <= Get(NEW_GAME).Input.Length && Get(NEW_GAME).Input.Length <= 10)
                );

            page.Body = "What is your name?";
            page.HasInputField = true;
        }

        private void UITutorialPage(string name) {
            Page hotkeys = Get(UI_TUTORIAL);
            hotkeys.HasInputField = true;
            hotkeys.Body = "BUTTON INPUT\nUse the mouse or keyboard to interact with buttons! You will see a character near the bottom right of a button if you can use hotkeys.\n\nTOOLTIPS\nJust about everything you can see in this game has a tooltip (including some textboxes)! Hover your mouse over an element to learn more about the element.";
            hotkeys.OnEnter = () => {
                hotkeys.AddText(
                    new TextBox("Hover over this tooltip to figure out what to type into the input to advance! In battle, you can hover over these to figure out the details of a spell that you don't own.",
                    new Model.Tooltips.TooltipBundle(Util.GetSprite("talk"), "The password is...",
                    SECRET_PASSWORD
                    )));
            };
            hotkeys.Actions = new IButtonable[] {
                new Process("Advance!",
                    "This button will become enabled when you type the password into the input field. Did you check the textbox?",
                    () => {
                            BattleTutorial(name).Invoke();
                        },
                    () => hotkeys.Input.Equals(SECRET_PASSWORD)),
                new Process("Back",
                    () => {
                            Get(NEW_GAME).Invoke();
                    })
            };
        }

        private Battle BattleTutorial(string name) {
            IntroPages destination = new IntroPages(name);
            Battle battle = new Battle(
                    destination.Root,
                    destination.Root,
                    Music.RUINS,
                    "Battle Tutorial",
                    new Character[] { CharacterList.Hero(name) },
                    new Character[] { CharacterList.TrainingDummy() },
                    true,
                    false
                );
            battle.Icon = Util.GetSprite("white-book");
            return battle;
        }
    }
}
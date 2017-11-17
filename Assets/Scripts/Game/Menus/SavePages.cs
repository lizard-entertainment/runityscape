using Scripts.Model.Characters;
using Scripts.Model.Pages;
using Scripts.Game.Serialization;
using Scripts.Model.Interfaces;
using System.Collections.Generic;
using Scripts.Model.Processes;
using Scripts.Model.SaveLoad.SaveObjects;
using Scripts.Model.TextBoxes;
using Scripts.Game.Serialized;
using System;

namespace Scripts.Game.Pages {

    public class SavePages : PageGroup {

        public SavePages(Page previous, Party party, Flags flags) : base(new Page("Save")) {
            Setup(previous, party, flags);
        }

        private void Setup(Page previous, Party party, Flags flags) {
            Page p = Get(ROOT_INDEX);
            p.Icon = Util.GetSprite("save");
            p.OnEnter = () => {
                WorldSave currentGame = new WorldSave(party.GetSaveObject(), flags.GetSaveObject());
                p.Right.Clear();
                p.AddCharacters(Side.LEFT, party.Collection);
                p.HasInputField = false;
                List<IButtonable> buttons = new List<IButtonable>();
                buttons.Add(PageUtil.GenerateBack(previous));
                for (int i = 0; i < SaveLoad.MAX_SAVE_FILES; i++) {
                    if (SaveLoad.IsSaveUsed(i)) {
                        buttons.Add(GetOverrideSavePage(p, i, currentGame)); // oink
                    } else {
                        buttons.Add(
                            GetSaveProcess(
                                p,
                                string.Format("<color=grey>FILE {0}</color>", i),
                                i,
                                currentGame,
                                string.Format("Save to file {0}.", i),
                                string.Format("Saved to file {0}.", i)
                            )
                        );
                    }
                }
                buttons.Add(
                        PageUtil.GetConfirmationGrid(
                            p,
                            p,
                            GetExitProcess(),
                            "Exit",
                            "Return to the main menu",
                            "Are you sure you want to return to the main menu?\n<color=red>Any unsaved progress will be lost.</color>"
                            )
                    );
                p.Actions = buttons;
            };
        }

        private Page GetOverrideSavePage(Page previous, int saveIndex, WorldSave currentGame) {
            WorldSave currentSaveInTheSlot = SaveLoad.Load(saveIndex, saveIndex.ToString());
            World world = new World();
            world.InitFromSaveObject(currentSaveInTheSlot);
            Party party = world.Party;
            Flags flags = world.Flags;
            string saveName = world.FileName;

            Page page = PageUtil.GetConfirmationPage(
                previous,
                previous,
                GetSaveProcess(previous, "Overwrite", saveIndex, currentGame, string.Format("Save over file {0}.", saveIndex), string.Format("File {0} was overwritten.", saveIndex)),
                saveName,
                string.Format("Overwrite file {0}.", saveIndex),
                string.Format("Are you sure you want to overwrite file {0}?", saveIndex)
                );
            page.AddCharacters(Side.RIGHT, party.Collection);
            return page;
        }

        private Process GetSaveProcess(Page previous, string name, int saveIndex, WorldSave save, string tooltip, string saveMessage) {
            return new Process(
                name,
                tooltip,
                () => {
                    SaveLoad.Save(save, saveIndex);
                    previous.Invoke();
                    previous.AddText(saveMessage);
                }
                );
        }

        private Process GetExitProcess() {
            return new Process(
                                "Exit without saving",
                                "Return to the main menu without saving.",
                                () => (new Menus()).Invoke()
                                );
        }
    }
}
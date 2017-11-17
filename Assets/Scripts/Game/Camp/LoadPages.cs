using Scripts.Game.Serialization;
using Scripts.Game.Serialized;
using Scripts.Model.Characters;
using Scripts.Model.Interfaces;
using Scripts.Model.Pages;
using Scripts.Model.Processes;
using Scripts.Model.SaveLoad.SaveObjects;
using Scripts.Model.TextBoxes;
using Scripts.Presenter;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Game.Pages {

    /// <summary>
    /// Loading serializing page.
    /// </summary>
    /// <seealso cref="Scripts.Model.Pages.PageGroup" />
    public class LoadPages : PageGroup {
        private bool isUploadingToGameJolt;

        public LoadPages(Page previous) : base(new Page("Load")) {
            SetupRoot(previous);
        }

        /// <summary>
        /// Set up the root.
        /// </summary>
        /// <param name="previous">The previous.</param>
        private void SetupRoot(Page previous) {
            Page p = Get(ROOT_INDEX);
            p.Icon = Util.GetSprite("load");
            p.OnEnter = () => {
                p.Left.Clear();
                p.Right.Clear();
                List<IButtonable> buttons = new List<IButtonable>();
                buttons.Add(PageUtil.GenerateBack(previous));
                for (int i = 0; i < SaveLoad.MAX_SAVE_FILES; i++) {
                    if (SaveLoad.IsSaveUsed(i)) {
                        buttons.Add(GetSpecificLoadPage(p, i));
                    } else {
                        buttons.Add(GetImportPage(p, i));
                    }
                }

                p.Actions = buttons;
            };
        }

        /// <summary>
        /// Gets the specific load page associated with a save index.
        /// </summary>
        /// <param name="previous">The previous.</param>
        /// <param name="index">The save index.</param>
        /// <returns></returns>
        private Page GetSpecificLoadPage(Page previous, int index) {
            Party party;
            Camp camp;
            string saveName;
            try {
                RestoreCamp(SaveLoad.Load(index, index.ToString()), out camp, out party, out saveName);
            } catch (Exception e) {
                previous.AddText(string.Format("Save {0} is corrupted, deleting...\n{1}", index, e.Message));
                SaveLoad.DeleteSave(index);
                return GetImportPage(previous, index);
            }
            Page page = new Page(saveName);
            page.Body = string.Format("What would you like to do with file {0}?", index);
            page.Actions = new IButtonable[] {
                PageUtil.GenerateBack(previous).SetCondition(() => !isUploadingToGameJolt),
                GetLoadProcess(camp, index).SetCondition(() => !isUploadingToGameJolt),
                PageUtil.GetConfirmationGrid(
                    previous,
                    previous,
                    GetDeleteProcess(previous, index),
                    "Delete",
                    string.Format("Delete file {0}.", index),
                    string.Format("Are you sure you want to delete file {0}?\n{1}", index, saveName)
                    ).SetCondition(() => !isUploadingToGameJolt),
                GetExportPage(previous, index),
            };
            page.AddCharacters(Side.RIGHT, party.Collection);
            return page;
        }

        /// <summary>
        /// Restores a camp from serialized object.
        /// </summary>
        /// <param name="save">The save.</param>
        /// <param name="camp">The camp.</param>
        /// <param name="party">The party.</param>
        /// <param name="saveName">Name of the save.</param>
        private void RestoreCamp(WorldSave save, out Camp camp, out Party party, out string saveName) {
            World world = new World();
            world.InitFromSaveObject(save);
            party = world.Party;
            camp = new Camp(party, world.Flags);
            saveName = world.FileName;
        }

        /// <summary>
        /// Gets the import page used for importing saves from the cloud.
        /// </summary>
        /// <param name="previous">The previous page.</param>
        /// <param name="index">The index we're importing a save to.</param>
        /// <returns></returns>
        private Page GetImportPage(Page previous, int index) {
            Page p = new Page(Util.ColorString("Import", Color.grey));
            p.Body = "Type a key associated with a save for this game."
                + "\nImporting from saves created in a different version of the game may cause issues.";
            p.HasInputField = true;
            p.OnEnter = () => {
                p.Actions = new IButtonable[] {
                    PageUtil.GenerateBack(previous),
                    GetImportProcess(previous, p, index)
                };
            };
            return p;
        }

        /// <summary>
        /// Gets the import process.
        /// </summary>
        /// <param name="previous">The previous page.</param>
        /// <param name="current">The current page.</param>
        /// <param name="index">The save index to import into.</param>
        /// <returns></returns>
        private Process GetImportProcess(Page previous, Page current, int index) {
            return new Process(
                "Import",
                "Import a save from the inputted key.",
                () => {
                    LoadFromKey(previous, current, index, current.Input);
                }
                );
        }

        /// <summary>
        /// Loads the save from a save key associated with a value from gamejolt servers
        /// </summary>
        /// <param name="previous">The previous page.</param>
        /// <param name="current">The current page.</param>
        /// <param name="index">The current save index.</param>
        /// <param name="key">The key associated with a save.</param>
        private void LoadFromKey(Page previous, Page current, int index, string key) {
            current.AddText("Attempting to import from GameJolt...");
            GameJolt.API.DataStore.Get(key, true, possibleSave => {
                if (string.IsNullOrEmpty(possibleSave)) {
                    current.Invoke();
                    current.AddText(string.Format("No save associated with key {0}.", key));
                } else {
                    WorldSave gameSave = null;
                    try {
                        gameSave = SaveLoad.Load(possibleSave, possibleSave);
                    } catch (Exception e) {
                        current.AddText(e.Message);
                        return;
                    }
                    SaveLoad.Save(gameSave, index);
                    previous.Invoke();
                }
            });
        }

        /// <summary>
        /// Gets the export page.
        /// </summary>
        /// <param name="previous">The previous page.</param>
        /// <param name="index">The save index to export from.</param>
        /// <returns></returns>
        private Page GetExportPage(Page previous, int index) {
            Page p = new Page("Export");
            p.HasInputField = true;
            p.Body = "Type a key to associate with this save.";
            p.Actions = new IButtonable[] {
                PageUtil.GenerateBack(previous).SetCondition(() => !isUploadingToGameJolt),
                GetExportProcess(p, SaveLoad.GetSaveValue(index))
            };
            return p;
        }

        /// <summary>
        /// Gets the export process.
        /// </summary>
        /// <param name="current">The current page.</param>
        /// <param name="save">The save.</param>
        /// <returns></returns>
        private Process GetExportProcess(Page current, string save) {
            return new Process(
                "Export",
                string.Format("Associates this save file with the provided key. The key can then be used to retrieve the save."),
                () => {
                    current.AddText("Attempting to export...");
                    try {
                        AttemptToUploadSave(current, save, current.Input);
                    } catch (Exception e) {
                        current.AddText(e.Message);
                    }
                },
                () => !isUploadingToGameJolt
                );
        }

        /// <summary>
        /// Attempts to upload save. Asynchronously!
        /// </summary>
        /// <param name="current">The current page.</param>
        /// <param name="save">The save to upload.</param>
        /// <param name="key">The key to associate the save with.</param>
        private void AttemptToUploadSave(Page current, string save, string key) {
            isUploadingToGameJolt = true;
            GameJolt.API.DataStore.Contains(
                key,
                true,
                isExists => {
                    if (isExists) {
                        current.AddText("The specified key already exists.");
                    } else {
                        GameJolt.API.DataStore.Set(
                            key,
                            save,
                            true,
                            isSuccessful => {
                                if (isSuccessful) {
                                    current.AddText(string.Format("Save was successfully uploaded with key: {0}", key));
                                } else {
                                    current.AddText("Unable to upload key.");
                                }
                            }
                            );
                    }
                    isUploadingToGameJolt = false;
                }
                );
        }

        /// <summary>
        /// Gets the load process used to send the player to camp from a loaded save.
        /// </summary>
        /// <param name="camp">The camp.</param>
        /// <param name="index">The index.</param>
        /// <returns>A process that sends the player to camp.</returns>
        private Process GetLoadProcess(Camp camp, int index) {
            return new Process(
                "Load",
                "Load this save file.",
                () => {
                    camp.Invoke();
                    camp.Root.AddText(string.Format("Loaded from save file {0}.", index));
                }
                );
        }

        /// <summary>
        /// Gets the delete process used to delete saves.
        /// </summary>
        /// <param name="previous">The previous page.</param>
        /// <param name="index">The save index to delete.</param>
        /// <returns></returns>
        private Process GetDeleteProcess(Page previous, int index) {
            return new Process(
                "Delete",
                "Delete this save file.",
                () => {
                    SaveLoad.DeleteSave(index);
                    previous.Invoke();
                    previous.AddText(string.Format("File {0} was deleted.", index));
                }
                );
        }
    }
}
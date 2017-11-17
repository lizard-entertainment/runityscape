using Scripts.Model.SaveLoad.SaveObjects;
using Scripts.Model.TextBoxes;
using Scripts.Presenter;
using SimpleJSON;
using System;
using UnityEngine;

namespace Scripts.Game.Serialization {

    /// <summary>
    /// Utility class that allows saving and loading of data saved as JSONs.
    /// In order to ensure multiplatform support, we encrypt these JSONs
    /// and store them in PlayerPrefs.
    /// </summary>
    public static class SaveLoad {

        /// <summary>
        /// If true, saves are encrypted
        /// </summary>
        public const bool IS_ENCRYPT_VALUES = true;

        /// <summary>
        /// The maximum number of allowed save files
        /// </summary>
        public const int MAX_SAVE_FILES = 10;

        /// <summary>
        /// The prefix before a number in a save key
        /// </summary>
        private const string SAVE_PREFIX = "save_";

        /// <summary>
        /// The configuration folder name
        /// </summary>
        private const string CONFIG_FOLDER_NAME = "Config";

        /// <summary>
        ///
        /// The configuration file name
        /// </summary>
        private const string CONFIG_FILE_NAME = "config";

        /// <summary>
        /// The encryption key key
        /// </summary>
        private const string ENCRYPTION_KEY_KEY = "encryptionKey";

        /// <summary>
        /// The encryption key
        /// </summary>
        private static readonly string ENCRYPTION_KEY;

        /// <summary>
        /// Initializes the <see cref="SaveLoad"/> class.
        /// </summary>
        static SaveLoad() {
            JSONNode node = JSON.Parse(ConfigJSON);
            ENCRYPTION_KEY = node[ENCRYPTION_KEY_KEY];
        }

        /// <summary>
        /// Gets the configuration json.
        /// </summary>
        /// <value>
        /// The configuration json.
        /// </value>
        public static string ConfigJSON {
            get {
                TextAsset ta = Resources.Load<TextAsset>(string.Format("{0}/{1}", CONFIG_FOLDER_NAME, CONFIG_FILE_NAME));
                return ta.text;
            }
        }

        /// <summary>
        /// Prints the saves.
        /// </summary>
        public static void PrintSaves() {
            for (int i = 0; i < MAX_SAVE_FILES; i++) {
                PostMessage("Save index {0}\n{1}", GetSaveKey(i), GetSaveValue(i));
            }
        }

        /// <summary>
        /// Determines whether the index has a save in it.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>
        ///   <c>true</c> if [is save used] [the specified index]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsSaveUsed(int index) {
            return !string.IsNullOrEmpty(PlayerPrefs.GetString(GetSaveKey(index)));
        }

        /// <summary>
        /// Deletes all saves.
        /// </summary>
        public static void DeleteAllSaves() {
            for (int i = 0; i < MAX_SAVE_FILES; i++) {
                PlayerPrefs.DeleteKey(GetSaveKey(i));
            }
            Util.Log("Saves deleted.");
        }

        /// <summary>
        /// Loads the specified save index.
        /// </summary>
        /// <param name="saveIndex">Index of the save.</param>
        /// <param name="successNoun">The success noun.</param>
        /// <returns></returns>
        public static WorldSave Load(int saveIndex, string successNoun) {
            return Load(GetSaveValue(saveIndex), successNoun);
        }

        /// <summary>
        /// Loads the specified save.
        /// </summary>
        /// <param name="save">The save.</param>
        /// <param name="successNoun">The success noun.</param>
        /// <returns></returns>
        public static WorldSave Load(string save, string successNoun) {
            string json = null;
            try {
                string saveValue = save;
                if (IS_ENCRYPT_VALUES) {
                    json = DecryptString(saveValue);
                } else {
                    json = saveValue;
                }
            } catch (Exception e) {
                PostMessage(Util.ColorString(e.ToString(), Color.red));
                return null;
            }
            WorldSave load = JsonUtility.FromJson<WorldSave>(json);
            PostMessage("Load from {0} was successful.", successNoun);
            return load;
        }

        /// <summary>
        /// Saves the specified save object.
        /// </summary>
        /// <param name="saveObject">The save object.</param>
        /// <param name="saveIndex">Index of the save.</param>
        public static void Save(WorldSave saveObject, int saveIndex) {
            string json = JsonUtility.ToJson(saveObject);
            try {
                string modified = string.Empty;
                if (IS_ENCRYPT_VALUES) {
                    modified = EncryptString(json);
                } else {
                    modified = json;
                }
                SetSave(saveIndex, modified);
            } catch (Exception e) {
                PostMessage(Util.ColorString(e.ToString(), Color.red));
                return;
            }
            PostMessage("Saved successfully to slot {0}.", saveIndex);
        }

        /// <summary>
        /// Deletes the save.
        /// </summary>
        /// <param name="index">The index.</param>
        public static void DeleteSave(int index) {
            try {
                PlayerPrefs.DeleteKey(GetSaveKey(index));
            } catch (Exception e) {
                PostMessage(Util.ColorString(e.ToString(), Color.red));
                return;
            }
            PostMessage(string.Format("Save file {0} was deleted.", index));
        }

        /// <summary>
        /// Gets the save value.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public static string GetSaveValue(int index) {
            return PlayerPrefs.GetString(GetSaveKey(index));
        }

        /// <summary>
        /// Sets the save.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="value">The value.</param>
        private static void SetSave(int index, string value) {
            PlayerPrefs.SetString(GetSaveKey(index), value);
        }

        /// <summary>
        /// Gets the save key.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        private static string GetSaveKey(int index) {
            return string.Format("{0}{1}", SAVE_PREFIX, index);
        }

        /// <summary>
        /// Encrypts the string.
        /// </summary>
        /// <param name="jsonToEncrypt">The json to encrypt.</param>
        /// <returns></returns>
        private static string EncryptString(string jsonToEncrypt) {
            return Encrypter.Encrypt(jsonToEncrypt, ENCRYPTION_KEY);
        }

        /// <summary>
        /// Decrypts the string.
        /// </summary>
        /// <param name="encryptedJson">The encrypted json.</param>
        /// <returns></returns>
        private static string DecryptString(string encryptedJson) {
            return Encrypter.Decrypt(encryptedJson, ENCRYPTION_KEY);
        }

        /// <summary>
        /// Posts the message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        private static void PostMessage(string message, params object[] args) {
            Presenter.Main.Instance.TextBoxes.AddTextBox(new TextBox(string.Format(message, args)));
        }
    }
}
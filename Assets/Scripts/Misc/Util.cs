using Scripts.Model.Interfaces;
using Scripts.Model.Processes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class holds various helper methods that don't fit anywhere else
/// </summary>
public static class Util {
    public static readonly bool IS_DEBUG = false && Application.isEditor;
    private static char RANDOM_STRING_DELIMITER = '/';

    public static string PickRandom(string big) {
        string[] split = big.Split(RANDOM_STRING_DELIMITER);
        return split.ChooseRandom();
    }

    public static bool IsDictionariesEqual<A, B>(IDictionary<A, B> dic1, IDictionary<A, B> dic2) {
        return (dic1 == dic2) ||
             (dic1.Count == dic2.Count && !dic1.Except(dic2).Any());
    }

    public static T TypeToObject<T>(Type type) {
        return (T)Activator.CreateInstance(type);
    }

    public static Type StringToType(string s) {
        return Type.GetType(s);
    }

    /// <summary>
    /// From https://stackoverflow.com/questions/12172162/how-to-insert-item-into-list-in-order
    /// </summary>
    /// <typeparam name="T">Type of item</typeparam>
    /// <param name="list">List for item to be inserted</param>
    /// <param name="item">Item to be inserted into list</param>
    public static void InsertIntoAscendingList<T>(List<T> list, T item) {
        int index = list.BinarySearch(item);
        if (index < 0) {
            index = ~index;
        }
        list.Insert(index, item);
    }

    public static IButtonable[] UpdateArray<T>(IButtonable[] currentArray, Tuple tuple) {
        IButtonable[] array = currentArray;
        if (currentArray.Length - 1 < tuple.Index) {
            array = new IButtonable[tuple.Index];
            Array.Copy(currentArray, array, currentArray.Length);
        }
        array[tuple.Index] = tuple.Data;
        return array;
    }

    public static IButtonable[] GetArray(params Tuple[] indicies) {
        return GetArray(indicies, SubGrid.DEFAULT_BUTTON_COUNT);
    }

    private static IButtonable[] GetArray(Tuple[] indices, int smallestAllowedSize = 0) {
        int size = Mathf.Max(indices.Max(t => t.Index) + 1, smallestAllowedSize);
        IButtonable[] array = new IButtonable[size];
        HashSet<int> indexSet = new HashSet<int>();
        foreach (Tuple t in indices) {
            if (indexSet.Contains(t.Index)) {
                throw new UnityException(string.Format("Index collision detected at index: {0}. Tried to place {1}, but {2} was already there.", t.Data.ButtonText, array[t.Index].ButtonText));
            }
            indexSet.Add(t.Index);
            array[t.Index] = t.Data;
        }
        return array;
    }

    /// <summary>
    /// From
    /// https://stackoverflow.com/questions/13049732/automatically-rename-a-file-if-it-already-exists-in-windows-way
    /// </summary>
    public static string GetUniqueFilePath(string filepath) {
        if (File.Exists(filepath)) {
            string folder = Path.GetDirectoryName(filepath);
            string filename = Path.GetFileNameWithoutExtension(filepath);
            string extension = Path.GetExtension(filepath);
            int number = 1;

            Match regex = Regex.Match(filepath, @"(.+) \((\d+)\)\.\w+");

            if (regex.Success) {
                filename = regex.Groups[1].Value;
                number = int.Parse(regex.Groups[2].Value);
            }

            do {
                number++;
                filepath = Path.Combine(folder, string.Format("{0} ({1}){2}", filename, number, extension));
            }
            while (File.Exists(filepath));
        }

        return filepath;
    }

    public static T StringToObject<T>(string className) {
        return (T)Activator.CreateInstance(Type.GetType(className));
    }

    public static string GetClassName(object o) {
        return o.GetType().AssemblyQualifiedName;
    }

    public static string Sign(float num) {
        return num.ToString("+#;-#;0");
    }

    public static void Swap(GameObject a, GameObject b) {
        int ai = a.transform.GetSiblingIndex();
        a.transform.SetSiblingIndex(b.transform.GetSiblingIndex());
        b.transform.SetSiblingIndex(ai);
    }

    public static void Log(string message) {
        if (IS_DEBUG) {
            UnityEngine.Debug.Log(message);
        }
    }

    public static void SetAllColorOfChildren(GameObject parent, Color color) {
        Image[] images = parent.GetComponentsInChildren<Image>();
        Text[] texts = parent.GetComponentsInChildren<Text>();

        foreach (Image i in images) {
            i.color = color;
        }
        foreach (Text t in texts) {
            t.color = color;
        }
    }

    public static void SetAllAlphaOfChildren(GameObject parent, float alpha) {
        Image[] images = parent.GetComponentsInChildren<Image>();
        Text[] texts = parent.GetComponentsInChildren<Text>();

        foreach (Image i in images) {
            Util.SetImageAlpha(i, alpha);
        }
        foreach (Text t in texts) {
            Util.SetTextAlpha(t, alpha);
        }
    }

    public static int RandomRange(int min, int max) {
        return UnityEngine.Random.Range(min, max + 1);
    }

    public static int Random(int mean, float variance) {
        return (int)UnityEngine.Random.Range(mean * (1 - variance), mean * (1 + variance));
    }

    public static bool IsExactly<T>(this object obj) where T : class {
        return obj != null && obj.GetType() == typeof(T);
    }

    public static Sprite TextureToSprite(Texture2D texture) {
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }

    public static bool IsChance(double probability) {
        return UnityEngine.Random.value < probability;
    }

    public static int Range(double min, double max) {
        return (int)UnityEngine.Random.Range((float)min, (float)max);
    }

    public static void SetTextAlpha(Text target, float alpha) {
        if (target != null) {
            Color c = target.color;
            c.a = alpha;
            target.color = c;
        }
    }

    public static void SetOutlineAlpha(Outline target, float alpha) {
        Color c = target.effectColor;
        c.a = alpha;
        target.effectColor = c;
    }

    public static void SetImageAlpha(Image target, float alpha) {
        Color c = target.color;
        c.a = alpha;
        target.color = c;
    }

    public static void Parent(GameObject child, GameObject parent) {
        child.transform.SetParent(parent.transform);
        //child.transform.localPosition = new Vector3(0, 0, 0);
    }

    public static void Unparent(GameObject child) {
        child.transform.SetParent(null);
    }

    public static void Parent(IList<GameObject> child, GameObject parent) {
        foreach (GameObject myG in child) {
            GameObject g = myG;
            Parent(g, parent);
        }
    }

    public static GameObject FindChild(GameObject parent, string childName) {
        return parent.transform.Find(childName).gameObject;
    }

    /**
     * Converts an AudioClip to an AudioSource
     */

    public static AudioSource ClipToSource(AudioClip clip) {
        AudioSource source = new AudioSource();
        source.clip = clip;
        return source;
    }

    public static void Assert(bool statement, string message = "Expected value: true") {
        if (!statement) {
            throw new UnityException(message);
        }
    }

    public static Color InvertColor(Color color) {
        return new Color(1.0f - color.r, 1.0f - color.g, 1.0f - color.b);
    }

    private static char[] splitChars = new char[] { ' ', '-', '\t', '\n' };

    public static string WordWrap(string str, int width) {
        string[] words = Explode(str, splitChars);

        int curLineLength = 0;
        StringBuilder strBuilder = new StringBuilder();
        for (int i = 0; i < words.Length; i += 1) {
            string word = words[i];
            // If adding the new word to the current line would be too long,
            // then put it on a new line (and split it up if it's too long).
            if (curLineLength + word.Length > width) {
                // Only move down to a new line if we have text on the current line.
                // Avoids situation where wrapped whitespace causes emptylines in text.
                if (curLineLength > 0) {
                    strBuilder.Append(Environment.NewLine);
                    curLineLength = 0;
                }

                // If the current word is too long to fit on a line even on it's own then
                // split the word up.
                while (word.Length > width) {
                    strBuilder.Append(word.Substring(0, width - 1) + "-");
                    word = word.Substring(width - 1);

                    strBuilder.Append(Environment.NewLine);
                }

                // Remove leading whitespace from the word so the new line starts flush to the left.
                word = word.TrimStart();
            }
            strBuilder.Append(word);
            curLineLength += word.Length;
        }

        return strBuilder.ToString();
    }

    private static string[] Explode(string str, char[] splitChars) {
        List<string> parts = new List<string>();
        int startIndex = 0;
        while (true) {
            int index = str.IndexOfAny(splitChars, startIndex);

            if (index == -1) {
                parts.Add(str.Substring(startIndex));
                return parts.ToArray();
            }

            string word = str.Substring(startIndex, index - startIndex);
            char nextChar = str.Substring(index, 1)[0];
            // Dashes and the likes should stick to the word occuring before it. Whitespace doesn't have to.
            if (char.IsWhiteSpace(nextChar)) {
                parts.Add(word);
                parts.Add(nextChar.ToString());
            } else {
                parts.Add(word + nextChar);
            }

            startIndex = index + 1;
        }
    }

    public static string ColorString(string s, Color c) {
        return string.Format("<color={1}>{0}</color>", s, RGBToHex(c));
    }

    public static string ColorString(string s, bool isWhite) {
        return Util.ColorString(s, isWhite ? UnityEngine.Color.white : UnityEngine.Color.red);
    }

    public static string RGBToHex(Color color) {
        float redValue = color.r;
        float greenValue = color.g;
        float blueValue = color.b;
        float alpha = color.a;

        //1 checking is for strange [0-1] interval formatting of numbers. Otherwise 256 format
        return (string.Format("#{0}{1}{2}{3}",
            ((int)(redValue <= 1 ? redValue * 255 : redValue)).ToString("X2"),
            ((int)(greenValue <= 1 ? greenValue * 255 : greenValue)).ToString("X2"),
            ((int)(blueValue <= 1 ? blueValue * 255 : blueValue)).ToString("X2"),
            ((int)(alpha <= 1 ? alpha * 255 : alpha)).ToString("X2")));
    }

    public static T ChooseRandom<T>(params T[] items) {
        return items.ChooseRandom();
    }

    public static T ChooseRandom<T>(this IEnumerable<T> source) {
        if (source.Count() > 0) {
            return source.ElementAt(rng.Next(0, source.Count()));
        } else {
            return default(T);
        }
    }

    private static readonly System.Random rng = new System.Random();

    public static Sprite GetSprite(string name) {
        Sprite sprite = Resources.Load<Sprite>(string.Format("Images/Vector/{0}", name));
        if (sprite == null) {
            sprite = Resources.Load<Sprite>(string.Format("Images/Pixel/{0}", name));
        }
        Util.Assert(sprite != null, "Sprite from: " + name + " was null.");
        return sprite;
    }

    /**
     * Converts 1 to A, 2 to B, and so on
     */

    public static string IntToLetter(int column) {
        column++;
        string columnString = "";
        decimal columnNumber = column;
        while (columnNumber > 0) {
            decimal currentLetterNumber = (columnNumber - 1) % 26;
            char currentLetter = (char)(currentLetterNumber + 65);
            columnString = currentLetter + columnString;
            columnNumber = (columnNumber - (currentLetterNumber + 1)) / 26;
        }
        return columnString;
    }

    public static string GetEnumDescription(Enum value) {
        FieldInfo fi = value.GetType().GetField(value.ToString());

        DescriptionAttribute[] attributes =
            (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

        if (attributes != null && attributes.Length > 0)
            return attributes[0].Description;
        else
            return value.ToString();
    }

    public static TKey GetKey<TKey>(this Dictionary<TKey, string> dictionary, string value) {
        return dictionary.FirstOrDefault(x => x.Value.Equals(value)).Key;
    }

    public static string GetDescription(this Enum value) {
        FieldInfo field = value.GetType().GetField(value.ToString());
        object[] attribs = field.GetCustomAttributes(typeof(DescriptionAttribute), true);
        if (attribs.Length > 0) {
            return ((DescriptionAttribute)attribs[0]).Description;
        }
        return string.Empty;
    }

    public static bool EqualsIgnoreCase(this char a, char b) {
        return char.ToLower(a) == char.ToLower(b);
    }

    public static T[] EnumAsArray<T>() {
        return Enum.GetValues(typeof(T)).Cast<T>().ToArray();
    }

    public static string Color(this int num, Color color) {
        return Util.ColorString(num.ToString(), color);
    }

    public static Color SetAlpha(this Color color, float alpha) {
        return new UnityEngine.Color(color.r, color.g, color.b, alpha);
    }

    public static float ConvertToPercent(this int integer) {
        return (integer + 0.0f) / 100f;
    }

    // Modified to get keys from https://stackoverflow.com/questions/1028136/random-entry-from-dictionary
    public static IEnumerable<TKey> RandomValues<TKey, TValue>(this IDictionary<TKey, TValue> dict) {
        System.Random rand = new System.Random();
        List<TKey> keys = Enumerable.ToList(dict.Keys);
        int size = dict.Count;
        while (true) {
            yield return keys[rand.Next(size)];
        }
    }
}

public static class IListExtensions {

    /// <summary>
    /// Shuffles the element order of the specified list.
    /// </summary>
    public static void Shuffle<T>(this IList<T> ts) {
        var count = ts.Count;
        var last = count - 1;
        for (var i = 0; i < last; ++i) {
            var r = UnityEngine.Random.Range(i, count);
            var tmp = ts[i];
            ts[i] = ts[r];
            ts[r] = tmp;
        }
    }
}
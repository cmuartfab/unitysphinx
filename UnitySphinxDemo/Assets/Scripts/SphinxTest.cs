using UnityEngine;
using ui=UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;


public class SphinxTest : MonoBehaviour {

    [SerializeField] GameObject cat;
    [SerializeField] GameObject dog;
    [SerializeField] GameObject human;
    [SerializeField] GameObject horse;
    [SerializeField] GameObject mouse;
    [SerializeField] GameObject monkey;
    [SerializeField] Transform spawn;

    //SphinxProcessor processor = new SphinxProcessor();
    ui::Text text;

    readonly static Dictionary<string,int> numbers =
        new Dictionary<string,int> {
            { "zero", 0 },
            { "one", 1 },
            { "two", 2 },
            { "three", 3 },
            { "four", 4 },
            { "five", 5 },
            { "six", 6 },
            { "seven", 7 },
            { "eight", 8 },
            { "nine", 9 },
            { "ten", 10 },
            { "eleven", 11 },
            { "twelve", 12 },
            { "thirteen", 13 },
            { "fourteen", 14 },
            { "fifteen", 15 },
            { "sixteen", 16 },
            { "seventeen", 17 },
            { "eightteen", 18 },
            { "nineteen", 19 },
            { "twenty", 20 },
            { "thirty", 30 },
            { "forty", 40 },
            { "fifty", 50 },
            { "sixty", 60 },
            { "seventy", 70 },
            { "eighty", 80 },
            { "ninety", 90 },
            { "hundred", 100 },
            { "thousand", 1000 },
            { "million", 1000000 },
            { "billion", 1000000000 }};

    void Awake() {
        text = GetComponentInChildren<ui::Text>();
    }


    void Start() {
        UnitySphinx.Init();
        UnitySphinx.Run();
    }


    string Process(string s) {
        var digits = new Regex(
            @"\b(zero|one|two|three|four|five|six|seven|eight|nine)\b");
        var animals = new Regex(
            @"\b(cats|dogs|mice|humans|chains|monkeys|horses)\b");
        var verbs = new Regex(
            @"\b(open|shut|get|take|find|steal)\b");
        var nouns = new Regex(
            @"\b(door|jar|lamp)\b");
        var result = "";
        foreach (var word in s.Split(' '))
            if (digits.IsMatch(word)) result += numbers[word]+" ";
            else if (animals.IsMatch(word)) result += word+" ";
            else if (verbs.IsMatch(word)) result += word+" ";
            else if (nouns.IsMatch(word)) result += word+" ";
        return result;
    }


    void Update() {
        var input = UnitySphinx.DequeueString();
        if (string.IsNullOrEmpty(input)) return;
        switch (UnitySphinx.Model) {
            case SearchModel.kws: //print("listening for keyword");
                UnitySphinx.Model = SearchModel.jsgf;
                text.text += "\nWhat do <i>you</i> want? ";
                break;
            case SearchModel.jsgf: //print("listening for order");
                var result = Process(input);
                text.text += input;
                var regex = new Regex(
                    pattern: @"(?<number>-?\d+)\s+(?<animal>\w+)",
                    options: RegexOptions.IgnoreCase);
                var match = regex.Match(result);
#if DEBUG
print(@"
result: "+result+@"
----------------------------------------
number: "+match.Groups["number"].Value+@"
animal: "+match.Groups["animal"].Value);
#endif
                if (!string.IsNullOrEmpty(match.Groups["number"].Value))
                    CreateAnimals(
                        n: int.Parse(match.Groups["number"].Value),
                        animal: interpretAnimal(match.Groups["animal"].Value));
                UnitySphinx.Model = SearchModel.kws;
                break;
        }
    }

    void CreateAnimals(int n, GameObject animal) {
        for (var i=0; i<n; ++i) Instantiate(
            original: animal,
            position: spawn.position+Random.insideUnitSphere*0.1f,
            rotation: spawn.rotation);
    }


    GameObject interpretAnimal(string s) {
        switch (s) {
            default:
            case "cats": return cat;
            case "dogs": return dog;
            case "horses": return horse;
            case "humans": return human;
            case "monkeys": return monkey;
            case "mice": return mouse;
        }
    }


    int interpretNum(string s) {
        switch (s) {
            default:
            case "zero": return 0;
            case "one": return 1;
            case "two": return 2;
            case "three": return 3;
            case "four": return 4;
            case "five": return 5;
            case "six": return 6;
            case "seven": return 7;
            case "eight": return 8;
            case "nine": return 9;
        }
    }
}






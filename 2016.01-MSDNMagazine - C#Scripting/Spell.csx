#r ".\Newtonsoft.Json.7.0.1\lib\net45\Newtonsoft.Json.dll"
#load "Mashape.csx"  // Sets a value for the string Mashape.Key

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class Spell
{
    [JsonProperty("original")]
    public string Original { get; set; }
    [JsonProperty("suggestion")]
    public string Suggestion { get; set; }

    [JsonProperty(PropertyName ="corrections")]
    private JObject InternalCorrections { get; set; }

    public IEnumerable<string> Corrections 
    { 
        get 
        {
            if (!IsCorrect)
            {
                return InternalCorrections?[Original].Select(
                    x => x.ToString()) ?? Enumerable.Empty<string>();
            }
            else return Enumerable.Empty<string>();
        } 
    }

    public bool IsCorrect
    {
        get { return Original == Suggestion; }
    }

    static public bool Check(string word, out IEnumerable<string> corrections)
    {
        Task <Spell> taskCorrections = CheckAsync(word);
        corrections = taskCorrections.Result.Corrections;
        return taskCorrections.Result.IsCorrect;

    }
    static public async Task<Spell> CheckAsync(string word)
    {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(
            $"https://montanaflynn-spellcheck.p.mashape.com/check/?text={ word }");
        request.Method = "POST";
        request.ContentType = "application/json";
        request.Headers = new WebHeaderCollection();
        // Mashape.Key is the string key available for Mashape for the montaflynn API.
        request.Headers.Add("X-Mashape-Key", Mashape.Key);


        using (HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse)
        {
            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception(String.Format(
                "Server error (HTTP {0}: {1}).",
                response.StatusCode,
                response.StatusDescription));
            using(Stream stream = response.GetResponseStream())
            using(StreamReader streamReader = new StreamReader(stream))
            {
                string strsb = await streamReader.ReadToEndAsync();
                Spell spell = Newtonsoft.Json.JsonConvert.DeserializeObject<Spell>(strsb);
                // Assume spelling was only requested on first word.
                return spell;
            }
        }
    }
}



Console.WriteLine("entrepreneur" == (await Spell.CheckAsync("entreprenuer")).Corrections.First());
Console.WriteLine((await Spell.CheckAsync("entrepreneur")).IsCorrect == true);
Console.WriteLine((await Spell.CheckAsync("entreprenuer")).IsCorrect == false);
Console.WriteLine((await Spell.CheckAsync("mispeled")).IsCorrect == false);
Console.WriteLine((await Spell.CheckAsync("misspelled")).IsCorrect == true);

            

static public class Mashape
{
    const string DefaultKey = "PUT MASHAPE KEY HERE";
	static public string Key
	{
		get
		{
            string result = Environment.GetEnvironmentVariable("MashapeSpellCheckKey")??DefaultKey;
            if(result == InvalidMashapeKey)
			{
				throw new Exception($"{nameof(DefaultKey)} is not set. Retrieve key from https://market.mashape.com/montanaflynn/spellcheck and assign it to {nameof(Mashape)}.{nameof(DefaultKey)}.");
			}
			
			return result;
		}
	
	}
    const string InvalidMashapeKey = "PUT MASHAPE KEY HERE"; 	
}
if(Test-Path "$($env:USERPROFILE)SetMashapeSpellCheckKey.ps1") {
	. SetMashapeSpellCheckKey.ps1
}
else {
	[string] $value = 
		Retrieve key from https://market.mashape.com/montanaflynn/spellcheck and assign it to Environment Variable MashapeSpellCheckKey;
	setenv -u MashapeSpellCheckKey $value;
}
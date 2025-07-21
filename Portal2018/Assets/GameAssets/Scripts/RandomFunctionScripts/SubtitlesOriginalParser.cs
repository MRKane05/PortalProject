using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SubtitleEntry
{
	public string fileName = "";
	public string subtitleLine = "";
	public Color subtitleColor = Color.white;
}

public class SubtitlesOriginalParser : MonoBehaviour {

	public TextAsset OriginalSubtitles;

	public List<SubtitleEntry> subtitles;

	// Use this for initialization
	void Start () {
		ParseTextFileIntoJson();
	}
	
	void ParseTextFileIntoJson ()
    {
		string[] lines = OriginalSubtitles.text.Split('\n');
		Debug.Log("Lines: " + lines.Length);
		for (int i=0; i<lines.Length; i++)
        {
			SubtitleEntry newSubtitle = new SubtitleEntry();
			bool bHasFileName = false;
			bool bHasColor = false;
			bool bHasText = false;
			//string[] sections = lines[i].Split(' ');
			lines[i] = lines[i].Replace("   ", "`");//A hopefully unused character to split the lines
			string[] sections = lines[i].Split('`');
			string filename = sections[0];  //Will have to sanitize the quotation marks
			filename = filename.Replace("\"", "");
			newSubtitle.fileName = filename;
			if (filename.Length > 3)
            {
				bHasFileName = true;
            }

			for (int p=1; p<sections.Length; p++)	//Go through and collect the rest of our information
            {
				if (sections[p].Contains("<clr"))	//We want to use this, but need to seperate things out and handle them
                {
					//Get the first set of colour information
					int ColorClassEnd = sections[p].IndexOf('>');
					string ColorString = sections[p].Substring(0, ColorClassEnd+1);
					//Thus we get this: <clr:219,112,147>
					ColorString = ColorString.Replace("<clr:", "");
					ColorString = ColorString.Replace(">", "");
					string[] sCols = ColorString.Split(',');
					float R, G, B;
					if (sCols.Length == 3)
					{
						float.TryParse(sCols[0], out R);
						float.TryParse(sCols[1], out G);
						float.TryParse(sCols[2], out B);

						Color subtitleColor = new Color(R / 255f, G / 255f, B / 255f);
						newSubtitle.subtitleColor = subtitleColor;
						bHasColor = true;
						//Ok, now we need to get the rest of the actual subtitle itself...
					}

					string SubtitleString = sections[p].Substring(ColorClassEnd + 1, sections[p].Length - (ColorClassEnd + 2));
					
					//Now this is almost fine, except we need to close bold sections if they happen
					if (SubtitleString.Contains("<B>")) {
						
						List<int> boldIndex = AllIndexesOf(SubtitleString, "<B>");
						if (boldIndex.Count % 2 == 0)	//We've got an even number. Not sure what we'll do if it's not
                        {
							//Need to start modifying things from the back forward so as not to screw up our string
							for (int b = boldIndex.Count -1; b> 0; b--)	//because reverse loops aren't ever catastrophic...
                            {
								if (b%2 == 1)
                                {
									SubtitleString = SubtitleString.Insert(boldIndex[b] + 1, "/");
                                }
                            }
                        }
					}
					//One final cleanup pass least there were trailing spaces
					SubtitleString = SubtitleString.Replace("\"", "");
					SubtitleString = SubtitleString.Trim();
					if (SubtitleString.Length > 2)
                    {
						bHasText = true;
						newSubtitle.subtitleLine = SubtitleString;
                    }
				}
			}
			subtitles.Add(newSubtitle);
        }
    }

	public static List<int> AllIndexesOf(string str, string value)
	{
		List<int> indexes = new List<int>();
		for (int index = 0; ; index += value.Length)
		{
			index = str.IndexOf(value, index);
			if (index == -1)
				return indexes;
			indexes.Add(index);
		}
	}
}

using GameFramework.Helper;
using UnityEngine;

public class DrawQueueItem
{
	public Vector3 position; // world position
	public bool start = false; // is user started to drag
	public bool end = false; // is user ended to drag
	public Vector3 velocity; // velocity of his drag
	public string character = null; // character over which user drag
	public bool touched = false; // is user touched or untouched character of the word

	public override string ToString()
	{
		return ToJson();
	}

	public DrawQueueItem()
	{

	}

	public DrawQueueItem(string json)
	{
		JSONObject item = JSONObject.Parse(json);

		position = Vector3Utils.StringToVector3(item.GetString("position"));
		start = item.GetBoolean("start");
		end = item.GetBoolean("end");
		velocity = Vector3Utils.StringToVector3(item.GetString("velocity"));

		if (string.IsNullOrEmpty(item.GetString("character")) == false)
		{
			character = item.GetString("character");
		}

		touched = item.GetBoolean("touched");
	}

	public string ToJson()
	{
		JSONObject json = new JSONObject();
		json.Add("position", new JSONValue(position.ToString()));
		json.Add("start", new JSONValue(start));
		json.Add("end", new JSONValue(end));
		json.Add("velocity", new JSONValue(velocity.ToString()));
		json.Add("character", new JSONValue(character));
		json.Add("touched", new JSONValue(touched));

		return json.ToString();
	}
}

using UnityEngine;
using System.Collections;
using GameFramework.Helper;
using System.Collections.Generic;

public class JsonUtils
{
    public static JSONArray ListToArray(List <string> listItems) {
        JSONArray array = new JSONArray();

        foreach ( string _item in listItems ) {
            array.Add(new JSONValue(_item));
        }

        return array;
    }
}

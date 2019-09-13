using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class LanguageUtils {

    public static string RealLanguage(string language) {
        if ( language == null ) {
            return language;
        }

        switch ( language.ToLower() ) {
            case "serbian":
            case "croatian":
            case "montenegro":
            case "slovak":
            case "bosnian":
                return "Serbian";

            case "portuguese":
            case "brazil":
                return "Portuguese";

            case "bulgarian":
            case "macedonian":
                return "Bulgarian";

            default:
                return language;
        }
    }

    public static string CountryCodeToLanguage(string country) {
        if (country == "bg")
        {
            return "Bulgarian";
        }

        if (country == "mk")
        {
            return "Macedonian";
        }

        // || country == "hr" || country == "me" || country == "sk" || country == "ba"
        if (country == "rs")
        {
            return "Serbian";
        }

        if ( country == "hr" ) 
        {
            return "Croatian";
        }

        if (country == "me")
        {
            return "Montenegro";
        }

        if (country == "sk")
        {
            return "Slovak";
        }

        if (country == "ba")
        {
            return "Bosnian";
        }

        if (country == "pt")
        {
            return "Portuguese";
        }

        if (country == "br")
        {
            return "Brazil";
        }

        return Application.systemLanguage.ToString();
    }
}

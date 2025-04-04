using UnityEngine;

namespace Warzoom.MapShot
{
    public class MapShotAvailabilityChecker : MonoBehaviour
    {
        [SerializeField] private bool _testEditor;
        
        public bool IsAvailable()
        {

#if UNITY_EDITOR || UNITY_STANDALONE
            //This will fake the feature availability on the editor
            //true : is enable on selected_options[3] on database
            //False : is disable
            var value = _testEditor;
#endif
            //Here check if the mapshot feature is turned on or off and return the the value true is on, false is off
            //Hook the web of this feature to the app here.
#if !UNITY_EDITOR && UNITY_WEBGL
            var value = GetMapShotSelectedValueFromUrl();


#endif

            return value;
        }

        /// <summary>
        /// Read the value from URL
        /// Check the function onLobby on Basic.Blade.Php file
        /// If the parameter value is 1 the map shot is enable on the selected options in database
        /// </summary>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        private bool GetMapShotSelectedValueFromUrl()
        {
            string mapShotOption = GetUrlParameter("map_shot");

            if (string.IsNullOrEmpty(mapShotOption)) return false;
            // Use the value as needed
            Debug.Log("Map Shot Option: " + mapShotOption);
            return (mapShotOption == "1");


        }
        
        
        public static string GetUrlParameter(string parameterName)
        {
            // Get the URL of the current page
            string url = Application.absoluteURL;

            // Parse the URL parameters
            string[] parameters = url.Split('?');
            if (parameters.Length > 1)
            {
                string[] keyValuePairs = parameters[1].Split('&');
                foreach (string pair in keyValuePairs)
                {
                    string[] keyValue = pair.Split('=');
                    if (keyValue.Length == 2 && keyValue[0] == parameterName)
                    {
                        // Decode the URL-encoded value
                        return System.Uri.UnescapeDataString(keyValue[1]);
                    }
                }
            }

            return null;
        }
    }
}
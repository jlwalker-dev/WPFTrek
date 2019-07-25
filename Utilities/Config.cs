using System;
using System.Configuration;

/*
 * Read, create, update the app.config file
 * 
 */
namespace WPFTrek.Utilities
{
    class Config
    {
        /*
         * return a value (or supplied default value) for an element in the app.config
         * and optionally add if missing 
         * 
         */
        public static string ReadElement(string key, string defaultValue, bool addIfMissing)
        {
            var appSettings = ConfigurationManager.AppSettings;
            var result=appSettings[key];

            if (result==null)
            {
                WriteToLog.write("Adding key");
                result = defaultValue;
                AddUpdateElement(key, defaultValue);
            }
            else
                WriteToLog.write("Key Found");


            return result;
        }


        /*
         * return a value (or supplied default value) for an element in the app.config
         * 
         */
        public static string ReadElement(string key, string defaultValue)
        {
            return ReadElement(key) ?? defaultValue;
        }

        /*
         * return a value (or null if not found) for an element in the app.config
         * 
         */
        public static string ReadElement(string key)
        {
            var appSettings = ConfigurationManager.AppSettings;
            return appSettings[key];
        }


        /*
         * Add or update a value for an element in the app.config
         * 
         */
        public static void AddUpdateElement(string key, string value)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;
                if (settings[key] == null)
                {
                    settings.Add(key, value);
                }
                else
                {
                    settings[key].Value = value;
                }

                WriteToLog.write("CONFIG UPDATE ==> key '" + key.ToString() + "' with value '" + value.ToString() + "'");

                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException)
            {
                WriteToLog.write("CONFIG ERROR ==> could not update key '" + key.ToString() + "' with value '" + value.ToString() + "'");
            }
        }
    }
}

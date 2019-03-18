using System;

namespace OpenAIONDPS
{
    class Registry
    {
        private const string KeyName = @"Software\OpenAIONDPS";
        private const string ChatLogPathKeyName = "ChatLogPath";
        private const string InstallDirectoryKeyName = "InstallDirectory";
        private const string SaveResultDirectoryKeyName = "SaveResultDirectory";
        private const string AlwaysOnTopKeyName = "AlwaysOnTop";
        private const string DeleteLogWhenStartingKeyName = "DeleteLogWhenStarting";
        private const string DPSHighLightKeyName = "DPSHighLight";

        public static string ReadChatLogPath()
        {
            try
            {
                return ReadValue<string>(ChatLogPathKeyName);
            }
            catch
            {
                return @"C:\Program Files (x86)\NCSoft\The Tower of AION\Chat.log";
            }
        }

        public static string ReadInstallDirectory()
        {
            try
            {
                return ReadValue<string>(InstallDirectoryKeyName);
            }
            catch
            {
                return @"C:\Program Files (x86)\NCSoft\The Tower of AION\";
            }
        }

        //
        public static string ReadSaveResultDirectory()
        {
            try
            {
                return ReadValue<string>(SaveResultDirectoryKeyName);
            }
            catch
            {
                return "";
            }
        }

        public static bool ReadAlwaysOnTop()
        {
            try
            {
                return ReadValue<bool>(AlwaysOnTopKeyName);
            }
            catch
            {
                return false;
            }
        }

        public static bool ReadDeleteLogWhenStarting()
        {
            try
            {
                return ReadValue<bool>(DeleteLogWhenStartingKeyName);
            }
            catch
            {
                return true;
            }
        }

        public static bool ReadDPSHighLight()
        {
            try
            {
                return ReadValue<bool>(DPSHighLightKeyName);
            }
            catch
            {
                return false;
            }
        }

        public static string ReadFirstMemberName(string DefaultName)
        {
            string FirstMemberName = "";

            try
            {
                FirstMemberName = Registry.ReadValue<string>("FirstMemberName");
                if (String.IsNullOrEmpty(FirstMemberName))
                {
                    return DefaultName;
                }
                else
                {
                    return FirstMemberName;
                }
            }
            catch
            {
                return DefaultName;
            }
        }

        public static string ReadFirstMemberJob()
        {
            try
            {
                return Registry.ReadValue<string>("FirstMemberJob");
            }
            catch
            {
                return "";
            }
        }

        public static string ReadOwnName(int ID)
        {
            try
            {
                return Registry.ReadValue<string>("OwnName" + ID.ToString("D3"));
            }
            catch
            {
                return "";
            }
        }

        public static string ReadMemberName(int ID)
        {
            try
            {
                return Registry.ReadValue<string>("MemberName" + ID.ToString("D3"));
            }
            catch
            {
                return "";
            }
        }

        public static string ReadMemberJob(int ID)
        {
            try
            {
                return Registry.ReadValue<string>("MemberJob" + ID.ToString("D3"));
            }
            catch
            {
                return "";
            }
        }

        private static T ReadValue<T>(string Key)
        {
            try
            {
                if (typeof(T) == typeof(bool))
                {
                    Int32 Value = (Int32)Microsoft.Win32.Registry.CurrentUser.OpenSubKey(KeyName, false).GetValue(Key);
                    if (Value == 0)
                    {
                        return (dynamic)false;
                    }
                    else
                    {
                        return (dynamic)true;
                    }
                }
                else
                {
                    return (T)Microsoft.Win32.Registry.CurrentUser.OpenSubKey(KeyName, false).GetValue(Key);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void WriteChatLogPath(string Value)
        {
            WriteValue<string>(ChatLogPathKeyName, Value);
        }

        public static void WriteInstallDirectory(string Value)
        {
            WriteValue<string>(InstallDirectoryKeyName, Value);
        }

        public static void WriteSaveResultDirectory(string Value)
        {
            WriteValue<string>(SaveResultDirectoryKeyName, Value);
        }

        public static void WriteAlwaysOnTop(bool Value)
        {
            WriteValue<bool>(AlwaysOnTopKeyName, Value);
        }

        public static void WriteDeleteLogWhenStarting(bool Value)
        {
            WriteValue<bool>(DeleteLogWhenStartingKeyName, Value);
        }

        public static void WriteDPSHighLight(bool Value)
        {
            WriteValue<bool>(DPSHighLightKeyName, Value);
        }

        public static void WriteFirstMemberName(string Name)
        {
            try
            {
                Registry.WriteValue("FirstMemberName", Name);
            }
            catch
            {
            }
        }

        public static void WriteFirstMemberJob(string Job)
        {
            try
            {
                Registry.WriteValue("FirstMemberJob", Job);
            }
            catch
            {
            }
        }

        public static void WriteOwnName(int ID, string Name)
        {
            try
            {
                Registry.WriteValue("OwnName" + ID.ToString("D3"), Name);
            }
            catch
            {
            }
        }

        public static void WriteMemberName(int ID, string Name)
        {
            try
            {
                Registry.WriteValue("MemberName" + ID.ToString("D3"), Name);
            }
            catch
            {
            }
        }

        public static void WriteMemberJob(int ID, string Job)
        {
            try
            {
                Registry.WriteValue("MemberJob" + ID.ToString("D3"), Job);
            }
            catch
            {
            }
        }

        private static void WriteValue<T>(string Key, T Value)
        {
            try
            {
                if (typeof(T) == typeof(string))
                {
                    Microsoft.Win32.Registry.CurrentUser.CreateSubKey(KeyName).SetValue(Key, (object)Value, Microsoft.Win32.RegistryValueKind.String);
                }
                else if (typeof(T) == typeof(bool))
                {
                    Microsoft.Win32.Registry.CurrentUser.CreateSubKey(KeyName).SetValue(Key, (object)Value, Microsoft.Win32.RegistryValueKind.DWord);
                }
                else
                {
                    Microsoft.Win32.Registry.CurrentUser.CreateSubKey(KeyName).SetValue(Key, (object)Value);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void ClearFavoriteMember()
        {
            string[] ValueNames = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(KeyName, false).GetValueNames();

            foreach (string ValueName in ValueNames)
            {
                if (ValueName.IndexOf("MemberName") == 0 || ValueName.IndexOf("MemberJob") == 0)
                {
                    Microsoft.Win32.Registry.CurrentUser.OpenSubKey(KeyName, true).DeleteValue(ValueName);
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using System.Diagnostics;

namespace Vodamep.Data
{



    /// <summary>
    /// Werte der Code Provider auslesen
    /// </summary>
    public class CodeProviderValueEnumerator
    {


        /// <summary>
        /// Code Provider Werte auflisten
        /// </summary>
        public List<CodeProviderValue> Enumerate()
        {

            Dictionary<string, string> textDictionary = GetTextDictionary();
            List<CodeProviderValue> result = MapTextDictionaryToProtoEnums(textDictionary);

            return result;
        }




        /// <summary>
        /// Proto Enums auslesen und die Texte dazumappen
        /// </summary>
        private List<CodeProviderValue> MapTextDictionaryToProtoEnums(Dictionary<string, string> textDictionary)
        {
            List<CodeProviderValue> result = new List<CodeProviderValue>();


            List<Type> reflectionTypes = Assembly.GetAssembly(typeof(CodeProviderBase))
                                  .GetTypes()
                                  .Where(p => p.Name.EndsWith("Reflection"))
                                  .ToList();



            foreach (Type messageType in reflectionTypes)
            {
                PropertyInfo[] properties = messageType.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

                foreach (PropertyInfo property in properties)
                {
                    if (property.Name == "Descriptor")
                    {
                        FileDescriptor fileDescriptor = property.GetValue(messageType, null) as FileDescriptor;
                        IList<EnumDescriptor> enumDescriptors = fileDescriptor.EnumTypes;

                        foreach (EnumDescriptor enumDescriptor in enumDescriptors)
                        {
                            // Für die hier gibt's Beschreibungstexte
                            if (enumDescriptor.ClrType != typeof(Vodamep.Hkpv.Model.ActivityType))
                            {

                                Type enumClrType = enumDescriptor.ClrType;
                                List<string> names = Enum.GetNames(enumClrType).ToList();


                                IList<EnumValueDescriptor> enumDescriptorValues = enumDescriptor.Values;
                                foreach (EnumValueDescriptor enumDescriptorValue in enumDescriptorValues)
                                {

                                    CodeProviderValue codeProviderValue = new CodeProviderValue()
                                    {
                                        ReportType = messageType,
                                        EnumType = enumClrType,
                                        EnumValue = names[enumDescriptorValue.Index],
                                        ProtoValue = enumDescriptorValue.Name,
                                        Text = "",
                                    };

                                    if (textDictionary.ContainsKey(codeProviderValue.ProtoValue))
                                    {
                                        codeProviderValue.Text = textDictionary[codeProviderValue.ProtoValue];
                                    }
                                    else
                                    {
                                        throw new Exception("Keinen Wert im csv gefunden.");
                                    }

                                    result.Add(codeProviderValue);
                                }
                            }
                        }
                    }
                }
            }

            return result;

        }






        /// <summary>
        /// Keys und Texte aus den csv Dateien in ein Dictionary schreiben
        /// </summary>
        private Dictionary<string, string> GetTextDictionary()
        {
            // Alle Texte in ein Dictionary mit Keys schreiben

            List<Type> codeProviderTypes = Assembly.GetAssembly(typeof(CodeProviderBase))
                                                   .GetTypes()
                                                   .Where(TheType => TheType.IsClass && !TheType.IsAbstract && TheType.IsSubclassOf(typeof(CodeProviderBase)))
                                                   .ToList();

            Dictionary<string, string> textDictionary = new Dictionary<string, string>();

            foreach (Type codeProviderType in codeProviderTypes)
            {
                if (codeProviderType.FullName.Contains("Relation"))
                {
                }

                CodeProviderBase baseProvider = Activator.CreateInstance(codeProviderType) as CodeProviderBase;

                if (baseProvider.IsEnumProvider)
                {
                    IReadOnlyDictionary<string, CodeData> values = baseProvider.Values;

                    foreach (KeyValuePair<string, CodeData> key in values)
                    {
                        if (!textDictionary.ContainsKey(key.Key))
                        {
                            textDictionary.Add(key.Key, key.Value.Description);
                        }
                        else
                        {
                            // doppelte Keys, das ist nicht so schön
                            // potenzielles Problem, dass ein Key in einer anderen Auflistung anders heißt
                        }
                    }
                }
            }

            return textDictionary;
        }

    }
}

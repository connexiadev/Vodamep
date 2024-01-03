using Google.Protobuf.Reflection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Vodamep.Data
{


    public class CodeData
    {

        public string Code { get; set; }

        public string Description { get; set; }


        /// <summary>
        /// Dieser Code ist nur noch gültig bis zu diesem Datum
        /// </summary>
        public DateTime? ValidTo { get; set; }



    }



    public abstract class CodeProviderBase
    {
        private static Regex _commentPattern = new Regex("//.*$");
        private IDictionary<string, CodeData> _dict = new Dictionary<string, CodeData>();

        protected CodeProviderBase()
        {
            this.Init();
        }

        public abstract string Unknown { get; }

        /// <summary>
        /// Prüfung, ob der Code überhaupt im Dictionary existiert
        /// </summary>
        public virtual bool IsValid(string code)
        {
            return _dict.ContainsKey(code ?? string.Empty);
        }


        /// <summary>
        /// Prüfung, ob der Code auch für ein bestimmtes Datum gültig ist
        /// </summary>
        public virtual bool IsStillValid(string code, DateTime date)
        {
            if (_dict.ContainsKey(code))
            {
                CodeData validDateCode = _dict[code];

                if (validDateCode.ValidTo == null)
                {
                    // Null Werte = immer gültig
                    return true;
                }
                else
                {
                    if (date > validDateCode.ValidTo)
                    {
                        // Beispiel:
                        // - Gültigkeitsdatum des Reports (date) = 01.01.2021
                        // - Gültig bis (ValidTo) = 31.12.2020

                        return false;
                    }

                    return true;
                }

            }
            else
            {
                return false;
            }
        }


        private void Init()
        {
            var assembly = this.GetType().Assembly;

            var resourceStream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.{this.ResourceName}");

            using (var reader = new StreamReader(resourceStream))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();

                    line = _commentPattern.Replace(line, string.Empty).Trim();

                    if (string.IsNullOrEmpty(line))
                        continue;

                    var values = line.Split(';');

                    CodeData validDateCode = new CodeData();

                    if (values.Length > 0)
                        validDateCode.Code = values[0];

                    if (values.Length > 1)
                        validDateCode.Description = values[1];

                    if (values.Length > 2)
                    {
                        try
                        {
                            if (!String.IsNullOrWhiteSpace(values[2]))
                                validDateCode.ValidTo = DateTime.ParseExact(values[2], "dd.MM.yyyy", CultureInfo.InvariantCulture);
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }


                    _dict.Add(validDateCode.Code, validDateCode);
                }
            }
        }



        /// <summary>
        /// Wert anhand eines CLR Enum Key ausgeben (im Dictionary sind die Proto-Keys hinterlegt)
        /// </summary>
        public string GetEnumValue(string code)
        {
            try
            {
                EnumDescriptor descriptor = this.Descriptor.EnumTypes.Where(x => x.Name == this.GetType().Name.Replace("Provider", "")).FirstOrDefault();
                Type clrType = descriptor?.ClrType;
                List<string> names = Enum.GetNames(clrType).ToList();
                int index = names.IndexOf(code);
                EnumValueDescriptor valueDescriptior = descriptor.Values[index];
                return this._dict[valueDescriptior.Name].Description;
            }
            catch (Exception exception)
            {
                throw new Exception($"Error reading enum value {code}.", exception);
            }
        }



        public IEnumerable<string> GetCSV() => _dict.Select(x => $"{x.Key};{x.Value.Description}");

        public IReadOnlyDictionary<string, CodeData> Values => new ReadOnlyDictionary<string, CodeData>(_dict);

        public IReadOnlyDictionary<string, string> ValuesOld => new ReadOnlyDictionary<string, string>(_dict.ToDictionary(pair => pair.Key, pair => pair.Value.Description));

        protected abstract string ResourceName { get; }

        protected abstract FileDescriptor Descriptor { get; }


        /// <summary>
        /// Das sind Enum Provider
        /// Wenn der Protobuff Descriptor nicht gesetzt ist, handelt es sich um von Länder, Orten, Versicherungen, ...
        /// </summary>
        public bool IsEnumProvider
        {
            get
            {
                if (Descriptor != null)
                    return true;

                return false;
            }
        }


        internal static CodeProviderBase GetInstance<T>()
            where T : CodeProviderBase
        {
            CodeProviderBase result = null;
            try
            {
                result = (CodeProviderBase)typeof(T).GetProperty("Instance", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).GetValue(null);
            }
            catch
            {
                throw new System.Exception($"CodeProviderBase.GetInstance<{typeof(T).Name}> failed!");
            }

            return result;

        }
    }
}

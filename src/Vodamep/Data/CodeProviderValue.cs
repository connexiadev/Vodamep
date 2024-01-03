using System;

namespace Vodamep.Data
{
    public class CodeProviderValue
    {
        public Type ReportType { get; set; }

        public Type EnumType { get; set; }

        public string EnumValue { get; set; }

        public string ProtoValue { get; set; }

        public string Text { get; set; }


        public override string ToString()
        {
            string result = $"{ReportType?.Name} / {EnumType?.Name} / {EnumValue}";
            if (!String.IsNullOrWhiteSpace(Text))
                result += " - " + Text;

            return result;
        }
    }
}

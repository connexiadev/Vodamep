using System;
using Google.Protobuf.Reflection;

namespace Vodamep.Data
{
    public sealed class Postcode_CityProvider : CodeProviderBase
    {
        private static volatile Postcode_CityProvider instance;
        private static object syncRoot = new Object();        

        public static Postcode_CityProvider Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new Postcode_CityProvider();
                    }
                }

                return instance;
            }
        }

        public override bool IsValid(string code)
        {
            return base.IsValid(code);
        }

        protected override FileDescriptor Descriptor => null;

        protected override string ResourceName => "Datasets.postcode_cities.csv";

        public override string Unknown => "";
    }
}

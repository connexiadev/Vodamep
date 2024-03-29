﻿using System;
using Google.Protobuf.Reflection;

namespace Vodamep.Data.Mkkp
{
    public class ActivityTypeProvider : CodeProviderBase
    {
        private static volatile ActivityTypeProvider instance;
        private static object syncRoot = new Object();

        public static ActivityTypeProvider Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new ActivityTypeProvider();
                    }
                }

                return instance;
            }
        }


        protected override FileDescriptor Descriptor => Vodamep.Mkkp.Model.MkkpReflection.Descriptor;

        public override string Unknown => "";

        protected override string ResourceName => "Datasets.Mkkp.ActivityType.csv";
    }
}

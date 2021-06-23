﻿using System;
using Vodamep.ReportBase;

namespace Vodamep.Tb.Model
{
    public partial class Person : ICountryPerson
    {
        public DateTime BirthdayD { get => this.Birthday.AsDate(); set => this.Birthday = value.AsTimestamp(); }
        public string GetDisplayName()
        {
            return this.Id;
        }
    }
}
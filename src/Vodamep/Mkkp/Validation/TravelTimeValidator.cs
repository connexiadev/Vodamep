using System;
using System.Linq;
using FluentValidation;
using Vodamep.Mkkp.Model;
using Vodamep.ReportBase;
using Vodamep.ValidationBase;


namespace Vodamep.Mkkp.Validation
{
    internal class TravelTimeValidator : AbstractValidator<ITravelTime>
    {
        private readonly MkkpReport report;

        #region Documentation
        // AreaDef: MKKP
        // OrderDef: 04
        // SectionDef: Fahrtzeit
        // StrengthDef: Fehler

        // CheckDef: Erlaubte Werte
        // Fields: Zeit, Remark: > 0
        #endregion


        public TravelTimeValidator(MkkpReport report)
        {
            MkkpDisplayNameResolver displayNameResolver = new MkkpDisplayNameResolver();

            this.report = report;
            this.RuleFor(x => x.Minutes).GreaterThan(0)
                .WithMessage(
                    x => Validationmessages.ReportBaseValueMustBeGreaterThanZero(displayNameResolver.GetDisplayName(nameof(x.Minutes)), 
                        displayNameResolver.GetDisplayName(nameof(TravelTime)), 
                        displayNameResolver.GetDisplayName(nameof(Staff)), 
                        this.GetStaff(x.StaffId)));
        }

        private string GetStaff(string id)
        {
            string staffName = id;

            var staff = this.report.Staffs.FirstOrDefault(p => p.Id == id);

            if (staff != null)
            {
                staffName = $"{staff.GivenName} {staff.FamilyName}";
            }

            return staffName;
        }
    }
}
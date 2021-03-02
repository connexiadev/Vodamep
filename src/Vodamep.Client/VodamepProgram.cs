﻿using PowerArgs;
using System;
using System.IO;
using Vodamep.Data;

namespace Vodamep.Client
{
    [ArgExceptionBehavior(ArgExceptionPolicy.StandardExceptionHandling)]
    [ArgDescription("(dmc) Daten-Meldungs-Client:")]
    public class VodamepProgram
    {
        private HandlerFactory handlerFactory = new HandlerFactory();

        [ArgActionMethod, ArgDescription("Absenden der Meldung.")]
        public void Send(SendArgs args)
        {
            HandlerBase handler = this.handlerFactory.CreateFromType(args.Type);
            handler.Send(args);
        }

        [ArgActionMethod, ArgDescription("Prüfung der Meldung.")]
        public void Validate(ValidateArgs args)
        {
            HandlerBase handler = this.handlerFactory.CreateFromType(args.Type);
            handler.Validate(args);
        }

        [ArgActionMethod, ArgDescription("Meldung neu verpacken.")]
        public void PackFile(PackFileArgs args)
        {
            HandlerBase handler = this.handlerFactory.CreateFromType(args.Type);
            handler.PackFile(args);
        }

        [ArgActionMethod, ArgDescription("Meldung mit Testdaten erzeugen.")]
        public void PackRandom(PackRandomArgs args)
        {
            var handler = this.handlerFactory.CreateFromType(args.Type);
            handler.PackRandom(args);
        }

        [ArgActionMethod, ArgDescription("Listet erlaubte Werte.")]
        public void List(ListArgs args)
        {
            CodeProviderBase provider = null;

            switch (args.Source)
            {
                case ListSources.Insurances:
                    provider = InsuranceCodeProvider.Instance;
                    break;
                case ListSources.CountryCodes:
                    provider = CountryCodeProvider.Instance;
                    break;
                case ListSources.Postcode_City:
                    provider = Postcode_CityProvider.Instance;
                    break;
                case ListSources.Qualifications:
                    provider = QualificationCodeProvider.Instance;
                    break;
                default:
                    HandleFailure($"Source '{args.Source}' not implemented.");
                    return;
            }

            foreach (var line in provider?.GetCSV())
                Console.WriteLine(line);
        }

        private Type CheckType(string typeName)
        {
            Type type;


            switch (typeName.ToLower())
            {
                case "agp":
                    type = Type.Agp;
                    break;

                case "hkpv":
                    type = Type.Hkpv;
                    break;

                case "mkkp":
                    type = Type.Mkkp;
                    break;

                default:
                    type = Type.Hkpv;
                    break;
            }


            return type;
        }

        protected void HandleFailure(string message = null)
        {
            throw new Exception(message);
        }

    }

}

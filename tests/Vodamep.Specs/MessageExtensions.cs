﻿using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Linq;

namespace Vodamep.Specs
{
    public static class MessageExtensions
    {
        public static void SetDefault(this IMessage m, string name)
        {
            var field = m.Descriptor.Fields.InDeclarationOrder().Where(x => x.Name == name).First();

            switch (field.FieldType)
            {
                case FieldType.String:
                    field.Accessor.SetValue(m, string.Empty);
                    break;
                case FieldType.Int64:
                case FieldType.Int32:
                case FieldType.SInt32:
                case FieldType.SInt64:
                case FieldType.UInt32:
                case FieldType.UInt64:
                case FieldType.SFixed32:
                case FieldType.SFixed64:
                case FieldType.Double:
                case FieldType.Float:
                case FieldType.Enum:
                    field.Accessor.SetValue(m, 0);
                    break;
                case FieldType.Message:
                    field.Accessor.SetValue(m, null);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }


        public static void SetValue(this IMessage m, string name, string value)
        {
            var field = m.GetField(name);

            switch (field.FieldType)
            {
                case FieldType.String:
                    field.Accessor.SetValue(m, value);
                    break;
                case FieldType.Int64:
                case FieldType.Int32:
                case FieldType.SInt32:
                case FieldType.SInt64:
                    field.Accessor.SetValue(m, int.Parse(value));
                    break;
                case FieldType.UInt32:
                case FieldType.UInt64:
                case FieldType.SFixed32:
                case FieldType.SFixed64:
                    field.Accessor.SetValue(m, long.Parse(value));
                    break;
                case FieldType.Enum:
                    var enumValue = System.Enum.TryParse(field.Accessor.Descriptor.EnumType.ClrType, value, out object r) ? r : 0;
                    field.Accessor.SetValue(m, enumValue);
                    
                    
                    break;
                case FieldType.Double:
                case FieldType.Float:
                    field.Accessor.SetValue(m, float.Parse(value, System.Globalization.CultureInfo.InvariantCulture));
                    break;
                case FieldType.Message:
                    if (field.MessageType == Timestamp.Descriptor)
                    {
                        field.Accessor.SetValue(m, Timestamp.FromDateTime(value.AsDate()));
                        break;
                    }

                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }
        }

        public static FieldDescriptor GetField(this IMessage m, string name)
        {
            var fields = m.Descriptor.Fields.InDeclarationOrder().Select(x => x.Name).ToList();

            return m.Descriptor.Fields.InDeclarationOrder().Where(x => x.Name == name).First();
        }
    }
}


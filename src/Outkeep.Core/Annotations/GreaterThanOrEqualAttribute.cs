using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Outkeep.Core.Annotations
{
    internal class GreaterThanOrEqualAttribute : CompareAttribute
    {
        public GreaterThanOrEqualAttribute(string otherProperty) : base(otherProperty)
        {
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // todo: throw proper exception
            if (!(value is IComparable comparable)) throw new InvalidOperationException();

            var property = validationContext.ObjectType.GetProperty(OtherProperty, BindingFlags.Public | BindingFlags.Instance);

            // todo: throw proper exception
            if (property == null) throw new InvalidOperationException();

            var other = property.GetValue(validationContext.ObjectInstance);

            return comparable.CompareTo(other) > 0
                ? ValidationResult.Success
                : new ValidationResult("Nope");
        }
    }
}
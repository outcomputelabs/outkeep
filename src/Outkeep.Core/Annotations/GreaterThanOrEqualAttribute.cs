using Outkeep.Core.Properties;
using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Outkeep.Core.Annotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    internal class GreaterThanOrEqualAttribute : CompareAttribute
    {
        public GreaterThanOrEqualAttribute(string otherProperty) : base(otherProperty)
        {
        }

        public override bool RequiresValidationContext => true;

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var otherType = validationContext.ObjectType.GetProperty(OtherProperty, BindingFlags.Public | BindingFlags.Instance);

            if (otherType is null)
                return new ValidationResult(Resources.Exception_CannotFindAPublicInstancePropertyNamed_X.Format(OtherProperty));

            var otherValue = otherType.GetValue(validationContext.ObjectInstance);

            try
            {
                return Comparer.Default.Compare(value, otherValue) >= 0
                    ? ValidationResult.Success
                    : new ValidationResult(Resources.Exception_Property_X_WithValue_X_MustBeGreaterThanOrEqualToProperty_X_WithValue_X
                        .Format(validationContext.MemberName, value, OtherProperty, otherValue));
            }
            catch (ArgumentException)
            {
                return new ValidationResult(Resources.Exception_CannotCompareProperties_X_And_X.Format(validationContext.MemberName, OtherProperty));
            }
        }
    }
}
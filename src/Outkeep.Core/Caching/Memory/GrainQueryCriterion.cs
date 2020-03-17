using ProtoBuf;
using static System.String;

namespace Outkeep.Caching.Memory
{
    [ProtoContract]
    [ProtoInclude(1, typeof(FilterCriterion))]
    internal abstract class GrainQueryCriterion
    {
    }

    [ProtoContract]
    internal class FilterCriterion : GrainQueryCriterion
    {
        protected FilterCriterion()
        {
            Name = Empty;
            Value = CriterionUndefinedValue.Default;
        }

        public FilterCriterion(string name, string? value)
        {
            Name = name;
            Value = new CriterionStringValue(value);
        }

        public FilterCriterion(string name, int? value)
        {
            Name = name;
            Value = new CriterionInt32Value(value);
        }

        [ProtoMember(1)]
        public string Name { get; protected set; }

        [ProtoMember(2)]
        public CriterionValue Value { get; protected set; }

        [ProtoMember(3)]
        public FilterType Type { get; protected set; }
    }

    [ProtoContract]
    internal enum FilterType
    {
        Undefined = 0,
        And = 1,
        Or = 2,
        Equal = 3,
        NotEqual = 4,
        LessThan = 5,
        LessThanOrEqual = 6,
        GreaterThan = 7,
        GreaterThanOrEqual = 8
    }

    [ProtoContract]
    [ProtoInclude(1, typeof(CriterionUndefinedValue))]
    [ProtoInclude(2, typeof(CriterionStringValue))]
    [ProtoInclude(3, typeof(CriterionInt32Value))]
    internal class CriterionValue
    {
    }

    [ProtoContract]
    internal class CriterionUndefinedValue : CriterionValue
    {
        public static readonly CriterionUndefinedValue Default = new CriterionUndefinedValue();
    }

    [ProtoContract]
    internal class CriterionStringValue : CriterionValue
    {
        protected CriterionStringValue()
        {
            Value = Empty;
        }

        public CriterionStringValue(string? value)
        {
            Value = value;
        }

        [ProtoMember(1)]
        public string? Value { get; protected set; }
    }

    [ProtoContract]
    internal class CriterionInt32Value : CriterionValue
    {
        protected CriterionInt32Value()
        {
        }

        public CriterionInt32Value(int? value)
        {
            Value = value;
        }

        [ProtoMember(1)]
        public int? Value { get; protected set; }
    }
}
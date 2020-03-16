using ProtoBuf;
using static System.String;

namespace Outkeep.Caching.Memory
{
    [ProtoContract]
    [ProtoInclude(1, typeof(WhereCriterion))]
    internal abstract class GrainQueryCriterion
    {
    }

    [ProtoContract]
    internal class WhereCriterion : GrainQueryCriterion
    {
        protected WhereCriterion()
        {
            Name = Empty;
            Value = CriterionUndefinedValue.Default;
        }

        public WhereCriterion(string name, string? value)
        {
            Name = name;
            Value = new CriterionStringValue(value);
        }

        [ProtoMember(1)]
        public string Name { get; protected set; }

        [ProtoMember(2)]
        public CriterionValue Value { get; protected set; }
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
using System.ComponentModel.DataAnnotations;

namespace Hookio.Shared
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    sealed public class EnumerableLength(int min, int max, string errorMessage) : ValidationAttribute(errorMessage)
    {
        private readonly int Min = min;
        private readonly int Max = max;

        public override bool IsValid(object? value)
        {
            if (value == null) return false;
            var val = (IEnumerable<object>)(value);
            if (val.Count() > Min && val.Count() < Max) return true;
            return false;
        }
    }
}

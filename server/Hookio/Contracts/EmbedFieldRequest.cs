using Discord;

namespace Hookio.Contracts
{
    public class EmbedFieldRequest
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public bool Inline { get; set; }
        public bool IsValid
        {
            get
            {
                return Value.Length <= EmbedFieldBuilder.MaxFieldValueLength && Name.Length <= EmbedFieldBuilder.MaxFieldNameLength;
            }
        }
    }
}

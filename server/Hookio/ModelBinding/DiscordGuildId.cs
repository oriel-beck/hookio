using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.RegularExpressions;

namespace Hookio.ModelBinding;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
public class DiscordGuildIdAttribute : ModelBinderAttribute
{
    public DiscordGuildIdAttribute() : base(typeof(DiscordGuildIdBinder))
    {
        Name = "guildId";
        BindingSource = BindingSource.Path;
    }
}

public partial class DiscordGuildIdBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var valueProviderResult = bindingContext.ValueProvider.GetValue("guildId");
        if (valueProviderResult == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        bindingContext.ModelState.SetModelValue("guildId", valueProviderResult);

        var valueAsString = valueProviderResult.FirstValue;
        if (string.IsNullOrEmpty(valueAsString))
        {
            return Task.CompletedTask;
        }

        if (!IsGuildId().IsMatch(valueAsString) || !ulong.TryParse(valueAsString, out var guildId))
        {
            bindingContext.ModelState.TryAddModelError("guildId", "Invalid Discord guild ID format.");
            return Task.CompletedTask;
        }

        bindingContext.Result = ModelBindingResult.Success(guildId);
        return Task.CompletedTask;
    }

    [GeneratedRegex(@"^\d{17,19}$")]
    private static partial Regex IsGuildId();
}

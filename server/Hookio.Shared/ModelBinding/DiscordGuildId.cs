using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.RegularExpressions;

namespace Hookio.Shared.ModelBindings;
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
public partial class DiscordGuildId : Attribute, IModelNameProvider
{
    public string Name => "guildId";

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var valueProviderResult = bindingContext.ValueProvider.GetValue(Name);
        if (valueProviderResult == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        bindingContext.ModelState.SetModelValue(Name, valueProviderResult);

        var valueAsString = valueProviderResult.FirstValue;
        if (string.IsNullOrEmpty(valueAsString))
        {
            return Task.CompletedTask;
        }

        // Validate the value as a valid Discord guild ID using regex
        if (!IsGuildId().IsMatch(valueAsString))
        {
            bindingContext.ModelState.TryAddModelError(Name, "Invalid Discord guild ID format.");
            return Task.CompletedTask;
        }

        bindingContext.Result = ModelBindingResult.Success(valueAsString);
        return Task.CompletedTask;
    }

    [GeneratedRegex(@"^\d{17,19}$")]
    private static partial Regex IsGuildId();
}

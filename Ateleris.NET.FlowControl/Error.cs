using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace Ateleris.NET.FlowControl;

public class Error
{
    protected Error() { }

    public ImmutableList<Error>? ChildErrors { get; init; } = null;

    private string Code => GetType().Name;

    public string Message { get; init; } = string.Empty;

    public bool IsComposite => ChildErrors is not null && ChildErrors.Count > 0;

    public override string ToString()
    {
        if (!IsComposite)
        {
            return string.IsNullOrWhiteSpace(Message) ? $"Error[{Code}]" : $"Error[{Code}]: {Message}";
        }

        Debug.Assert(ChildErrors is not null, "ChildErros must be initialized when the error is a composite error");

        return $"CompositeError[{Code}]: {Message}\n" +
               string.Join("\n", ChildErrors.Select((e, i) => $"  {i + 1}. {e}"));
    }
}

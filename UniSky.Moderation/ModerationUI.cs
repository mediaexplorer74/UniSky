using System.Collections.Generic;
using System.Linq;

namespace UniSky.Moderation;

public class ModerationUI(bool noOverride,
                    IList<ModerationCause> filters,
                    IList<ModerationCause> blurs,
                    IList<ModerationCause> alerts,
                    IList<ModerationCause> informs)
{
    public bool NoOverride { get; }
        = noOverride;
    public IReadOnlyList<ModerationCause> Filters { get; } 
        = [.. filters.OrderBy(p => p.Priority)];
    public IReadOnlyList<ModerationCause> Blurs { get; }
        = [.. blurs.OrderBy(p => p.Priority)];
    public IReadOnlyList<ModerationCause> Alerts { get; } 
        = [.. alerts.OrderBy(p => p.Priority)];
    public IReadOnlyList<ModerationCause> Informs { get; } 
        = [.. informs.OrderBy(p => p.Priority)];

    public bool Filter
        => Filters.Count > 0;
    public bool Blur
        => Blurs.Count > 0;
    public bool Alert
        => Alerts.Count > 0;
    public bool Inform
        => Informs.Count > 0;
}

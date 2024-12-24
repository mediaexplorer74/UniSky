using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FishyFlip;
using FishyFlip.Lexicon.App.Bsky.Labeler;
using UniSky.Moderation;
using Windows.ApplicationModel.Resources;

namespace UniSky.Services;

public record struct LabelStrings(string Name, string Description);

public class ModerationService(IProtocolService protocolService) : IModerationService
{
    private readonly ResourceLoader resources
        = ResourceLoader.GetForCurrentView();

    public ModerationOptions ModerationOptions { get; set; }

    public async Task ConfigureModerationAsync()
    {
        var protocol = protocolService.Protocol;
        var moderationPrefs = await protocol.GetModerationPrefsAsync()
            .ConfigureAwait(false);

        var labelDefs = await protocol.GetLabelDefinitionsAsync(moderationPrefs)
            .ConfigureAwait(false);

        await protocol.ConfigureLabelersAsync(moderationPrefs.Labelers)
            .ConfigureAwait(false);

        ModerationOptions = new ModerationOptions(protocol.Session.Did, moderationPrefs, labelDefs.LabelDefs);
    }

    public bool TryGetLocalisedStringsForLabel(InterpretedLabelValueDefinition labelDef, out LabelStrings label)
    {
        label = default;

        if (labelDef.DefinedBy == null)
        {
            // sanitise this for resource lookup
            var sanitisedIdentifier = labelDef.Identifier
                .ToUpperInvariant()
                .Replace('-', '_')
                .TrimStart('!');

            var nameResId = $"GlobalLabel_{sanitisedIdentifier}_Name";
            var descriptionResId = $"GlobalLabel_{sanitisedIdentifier}_Description";

            label = new LabelStrings(resources.GetString(nameResId), resources.GetString(descriptionResId));
            return true;
        }

        var locale = labelDef.Locales.FirstOrDefault();
        if (locale == null)
            return false;

        label = new LabelStrings(locale.Name, locale.Description);
        return true;
    }
}

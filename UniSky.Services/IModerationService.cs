using UniSky.Moderation;

namespace UniSky.Services
{
    public interface IModerationService
    {
        ModerationOptions ModerationOptions { get; set; }
    }
}
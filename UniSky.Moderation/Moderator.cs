using UniSky.Moderation.Decisions;

namespace UniSky.Moderation;

public class Moderator(ModerationOptions options)
{
    private readonly ModerationOptions options = options;

    public ModerationDecision ModerateProfile(ModerationSubjectProfile profile)
    {
        return ModerationDecision.Merge(
            AccountDecider.Decide(profile, options),
            ProfileDecider.Decide(profile, options));
    }

    public ModerationDecision ModeratePost(ModerationSubjectPost post)
        => PostDecider.Decide(post, options);

    public ModerationDecision ModerateNotification(ModerationSubjectNotification notification)
        => NotificationDecider.Decide(notification, options);

    public ModerationDecision ModerateFeedGenerator(ModerationSubjectFeedGenerator generator)
        => FeedGeneratorDecider.Decide(generator, options);

    public ModerationDecision ModerateUserList(ModerationSubjectUserList userList)
        => UserListDecider.Decide(userList, options);
}

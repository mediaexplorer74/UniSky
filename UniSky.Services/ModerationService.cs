using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniSky.Moderation;

namespace UniSky.Services;

public class ModerationService : IModerationService
{
    public ModerationOptions ModerationOptions { get; set; }
}

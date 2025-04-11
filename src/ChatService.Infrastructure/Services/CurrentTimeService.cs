using System;
using ChatService.Contract.Infrastructure.Services;

namespace ChatService.Infrastructure.Services;

public class CurrentTimeService : ICurrentTimeService
{
    public DateTime GetCurrentTime() => DateTime.UtcNow;
}
  
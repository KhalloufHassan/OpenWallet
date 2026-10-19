using OpenWallet.Mobile.Core.Lock;

namespace OpenWallet.Mobile.Core.Tests;

public sealed class AppLockStateTests
{
    [Fact]
    public void TheAppStartsLocked()
    {
        AppLockState state = new(new FakeTime(DateTimeOffset.UtcNow));

        Assert.True(state.IsLocked);
        Assert.True(state.NeedsUnlock);
    }

    [Fact]
    public void AShortTripToTheBackground_KeepsItUnlocked_ButHidesItMeanwhile()
    {
        FakeTime time = new(DateTimeOffset.UtcNow);
        AppLockState state = new(time);
        state.Unlock();

        state.OnBackground();
        Assert.True(state.IsHidden);
        Assert.False(state.NeedsUnlock);

        time.Now += TimeSpan.FromSeconds(90);
        state.OnForeground();

        Assert.False(state.IsLocked);
        Assert.False(state.IsHidden);
    }

    [Fact]
    public void TwoMinutesInTheBackground_LocksIt()
    {
        FakeTime time = new(DateTimeOffset.UtcNow);
        AppLockState state = new(time);
        state.Unlock();

        state.OnBackground();
        time.Now += AppLockState.GracePeriod;
        state.OnForeground();

        Assert.True(state.IsLocked);
        Assert.True(state.NeedsUnlock);
    }

    [Fact]
    public void TheTimeIsCountedFromWhenTheAppFirstLeftTheScreen()
    {
        FakeTime time = new(DateTimeOffset.UtcNow);
        AppLockState state = new(time);
        state.Unlock();

        state.OnBackground();
        time.Now += TimeSpan.FromMinutes(1);
        state.OnBackground();
        time.Now += TimeSpan.FromMinutes(1);
        state.OnForeground();

        Assert.True(state.IsLocked);
    }

    [Fact]
    public void LockingAndUnlocking_AreReported()
    {
        AppLockState state = new(new FakeTime(DateTimeOffset.UtcNow));
        int changes = 0;
        state.Changed += () => changes++;

        state.Unlock();
        state.Unlock();
        state.Lock();

        Assert.Equal(2, changes);
        Assert.True(state.IsLocked);
    }
}

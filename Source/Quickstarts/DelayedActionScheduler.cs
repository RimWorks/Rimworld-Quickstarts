using System;
using System.Collections.Generic;
using Verse;

namespace RimWorks.Quickstarts;

/// <summary>
/// Runs an action N ticks from now. A quickstart needs this because the map is not settled the
/// instant generation returns: pawns spawn over the first few ticks.
/// </summary>
public static class DelayedActionScheduler {
  private static readonly List<ScheduledAction> Scheduled = [];

  /// <summary>Queues an action to run after the given number of game ticks.</summary>
  /// <param name="action">What to run.</param>
  /// <param name="delayTicks">Ticks to wait. One second is <see cref="GenTicks.TicksPerRealSecond"/>.</param>
  public static void Schedule(Action action, int delayTicks) {
    Scheduled.Add(new ScheduledAction { ticksLeft = delayTicks, action = action });
  }

  /// <summary>Advances every queued action by one tick and runs the ones that came due.</summary>
  public static void Tick() {
    for (int i = Scheduled.Count - 1; i >= 0; i--) {
      ScheduledAction item = Scheduled[i];
      item.ticksLeft--;
      if (item.ticksLeft > 0) {
        continue;
      }

      try {
        item.action?.Invoke();
      } catch (Exception ex) {
        Logger.Error($"Scheduled action threw: {ex}");
      } finally {
        Scheduled.RemoveAt(i);
      }
    }
  }

  private sealed class ScheduledAction {
    public Action? action;
    public int ticksLeft;
  }

  /// <summary>Drives <see cref="Tick"/>. RimWorld finds and instantiates this itself.</summary>
#pragma warning disable S1144 // reported at compilation end, so .editorconfig cannot scope it
  private sealed class Driver : GameComponent {
    public Driver() { }

    public Driver(Game game) { }

    public override void GameComponentTick() {
      Tick();
    }
  }
#pragma warning restore S1144
}

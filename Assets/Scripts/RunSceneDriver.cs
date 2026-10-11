using System;

namespace InfernoJockey
{
    /// <summary>
    /// Scene adapter for one run. Owns the pure Run and advances it by scroll progress.
    /// Does not draw, buy, or own a mesh. Camera positioning is left to the scene.
    /// Each obstacle set is one scroll unit long. Progress carries the remainder across advances.
    /// </summary>
    public sealed class RunSceneDriver
    {
        public const float SetLength = 1f;

        RunSceneDriver(Run run, float progress)
        {
            Run = run ?? throw new ArgumentNullException(nameof(run));
            Progress = progress;
        }

        public Run Run { get; }
        public float Progress { get; }

        public static RunSceneDriver Start(int fuel, int ammo)
        {
            return new RunSceneDriver(Run.Start(fuel, ammo), 0f);
        }

        public RunSceneDriver Tap(int tappedLane)
        {
            if (Run.IsOver)
                return this;
            Run after = Run.Tap(tappedLane);
            if (ReferenceEquals(after, Run))
                return this;
            return new RunSceneDriver(after, Progress);
        }

        public RunSceneDriver Tick(float deltaTime)
        {
            if (Run.IsOver || deltaTime <= 0f)
                return this;

            float distance = Run.Speed.Speed * deltaTime;
            float nextProgress = Progress + distance;
            Run nextRun = Run;

            while (nextProgress >= SetLength && !nextRun.IsOver)
            {
                nextProgress -= SetLength;
                nextRun = nextRun.Advance();
            }

            if (nextRun.IsOver)
                nextProgress = 0f;

            if (ReferenceEquals(nextRun, Run) && nextProgress == Progress)
                return this;

            return new RunSceneDriver(nextRun, nextProgress);
        }
    }
}

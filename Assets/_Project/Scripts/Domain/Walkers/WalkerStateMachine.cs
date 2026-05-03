using System;

namespace HollowGround.Domain.Walkers
{
    public enum WalkerState
    {
        None,
        WalkingToTarget,
        WaitingAtTarget,
        ReturningHome,
        Resting
    }

    public class WalkerStateMachine
    {
        public WalkerState State { get; private set; } = WalkerState.None;
        public float WaitTimer { get; private set; }
        public float RestTimer { get; private set; }
        public bool HasJob { get; private set; }
        public (int x, int z)? HomeCell { get; private set; }
        public (int x, int z)? TargetCell { get; private set; }
        public float WaitDuration { get; private set; }
        public float RestDuration { get; private set; }

        public void SetRestDuration(float duration) => RestDuration = duration;
        public void SetWaitDuration(float duration) => WaitDuration = duration;
        public void SetHomeCell(int x, int z) => HomeCell = (x, z);

        public void AssignJob(int homeX, int homeZ, float workDuration)
        {
            HasJob = true;
            HomeCell = (homeX, homeZ);
            WaitDuration = workDuration;
        }

        public void ClearJob()
        {
            HasJob = false;
            TargetCell = null;
            State = WalkerState.None;
            RestTimer = 0f;
            WaitTimer = 0f;
        }

        public void StartWalkingToTarget((int x, int z) target)
        {
            TargetCell = target;
            State = WalkerState.WalkingToTarget;
            WaitTimer = 0f;
        }

        public void StartWalkingHome()
        {
            State = WalkerState.ReturningHome;
        }

        public void StartWaiting()
        {
            State = WalkerState.WaitingAtTarget;
            WaitTimer = 0f;
        }

        public void StartResting()
        {
            State = WalkerState.Resting;
            RestTimer = 0f;
        }

        public void Deactivate()
        {
            State = WalkerState.None;
            HasJob = false;
        }

        public TickResult Tick(float dt, float gameSpeed)
        {
            if (State == WalkerState.None) return TickResult.Idle;

            switch (State)
            {
                case WalkerState.WalkingToTarget:
                case WalkerState.ReturningHome:
                    return TickResult.Walking;

                case WalkerState.WaitingAtTarget:
                    WaitTimer += dt * gameSpeed;
                    if (WaitTimer >= WaitDuration)
                        return TickResult.WaitComplete;
                    return TickResult.Waiting;

                case WalkerState.Resting:
                    RestTimer += dt * gameSpeed;
                    if (RestTimer >= RestDuration)
                        return TickResult.RestComplete;
                    return TickResult.Resting;

                default:
                    return TickResult.Idle;
            }
        }

        public void OnPathComplete()
        {
            if (State == WalkerState.WalkingToTarget)
            {
                StartWaiting();
            }
            else if (State == WalkerState.ReturningHome)
            {
                if (HasJob)
                    StartResting();
                else
                    Deactivate();
            }
        }

        public WalkerStateSnapshot CaptureSnapshot((int x, int z) gridPos)
        {
            return new WalkerStateSnapshot
            {
                GridX = gridPos.x,
                GridZ = gridPos.z,
                State = State.ToString(),
                HomeCellX = HomeCell?.x ?? -1,
                HomeCellZ = HomeCell?.z ?? -1,
                WaitTimer = WaitTimer,
                RestTimer = RestTimer
            };
        }

        public void RestoreFromSnapshot(WalkerStateSnapshot snapshot)
        {
            if (snapshot.HomeCellX >= 0)
                HomeCell = (snapshot.HomeCellX, snapshot.HomeCellZ);

            if (Enum.TryParse<WalkerState>(snapshot.State, out var state))
                State = state;
            else
                State = WalkerState.None;

            WaitTimer = snapshot.WaitTimer;
            RestTimer = snapshot.RestTimer;
        }
    }

    public enum TickResult
    {
        Idle,
        Walking,
        Waiting,
        WaitComplete,
        Resting,
        RestComplete
    }

    [System.Serializable]
    public class WalkerStateSnapshot
    {
        public int GridX;
        public int GridZ;
        public string State;
        public int HomeCellX = -1;
        public int HomeCellZ = -1;
        public float WaitTimer;
        public float RestTimer;
    }
}

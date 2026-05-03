using HollowGround.Domain.Walkers;
using NUnit.Framework;

namespace HollowGround.Tests
{
    [TestFixture]
    public class WalkerStateMachineTests
    {
        [Test]
        public void InitialState_IsNone()
        {
            var sm = new WalkerStateMachine();
            Assert.AreEqual(WalkerState.None, sm.State);
        }

        [Test]
        public void StartWalkingToTarget_TransitionsState()
        {
            var sm = new WalkerStateMachine();
            sm.StartWalkingToTarget((5, 3));
            Assert.AreEqual(WalkerState.WalkingToTarget, sm.State);
        }

        [Test]
        public void OnPathComplete_FromWalkingToTarget_StartsWaiting()
        {
            var sm = new WalkerStateMachine();
            sm.SetWaitDuration(5f);
            sm.StartWalkingToTarget((5, 3));
            sm.OnPathComplete();
            Assert.AreEqual(WalkerState.WaitingAtTarget, sm.State);
        }

        [Test]
        public void OnPathComplete_FromReturningHome_WithJob_StartsResting()
        {
            var sm = new WalkerStateMachine();
            sm.AssignJob(0, 0, 8f);
            sm.StartWalkingToTarget((5, 3));
            sm.OnPathComplete();
            sm.StartWalkingHome();
            sm.OnPathComplete();
            Assert.AreEqual(WalkerState.Resting, sm.State);
        }

        [Test]
        public void OnPathComplete_FromReturningHome_NoJob_Deactivates()
        {
            var sm = new WalkerStateMachine();
            sm.StartWalkingToTarget((5, 3));
            sm.OnPathComplete();
            sm.StartWalkingHome();
            sm.OnPathComplete();
            Assert.AreEqual(WalkerState.None, sm.State);
        }

        [Test]
        public void TickWaiting_AccumulatesTime()
        {
            var sm = new WalkerStateMachine();
            sm.SetWaitDuration(5f);
            sm.StartWaiting();
            sm.Tick(2f, 1f);
            sm.Tick(2f, 1f);
            var result = sm.Tick(2f, 1f);
            Assert.AreEqual(TickResult.WaitComplete, result);
        }

        [Test]
        public void TickWaiting_GameSpeed2X()
        {
            var sm = new WalkerStateMachine();
            sm.SetWaitDuration(4f);
            sm.StartWaiting();
            var result = sm.Tick(2f, 2f);
            Assert.AreEqual(TickResult.WaitComplete, result);
        }

        [Test]
        public void TickResting_RestartsWorkCycle()
        {
            var sm = new WalkerStateMachine();
            sm.AssignJob(0, 0, 8f);
            sm.SetRestDuration(3f);
            sm.StartResting();

            var result = sm.Tick(3f, 1f);
            Assert.AreEqual(TickResult.RestComplete, result);
        }

        [Test]
        public void TickWalking_ReturnsWalking()
        {
            var sm = new WalkerStateMachine();
            sm.StartWalkingToTarget((5, 3));
            var result = sm.Tick(0.016f, 1f);
            Assert.AreEqual(TickResult.Walking, result);
        }

        [Test]
        public void TickNone_ReturnsIdle()
        {
            var sm = new WalkerStateMachine();
            var result = sm.Tick(0.016f, 1f);
            Assert.AreEqual(TickResult.Idle, result);
        }

        [Test]
        public void ClearJob_ResetsState()
        {
            var sm = new WalkerStateMachine();
            sm.AssignJob(2, 3, 8f);
            sm.StartWalkingToTarget((5, 5));
            sm.ClearJob();
            Assert.AreEqual(WalkerState.None, sm.State);
            Assert.IsFalse(sm.HasJob);
        }

        [Test]
        public void Snapshot_RoundTrip()
        {
            var sm = new WalkerStateMachine();
            sm.AssignJob(1, 2, 8f);
            sm.SetRestDuration(5f);
            sm.StartResting();
            sm.Tick(2f, 1f);

            var snapshot = sm.CaptureSnapshot((3, 4));

            var sm2 = new WalkerStateMachine();
            sm2.RestoreFromSnapshot(snapshot);

            Assert.AreEqual(WalkerState.Resting, sm2.State);
            Assert.AreEqual(1, sm2.HomeCell?.x);
            Assert.AreEqual(2, sm2.HomeCell?.z);
        }

        [Test]
        public void FullWorkCycle()
        {
            var sm = new WalkerStateMachine();
            sm.AssignJob(0, 0, 3f);
            sm.SetRestDuration(2f);

            sm.StartWalkingToTarget((5, 0));
            Assert.AreEqual(WalkerState.WalkingToTarget, sm.State);

            sm.OnPathComplete();
            Assert.AreEqual(WalkerState.WaitingAtTarget, sm.State);

            sm.Tick(3f, 1f);
            sm.StartWalkingHome();
            Assert.AreEqual(WalkerState.ReturningHome, sm.State);

            sm.OnPathComplete();
            Assert.AreEqual(WalkerState.Resting, sm.State);

            sm.Tick(2f, 1f);
        }
    }
}

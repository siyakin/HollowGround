using System.Collections.Generic;
using HollowGround.Domain.Combat;
using NUnit.Framework;

namespace HollowGround.Tests
{
    [TestFixture]
    public class BattleCalcTests
    {
        private Dictionary<int, int> MakeTroops(params (int type, int count)[] entries)
        {
            var dict = new Dictionary<int, int>();
            foreach (var (type, count) in entries)
                dict[type] = count;
            return dict;
        }

        [Test]
        public void StrongerAttackerWins()
        {
            var attacker = new BattleCalc.BattleSide
            {
                Troops = MakeTroops((0, 100)),
                Morale = 1f
            };
            var defender = new BattleCalc.BattleSide
            {
                Troops = MakeTroops((0, 10)),
                Morale = 1f
            };

            var result = BattleCalc.Calculate(attacker, defender, new System.Random(42));
            Assert.IsTrue(result.AttackerWins);
        }

        [Test]
        public void WeakerAttackerLoses()
        {
            var attacker = new BattleCalc.BattleSide
            {
                Troops = MakeTroops((0, 5)),
                Morale = 1f
            };
            var defender = new BattleCalc.BattleSide
            {
                Troops = MakeTroops((0, 50)),
                Morale = 1f
            };

            var result = BattleCalc.Calculate(attacker, defender, new System.Random(42));
            Assert.IsFalse(result.AttackerWins);
        }

        [Test]
        public void SurvivorsNeverExceedTroops()
        {
            var rng = new System.Random(123);
            for (int i = 0; i < 100; i++)
            {
                var attacker = new BattleCalc.BattleSide
                {
                    Troops = MakeTroops((0, 50), (1, 30)),
                    Morale = 0.8f
                };
                var defender = new BattleCalc.BattleSide
                {
                    Troops = MakeTroops((0, 40), (2, 20)),
                    Morale = 0.9f
                };

                var result = BattleCalc.Calculate(attacker, defender, rng);

                foreach (var kvp in attacker.Troops)
                {
                    int survivors = result.AttackerSurvivors.TryGetValue(kvp.Key, out int s) ? s : 0;
                    Assert.LessOrEqual(survivors, kvp.Value, $"Attacker survivor exceeds troops for type {kvp.Key}");
                }

                foreach (var kvp in defender.Troops)
                {
                    int survivors = result.DefenderSurvivors.TryGetValue(kvp.Key, out int s) ? s : 0;
                    Assert.LessOrEqual(survivors, kvp.Value, $"Defender survivor exceeds troops for type {kvp.Key}");
                }
            }
        }

        [Test]
        public void SurvivorsEqualTroopsMinusLosses()
        {
            var attacker = new BattleCalc.BattleSide
            {
                Troops = MakeTroops((0, 100)),
                Morale = 1f
            };
            var defender = new BattleCalc.BattleSide
            {
                Troops = MakeTroops((0, 100)),
                Morale = 1f
            };

            var result = BattleCalc.Calculate(attacker, defender, new System.Random(7));

            foreach (var kvp in attacker.Troops)
            {
                int lost = result.AttackerLosses.TryGetValue(kvp.Key, out int l) ? l : 0;
                int survived = result.AttackerSurvivors.TryGetValue(kvp.Key, out int s) ? s : 0;
                Assert.AreEqual(kvp.Value, lost + survived, $"Attacker troops mismatch for type {kvp.Key}");
            }
        }

        [Test]
        public void CalculateSidePowerWithMorale()
        {
            var side = new BattleCalc.BattleSide
            {
                Troops = MakeTroops((0, 10)),
                Morale = 0.5f
            };

            int power = BattleCalc.CalculateSidePower(side);
            Assert.AreEqual(50, power);
        }

        [Test]
        public void CalculateSidePowerZeroMorale()
        {
            var side = new BattleCalc.BattleSide
            {
                Troops = MakeTroops((0, 100)),
                Morale = 0f
            };

            int power = BattleCalc.CalculateSidePower(side);
            Assert.AreEqual(0, power);
        }

        [Test]
        public void DeterministicWithSeed()
        {
            var attacker = new BattleCalc.BattleSide
            {
                Troops = MakeTroops((0, 50)),
                Morale = 1f
            };
            var defender = new BattleCalc.BattleSide
            {
                Troops = MakeTroops((0, 50)),
                Morale = 1f
            };

            var r1 = BattleCalc.Calculate(attacker, defender, new System.Random(999));
            var r2 = BattleCalc.Calculate(attacker, defender, new System.Random(999));

            Assert.AreEqual(r1.AttackerWins, r2.AttackerWins);
            Assert.AreEqual(r1.TotalAttackerPower, r2.TotalAttackerPower);
            Assert.AreEqual(r1.TotalDefenderPower, r2.TotalDefenderPower);
            Assert.AreEqual(r1.AttackerLosses[0], r2.AttackerLosses[0]);
        }
    }
}

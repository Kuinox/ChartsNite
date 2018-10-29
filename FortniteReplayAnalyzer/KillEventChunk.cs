using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ReplayAnalyzer;

namespace FortniteReplayAnalyzer
{
    public class KillEventChunk : EventInfo
    {
        public readonly uint Size;
        public readonly byte[] UnknownData;
        public readonly string PlayerKilled;
        public readonly string PlayerKilling;
        public readonly WeaponType Weapon;
        public readonly State VictimState;
        public KillEventChunk(EventInfo info, uint size, byte[] unknownData, string playerKilled, string playerKilling, WeaponType weapon, State victimState) :base(info)
        {
            Size = size;
            UnknownData = unknownData;
            PlayerKilled = playerKilled;
            PlayerKilling = playerKilling;
            Weapon = weapon;
            VictimState = victimState;
        }

        public enum State
        {
            Died,
            KnockedDown
        }

        public enum WeaponType : byte
        {
            Storm,
            Fall,
            Pistol,
            Shotgun,
            AR,
            SMG,
            Sniper,
            PickAxe,
            Grenade,
            GrenadeLauncher = 10,
            RocketLauncher = 11,
            MiniGun = 12,
            CrossBow,
            Trap,
            QuadRocketLauncher = 24,
            Kevin = 25,
            Suicide = 32
        }
    }
}

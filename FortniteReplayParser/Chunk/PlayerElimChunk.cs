using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnrealReplayParser;

namespace FortniteReplayParser
{
    public class PlayerElimChunk : EventOrCheckpointInfo
    {
        public readonly bool CorrectlyParsed;
        public readonly byte[] UnknownData;
        public readonly string PlayerKilled;
        public readonly string PlayerKilling;
        public readonly WeaponType Weapon;
        public readonly State VictimState;
        public PlayerElimChunk(EventOrCheckpointInfo info, byte[] unknownData, string playerKilled, string playerKilling, WeaponType weapon, State victimState) :base(info)
        {
            UnknownData = unknownData;
            PlayerKilled = playerKilled;
            PlayerKilling = playerKilling;
            Weapon = weapon;
            VictimState = victimState;
        }
        public enum State
        {
            Died,
            KnockedDown,
            Unknow = int.MaxValue
        }

        public enum WeaponType : byte
        {
            Storm = 0,
            Fall = 1,
            Pistol = 2,
            Shotgun = 3,
            AR = 4,
            SMG = 5,
            Sniper = 6,
            PickAxe = 7,
            Grenade = 8,
            AlsoGrenade = 9,
            GrenadeLauncher = 10,
            RocketLauncher = 11,
            MiniGun = 12,
            CrossBow = 13,
            Trap = 14,
            DyingFromWound = 15,
            VehicleKill = 21,
            LMG = 22,
            StinkBomb = 23,
            OutOfMap = 24,
            Kevin = 25,
            Turret = 26,
            KevinZombie = 28,
            Suicide = 32,
            BiplaneGun = 38,
            Unknown = byte.MaxValue
        }
    }
}

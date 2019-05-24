using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnrealReplayParser;

namespace FortniteReplayParser.Chunk
{
    public readonly struct PlayerId
    {
        public enum Kind
        {
            UserName,
            EpicId
        }

        public readonly Kind IdKind;
        public readonly string PlayerNameOrEpicId;
        PlayerId(Kind kind, string playerNameOrEpicId )
        {
            IdKind = kind;
            PlayerNameOrEpicId = playerNameOrEpicId;
        }

        public static PlayerId FromPlayerName( string username ) => new PlayerId(Kind.UserName, username );
        public static PlayerId FromEpicId( byte[] idBytes )
        {
            if( idBytes.Length != 16 ) throw new ArgumentException();
            string idString = BitConverter.ToString( idBytes ).Replace( "-", "" ).ToLower();
            return new PlayerId( Kind.EpicId, idString );
        }
    }
    public class PlayerElimChunk : EventOrCheckpointInfo
    {
        public readonly bool CorrectlyParsed;
        public readonly PlayerId PlayerKilled;
        public readonly PlayerId PlayerKilling;
        [Obsolete("Weapons change at each update, not reliable.")]
        public readonly WeaponType Weapon;
        public readonly State VictimState;
        public PlayerElimChunk(EventOrCheckpointInfo info, PlayerId playerKilled, PlayerId playerKilling, WeaponType weapon, State victimState) :base(info)
        {
            PlayerKilled = playerKilled;
            PlayerKilling = playerKilling;
#pragma warning disable CS0618 // Type or member is obsolete
            Weapon = weapon;
#pragma warning restore CS0618 // Type or member is obsolete
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

create type KillListType as table
(
    OccuredAt time(7),
    KillerUserName nvarchar(255) collate Latin1_General_100_BIN2,
    VictimUserName nvarchar(255) collate Latin1_General_100_BIN2,
    WeaponType tinyint,
    KnocnedDown bit
);
create table ChartsNite.tKill (
	KillId bigint not null,
	KillerId int not null,
	VictimId int not null,
	WeaponType tinyint not null,
	KnockedDown bit not null,
	constraint PK_ChartsNite_tKill primary key ( KillId ),
	constraint Fk_ChartsNite_tKill_tEvent foreign key ( KillId ) references ChartsNite.tEvent(EventId),
	constraint FK_ChartsNite_tKill_tUser_Killer foreign key ( KillerId ) references CK.tUser(UserId),
	constraint FK_ChartsNite_tKill_tUser_Victim foreign key ( VictimId ) references CK.tUser(UserId)
);

create type ChartsNite.KillListType as table
(
    OccuredAt time(7),
    KillerUserName nvarchar(255) collate Latin1_General_100_BIN2,
    VictimUserName nvarchar(255) collate Latin1_General_100_BIN2,
    WeaponType tinyint,
    KnocnedDown bit
);
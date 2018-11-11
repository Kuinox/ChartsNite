-- SetupConfig: {}
create type KillListType as table
(
	OccuredAt time(7),
	KillerUserName nvarchar(255),
	VictimUserName nvarchar(255),
	WeaponType tinyint,
	KocnedDown bit
);

create type PlayerListType as table
(
	UserName nvarchar(255)
);

GO

create procedure ChartsNite.sReplayCreate
(
	@ActorId int,
	@OwnerId int,
	@ReplayDate datetime2,
	@UploadDate datetime2,
	@Duration time(7),
	@CodeName varchar(50),
	@FortniteVersion int,
	@Kills KillListType readonly,
	@Players PlayerListType readonly
)
as
begin

	select distinct kills.UserName
	from @Kills
	unpivot
	(
		t for UserName IN (KillerUserName, VictimUserName)
	) as kills
	left join CK.tUser u
	on kills.UserName = u.UserName
	where u.UserName is null
	

end
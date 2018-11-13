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

    declare user_cursor CURSOR FOR
    select distinct kills.UserName
    from @Kills
    unpivot
    (
        t for UserName IN (KillerUserName, VictimUserName)
    ) as kills
        left join CK.tUser u
        on kills.UserName = u.UserName
    where u.UserName is null

    open user_cursor
    fetch next from user_cursor into @UserName
    begin
        exec sUserCreate @UserName, null
        fetch next from user_cursor into @UserName
    end
    insert into ChartsNite.tReplay (OwnerId, ReplayDate, UploadDate, Duration, CodeName, FortniteVersion)
    values(@OwnerId, @ReplayDate, @UploadDate, @Duration, @CodeName, @FortniteVersion)
    
end
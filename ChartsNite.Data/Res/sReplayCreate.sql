-- SetupConfig: {}
create type KillListType as table
(
    OccuredAt time(7),
    KillerUserName nvarchar(255),
    VictimUserName nvarchar(255),
    WeaponType tinyint,
    KocnedDown bit
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
    --Retrieving missing user.
    declare user_cursor CURSOR FOR
    select distinct kills.UserName
    from @Kills
    unpivot
    (
        t for UserName IN (KillerUserName, VictimUserName)
    ) as kills
        left join CK.tUser u
        on kills.UserName = u.UserName
    where u.UserName is null;

    open user_cursor;
    fetch next from user_cursor into @UserName
    begin
        exec sUserCreate @UserName, null
        fetch next from user_cursor into @UserName
    end

    --inserting the replay
    insert into ChartsNite.tReplay
        (OwnerId, ReplayDate, UploadDate, Duration, CodeName, FortniteVersion)
    values(@OwnerId, @ReplayDate, @UploadDate, @Duration, @CodeName, @FortniteVersion);

    declare @TemptIdTable table
    (
        NewEventId bigint
    );
    insert into ChartsNite.tEvent
        (OccuredAt, ReplayId)
    --Inserting all the events
    output inserted.EventId
        into @TemptIdTable
    select OccuredAt, CAST(scope_identity() AS int)
    from @Kills;

    insert into ChartsNite.tKill(KillId, KillerId,  );
end
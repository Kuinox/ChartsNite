-- SetupConfig: {}
create type KillListType as table
(
    OccuredAt time(7),
    KillerUserName nvarchar(255),
    VictimUserName nvarchar(255),
    WeaponType tinyint,
    KnocnedDown bit
);
GO

create procedure ChartsNite.sReplayCreate
    (
    @ActorId int,
    @OwnerId int,
    @ReplayDate datetime2,
    @Duration time(2),
    @CodeName varchar(50),
    @FortniteVersion int,
    @Kills KillListType readonly
)
as
begin
	--[beginsp]
    declare @UserName nvarchar(255);

	--<PreCreate revert />

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
        exec CK.sUserCreate 1, @UserName, null
        fetch next from user_cursor into @UserName
    end

    --inserting the replay
    insert into ChartsNite.tReplay
        (OwnerId, ReplayDate, UploadDate, Duration, CodeName, FortniteVersion)
    values(@OwnerId, @ReplayDate, GETUTCDATE(), @Duration, @CodeName, @FortniteVersion);

    declare @TempIdTable table
    (
        NewEventId bigint
    );
    
    insert into ChartsNite.tEvent
        (OccuredAt, ReplayId)
    --Inserting all the events
    output inserted.EventId
        into @TempIdTable
    select OccuredAt, CAST(scope_identity() AS int)
    from @Kills;

    --Inserting all the kills
    insert into
        ChartsNite.tKill(
            KillId,
            KillerId,
            VictimId,
            WeaponType,
            KnockedDown
        )
    select
        ROW_NUMBER() over(order by (select null) desc),
        uk.UserId as KillerId,
        uv.UserId as VictimId,
        k.WeaponType,
        k.KnocnedDown
    from @Kills k
        left join CK.tUser uk
            on k.KillerUserName = uk.UserName
        left join CK.tUser uv
            on k.VictimUserName = uv.UserName;
    --<PostCreate />
    --[endsp]
end
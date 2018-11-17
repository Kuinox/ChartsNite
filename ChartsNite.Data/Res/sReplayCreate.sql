-- SetupConfig: {}

create procedure ChartsNite.sReplayCreate
    (
    @ActorId int,
    @OwnerId int,
    @ReplayDate datetime2,
    @Duration time(2),
    @CodeName varchar(50),
    @FortniteVersion int,
    @Kills KillListType readonly,
    @Output int output
)
as
begin
    --[beginsp]
    declare @UserName nvarchar(255);
    declare @TempIdTable table
    (
        NewEventId bigint
    );
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
        on kills.UserName = u.UserName collate Latin1_General_100_CI_AS
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
    set @Output = SCOPE_IDENTITY();
    
    insert into ChartsNite.tEvent
        (OccuredAt, ReplayId)
    --Inserting all the events
    output inserted.EventId
        into @TempIdTable
    select OccuredAt, CAST(scope_identity() AS int)
    from @Kills;

    --Inserting all the kills
    insert into
        ChartsNite.tKill
        (
        KillId,
        KillerId,
        VictimId,
        WeaponType,
        KnockedDown
        )
    select
        t.NewEventId as EventId,
        uk.UserId as KillerId,
        uv.UserId as VictimId,
        k.WeaponType as WeaponType,
        k.KnocnedDown as KnocnedDown
    from (
        select
            KillerUserName,
            VictimUserName,
            WeaponType,
            KnocnedDown,
            ROW_NUMBER() over(order by (select null) desc) as KillLineId
        from @Kills) k
        join (select NewEventId, ROW_NUMBER() over(order by (select null) desc) as InsertedLineId
        from @TempIdTable) t --it just map the line number on the row.
        on k.KillLineId = t.InsertedLineId
        left join CK.tUser uk
        on k.KillerUserName = uk.UserName collate Latin1_General_100_CI_AS
        left join CK.tUser uv
        on k.VictimUserName = uv.UserName collate Latin1_General_100_CI_AS;
--<PostCreate />
--[endsp]
end
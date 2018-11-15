create table ChartsNite.tEvent(
	EventId bigint not null identity(0, 1),
	OccuredAt Time(2) not null,
	ReplayId int not null,
	constraint PK_ChartsNite_tEvent primary key ( EventId ),
	constraint FK_ChartsNite_tEvent_tReplay_tReplay foreign key ( ReplayId ) references ChartsNite.tReplay( ReplayId )
);
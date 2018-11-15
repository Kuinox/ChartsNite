create table ChartsNite.tReplay (
	ReplayId int not null identity(0, 1),
	OwnerId int not null,
	ReplayDate DateTime2(2) not null,
	UploadDate DateTime2(2) not null,
	Duration Time(2) not null,
	CodeName varchar(50) not null,
	FortniteVersion int not null,
	constraint PK_ChartsNite_tReplay primary key ( ReplayId ),
	constraint FK_ChartsNite_tReplay_tUser foreign key ( OwnerId ) references CK.tUser( UserId ),
	constraint UK_ChartsNite_tReplay_FileName unique (CodeName)
);
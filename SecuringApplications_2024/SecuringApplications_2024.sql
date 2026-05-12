use SecuringApplications_2024;
go

select *
from dbo.Books;

select *
from dbo.AspNetUsers;

select *
from dbo.AspNetRoles;

select *
from dbo.AspNetUserRoles
order by UserId asc;

delete from dbo.AspNetUsers
where UserName <> 'chrisfarrugia@live.com'

select u.Id, u.UserName, r.NormalizedName
from dbo.AspNetUsers u inner join dbo.AspNetUserRoles ur
	on u.Id = ur.UserId
inner join dbo.AspNetRoles r
	on ur.RoleId = r.Id;

select *
from dbo.[Permissions];
Instructions to start from scratch with database creation:

In Package Manager window, select Package source: nuget.org, Default project: This data project

PM> Add-Migration InitialCreate
PM> Update-Database

The second command actually creates the database.  Note you can specify a connection string here to do this on any remote database

To connect to this db with SSMS, log in to this server:
(localdb)\MSSQLLocalDB

This server name was found by going to the SQL Server Object Explorer in VS, and looking at the server name there that contains our db.

Azure database server: gamage-db
Admin account: randygamage / August8794
Server=tcp:gamages-db.database.windows.net,1433
db name: mytennispartnerdb-dev

To set up logins and users in db:
1) Obtain master or admin account for db server
2) Login to 'master' db with this account
3) execute SQL: create login mytennispartnerdev with password = 'mypassword'
4) Login to 'mydatabase' with master account
5) execute SQL: create user mytennispartnerdev from login mytennispartnerdev
6) exec sp_addrolemember 'db_owner', 'mytennispartnerdev'
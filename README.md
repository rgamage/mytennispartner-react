# mytennispartner-react
Full-featured website for organizing social tennis, using .NET Core &amp; React

This code was originally deployed and hosted at https://mytennispartner.com but the website has since been re-written using the .NET Blazor framework.  I am keeping this code posted in the hopes it may benefit someone at some point, to see some examples of a fairly complex .NET / React web application.

My Tennis Partner is a full-featured application to organize social tennis.  It allows users to create a profile, create leagues/groups, and then schedule matches and assign players to the line-ups.

The app was originally designed for structured leagues that play on a regular basis, with mostly the same players.  But it can also be used for organizing ad-hoc games at irregular intervals.

## User authentication
.NET Identity is used for user authentication, with JWT tokens.  Three other options for authentication are also supported (OAUTH2): Facebook, Twitter, and Google.  Users can edit their account info, reset passwords, etc.  They can also customize their player profile, upload a photo, etc.

## Sub Management
One of the features of the app is management of substitute players ("subs").  If a player is in the line-up, but then cancels, the next sub that is available can automatically be added in their place.  E-mail notifications are sent to keep all players informed of these and other changes.

## Court Reservations
Courts can be automatically reserved if the club uses a supported reservation system.  Currently only tennisbookings.com is supported.

## Match Management
Players can be assigned to matches manually, or shuffled randomly for each match.  Match views clearly show which players are 'regulars' and which are 'subs', and if they've confirmed or not, etc.

## Group Rosters
Each league or group has a Roster page that allows other members of the group to see their contact info, skill level, home club, etc. for easy coordination between players.

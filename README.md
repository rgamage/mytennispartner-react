# mytennispartner-react
Full-featured website for organizing social tennis, using .NET Core &amp; React

This code was originally deployed and hosted at https://mytennispartner.com but the website has since been re-written using the .NET Blazor framework.  I am keeping this code posted in the hopes it may benefit someone at some point, to see some examples of a fairly complex .NET / React web application.

My Tennis Partner is a full-featured application to organize social tennis.  It allows users to create a profile, create leagues/groups, and then schedule matches and assign players to the line-ups.

The app was originally designed for structured leagues that play on a regular basis, with mostly the same players.  But it can also be used for organizing ad-hoc games at irregular intervals.

## User authentication
.NET Identity is used for user authentication, with JWT tokens.  Three other options for authentication are also supported (OAUTH2): Facebook, Twitter, and Google.  Users can edit their account info, reset passwords, etc. 

## Members
Members are managed separately from Users.  The Members class holds all tennis-specific info, and the User class is strictly for authentication.  The Member class holds the user's "profile" information, including age, gender, skill level, home club, profile photo, etc.

## Groups
Members can create Groups, and add any other Members they want.  Each group or league has a Roster page that allows other members of the group to see their contact info, skill level, home club, etc. for easy coordination between players.

## Matches
Groups can have a collection of matches.  Only Captains of the group can add/remove matches (but there can be multiple captains in a group).  When creating a match, users must specify the date, time, and venue (location).  Players can be assigned to matches manually, or shuffled randomly for each match.  Match views clearly show which players are 'regulars' and which are 'subs', and if they've confirmed or not, etc.

## Lines
Each Match has a collection of Lines, which correspond to Courts.  A match with three linees, for example, is played on three courts.  Each line/court can have either 2 or 4 players, depending on the specified Format of the match (singles or doubles).  When adding players to a match, validation is performed to ensure certain rules like gender and quantity are enforced.  Any player that is added to a Line or Court, is considered "in the line-up" for the match.

## Availability
Each Group has an Availability page that shows a grid of all upcoming matches, and each player's availability for that match.  Users can see at a glance if they are short players for any match dates, and can also easily set / change their availability on that page.

# Players
the Player class holds information about a specific Member, in a specic Group, for a given Match.  It holds their availability info for that match, as well as other match/member specific information.  When a player changes their availability, it is the Player class that is updated.

## Sub Management
One of the features of the app is management of substitute players ("subs").  If a player is in the line-up, but then cancels (or changes their availability), the next sub that is available can automatically be added in their place.  The priority order of the subs is determined based on how recently each sub has played - those who have played recently/often have lower priority.  E-mail notifications are sent to keep all players informed of these and other changes.

## Court Reservations
Courts can be automatically reserved if the club uses a supported reservation system.  Currently only tennisbookings.com is supported.



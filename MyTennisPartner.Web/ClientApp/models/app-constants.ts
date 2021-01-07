import { SelectOptionList } from "./SelectOption";
import { PlayFormat, Gender } from "./viewmodels/Enums";

/**
 * class to hold application-wide constants and configuration
 */
export default class AppConstants {

    // application configuration properties
    static LandingPageAfterLogin = "/dashboard";
    static LandingPageAfterLogout = "/home";
    static MobileWindowSize = 576;  // bootstrap 4 "sm" breakpoint

    // api urls - authentication
    static loginUrl = "/api/auth/login";
    static registerUrl = "/api/auth/register";
    static forgotPasswordUrl = "/api/auth/forgot";
    static resetPasswordUrl = "/api/auth/reset";
    static getUserInfoUrl = "/api/auth/getuserinfo";
    static checkPasswordUrl = "/api/auth/checkpassword";
    static sendConfirmEmailUrl = "/api/auth/sendConfirmEmail";
    static updateAccountInfo = "/api/auth/update";

    // api urls - members
    static getMemberByUser = "/api/Member/GetMemberByUser";
    static getMembers = "/api/Member"
    static createMember = "/api/Member";
    static updateMember = "/api/Member";
    static createMemberImage = "/api/MemberImage";
    static updateMemberImage = "/api/MemberImage";
    static memberImageFile = "/api/MemberImage/file";
    static searchMembers = "/api/Member/search";
    static getPlayerPickList = "/api/Member/GetPlayerPickList";
    static getSubPickList = "/api/Member/GetSubPickList";

    // api urls - leaguemembers
    static leagueMembersApi = "/api/Member/LeagueMembers"

    // api urls - leagues
    static searchLeagues = "/api/League/Search";
    static getLeagueSummary = "/api/League/Summary";
    static createLeague = "/api/League";
    static updateLeague = "/api/League";
    static deleteLeague = "/api/League";
    static leaguesByMember = "/api/League/ByMember";
    static getLeagueEditAbility = "/api/League/GetLeagueEditAbility";

    // api urls - venues
    static searchVenues = "/api/venue/Search";

    // api urls - court reservations
    static testSparetimeLogins = "/api/reservation/TestSparetimeLogins";
    static updateTargetCourts = "/api/reservation/UpdateTargetCourts";

    // api urls - matches
    static getMatchesByLeagueApi = "/api/Match/ByLeague";
    static getMatchesByMemberApi = "/api/Match/ByMember";
    static updateMatchAvailability = "/api/Match/UpdateAvailability";
    static matchApi = "/api/Match";
    static getNewMatchApi = "/api/Match/GetNew";
    static respondToMatchApi = "/api/Match/RespondToMatch";
    static getLeagueAvailability = "/api/Match/GetLeagueAvailability";
    static getUnansweredAvailability = "/api/Match/GetUnansweredAvailability";
    static reserveCourtsForMatch = "/api/Match/ReserveCourtsForMatch";
    static getProspectiveMatches = "/api/Match/Prospective";

    // api urls - lines
    static getLinesByMatchApi = "/api/Line/GetByMatch";

    // enums
    static genders: SelectOptionList = {
        options: [
            { value: 0, label: "Select..." },
            { value: 1, label: "Male" },
            { value: 2, label: "Female" }
        ]
    };

    static ampm: SelectOptionList = {
        options: [
            { value: 0, label: "AM" },
            { value: 1, label: "PM" }
        ]
    };

    static playformats: SelectOptionList = {
        options: [
            { value: 0, label: "Men's Singles" },
            { value: 1, label: "Women's Singles" },
            { value: 2, label: "Men's Doubles" },
            { value: 3, label: "Women's Doubles" },
            { value: 4, label: "Mixed Doubles" },
            { value: 5, label: "Singles Practice" },
            { value: 6, label: "Private Lesson" },
            { value: 7, label: "Group Lesson / Clinic" }
        ]
    };

    static mapFormatToGender(format: PlayFormat): Gender {
        switch (format) {
            case PlayFormat.MensSingles:
            case PlayFormat.MensDoubles:
                return Gender.Male;
            case PlayFormat.WomensSingles:
            case PlayFormat.WomensDoubles:
                return Gender.Female;
            default:
                return Gender.Unknown;
        } 
    }

    static frequency: SelectOptionList = {
        options: [
            { value: 0, label: "weekly" },
            { value: 1, label: "every other week" },
            { value: 2, label: "every third week" },
            { value: 3, label: "monthly" },
            { value: 4, label: "at random times" }
        ]
    };

    // same as frequency but with first letter capitalized
    static Frequency: SelectOptionList = {
        options: [
            { value: 0, label: "Weekly" },
            { value: 1, label: "Every other week" },
            { value: 2, label: "Every third week" },
            { value: 3, label: "Monthly" },
            { value: 4, label: "At random times" }
        ]
    };

    static DefaultRole = 0;

    static roles: SelectOptionList = {
        options: [
            { value: 0, label: "Player" },
            { value: 1, label: "Pro/Instructor" },
            { value: 2, label: "Club Manager" },
            { value: 3, label: "Admin", disabled: true }
        ]
    };

    static dayOfWeek: SelectOptionList = {
        options: [
            { value: 0, label: "Sunday" },
            { value: 1, label: "Monday" },
            { value: 2, label: "Tuesday" },
            { value: 3, label: "Wednesday" },
            { value: 4, label: "Thursday" },
            { value: 5, label: "Friday" },
            { value: 6, label: "Saturday" },
        ]
    }

    static months: SelectOptionList = {
        options: [
            { value: 0, label: "Jan" },
            { value: 1, label: "Feb" },
            { value: 2, label: "Mar" },
            { value: 3, label: "Apr" },
            { value: 4, label: "May" },
            { value: 5, label: "Jun" },
            { value: 6, label: "Jul" },
            { value: 7, label: "Aug" },
            { value: 8, label: "Sep" },
            { value: 9, label: "Oct" },
            { value: 10, label: "Nov" },
            { value: 11, label: "Dec" },
        ]
    }

    static DefaultSkillRanking = "3.0";

    static rankings: SelectOptionList = {
        options: [
            { value: "2.0", label: "2.0" },
            { value: "2.5", label: "2.5" },
            { value: "3.0", label: "3.0" },
            { value: "3.5", label: "3.5" },
            { value: "4.0", label: "4.0" },
            { value: "4.5", label: "4.5" },
            { value: "5.0", label: "5.0" },
            { value: "5.5", label: "5.5" }
        ]
    }

    static timeOfDay: SelectOptionList = {
        options: [
            { value: "06:00", label: "6:00 AM" },
            { value: "06:30", label: "6:30 AM" },
            { value: "07:00", label: "7:00 AM" },
            { value: "07:30", label: "7:30 AM" },
            { value: "08:00", label: "8:00 AM" },
            { value: "08:30", label: "8:30 AM" },
            { value: "09:00", label: "9:00 AM" },
            { value: "09:30", label: "9:30 AM" },
            { value: "10:00", label: "10:00 AM" },
            { value: "10:30", label: "10:30 AM" },
            { value: "11:00", label: "11:00 AM" },
            { value: "11:30", label: "11:30 AM" },
            { value: "12:00", label: "12:00 PM" },
            { value: "12:30", label: "12:30 PM" },
            { value: "13:00", label: "1:00 PM" },
            { value: "13:30", label: "1:30 PM" },
            { value: "14:00", label: "2:00 PM" },
            { value: "14:30", label: "2:30 PM" },
            { value: "15:00", label: "3:00 PM" },
            { value: "15:30", label: "3:30 PM" },
            { value: "16:00", label: "4:00 PM" },
            { value: "16:30", label: "4:30 PM" },
            { value: "17:00", label: "5:00 PM" },
            { value: "17:30", label: "5:30 PM" },
            { value: "18:00", label: "6:00 PM" },
            { value: "18:30", label: "6:30 PM" },
            { value: "19:00", label: "7:00 PM" },
            { value: "19:30", label: "7:30 PM" },
            { value: "20:00", label: "8:00 PM" },
            { value: "20:30", label: "8:30 PM" },
            { value: "21:00", label: "9:00 PM" },
            { value: "21:30", label: "9:30 PM" },
            { value: "22:00", label: "10:00 PM" },
            { value: "22:30", label: "10:30 PM" },
            { value: "23:00", label: "11:00 PM" }
        ]
    }

    // global max number of players on league roster
    static maxPlayersOnRoster = 200;

    // global page size for lists
    static pageSize = 10;

    // page size for dashboard lists
    static dashboardPageSize = 5;

    // global date format
    static dateFormat = "ddd, M/D, h:mm a";

    // moment calendar format
    static calendarFormat = {
        sameDay: '[Today], h:mm a',
        nextDay: '[Tomorrow], h:mm a',
        nextWeek: 'ddd, h:mm a',
        lastDay: '[Yesterday], h:mm a',
        lastWeek: '[Last] ddd, h:mm a',
        sameElse: AppConstants.dateFormat
    }
}

export interface SelectItem {
    value: number;
    label: string;
}


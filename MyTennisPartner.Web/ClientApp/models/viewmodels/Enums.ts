export enum AmPm {
    AM,
    PM
}

export enum DayOfWeek {
    Sunday,
    Monday,
    Tuesday,
    Wednesday,
    Thursday,
    Friday,
    Saturday
}

export enum Frequency {
    Weekly,
    BiWeekly,
    TriWeekly,
    Monthly,
    AdHoc
}

export enum PlayFormat {
    MensSingles,
    WomensSingles,
    MensDoubles,
    WomensDoubles,
    MixedDoubles,
    SinglesPractice,
    PrivateLesson,
    GroupLesson
}

export enum Availability {
    Unknown,
    Confirmed,
    Unavailable
}

export enum Gender {
    Unselected,
    Male,
    Female,
    Unknown
}

export enum Roles {
    Player,
    Pro,
    VenueAdmin,
    Admin
}

export enum MatchDeclineAction {
    DoNothing,
    InviteAll,
    InviteSome,
    InviteExternal
}

export enum CourtReservationProvider {
    TennisBookings = 1,
    LifetimeFitness = 2
}

export enum UserPreferenceFlags {
    None = 0,
    ShowDeclinedMatchesOnDashboard = 1 << 0
}

export enum HelpTipTrackers {
    None = 0,
    DashboardMatchOpportunityExpand = 1 << 0,
    DashboardCommittedMatchDecline = 1 << 1
}


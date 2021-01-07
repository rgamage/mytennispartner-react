import SelectOption, { SelectOptionList } from "..//SelectOption";
import AppConstants from "../app-constants";
import VenueViewModel from "./VenueViewModel";
import { Gender, Availability, UserPreferenceFlags, HelpTipTrackers } from "./Enums";

export class MemberNameViewModel extends SelectOption {

    //static fromMemberViewModel(member: MemberViewModel): MemberNameViewModel {
    //    let model = new MemberNameViewModel();
    //    model.memberId = member.memberId;
    //}

    memberId: number;
    userId: string;
    firstName: string;
    lastName: string;
    isSubstitute: boolean;
    availability: Availability;
    gender: Gender;
    isCaptain: boolean;
    leagueMemberId: number;
    playerId: number;

    // for client-side use only
    selected: boolean;
}

export default class MemberViewModel extends MemberNameViewModel {
    constructor() {
        super();
        this.gender = 0;
        this.skillRanking = AppConstants.DefaultSkillRanking;
        this.memberRoles = [AppConstants.roles.options[AppConstants.DefaultRole]];
        this.notifyAddOrRemoveMeFromMatch = true;
        this.notifyCourtChange = true;
        this.notifyMatchDetailsChangeOrCancelled = true;
        this.notifySubForMatchOpening = true;
        this.notifyMatchAdded = true;
        this.autoReserveVenues = "";
        this.userPreferenceFlags = 0xFFFF;
        this.helpTipTrackers = 0xFFFF;
        //this.phoneNumber = "";
    }

    //memberId: number;
    //userId: string;
    //firstName: string;
    //lastName: string;
    homeVenueId: number;
    zipCode: string;
    skillRanking: string;
    birthYear: number;
    image: File;
    memberRoles: SelectOption[];
    playerPreferences: SelectOption[] = [];
    leagueId: number;
    homeVenue: VenueViewModel;
    notifyAddOrRemoveMeFromMatch: boolean;
    notifyMatchDetailsChangeOrCancelled: boolean;
    notifySubForMatchOpening: boolean;
    notifyCourtChange: boolean;
    notifyMatchAdded: boolean;
    phoneNumber: string;
    email: string;
    userPreferenceFlags: UserPreferenceFlags;
    helpTipTrackers: HelpTipTrackers;

    // court reservation credentials - web ui is only supporting one provider for now, may expand later to n providers
    spareTimeUsername: string;
    spareTimePassword: string;
    spareTimeMemberNumber: number;
    autoReserveVenues: string;

    [key: string]: any;  // add "| boolean" for example if model has other types, or "any"
}


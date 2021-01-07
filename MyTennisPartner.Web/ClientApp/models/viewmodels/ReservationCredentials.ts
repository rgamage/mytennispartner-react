import { CourtReservationProvider } from "./Enums";

export default class ReservationCredentials {
    CourtReservationProvider?: CourtReservationProvider;
    host?: string;
    username: string;
    password: string;
}
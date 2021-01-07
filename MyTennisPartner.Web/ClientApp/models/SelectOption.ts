export default class SelectOption {
    label?: string;
    value?: string | number;
    disabled?: boolean;
}

export class SelectOptionList {
    options: SelectOption[];
}

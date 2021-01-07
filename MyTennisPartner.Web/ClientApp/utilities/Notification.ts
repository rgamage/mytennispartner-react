import { ToastStore } from 'react-toasts';


export default class Notification {

    static notifySuccess(message: string = "Your changes have been saved!") {
        //toast.success(message, this.toastSuccessOptions);
        ToastStore.success(message);
    }

    static notifyError(message: string) {
        //toast.error(message, this.toastErrorOptions);
        ToastStore.error(message);
    }

    static notifyInfo(message: string) {
        //toast.info(message, this.toastSuccessOptions);
        ToastStore.info(message);
    }

    // todo: set success toast color to light green = #d4edda (bootstrap success color), but need to figure out how to set text color to drk green instead of white

}

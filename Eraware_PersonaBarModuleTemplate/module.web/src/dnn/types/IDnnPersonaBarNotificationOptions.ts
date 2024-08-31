/** The possible options for persona bar notifications. */
export type IDnnPersonaBarNotificationOptions = {
    /** For how many miliseconds to display the notification. */
    timeout?: number;

    /** The size of the notification */
    size?: undefined | "" | "large";

    /** If true, a close button will display. */
    clickToClose? : boolean;

    /** Can be used to customize the text ofthe close button. */
    closeButtonText?: string;

    /** Displays just a normal notification or a more important error style. */
    type?: "notify" | "error"
}
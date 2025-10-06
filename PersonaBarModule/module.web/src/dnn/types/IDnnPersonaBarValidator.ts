import type { IDnnPersonaBarCustomValidator } from "./IDnnPersonaBarCustomValidator";

/** Utility to validate inputs. */
export type IDnnPersonaBarValidator = {
    /** The HTML elements that can be validated. */
    selector: "input, textarea, select";
    /** The list of standard error messages. */
    errorMessages: {
        required: "Text is required",
        minLength: "Text must be at least {0} chars",
        number:  "Only numbers are allowed",
        nonNegativeNumber: "Negative numbers are not allowed",
        positiveNumber: "Only positive numbers are allowed",
        nonDecimalNumber: "Decimal numbers are not allowed",
        email: "Only valid email is allowed",
    };

    /** Performs field validation and reports the errors. */
    validate: (
        /** The parent HTML element containing the fields to validate. */
        container: HTMLElement | JQuery<HTMLElement>,
        customValidators?: IDnnPersonaBarCustomValidator[],
    ) => boolean;
}
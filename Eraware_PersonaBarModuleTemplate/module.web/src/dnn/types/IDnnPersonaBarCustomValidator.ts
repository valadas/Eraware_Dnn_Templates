/** A custom field validator. */
export type IDnnPersonaBarCustomValidator = {
    /** The name of the custom validator. */
    name: string;

    /** Validates an input element. */
    validate(
        /** The value of the input element. */
        value: any,
        /** The input element to validate. */
        input: HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement,
    ): boolean;
};
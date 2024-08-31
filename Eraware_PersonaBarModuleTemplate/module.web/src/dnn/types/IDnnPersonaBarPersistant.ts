import type { IDnnPersonaBarPersistantData } from "./IDnnPersonaBarPersistantData";

/** Allows persisting the persona bar state for a user. */
export type IDnnPersonaBarPersistant = {
    /** Loads the current user settings. */
    load(): IDnnPersonaBarPersistantData;
    save(
        /** The settings to save. */
        settings: IDnnPersonaBarPersistantData,
        /** A callback that gets called when saving succeeded. */
        success?: () => void,
        /** A callback that gets called when saving failed. */
        error?: () => void
    ): void;
};

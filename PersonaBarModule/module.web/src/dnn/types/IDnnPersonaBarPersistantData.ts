/** The data that can be persisted for a user. */
export type IDnnPersonaBarPersistantData = {
    /** The identifier of the currently viewed persona bar page. */
    activeIdentifier: string;
    /** Whether the persona bar should be expanded on load. */
    expandPersonaBar: boolean;
};

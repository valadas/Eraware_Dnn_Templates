/** Represents one item in the persona bar menu structure as represented in the backend. */
export type IDnnPersonaBarMenuStructureItem = {
    /** The name of the parent menu item. */
    Parent: string;

    /** The user friendly name of the menu item. */
    DisplayName: string;

    /** The ID of the menu item. */
    MenuId: number,

    /** The unique string identifier for the menu item. */
    Identifier: string,

    /** The name of the module that displays for this menu item. */
    ModuleName: string,

    /** The name of the folder containing the module.
     * It appears to me that the folder is located by convention and this value is always an empty string.
     */
    FolderName: string,

    /** The resource key to use for the localized title. */
    ResourceKey: string,

    /** The path for the menu item. */
    Path: string,

    /** Provides the link for when this menu item should display another page in an iframe instead of a persona bar module. */
    Link: string,

    /** Css class to add to the menu item. */
    CssClass: "",

    /** The icon file to use for the menu item. */
    IconFile: string,

    /** The ID of the parent menu item. */
    ParentId: number,

    /** The prefered sort order for the placement of the menu item. */
    Order: number,

    /** Indicates if host users should be allowed to view this menu item.
     * Pretty much always true, but maybe there could be some scenarios of modules that should not be available to host users.
     */
    AllowHost: boolean,

    /** Indicates if the menu item is enabled. Appears to always be false even though the items show... */
    Enabled: boolean,

    /** A JSON string that represents the settings for the module. The shape of this object is entirely dependent on the specific module. */
    Settings: string,
    
    /** The children items of this menu item. */
    Children: IDnnPersonaBarMenuStructureItem[],
};
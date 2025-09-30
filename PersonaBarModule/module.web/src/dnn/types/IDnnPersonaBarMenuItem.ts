/** A menu item in the persona bar. */
export type IDnnPersonaBarMenuItem = {
    /** The unique identifier for the menu item. */
    id: string;

    /** The localization key for the menu item. */
    resourceKey: string;

    /** The name of the module to display in the panel, use LinkMenu to do a link instead of a module.*/
    moduleName: string;

    /** Not sure on the usage, only found empty string PR welcome if anyone know the purpose. */
    folderName: string;

    /** Not sure on the purpose, usually same as moduleName. */
    path: string;

    /** An optional querystring to pass to a link (when in link mode).*/
    query: string;

    /** If set, the panel will display a site page instead of a persona bar module. */
    link: string;

    /** Css classes that can be added to the menu item. */
    css: string;

    /** The path to an icon to display in the menu item. */
    icon: string;

    /** A user friendly and localizaed display name. */
    displayName: string;

    /** A json STRING that can contain settings for the persona bar page or module to use with no specific shape (depends on the module). */
    settings: string;

    /** The child menu items contained within this page. */
    menuItems: IDnnPersonaBarMenuItem[];
};
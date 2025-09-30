import type { Dayjs } from "dayjs";
import type { IPersonaBarServicesFramework } from "./IDnnPersonaBarServicesFramework";
import type { IDnnPersonaBarPersistant } from "./IDnnPersonaBarPersistant";
import type { IDnnPersonaBarMenuItem } from "./IDnnPersonaBarMenuItem";
import type { IDnnPersonaBarNotificationOptions } from "./IDnnPersonaBarNotificationOptions";
import type { IDnnPersonaBarValidator } from "./IDnnPersonaBarValidator";
import type { IDnnPersonaBarMenuStructure } from "./IDnnPersonaBarMenuStructure";

/** Utilities available in the persona bar. */
export type IDnnPersonaBarUtilities =
{
    /** The DNN Services Framework that allows easy interation with WEB APIs. */
    sf: IPersonaBarServicesFramework;

    /** If true, we are on a device that supports touch. */
    onTouch: boolean;

    /** Provides access to a pre-configured DayJs constructor. */
    dayjs: () => Dayjs;

    /** Allows persisting persona bar user level state. */
    persistant: IDnnPersonaBarPersistant;

    /** If true, the persona bar is currently animating. */
    inAnimatin: boolean;

    /** Can be called to center the confirmation dialog. */
    setConfirmationDialogPosition: () => void;

    /** Opens the social tasks (unkown/undocumented feature, probably Evoq only). */
    openSocialTasks: () => void;

    /** Closes the social tasks (unkown/undocumented feature, probably Evoq only). */
    closeSocialTasks: () => void;

    /** Makes the persona bar panel wider. (INOP ?) */
    expandPersonaBarPage: () => void;

    /** Makes the persona bar panel back to the initial size. (INOP ?) */
    contractPersonaBarPage: () => void;

    /** Closes the persona bar. */
    closePersonaBar: (
        /** Gets called once the persona bar is finished closing. */
        callback?: (() => void) | any,
        /** If trure, the selection will be kept in focus. */
        keepSelection?: boolean
    ) => void;

    /** Opens a specific persona bar panel (page) */
    loadPanel: (
        /** The unique identifier of the page such as Dnn.Themes . */
        identifier: string,
        /** Paramaters to pass to the persona bar module on that page. */
        params: any,
    ) => void;

    /** Internel method. */
    panelLoaded: (
        params: any,
        loaded: boolean,
    ) => void;

    /** Internel method. */
    initCustomModules: (callback: () => void) => void;

    /** Internal method. */
    loadCustomModules: () => void;

    /** Internal method. */
    leaveCustomModules: () => void;

    /** Finds the settings for a given menu identifier pased into an object (as opposed to just a json string). */
    findMenuSettings: (
        /** The unique identifier of the page such as Dnn.Themes . */
        identifier: string,
        /** The list of menu items. */
        menuItems?: IDnnPersonaBarMenuItem[],
    ) => any;

    /** Saves the settings for a given menu item. */
    updateMenuSettings: (
        /** The unique identifier of the page such as Dnn.Themes . */
        identifier: string,
        /** The settings to save. */
        settings: any,
        /** The list of menu items. */
        menuItems?: IDnnPersonaBarMenuItem[],
    ) => void;

    /** Loads javascript bundle files using ajax. */
    loadBundleScript: (
        /** The path or paths for the scripts to load. */
        paths: string | string[]
    ) => void;

    /** Saves cache data in the browser local storage. Can also read if no viewData is passed. */
    panelViewData: (
        /** The ID of the panel this data should be scoped to. */
        panelId: string,
        /** The data to save, can be undefined to only read the current data. */
        viewData: any
    ) => any;

    /** Saves which tab was selected to persist that selection.
     * Assumes that dnn-react-common is used.
     */
    savePanelTabView: (panelId: string) => void;

    /** Updates the saved state of the selected tab.
     * Assumes that dnn-react-common is used.
     */
    updatePanelTabView: (panelId: string) => void;

    /** Check if a template is loaded. */
    loaded: (template: string) => boolean;

    /** Loads the specified template */
    loadTemplate: (
        /** The folder to load from. */
        folder: string,
        /** The name of the template file. */
        template: string,
        /** The wrapper jquery html element. */
        wrapper: JQuery<HTMLElement>,
        /** The callback to call to indicate the template is finished loading. */
        cb: () => void,
    ) => void;

    /** Loads resource files into context. */
    loadResx: (cb: () => void) => void;

    /** Gets a localized string. */
    getResx: (
        /** The name of the module for which to get a localized string. */
        moduleName: string,
        /** The key of the localized string to get. */
        key: string
    ) => string;

    /** Gets the module name if it exists in params.moduleName. */
    getModuleNameByParams: (params: any) => string;

    /** Gets the identifier if it exists in params.identifier. */
    getIdentifierByParams: (params: any) => string;

    /** Gets the folder name if it exists in params.folderName */
    getFolderNameByParams: (params: any) => string;

    /** Executes functions in parallel. */
    asyncParallel: (deferreds: (() => any)[], callback: () => any) => void;

    /** Executes functions in series. */
    asyncWaterfall: (deferreds: (() => any)[], callback: () => any) => void;

    /** Shows a confirmation dialog. */
    confirm: (
        /** The text to show in the confirmation dialog. */
        text: string,
        /** The text of the confirm button */
        confirmBtn: string,
        /** The text of the cancel button, if empty, no cancel button will display. */
        cancelBtn: string,
        /** Gets called when the confirm button was clicked. */
        confirmHandler: () => void,
        /** Gets called when teh cancel button was clicked. */
        cancelHandler: () => void,
    ) => void;

    /** Shows a notification temporary notification message. */
    notify: (
        /** The text to display. */
        text: string,
        /** The options of the notification. */
        options?: IDnnPersonaBarNotificationOptions | number,
    ) => void;

    /** Shows an error notification. */
    notifyError: (
        /** The error message to display. */
        text: string,
        /** The options of the error notification. */
        options?: IDnnPersonaBarNotificationOptions
    ) => void;

    /** Localizes the error messages for a give validator. */
    localizeErrMessages: (
        /** The validator to localize. */
        validator: IDnnPersonaBarValidator
    ) => void;

    /** Trims a string to fix in a specified width (with ellipsis).
     * Highly recommend using css for that instead.
     * This assumes a fixed 8.5px fixed letter width. */
    trimContentToFit: (
        /** The string to trim. */
        content: string,
        /** The width in pixels to trim to. */
        width: number,
    ) => string;

    /** Copies an object into another one.
     * Recommend using Object.assign or object propagation instead.
     */
    getObjectCopy: (object: object) => object;

    /** Throttles the execution of a function to the next available cycle.
     * Same as setTimeout(callback, 0);
    */
    throttleExecution: (callback: ()=>any) => void;

    /** Just in case a developer forgets what numbers are. */
    ONE_THOUSAND: 1000,

    /** Just in case a developer forgets what numbers are. */
    ONE_MILLION: 1000000,

    /** Gets a string representing big numbers with an abbreviation.
     * Example: 1000 becomes 1K, 1000000 becomes 1M.
     */
    formatAbbreviateBigNumbers: (
        number: number,
    ) => string;

    /** Gets the current culture from the config. */
    getCulture: () => string;

    /** Gets the current SKU (useful if doing something different for different DNN editions.*/
    getSKU: () => string;

    /** Gets the current locale number separator symbol. */
    getNumbersSeparatorByLocale: () => string;

    /** Formats a number with the proper decimal separator (according to DNN localization). */
    formatCommaSeparate: (number: number) => string;

    /** Formats an amount of seconds into a string that is more readable for a human.
     * Example: 3600 becomes 1:00:00
     * Example: 3661 becomes 1:01:01
     * Example: 61 becomes 1:01
     * Example: 1 becomes 0:01
    */
    secondsFormatter: (
        /** The amount of seconds. */
        seconds: number
    ) => string;

    /** Gets the application root path */
    getApplicationRootPath: () => string;

    /** Gets the ID of the panel for a given path. */
    getPanelIdFromPath: (
        /** The path. */
        path: string
    ) => string;

    /** Parses the query string from the Path or path property of the given menu item object
     * and assigns it to a corresponding Query or query property,
     * while updating the original Path or path property to exclude the query string. */
    parseQueryParameter: (
        /** The menu item to parse the query for. */
        item: IDnnPersonaBarMenuItem
    ) => void;

    /** Builds the menu viewmodel. */
    buildMenuViewModel: (
        /** The menu structure to build the view model from. */
        menuStructure: IDnnPersonaBarMenuStructure
    ) => {menu: {menuItems: IDnnPersonaBarMenuItem[]}};

    /** Gets the path defined by the first menu item with a given module name. */
    getPathByModuleName: (
        /** The menu structure to search. */
        menuStructure: IDnnPersonaBarMenuStructure,
        /** The name of the module for which to get the path for. */
        moduleName: string,
    ) => string;
}

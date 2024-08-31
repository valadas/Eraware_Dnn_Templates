import type { IDnnPersonaBarUtilities } from "./types";

    /** Initializes the module.
     * @param _wrapper - The module wrapper.
     * @param _util - A utility object to interect with the Persona Bar.
     * @param _params - The parameters, could be anything your module has for settings.
     * @param callback - The callback function that can call to indicate initialization has completed.
     */
    export function init(wrapper: JQuery<HTMLElement>, util: IDnnPersonaBarUtilities, _params: any, callback: () => void): void {
        // jQuery? Where we are going, we don't need jQuery!
        var wrappingElement = wrapper.get(0) as HTMLElement;
        var module = wrappingElement.querySelector("my-app-root") as any;
        module.wrapper = wrappingElement;
        module.util = util;

        if (typeof callback === "function"){
            callback();
        }
    };

    /** Called when the module is already initialized but the user went to another persona bar page and came back to our module. */
    export function load(_util: IDnnPersonaBarUtilities, _params: any, callback: () => void): void {
        // Your logic here
        if (typeof callback === "function"){
            callback();
        }
    };

    /** In theory called when the user leaves the module but I could not make it work. */
    export function leave(_params: any, callback: () => void): void {
        // Your logic here
        if (typeof callback === "function"){
            callback();
        }
    }

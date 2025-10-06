import type { IDnnPersonaBarMenuStructureItem } from "./IDnnPersonaBarMenuStructureItem";

/** Represents the persona bar menu structure as received from the backend config. */
export type IDnnPersonaBarMenuStructure = {
    MenuItems: IDnnPersonaBarMenuStructureItem[]
};
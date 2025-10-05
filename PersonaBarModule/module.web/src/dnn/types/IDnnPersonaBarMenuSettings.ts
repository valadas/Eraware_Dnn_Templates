/** Settings object returned by findMenuSettings */
export interface IDnnPersonaBarMenuSettings {
  /** Whether the current user is an admin */
  isAdmin?: boolean;

  /** Whether the current user is a host/super user */
  isHost?: boolean;

  /** Additional custom settings specific to the menu item (parsed from JSON) */
  [key: string]: unknown;
}

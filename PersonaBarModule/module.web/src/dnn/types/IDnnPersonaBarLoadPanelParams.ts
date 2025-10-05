/** Parameters passed to the loadPanel function */
export interface IDnnPersonaBarLoadPanelParams {
  /** The unique identifier of the page/panel (e.g., "Dnn.Themes") */
  identifier?: string;

  /** The name of the module that handles this panel */
  moduleName?: string;

  /** The folder name where the panel resources are located (defaults to identifier if not provided) */
  folderName?: string;

  /** The path to the panel template/view */
  path?: string;

  /** Query string parameters to be passed to the panel */
  query?: string;

  /** Configuration settings specific to this panel (gets merged with default settings from menu structure) */
  settings?: Record<string, unknown>;

  /** 
   * If true, prevents automatic tab view handling in the module
   * Used when the module wants to handle tab persistence manually
   */
  handleTabViewInModule?: boolean;

  /** Additional custom parameters that can be passed to specific panels */
  [key: string]: unknown;
}

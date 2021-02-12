import { createStore } from "@stencil/store";
import { IItemViewModel } from "../services/services";

/** Defines the shape of the global state store. */
interface IStore {
  /** The list of items, could be partial since we have paging. */
  items: IItemViewModel[];
  /** The id of the last page of items we already fetched. */
  lastFetchedPage: number;
  /** The total amount of pages we know will be available to fetch. */
  totalPages: number;
  /** The total amount of items availalble
   * (but may not be fetched yet because of paging support)
  */
  availableItems: number;
  /** The current search query */
  searchQuery: string;
  /** The currently expanded item id */
  expandedItemId: number;
  /** Indicates whether the current user can edit items. */
  userCanEdit: boolean;
  /** The id of the Dnn module */
  moduleId: number;
}

/** Initializes the store with an initial (default) state. */
const { state } = createStore<IStore>({
  items: [],
  lastFetchedPage: 0,
  totalPages: 0,
  availableItems: 0,
  searchQuery: "",
  expandedItemId: -1,
  userCanEdit: false,
  moduleId: -1,
});

export default state;

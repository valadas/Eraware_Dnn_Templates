import { Component, h, Prop, State, Host, Element } from "@stencil/core";
import "@eraware/dnn-elements";
import { CreateItemDTO, ICreateItemDTO, IItemsPageViewModel, ItemClient, ItemsPageViewModel, ItemViewModel } from "../../services/services";
import { Debounce } from "@eraware/dnn-elements";
import state from "../../store/store";

@Component({
  tag: 'my-component',
  styleUrl: 'my-component.scss',
  shadow: true
})
export class MyComponent {
  private service: ItemClient;
  private pageSize = 10; // How many items to fetch per call.
  private lastScrollPosition = 0;
  private preloadOffset = 1000; // How many pixels to autoload items under the fold.
  private loadMoreButton: HTMLButtonElement;

  /**
   * Initializes the component
   */
  constructor() {
    this.service = new ItemClient({ moduleId: this.moduleId });
    state.moduleId = this.moduleId;
  }

  @Element() el: HTMLMyComponentElement;

  @State() elementWidth: number = 1200;
  @State() loading = true;
  @State() newItem: ICreateItemDTO = {
    name: "",
    description: ""
  };

  /** The Dnn module id, required in order to access web services. */
  @Prop() moduleId!: number;

  componentDidLoad(): void {
    this.scrollHandler();
    document.addEventListener('scroll', () => this.scrollHandler());
    this.resizeHandler();
    window.addEventListener('resize', () => this.resizeHandler());
    this.updateSearchQuery();
    this.service.userCanEdit().then(canEdit => state.userCanEdit = canEdit);
  }

  // eslint-disable-next-line @stencil/own-methods-must-be-private
  disconnectedCallback(): void {
    document.removeEventListener('scroll', () => this.scrollHandler());
    window.removeEventListener('resize', () => this.resizeHandler());
  }

  @Debounce()
  private scrollHandler() {
    if (this.lastScrollPosition < window.scrollY) {
      this.lastScrollPosition = window.scrollY;
    }
    this.infiniteScrollHandler();
  }

  private infiniteScrollHandler() {
    if (this.loadMoreButton != null) {
      const rect = this.loadMoreButton.getBoundingClientRect();
      const isInViewport = (
        rect.top >= 0 &&
        rect.left >= 0 &&
        rect.bottom <= (window.innerHeight + this.preloadOffset || document.documentElement.clientHeight + this.preloadOffset) &&
        rect.right <= (window.innerWidth || document.documentElement.clientWidth)
      );
      if (isInViewport && state.items.length > 0 && state.items.length !== state.availableItems) {
        this.getNextPage();
      }
    }
  }

  @Debounce(100)
  private resizeHandler() {
    this.elementWidth = this.el.getBoundingClientRect().width;
  }

  private updateSearchQuery(query = "") {
    state.expandedItemId = -1;
    state.lastFetchedPage = 0;
    this.loading = true;
    this.service.getItemsPage(query, state.lastFetchedPage + 1, this.pageSize, false)
      .then(response => {
        if (state.items.length >= response.resultCount || query === "") {
          // We are getting less items than displayed or the search was reset
          // Reset the view to start a new preload behaviour
          this.lastScrollPosition = 0;
          this.infiniteScrollHandler();
        }
        this.handleNewData(response);
        state.searchQuery = query;
        this.infiniteScrollHandler();
        this.loading = false;
      })
      .catch(reason => alert(reason));
  }

  private getNextPage() {
    if (state.items.length >= state.availableItems) {
      // We have less items than before, so we are in the process of resetting and won't fetch more pages yet.
      return;
    }
    this.service.getItemsPage(state.searchQuery, state.lastFetchedPage + 1, this.pageSize, false)
      .then(response => {
        this.addResults(response);
      })
  }

  /** Adds results to the existing ones */
  private addResults(data: IItemsPageViewModel) {
    if (data.page > state.lastFetchedPage) {
      // We have new data (not and old request we already have)
      state.items = [...state.items, ...data.items];
      state.lastFetchedPage = data.page;
      state.totalPages = data.pageCount;
      state.availableItems = data.resultCount;
      // Gives some time to the UI to render before the scroll handler fires to prevent too fast execution of infinite scroll.
      setTimeout(() => {
        this.infiniteScrollHandler();
      }, 300);
    }
  }

  /** Clears current results in favor of new ones */
  private handleNewData(data: ItemsPageViewModel) {
    state.items = data.items;
    state.totalPages = data.pageCount;
    state.lastFetchedPage = data.page;
    state.availableItems = data.resultCount;
    this.lastScrollPosition = 0;
  }

  private submitNewItem() {
    this.service.createItem(this.newItem as CreateItemDTO)
      .then(() => {
        this.newItem = {
          name: "",
          description: ""
        }
        this.updateSearchQuery(); // Forces update the UI.
      });
  }

  private deleteItem(item: ItemViewModel) {
    this.service.deleteItem(item.id)
      .then(() => this.updateSearchQuery());
  }

  render() {
    return <Host>
      <dnn-searchbox placeholder="Search" onQueryChanged={e => this.updateSearchQuery(e.detail)} />
      <div class="results-summary">
        <p>
          {state.items?.length > 0 ? `Showing ${state.items.length} of ${state.availableItems} Items` : "No Results"}
        </p>
        {this.loading && <p>Loading...</p>}
      </div>
      {state.items.map((item) =>
        <div class="collapsible-row">
          <div class="collapsible-title">
            <dnn-chevron expanded={state.expandedItemId == item.id} onChanged={(e) => e.detail ? state.expandedItemId = item.id : state.expandedItemId = -1} />
            <strong>{item.name}</strong>
          </div>
          <dnn-collapsible expanded={state.expandedItemId == item.id}>
            {item.description?.length > 0 ? <div>{item.description}</div> : <div>No description</div>}
            <dnn-button type="tertiary" confirm={true} onConfirmed={() => this.deleteItem(item as ItemViewModel)}>Delete</dnn-button>
          </dnn-collapsible>
        </div>
      )
      }
      {state.userCanEdit &&
        <div class="add-item">
          <form class="add-grid" style={{ display: 'grid', gridTemplateColumns: this.elementWidth > 800 ? '1fr 3fr' : '1fr' }}>
            <label>Name:</label>
            <input type="text" value={this.newItem.name} required
              onInput={e => this.newItem = { ...this.newItem, name: (e.target as HTMLInputElement).value }}
            ></input>

            <label>Description:</label>
            <textarea value={this.newItem.description}
              onInput={e => this.newItem = { ...this.newItem, description: (e.target as HTMLTextAreaElement).value }}
            ></textarea>
          </form>
          <dnn-button type="primary"
            disabled={this.newItem.name.length < 1}
            onClick={() => this.submitNewItem()}
          >Add Item</dnn-button><br />
        </div>
      }

      <button id="load-more-button" ref={e => this.loadMoreButton = e}>Load More</button>
    </Host>;
  }
}

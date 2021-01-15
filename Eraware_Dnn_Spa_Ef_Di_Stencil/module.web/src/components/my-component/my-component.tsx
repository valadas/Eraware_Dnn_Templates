import { Component, h, Prop, State, Host, Element } from '@stencil/core';
import '@eraware/dnn-elements';
import { ItemsService, IItem, IGetItemsResponse } from '../../services/ItemService';
import { Debounce } from '@eraware/dnn-elements';

@Component({
  tag: 'my-component',
  styleUrl: 'my-component.scss',
  shadow: true
})
export class MyComponent {
  private service: ItemsService;
  private pageSize = 10; // How many items to fetch per call.
  private lastScrollPosition = 0;
  private preloadOffset = 1000; // How many pixels to autoload items under the fold.
  private loadMoreButton: HTMLButtonElement;

  /**
   * Initializes the component
   */
  constructor() {
    this.service = new ItemsService(this.moduleId);
  }

  @Element() el: HTMLMyComponentElement;

  @State() items: IItem[] = [];
  @State() lastFetchedPage = 0;
  @State() totalPages = 0;
  @State() availableItems = 0;
  @State() searchQuery = "";
  @State() expandedItem: number | null = null;
  @State() elementWidth: number = 1200;
  @State() canEdit = false;
  @State() loading = true;
  @State() newItem: IItem = {
    Id: -1,
    Name: "",
    Description: ""
  };

  /** The Dnn module id */
  @Prop() moduleId: number;

  componentDidLoad() {
    this.scrollHandler();
    document.addEventListener('scroll', () => this.scrollHandler());
    this.resizeHandler();
    window.addEventListener('resize', () => this.resizeHandler());
    this.updateSearchQuery();
  }

  // eslint-disable-next-line @stencil/own-methods-must-be-private
  disconnectedCallback() {
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
      if (isInViewport && this.items.length > 0 && this.items.length !== this.availableItems) {
        this.getNextPage();
      }
    }
  }

  @Debounce(100)
  private resizeHandler() {
    this.elementWidth = this.el.getBoundingClientRect().width;
  }

  private updateSearchQuery(query: string = "") {
    this.expandedItem = null;
    this.lastFetchedPage = 0;
    this.loading = true;
    this.service.GetItems(query, this.lastFetchedPage + 1, this.pageSize)
      .then(data => {
        if (this.items.length >= data.resultCount || query === "") { // We are getting less items than displayed or the search was reset
          // Reset the view to start a new preload behaviour
          this.lastScrollPosition = 0;
          this.infiniteScrollHandler();
        }
        this.handleNewData(data);
        this.searchQuery = query;
        this.infiniteScrollHandler();
        this.loading = false;
      })
      .catch(error => alert(error));
  }

  private getNextPage() {
    if (this.items.length >= this.availableItems) {
      // We have less items than before, so we are in the process of resetting and won't fetch more pages yet.
      return;
    }
    this.service.GetItems(this.searchQuery, this.lastFetchedPage + 1, this.pageSize)
      .then(data => {
        this.addResults(data);
      })
  }

  /** Adds results to the existing ones */
  private addResults(data: IGetItemsResponse) {
    if (data.page > this.lastFetchedPage) {
      // We have new data (not and old request we already have)
      this.items = [...this.items, ...data.items];
      this.lastFetchedPage = data.page;
      this.totalPages = data.pageCount;
      this.availableItems = data.resultCount;
      this.canEdit = data.CanEdit;
      // Gives some time to the UI to render before the scroll handler fires to prevent too fast execution of infinite scroll.
      setTimeout(() => {
        this.infiniteScrollHandler();
      }, 300);
    }
  }

  /** Clears current results in favor of new ones */
  private handleNewData(data: IGetItemsResponse) {
    this.items = data.items;
    this.totalPages = data.pageCount;
    this.lastFetchedPage = data.page;
    this.availableItems = data.resultCount;
    this.canEdit = data.CanEdit;
    this.lastScrollPosition = 0;
  }

  private submitNewItem() {
    this.service.CreateItem(this.newItem)
      .then(() => {
        this.newItem = {
          Id: -1,
          Name: "",
          Description: ""
        }
        this.updateSearchQuery(); // Forces update the UI.
      });
  }

  private deleteItem(item: IItem) {
    this.service.DeleteItem(item)
      .then(() => this.updateSearchQuery());
  }

  render() {
    return <Host>
      <dnn-searchbox placeholder="Search" onQueryChanged={e => this.updateSearchQuery(e.detail)} />
      <div class="results-summary">
        <p>
          {this.items?.length > 0 ? `Showing ${this.items.length} of ${this.availableItems} Items` : "No Results"}
        </p>
        {this.loading && <p>Loading...</p>}
      </div>
      {this.items.map((item) =>
        <div class="collapsible-row">
          <div class="collapsible-title">
            <dnn-chevron expanded={this.expandedItem == item.Id} onChanged={(e) => e.detail ? this.expandedItem = item.Id : this.expandedItem = null} />
            <strong>{item.Name}</strong>
          </div>
          <dnn-collapsible expanded={this.expandedItem == item.Id}>
            {item.Description?.length > 0 ? <div>{item.Description}</div> : <div>No description</div>}
            <dnn-button type="tertiary" confirm={true} onConfirmed={() => this.deleteItem(item)}>Delete</dnn-button>
          </dnn-collapsible>
        </div>
      )
      }
      {this.canEdit &&
        <div class="add-item">
          <form class="add-grid" style={{ display: 'grid', gridTemplateColumns: this.elementWidth > 800 ? '1fr 3fr' : '1fr' }}>
            <label>Name:</label>
            <input type="text" value={this.newItem.Name} required
              onInput={e => this.newItem = { ...this.newItem, Name: (e.target as HTMLInputElement).value }}
            ></input>

            <label>Description:</label>
            <textarea value={this.newItem.Description}
              onInput={e => this.newItem = { ...this.newItem, Description: (e.target as HTMLTextAreaElement).value }}
            ></textarea>
          </form>
          <dnn-button type="primary"
            disabled={this.newItem.Name.length < 1}
            onClick={() => this.submitNewItem()}
          >Add Item</dnn-button><br />
        </div>
      }

      <button id="load-more-button" ref={e => this.loadMoreButton = e as HTMLButtonElement}>Load More</button>
    </Host>;
  }
}

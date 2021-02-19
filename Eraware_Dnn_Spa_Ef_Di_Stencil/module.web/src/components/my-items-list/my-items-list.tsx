import { Debounce } from '@eraware/dnn-elements';
import { Component, Host, h, State, Prop, Element } from '@stencil/core';
import { ItemClient } from '../../services/services';
import state from '../../store/state';

@Component({
  tag: 'my-items-list',
  styleUrl: 'my-items-list.scss',
  shadow: true,
})
export class MyItemsList {
  /** Defines how many items to fetch per request. */
  @Prop() pageSize = 100;

  /** Defines how many pixels under the fold to preload. */
  @Prop() preloadPixels = 1000;

  @State() loading = true;

  @Element() el: HTMLMyItemsListElement;

  private itemClient!: ItemClient;
  private abortController: AbortController;

  constructor() {
    this.itemClient = new ItemClient({
      moduleId: state.moduleId,
    });
  }

  connectedCallback() {
    window.addEventListener("scroll", this.scrollHandler);
  }

  disconnectedCallback() {
    window.removeEventListener("scroll", this.scrollHandler);
  }

  componentDidLoad() {
    this.preload();
  }

  componentDidUpdate() {
    this.preload();
  }

  private preload() {
    if (this.el.getBoundingClientRect().bottom - window.innerHeight < this.preloadPixels) {
      if (!state.allLoaded) {
        this.loadMore()
          .then(() => {
            if (!state.allLoaded) {
              this.preload();
            }
          })
          .catch(() => { });
      }
    }
  }

  private scrollHandler = () => {
    this.handleScroll();
  }

  @Debounce()
  private handleScroll() {
    if (this.el.getBoundingClientRect().bottom - window.innerHeight < this.preloadPixels) {
      this.loadMore();
    }
  }

  private loadMore() {
    return new Promise<void>((resolve, reject) => {
      if (state.items.length == 0 || state.items.length < state.availableItems) {
        this.loading = true;
        this.abortController?.abort();
        this.abortController = new AbortController();
        this.itemClient.getItemsPage(
          state.searchQuery,
          state.lastFetchedPage + 1,
          this.pageSize,
          false,
          this.abortController.signal)
          .then(results => {
            state.items = [...state.items, ...results.items];
            state.availableItems = results.resultCount;
            state.lastFetchedPage = results.page;
            state.totalPages = results.pageCount;
            this.loading = false;
            if (state.items.length === results.resultCount) {
              state.allLoaded = true;
            }
            resolve();
          }, rejectReason => {
            if (rejectReason instanceof DOMException && rejectReason.code === rejectReason.ABORT_ERR) {
              reject(() => { });
              return;
            }
            alert(rejectReason);
            reject(rejectReason);
          })
          .catch(rejectReason => {
            alert(rejectReason);
            reject(rejectReason);
          });
      }
    });
  }

  render() {
    return (
      <Host>
        {state.items.map(item =>
          <div class="item">
            <div
              class="title"
              onClick={() => {
                if (state.expandedItemId === item.id) {
                  state.expandedItemId = -1;
                } else {
                  state.expandedItemId = item.id;
                }
              }}
            >
              <dnn-chevron
                expanded={state.expandedItemId === item.id}
              />
              {item.name}
            </div>
            <dnn-collapsible expanded={state.expandedItemId === item.id}>
              {state.expandedItemId === item.id &&
                <my-item-details item={item} />
              }
            </dnn-collapsible>
          </div>
        )}
        {this.loading &&
          <div class="loading"></div>
        }
        <div class="footer">
          <p>Showing {state.items.length} of {state.availableItems} items.</p>
          {!this.loading && state.items.length < state.availableItems &&
            <dnn-button type="primary" reversed
              onClick={() => this.loadMore()}
            >
              Load More
            </dnn-button>
          }
        </div>
      </Host>
    );
  }
}

import {
  Component, Host, h, State, Element, Method,
  Event, EventEmitter
} from '@stencil/core';
import { CreateItemDTO, IItemViewModel, ItemClient } from '../../services/services';
import state from "../../store/store";

@Component({
  tag: 'my-edit',
  styleUrl: 'my-edit.scss',
  shadow: true,
})
export class MyEdit {
  @State() item: IItemViewModel;
  @Element() el!: HTMLMyEditElement;

  private nameInput!: HTMLInputElement;
  private itemClient!: ItemClient;

  constructor() {
    this.itemClient = new ItemClient({
      moduleId: state.moduleId,
    });
  }

  /** Sets focus on the first form element */
  @Method()
  public async setFocus() {
    setTimeout(() => {
      this.nameInput.focus();
    }, 500);
  }

  /** Fires up when an item got created. */
  @Event() itemCreated: EventEmitter

  componentWillLoad() {
    this.item = {
      id: -1,
      name: "",
      description: "",
    }
  }

  private hideModal(): void {
    this.el.closest("dnn-modal").hide();
  }

  private saveItem(): void {
    const createItemDTO = new CreateItemDTO({
      name: this.item.name,
      description: this.item.description,
    });
    this.itemClient.createItem(createItemDTO)
      .then(() => {
        this.itemCreated.emit();
        this.hideModal();
      },
        reason => alert(reason))
      .catch(reason => alert(reason));
  }

  render() {
    return (
      <Host>
        <div class="grid">
          <label htmlFor="name">Name</label>
          <input
            id="name"
            type="text"
            value={this.item.name}
            required
            ref={e => this.nameInput = e}
            onInput={e => this.item = { ...this.item, name: (e.target as HTMLInputElement).value }}
          />

          <label htmlFor="description">Description</label>
          <textarea
            id="description"
            value={this.item.description}
            onInput={e => this.item = { ...this.item, description: (e.target as HTMLTextAreaElement).value }} />
        </div>
        <div class="controls">
          <dnn-button
            type="secondary"
            reversed
            onClick={() => this.hideModal()}
          >
            Cancel
          </dnn-button>
          <dnn-button
            type="primary"
            disabled={this.item.name.length === 0}
            onClick={() => this.saveItem()}
          >
            Save
          </dnn-button>
        </div>
      </Host>
    );
  }
}

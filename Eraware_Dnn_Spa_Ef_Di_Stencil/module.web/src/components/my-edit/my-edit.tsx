import {
  Component, Host, h, Element, Method,
  Event, EventEmitter, Prop
} from '@stencil/core';
import { CreateItemDTO, IItemViewModel, ItemClient, UIInfo, UpdateItemDTO } from '../../services/services';
import state, { store, localizationState } from "../../store/state";
import alertError from "../../services/alert-error";

@Component({
  tag: 'my-edit',
  styleUrl: 'my-edit.scss',
  shadow: true,
})
export class MyEdit {
  /** The item to create or edit. */
  @Prop({ mutable: true }) item: IItemViewModel;

  @Element() el!: HTMLMyEditElement;

  private nameInput!: HTMLInputElement;
  private itemClient!: ItemClient;
  private resx: UIInfo;

  constructor() {
    this.itemClient = new ItemClient({
      moduleId: state.moduleId,
    });
    this.resx = localizationState.viewModel.uI;
  }

  /** Sets focus on the first form element */
  @Method()
  public async setFocus() {
    setTimeout(() => {
      this.nameInput.focus();
    }, 500);
  }

  /** Resets the form to insert a new item. */
  @Method()
  public async resetForm() {
    this.item = {
      id: -1,
      name: "",
      description: "",
    }
  }

  /** Fires up when an item got created. */
  @Event() itemCreated: EventEmitter

  componentWillLoad() {
    if (this.item == undefined) {
      this.resetForm();
    }
  }

  private hideModal(): void {
    this.el.closest("dnn-modal").hide();
  }

  private saveItem(): void {
    if (this.item.id < 1) {
      const createItemDTO = new CreateItemDTO({
        name: this.item.name,
        description: this.item.description,
      });
      this.itemClient.createItem(createItemDTO)
        .then(() => {
          this.itemCreated.emit();
          this.hideModal();
        },
          reason => alertError(reason))
        .catch(reason => alertError(reason));
    }
    else {
      const updateItemDTO = new UpdateItemDTO({
        id: this.item.id,
        name: this.item.name,
        description: this.item.description,
      });
      this.itemClient.updateItem(updateItemDTO)
        .then(() => {
          this.hideModal();
        }, reason => alert(reason))
        .catch(reason => alert(reason));
    }
    const oldCanEdit = state.userCanEdit;
    store.reset();
    state.userCanEdit = oldCanEdit;
  }

  render() {
    return (
      <Host>
        <div class="grid">
          <label htmlFor="name">{this.resx.name || "Name"}</label>
          <input
            id="name"
            type="text"
            value={this.item.name}
            required
            ref={e => this.nameInput = e}
            onInput={e => this.item = { ...this.item, name: (e.target as HTMLInputElement).value }}
          />

          <label htmlFor="description">{this.resx.description || "Description"}</label>
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
            {this.resx.cancel || "Cancel"}
          </dnn-button>
          {this.item.id < 1 &&
            <dnn-button
              type="primary"
              disabled={this.item.name.trim().length === 0}
              onClick={() => this.saveItem()}
            >
              {this.resx.create || "Create"}
            </dnn-button>
          }
          {this.item.id > 0 &&
            <dnn-button
              type="primary"
              disabled={this.item.name.trim().length === 0}
              onClick={() => this.saveItem()}
            >
              {this.resx.save || "Save"}
            </dnn-button>
          }
        </div>
      </Host>
    );
  }
}

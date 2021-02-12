import { Component, h, Prop, Host, Element, Listen } from "@stencil/core";
import "@eraware/dnn-elements";
import { ItemClient } from "../../services/services";
import state from "../../store/state";

@Component({
  tag: 'my-component',
  styleUrl: 'my-component.scss',
  shadow: true
})
export class MyComponent {
  private service: ItemClient;

  constructor() {
    this.service = new ItemClient({ moduleId: this.moduleId });
    state.moduleId = this.moduleId;
  }

  @Element() el: HTMLMyComponentElement;

  /** The Dnn module id, required in order to access web services. */
  @Prop() moduleId!: number;

  componentDidLoad(): void {
    this.service.userCanEdit().then(canEdit => state.userCanEdit = canEdit);
  }

  @Listen("itemCreated")
  handleItemCreated() {
    state.searchQuery = "";
  }

  render() {
    return <Host>
      <div class="header">
        <dnn-searchbox placeholder="Search" onQueryChanged={e => state.searchQuery = e.detail} />
        {state.userCanEdit &&
          <my-create />
        }
      </div>
      <my-items-list />
    </Host>;
  }
}

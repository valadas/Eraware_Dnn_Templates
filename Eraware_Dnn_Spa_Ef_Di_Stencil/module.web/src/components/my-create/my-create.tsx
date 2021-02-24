import { Component, Host, h } from '@stencil/core';
import { localizationState } from "../../store/state";

@Component({
  tag: 'my-create',
  styleUrl: 'my-create.scss',
  shadow: true,
})
export class MyCreate {

  private modal!: HTMLDnnModalElement;
  private editForm!: HTMLMyEditElement;

  render() {
    const resx = localizationState.viewModel.uI;
    return (
      <Host>
        <dnn-button
          type="primary"
          onClick={() =>
            this.modal.show()
              .then(() =>
                this.editForm.resetForm()
                  .then(() =>
                    this.editForm.setFocus()))}
        >
          {resx.addItem}
        </dnn-button>
        <dnn-modal
          ref={e => this.modal = e}
          showCloseButton={false}
          backdropDismiss={false}
        >
          <my-edit ref={e => this.editForm = e} />
        </dnn-modal>
      </Host>
    );
  }
}

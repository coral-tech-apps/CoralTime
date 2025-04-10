import { Component, Output, EventEmitter } from '@angular/core';

@Component({
    selector: 'ct-confirmation-dialog',
    templateUrl: 'confirmation.component.html',
    standalone: false
})

export class ConfirmationComponent {
	message: string;

	@Output() onSubmit = new EventEmitter();

	submit(confirm: boolean) {
		this.onSubmit.emit(confirm);
	}
}

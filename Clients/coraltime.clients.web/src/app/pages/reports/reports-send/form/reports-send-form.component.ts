import { UntypedFormArray, UntypedFormGroup, UntypedFormControl, UntypedFormBuilder, Validators } from '@angular/forms';
import { Component, Output, EventEmitter, Input, OnDestroy } from '@angular/core';
import { Client } from '../../../../models/client';
import { Subscription } from 'rxjs';

@Component({
    selector: 'ct-reports-send-form',
    templateUrl: 'reports-send-form.component.html',
    standalone: false
})

export class ReportsSendFormComponent implements OnDestroy {
	@Input() buttonText: string;
	@Input() clients: Client[];
	@Input() emailValues: string[] = [];
	@Input() isFormErrorsShown: boolean;

	@Output() formChanged = new EventEmitter();

	form: UntypedFormGroup;
	private subscription: Subscription;

	constructor(private fb: UntypedFormBuilder) {
		this.form = this.fb.group({
			emails: new UntypedFormArray([
				new UntypedFormControl('', Validators.required)
			])
		});
		this.subscription = this.form.valueChanges.subscribe(() => {
			this.formChanged.emit(this.form.valid);
		});
	}

	addNewEmail(): void {
		const arrayControl = <UntypedFormArray>this.form.controls['emails'];
		let newControl = new UntypedFormControl('', Validators.required);
		arrayControl.push(newControl);
	}

	delEmail(index: number): void {
		const arrayControl = <UntypedFormArray>this.form.controls['emails'];
		arrayControl.removeAt(index);
		this.emailValues.splice(index, 1);
	}

	get emails(): UntypedFormArray { return this.form.get('emails') as UntypedFormArray; }

	ngOnDestroy() {
		this.subscription.unsubscribe();
	}
}

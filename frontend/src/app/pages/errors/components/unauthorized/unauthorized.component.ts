import { Component, OnInit } from '@angular/core';
import { UIMessage } from 'primeng/message';

@Component({
	selector: 'ct-unauthorized',
	templateUrl: 'unauthorized.component.html'
})

export class UnauthorizedComponent implements OnInit {
	message: string;
	msgs: UIMessage[] = [];

	constructor() {
		this.message = '401: Unauthorized ';
	}

	ngOnInit() {
		this.showError();
	}

	showError(): void {
		this.msgs.push({severity: 'error', text: this.message, escape: false, style: false, styleClass: '', icon: ''});
	}
}

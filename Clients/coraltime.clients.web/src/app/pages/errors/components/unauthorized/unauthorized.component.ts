import { Component, OnInit} from '@angular/core';
import { Message} from 'primeng/message';

@Component({
    selector: 'ct-unauthorized',
    templateUrl: 'unauthorized.component.html',
    standalone: false
})

export class UnauthorizedComponent implements OnInit {
	message: string;
	msgs: Message[] = [];

	constructor() {
		this.message = '401: Unauthorized ';
	}

	ngOnInit() {
		this.showError();
	}

	showError(): void {
    let newMessage: Message;
    newMessage.text = this.message;
    newMessage.severity = 'error';
    newMessage.escape = false;
    this.msgs.push(newMessage);
	}
}
